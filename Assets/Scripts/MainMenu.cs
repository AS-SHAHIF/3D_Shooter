using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text highScoreUI;
    [SerializeField] private GameObject settingsPanel;
    string newGameScene = "SampleScene";
    public AudioClip bg_music;
    public AudioSource main_channel;

    private void Start()
    {
        settingsPanel.SetActive(false);
        main_channel.PlayOneShot(bg_music);
        // set the high score
        int highScore = SaveLoadManager.Instance.LoadHighScore();
        highScoreUI.text = $"Top Wave Survived:{highScore}";
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
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
