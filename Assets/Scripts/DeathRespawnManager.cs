using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Player Death & Respawn System (design spec "Player Death & Respawn System").
/// The death popup is ONLY for respawning and showing XP/crystal information.
/// Crystal purchasing happens in the Main Menu only.
///
/// Crystal model (no double counting):
/// - Bank          = CrystalSaveSystem.Total (CrystalPickup banks at pickup time).
/// - Pending (run) = crystals collected through CrystalManager that are not banked yet.
///                   Register them with RegisterRunCrystal(amount).
/// - Available     = Bank + Pending. Reconciling happens at most ONCE per run.
/// </summary>
public class DeathRespawnManager : MonoBehaviour
{
    public static DeathRespawnManager Instance { get; private set; }

    [Header("Death Popup UI")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private TextMeshProUGUI runXpUI;          // "Current Game XP"        -> +250 XP
    [SerializeField] private TextMeshProUGUI totalXpUI;        // "Total XP"               -> 1,250 XP
    [SerializeField] private TextMeshProUGUI totalCrystalsUI;  // "Total Crystals"         -> 55
    [SerializeField] private TextMeshProUGUI runCrystalsUI;    // "Current Game Crystals"  -> +15
    [SerializeField] private TextMeshProUGUI availableUI;      // "Available for respawn"  -> 70 crystals
    [SerializeField] private TextMeshProUGUI respawnCostUI;    // "Respawn cost"           -> 5 crystals
    [SerializeField] private Button respawnButton;             // wired in code - do NOT also hook in inspector
    [SerializeField] private Button giveUpButton;              // wired in code - do NOT also hook in inspector

    [Header("Respawn Countdown")]
    [SerializeField] private TextMeshProUGUI countdownUI;
    [SerializeField] private int countdownFrom = 3;

    [Header("Respawn Settings")]
    [SerializeField] private int baseRespawnCost = 5;          // doubles after every successful respawn
    [SerializeField] private Transform respawnPoint;           // optional; defaults to the player's start pose
    [SerializeField] private float popupFadeDuration = 1f;
    [SerializeField] private float gameOverDelay = 4f;         // matches the original Game Over -> Main Menu delay

    [Header("References")]
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private string mainMenuScene = "MainMenu";

    public int RespawnCost { get; private set; }
    public int RespawnCount { get; private set; }
    public bool IsPlayerDead { get; private set; }

    /// <summary>Permanent crystal balance before this run started ("Total Crystals" in the popup).</summary>
    public int PermanentTotal => startingBank;

    /// <summary>Crystals collected during this run ("Current Game Crystals" in the popup).</summary>
    public int RunCrystals => (CrystalSaveSystem.Total - startingBank) + pendingRunCrystals;

    /// <summary>Bank + pending run crystals ("Available for respawn" in the popup).</summary>
    public int AvailableForRespawn => CrystalSaveSystem.Total + pendingRunCrystals;

    private int startingBank;
    private int pendingRunCrystals;
    private bool runFinalized;
    private bool processingRespawn;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Snapshot the permanent balance before any pickup of this run is banked.
        startingBank = CrystalSaveSystem.Total;
        RespawnCost = Mathf.Max(1, baseRespawnCost);
        RespawnCount = 0;
        pendingRunCrystals = 0;
        runFinalized = false;

        XPSystem.ResetRun();

        GameObject playerObj = FindPlayer();
        if (playerObj != null)
        {
            startPosition = playerObj.transform.position;
            startRotation = playerObj.transform.rotation;
        }

        if (deathPanel != null) deathPanel.SetActive(false);
        if (countdownUI != null) countdownUI.gameObject.SetActive(false);

        if (respawnButton != null) respawnButton.onClick.AddListener(OnRespawnClicked);
        if (giveUpButton != null) giveUpButton.onClick.AddListener(OnGiveUpClicked);
    }

    // ------------------------------------------------------------------
    // Crystals
    // ------------------------------------------------------------------

