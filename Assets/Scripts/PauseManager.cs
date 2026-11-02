using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("UI")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private CanvasGroup pauseCanvasGroup;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private int countdownFrom = 3;
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private bool saveCrystalsOnExit = true;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool counting;

    private void Awake()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void Start()
    {
        pausePanel.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        pauseButton.onClick.AddListener(() => { Pause(); PlayButtonClick(); });
        resumeButton.onClick.AddListener(() => { Resume(); PlayButtonClick(); });
        exitButton.onClick.AddListener(() => { ExitToMenu(); PlayButtonClick(); });

    }

    private void PlayButtonClick()
    {
        SoundManager.Instance?.PlayButtonClick();
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (!IsPaused) Pause();
            else if (!counting) Resume();
        }
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;

        Time.timeScale = 0f;
        AudioListener.pause = true;
        if (MobileInputManager.Instance != null)
        {
            MobileInputManager.Instance.isFireHeld = false;
        }

        pauseButton.gameObject.SetActive(false);
        UIPopupAnimator.OpenPopup(pausePanel);

        SetCursorFree(true);
    }

    public void Resume()
    {
        if (!IsPaused || counting) return;
        StartCoroutine(ResumeRoutine());
    }

    private IEnumerator ResumeRoutine()
    {
        counting = true;
        UIPopupAnimator.ClosePopup(pausePanel);
        yield return new WaitForSecondsRealtime(0.2f);

        // Countdown 3, 2, 1
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            for (int i = countdownFrom; i > 0; i--)
            {
                countdownText.text = i.ToString();
                yield return new WaitForSecondsRealtime(1f);
            }
            countdownText.gameObject.SetActive(false);
        }

        // Continue exactly where the game stopped
        Time.timeScale = 1f;
        AudioListener.pause = false;
        IsPaused = false;
        counting = false;

        pauseButton.gameObject.SetActive(true);
        SetCursorFree(false);
    }

    public void ExitToMenu()
    {
        if (saveCrystalsOnExit) CrystalSaveSystem.Save();

        // Always reset these, or the main menu will be frozen
        Time.timeScale = 1f;
        AudioListener.pause = false;
        IsPaused = false;

        SoundManager.Instance?.PlayPopupClose();

        SceneManager.LoadScene(mainMenuScene);
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
        // Safety net if the scene changes while paused
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }







}
