using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] public TextMeshProUGUI loadingText;

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        StartCoroutine(LoadAsync());
        if (loadingText != null)
        {
            StartCoroutine(LoadingTextBar());
        }
    }

    IEnumerator LoadingTextBar()
    {
        while (true)
        {
            if (loadingText != null) loadingText.text = "Loading.";
            yield return new WaitForSecondsRealtime(0.3f);
            
            if (loadingText != null) loadingText.text = "Loading..";
            yield return new WaitForSecondsRealtime(0.3f);

            if (loadingText != null) loadingText.text = "Loading...";
            yield return new WaitForSecondsRealtime(0.3f);
        }
    }

    IEnumerator LoadAsync()
    {
        string targetScene = string.IsNullOrEmpty(LoadingManager.nextScene) ? "SampleScene" : LoadingManager.nextScene;
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        if (operation == null)
        {
            yield break;
        }

        operation.allowSceneActivation = false;
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            if (progressBar != null)
            {
                progressBar.value = progress;
            }

            if (operation.progress >= 0.9f)
            {
                if (progressBar != null)
                {
                    progressBar.value = 1f;
                }
                yield return new WaitForSecondsRealtime(0.5f);
                operation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}