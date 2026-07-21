using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TMP_Text highScoreUI;
    string newGameScene = "SampleScene";
    public AudioClip bg_music;
    public AudioSource main_channel;

    private void Start()
    {
        main_channel.PlayOneShot(bg_music);
        // set the high score
        int highScore = SaveLoadManager.Instance.LoadHighScore();
        highScoreUI.text = $"Top Wave Survived:{highScore}";
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
