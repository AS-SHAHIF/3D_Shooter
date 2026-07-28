using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("UI Reference")] [SerializeField]
    private TMP_Dropdown resolutionDropDown;

    [SerializeField] private TMP_Dropdown fpsDropDown;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private AudioMixer audioMixer;

    private Resolution[] resolutions;
    private readonly List<Resolution> uniqueResolutions = new List<Resolution>();

    // PlayerPrefs Keys
    private const string ResolutionKey = "ResolutionIndex";
    private const string FpsKey = "FpsIndex";
    private const string MusicKey = "MusicOn";
    private const string SfxKey = "SfxOn";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PopulateResolutionsDropDown();
        PopulateFPSDropDown();
        LoadAndApplySettings();
    }

    #region Resolution

    private void PopulateResolutionsDropDown()
    {
        resolutions = Screen.resolutions;
        resolutionDropDown.ClearOptions();
        uniqueResolutions.Clear();

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        HashSet<string> addedResolutions = new HashSet<string>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string resolutionString = resolutions[i].width + " x " + resolutions[i].height;

            // Avoid duplicates
            if (addedResolutions.Contains(resolutionString))
                continue;

            addedResolutions.Add(resolutionString);
            uniqueResolutions.Add(resolutions[i]);
            options.Add(new TMP_Dropdown.OptionData(resolutionString));

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = uniqueResolutions.Count - 1;
            }
        }

        resolutionDropDown.AddOptions(options);

        int savedIndex = PlayerPrefs.GetInt(ResolutionKey, currentResolutionIndex);
        savedIndex = Mathf.Clamp(savedIndex, 0, uniqueResolutions.Count - 1);

        resolutionDropDown.value = savedIndex;
        resolutionDropDown.RefreshShownValue();
    }

    public void OnResolutionChanged(int resolutionIndex)
    {
        resolutionIndex = Mathf.Clamp(resolutionIndex, 0, uniqueResolutions.Count - 1);

        Resolution resolution = uniqueResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        PlayerPrefs.SetInt(ResolutionKey, resolutionIndex);
        PlayerPrefs.Save();
    }

    #endregion

    #region FPS

    private void PopulateFPSDropDown()
    {
        fpsDropDown.ClearOptions();

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData("30 FPS"),
            new TMP_Dropdown.OptionData("60 FPS"),
            new TMP_Dropdown.OptionData("120 FPS")
        };

        fpsDropDown.AddOptions(options);

        int savedIndex = PlayerPrefs.GetInt(FpsKey, 1);
        fpsDropDown.value = savedIndex;
        fpsDropDown.RefreshShownValue();
    }

    public void OnFpsChanged(int fpsIndex)
    {
        PlayerPrefs.SetInt(FpsKey, fpsIndex);

        switch (fpsIndex)
        {
            case 0:
                Application.targetFrameRate = 30;
                break;
            case 1:
                Application.targetFrameRate = 60;
                break;
            case 2:
                Application.targetFrameRate = 120;
                break;
        }

        PlayerPrefs.Save();
    }

    #endregion

    #region Audio

    public void OnMusicToggled(bool isOn)
    {
        PlayerPrefs.SetInt(MusicKey, isOn ? 1 : 0);

        audioMixer.SetFloat(
            "MusicVolume",
            isOn ? 0f : -80f
        );

        PlayerPrefs.Save();
    }

    public void OnSfxToggled(bool isOn)
    {
        PlayerPrefs.SetInt(SfxKey, isOn ? 1 : 0);

        audioMixer.SetFloat(
            "SFXVolume",
            isOn ? 0f : -80f
        );

        PlayerPrefs.Save();
    } 

    // Easy access from other scripts
    public static bool IsSfxEnabled()
    {
        return PlayerPrefs.GetInt(SfxKey, 1) == 1;
    }

    #endregion

    #region Load

    private void LoadAndApplySettings()
    {
        // FPS
        int fpsIndex = PlayerPrefs.GetInt(FpsKey, 1);
        fpsDropDown.value = fpsIndex;
        OnFpsChanged(fpsIndex);

        // Music
        bool musicOn = PlayerPrefs.GetInt(MusicKey, 1) == 1;
        musicToggle.isOn = musicOn;
        audioMixer.SetFloat(
            "MusicVolume",
            musicOn ? 0f : -80f
        );

        // SFX
        bool sfxOn = PlayerPrefs.GetInt(SfxKey, 1) == 1;
        sfxToggle.isOn = sfxOn;
        audioMixer.SetFloat(
            "SFXVolume",
            sfxOn ? 0f : -80f
        );

        // Resolution
        int resolutionIndex = PlayerPrefs.GetInt(ResolutionKey, resolutionDropDown.value);
        resolutionIndex = Mathf.Clamp(resolutionIndex, 0, uniqueResolutions.Count - 1);

        resolutionDropDown.value = resolutionIndex;

        Resolution resolution = uniqueResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    #endregion
}