using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHP = 100;
    public int HP = 100;

    [Header("Health UI")]
    public Slider healthSlider;
    public Image healthBarFill;
    public TextMeshProUGUI playerHealthUI;
    public GameObject healthBarRoot;

    public GameObject bloodyScreen;
    public TextMeshProUGUI gameOverUI;
    private bool isDead;

    private void Start()
    {
        if (maxHP <= 0) maxHP = HP > 0 ? HP : 100;
        UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHP;
            healthSlider.value = Mathf.Max(0, HP);
        }

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = Mathf.Clamp01((float)HP / maxHP);
        }

        if (playerHealthUI != null)
        {
            playerHealthUI.text = $"{Mathf.Max(0, HP)}";
        }
    }

    public void TakeDamage(int damageAmount)
    {
        HP -= damageAmount;
        UpdateHealthUI();

        if (HP <= 0)
        {
            print("Player is Dead");
            PlayerDead();
            isDead = true;
        }
        else
        {
            print("Player Hit");
            if (bloodyScreen != null)
            {
                StartCoroutine(BloodyScreenEffect());
            }
            if (SoundManager.Instance != null && SoundManager.Instance.playerChannel != null && SoundManager.Instance.playerHurt != null)
            {
                SoundManager.Instance.playerChannel.PlayOneShot(SoundManager.Instance.playerHurt);
            }
        }
    }

    private void PlayerDead()
    {
        // Save collected crystals permanently to disk
        CrystalSaveSystem.Save();

        if (SoundManager.Instance != null && SoundManager.Instance.playerChannel != null)
        {
            if (SoundManager.Instance.playerDie != null)
            {
                SoundManager.Instance.playerChannel.PlayOneShot(SoundManager.Instance.playerDie);
            }
            if (SoundManager.Instance.gameOverMusic != null)
            {
                SoundManager.Instance.playerChannel.clip = SoundManager.Instance.gameOverMusic;
                SoundManager.Instance.playerChannel.PlayDelayed(2f);
            }
        }

        MouseMovement mm = GetComponentInParent<MouseMovement>();
        if (mm != null) mm.enabled = false;

        PlayerMovement pm = GetComponentInParent<PlayerMovement>();
        if (pm != null) pm.enabled = false;

        // Player Death Animation
        var anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.enabled = true;
        }
        else
        {
            Debug.LogWarning("No Animation component found for death animation.");
        }
        if (playerHealthUI != null) playerHealthUI.gameObject.SetActive(false);
        if (healthBarRoot != null) healthBarRoot.SetActive(false);
        else if (healthSlider != null) healthSlider.gameObject.SetActive(false);
        GetComponentInParent<ScreenFader>().StartFade();
        StartCoroutine(GameOverUI());
    }

    private IEnumerator GameOverUI()
    {
        yield return new WaitForSeconds(1f);
        gameOverUI.gameObject.SetActive(true);

        int waveSurvived = GlobalReferences.Instance.waveNumber;

        if (waveSurvived - 1 > SaveLoadManager.Instance.LoadHighScore())
        {
            SaveLoadManager.Instance.SaveHighScore(waveSurvived - 1);
        }

        StartCoroutine(ReturnToMainMenu());        
    }

    private IEnumerator ReturnToMainMenu()
    {
        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator BloodyScreenEffect()
    {
        if (bloodyScreen.activeInHierarchy == false)
        {
            bloodyScreen.SetActive(true);
        }
        // yield return new WaitForSeconds(4f);
        // yield return new WaitForSeconds(4f);
        var image = bloodyScreen.GetComponentInChildren<Image>();

        // Set the initial alpha value to 1 (fully visible).
        Color startColor = image.color;
        startColor.a = 1f;
        image.color = startColor;

        float duration = 3f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Calculate the new alpha value using Lerp.
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);

            // Update the color with the new alpha value.
            Color newColor = image.color;
            newColor.a = alpha;
            image.color = newColor;

            // Increment the elapsed time.
            elapsedTime += Time.deltaTime;

            yield return null; ; // Wait for the next frame.
        }
        if (bloodyScreen.activeInHierarchy)
        {
            bloodyScreen.SetActive(false);
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Triggered by: " + other.name);

        if (other.CompareTag("ZombieHand"))
        {
            // Debug.Log("Damage: " + other.GetComponent<ZombieHand>().damage);
            if(isDead==false)
            {
                TakeDamage(other.gameObject.GetComponent<ZombieHand>().damage);
            }
            
        }
    }
}
