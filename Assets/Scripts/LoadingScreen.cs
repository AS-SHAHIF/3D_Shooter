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
        StartCoroutine(LoadAsync());
        StartCoroutine(LoadingTextBar());
    }

    IEnumerator LoadingTextBar()
    {
        while (true)
        {
            loadingText.text = "Loading.";
            yield return new WaitForSeconds(0.3f);
            
            loadingText.text = "Loading..";
            yield return new WaitForSeconds(0.3f);

            loadingText.text = "Loading...";
            yield return new WaitForSeconds(0.3f);

        }
    }

    IEnumerator LoadAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(LoadingManager.nextScene);
        operation.allowSceneActivation = false;
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;
            if (operation.progress >= 0.9f)
            {
                progressBar.value = 1f;
                yield return new WaitForSeconds(1f);
                operation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}