    /// <summary>
    /// Register a crystal collected during the run that is NOT yet in the permanent bank
    /// (i.e. collected through CrystalManager). Call it exactly once per crystal.
    /// </summary>
    public void RegisterRunCrystal(int amount)
    {
        if (amount <= 0) return;
        pendingRunCrystals += amount;
        if (IsPlayerDead) RefreshDeathPopup();
    }

    /// <summary>Moves pending run crystals into the permanent bank. Runs at most once per run.</summary>
    private void ReconcileRunCrystals()
    {
        if (pendingRunCrystals <= 0) return;

        CrystalSaveSystem.Add(pendingRunCrystals);
        pendingRunCrystals = 0;
    }

    // ------------------------------------------------------------------
    // Death
    // ------------------------------------------------------------------

    /// <summary>Called by Player.PlayerDead() instead of the old auto Game Over flow.</summary>
    public void OnPlayerDeath()
    {
        if (IsPlayerDead) return;
        IsPlayerDead = true;

        // Freeze the world (waves, zombies, coroutines) while the popup is open.
        Time.timeScale = 0f;
        AudioListener.pause = true; // Silence zombie attacks, groans, etc. while dead
        if (MobileInputManager.Instance != null)
        {
            MobileInputManager.Instance.isFireHeld = false;
        }

        SetCursorFree(true);
        RefreshDeathPopup();
        StartCoroutine(ShowPopupRoutine());
    }

    private IEnumerator ShowPopupRoutine()
    {
        // Fade to black with UNSCALED time (the world is frozen at this point).
        yield return FadeScreen(1f, popupFadeDuration);

        if (deathPanel != null) deathPanel.SetActive(true);
    }

    private void RefreshDeathPopup()
    {
        if (runXpUI != null) runXpUI.text = $"+{XPSystem.RunXP} XP";
        if (totalXpUI != null) totalXpUI.text = $"{XPSystem.TotalXP:N0} XP";
        if (totalCrystalsUI != null) totalCrystalsUI.text = $"{startingBank}";
        if (runCrystalsUI != null) runCrystalsUI.text = $"+{RunCrystals}";
        if (availableUI != null) availableUI.text = $"{AvailableForRespawn} crystals";
        if (respawnCostUI != null) respawnCostUI.text = $"{RespawnCost} crystals";
    }

    // ------------------------------------------------------------------
    // Respawn
    // ------------------------------------------------------------------

    public void OnRespawnClicked()
    {
        if (!IsPlayerDead || processingRespawn) return;

        // Cannot afford it -> existing Game Over -> Main Menu flow.
        if (AvailableForRespawn < RespawnCost)
        {
            StartCoroutine(GameOverToMenuRoutine());
            return;
        }

        processingRespawn = true;

        // Bank the pending run crystals first, then deduct the cost EXACTLY ONCE.
        ReconcileRunCrystals();
        if (!CrystalSaveSystem.TrySpend(RespawnCost))
        {
            // Balance changed between the check and the spend -> treat as unaffordable.
            StartCoroutine(GameOverToMenuRoutine());
            return;
        }

        CrystalSaveSystem.Save();
        RespawnCount++;
        RespawnCost *= 2; // double the NEXT respawn cost after a successful respawn.

        ResetPlayer();
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        if (deathPanel != null) deathPanel.SetActive(false);

        // 3 - 2 - 1 countdown (realtime, the world is still frozen).
        if (countdownUI != null)
        {
            countdownUI.gameObject.SetActive(true);
            for (int i = countdownFrom; i > 0; i--)
            {
                countdownUI.text = i.ToString();
                yield return new WaitForSecondsRealtime(1f);
            }
            countdownUI.gameObject.SetActive(false);
        }

        // Clear the death fade while counting down.
        yield return FadeScreen(0f, popupFadeDuration);

        // Resume the existing wave exactly where it stopped.
        Time.timeScale = 1f;
        AudioListener.pause = false; // Unpause audio
        IsPlayerDead = false;
        processingRespawn = false;
        SetCursorFree(false);
    }

