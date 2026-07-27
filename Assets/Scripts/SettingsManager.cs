using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SettingsManager : MonoBehaviour
{

    public static SettingsManager Instance { get; set; }

    [Header("Ui Reference")]
    public TMP_Dropdown resolutionDropDown;
    public TMP_Dropdown fpsDropDown;
    public Toggle sfx;
    public Toggle music;

    private Resolution[] resolutions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        // LoadAndApplySetting();
    }
    private void Start()
    {
        PopulateResolutionsDropDown();
        // PopulateDropDown();
        // ApplySettingsToUI();
    }

    private void PopulateResolutionsDropDown()
    {
        resolutions = Screen.resolutions;
        resolutionDropDown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(new TMP_Dropdown.OptionData(option));
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropDown.AddOptions(options);
        resolutionDropDown.value = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        resolutionDropDown.RefreshShownValue();
    }

    public void OnResolutionChanged(int ResolutionIndex)
    {
        Resolution resolution=resolutions[ResolutionIndex];
        Screen.SetResolution(resolution.width,resolution.height,Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex",ResolutionIndex);
        PlayerPrefs.Save();
    }
}
