using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text highScoreUI;
    [SerializeField] private TMP_Text totalCrystalsUI; // <-- New field for your crystal text
    [SerializeField] private GameObject settingsPanel;
    public string newGameScene = "SampleScene";
    public AudioClip bg_music;
    public AudioSource main_channel;
    public UnityEngine.Audio.AudioMixer audioMixer;

    private void OnEnable()
    {
        // Automatically listens for any crystal balance changes and updates immediately
        CrystalSaveSystem.OnCrystalsChanged += UpdateTotalCrystalsUI;
        UpdateTotalCrystalsUI(CrystalSaveSystem.Total);
    }

    private void OnDisable()
    {
        CrystalSaveSystem.OnCrystalsChanged -= UpdateTotalCrystalsUI;
    }

    private void UpdateTotalCrystalsUI(int total)
    {
        if (totalCrystalsUI != null)
        {
            totalCrystalsUI.text = $"Crystals: {total}";
        }
    }

    private void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (audioMixer == null)
        {
            audioMixer = Resources.Load<UnityEngine.Audio.AudioMixer>("MainAudioMixer");
        }
        if (audioMixer != null && main_channel != null && main_channel.outputAudioMixerGroup == null)
        {
            var musicGroups = audioMixer.FindMatchingGroups("music");
            if (musicGroups != null && musicGroups.Length > 0)
            {
                main_channel.outputAudioMixerGroup = musicGroups[0];
            }
        }

        if (main_channel != null && bg_music != null)
        {
            main_channel.clip = bg_music;
            main_channel.loop = true;
            main_channel.Play();
        }

        // Set the high score
        if (SaveLoadManager.Instance != null)
        {
            int highScore = SaveLoadManager.Instance.LoadHighScore();
            highScoreUI.text = $"Top Wave Survived:{highScore}";
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void StartNewScene()
    {
        main_channel.Stop();

        LoadingManager.nextScene = newGameScene;

        SceneManager.LoadScene("LoadingScene");
    }

    public void ExitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}