    private void ResetPlayer()
    {
        GameObject playerObj = FindPlayer();
        if (playerObj == null) return;

        // 1. Original spawn position with full health.
        if (respawnPoint != null)
        {
            playerObj.transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
        }
        else
        {
            playerObj.transform.SetPositionAndRotation(startPosition, startRotation);
        }

        Player player = playerObj.GetComponentInParent<Player>();
        if (player == null) player = playerObj.GetComponentInChildren<Player>();
        if (player != null)
        {
            player.HP = player.maxHP;
            player.ResetDeadState();
            player.UpdateHealthUI();
            if (player.playerHealthUI != null) player.playerHealthUI.gameObject.SetActive(true);
            if (player.healthBarRoot != null) player.healthBarRoot.SetActive(true);
            if (player.healthSlider != null) player.healthSlider.gameObject.SetActive(true);
            if (player.healthBarFill != null) player.healthBarFill.gameObject.SetActive(true);
            if (player.bloodyScreen != null) player.bloodyScreen.SetActive(false);
        }

        // 2. Re-enable movement / look (they were disabled by Player.PlayerDead).
        MouseMovement mm = playerObj.GetComponentInParent<MouseMovement>();
        if (mm != null) mm.enabled = true;

        PlayerMovement pm = playerObj.GetComponentInParent<PlayerMovement>();
        if (pm != null) pm.enabled = true;

        // 3. Undo the death animation (the animator is only enabled while dead).
        Animator anim = playerObj.GetComponentInChildren<Animator>();
        if (anim != null) anim.enabled = false;
    }

    // ------------------------------------------------------------------
    // Give up / Game Over
    // ------------------------------------------------------------------

    public void OnGiveUpClicked()
    {
        if (!IsPlayerDead) return;

        FinalizeRun();
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(mainMenuScene);
    }

    private IEnumerator GameOverToMenuRoutine()
    {
        if (deathPanel != null) deathPanel.SetActive(false);
        if (countdownUI != null) countdownUI.gameObject.SetActive(false);

        FinalizeRun();

        // Show the existing Game Over UI, then return to the Main Menu.
        GameObject playerObj = FindPlayer();
        Player player = playerObj != null ? playerObj.GetComponentInParent<Player>() : null;
        if (player == null && playerObj != null) player = playerObj.GetComponentInChildren<Player>();
        if (player != null && player.gameOverUI != null) player.gameOverUI.gameObject.SetActive(true);

        Time.timeScale = 1f;
        AudioListener.pause = false;
        yield return new WaitForSecondsRealtime(gameOverDelay);

        SceneManager.LoadScene(mainMenuScene);
    }

    /// <summary>Reconcile crystals once, commit remaining run XP, preserve the highest-wave save.</summary>
    private void FinalizeRun()
    {
        if (runFinalized) return;
        runFinalized = true;

        // Reconcile and save the crystal balance without counting current-game crystals twice.
        ReconcileRunCrystals();
        CrystalSaveSystem.Save();

        // Finalize the run's XP (catches anything not committed after a wave).
        XPSystem.CommitRunXP();

        // Preserve the highest-wave save.
        if (GlobalReferences.Instance != null && SaveLoadManager.Instance != null)
        {
            int waveSurvived = GlobalReferences.Instance.waveNumber;
            if (waveSurvived - 1 > SaveLoadManager.Instance.LoadHighScore())
            {
                SaveLoadManager.Instance.SaveHighScore(waveSurvived - 1);
            }
        }
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private GameObject FindPlayer()
    {
        return GameObject.FindGameObjectWithTag("Player");
    }

    private IEnumerator FadeScreen(float targetAlpha, float duration)
    {
        if (screenFader == null || screenFader.fadeImage == null) yield break;

        Image img = screenFader.fadeImage;
        img.gameObject.SetActive(true);

        if (duration <= 0f)
        {
            Color c = img.color;
            c.a = targetAlpha;
            img.color = c;
            yield break;
        }

        float t = 0f;
        Color start = img.color;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            Color c = start;
            c.a = Mathf.Lerp(start.a, targetAlpha, t / duration);
            img.color = c;
            yield return null;
        }

        Color end = img.color;
        end.a = targetAlpha;
        img.color = end;
    }

    private void SetCursorFree(bool free)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = free;
#endif
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            // Safety net so the next scene is never stuck frozen.
            Instance = null;
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}



