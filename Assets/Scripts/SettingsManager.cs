using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("UI Reference")] [SerializeField]
    private TMP_Dropdown resolutionDropDown;

    [SerializeField] private TMP_Dropdown fpsDropDown;

    [Header("Audio")] [SerializeField] private AudioMixer audioMixer;


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

        HashSet<string> added = new HashSet<string>();

        int currentIndex = 0;


        for (int i = 0; i < resolutions.Length; i++)
        {
            string resolutionText =
                resolutions[i].width + " x " + resolutions[i].height;


            if (added.Contains(resolutionText))
                continue;


            added.Add(resolutionText);

            uniqueResolutions.Add(resolutions[i]);

            options.Add(
                new TMP_Dropdown.OptionData(resolutionText)
            );


            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentIndex = uniqueResolutions.Count - 1;
            }
        }


        resolutionDropDown.AddOptions(options);


        int savedIndex =
            PlayerPrefs.GetInt(ResolutionKey, currentIndex);


        savedIndex =
            Mathf.Clamp(savedIndex, 0, uniqueResolutions.Count - 1);


        resolutionDropDown.value = savedIndex;
        resolutionDropDown.RefreshShownValue();
    }


    public void OnResolutionChanged(int index)
    {
        index =
            Mathf.Clamp(index, 0, uniqueResolutions.Count - 1);


        Resolution resolution = uniqueResolutions[index];


        Screen.SetResolution(
            resolution.width,
            resolution.height,
            Screen.fullScreen
        );


        PlayerPrefs.SetInt(ResolutionKey, index);
        PlayerPrefs.Save();
    }

    #endregion


    #region FPS

    private void PopulateFPSDropDown()
    {
        fpsDropDown.ClearOptions();


        List<TMP_Dropdown.OptionData> options =
            new List<TMP_Dropdown.OptionData>()
            {
                new TMP_Dropdown.OptionData("30 FPS"),
                new TMP_Dropdown.OptionData("60 FPS"),
                new TMP_Dropdown.OptionData("120 FPS")
            };


        fpsDropDown.AddOptions(options);


        int savedFPS =
            PlayerPrefs.GetInt(FpsKey, 1);


        fpsDropDown.SetValueWithoutNotify(savedFPS);
        fpsDropDown.RefreshShownValue();
    }


    public void OnFPSChanged(int index)
    {
        PlayerPrefs.SetInt(FpsKey, index);


        switch (index)
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


    #region Music Buttons

    public void MusicOn()
    {
        audioMixer.SetFloat(
            "MusicVolume",
            0f
        );


        PlayerPrefs.SetInt(
            MusicKey,
            1
        );


        PlayerPrefs.Save();
    }


    public void MusicOff()
    {
        audioMixer.SetFloat(
            "MusicVolume",
            -80f
        );


        PlayerPrefs.SetInt(
            MusicKey,
            0
        );


        PlayerPrefs.Save();
    }

    #endregion


    #region SFX Buttons

    public void SfxOn()
    {
        audioMixer.SetFloat(
            "SFXVolume",
            0f
        );


        PlayerPrefs.SetInt(
            SfxKey,
            1
        );


        PlayerPrefs.Save();
    }


    public void SfxOff()
    {
        audioMixer.SetFloat(
            "SFXVolume",
            -80f
        );


        PlayerPrefs.SetInt(
            SfxKey,
            0
        );


        PlayerPrefs.Save();
    }


    public static bool IsSfxEnabled()
    {
        return PlayerPrefs.GetInt(SfxKey, 1) == 1;
    }

    #endregion


    #region Load Settings

    private void LoadAndApplySettings()
    {
        // FPS
        int fps =
            PlayerPrefs.GetInt(FpsKey, 1);


        OnFPSChanged(fps);


        // Music
        bool music =
            PlayerPrefs.GetInt(MusicKey, 1) == 1;


        if (music)
            MusicOn();
        else
            MusicOff();


        // SFX
        bool sfx =
            PlayerPrefs.GetInt(SfxKey, 1) == 1;


        if (sfx)
            SfxOn();
        else
            SfxOff();


        // Resolution
        int resolutionIndex =
            PlayerPrefs.GetInt(
                ResolutionKey,
                resolutionDropDown.value
            );


        resolutionIndex =
            Mathf.Clamp(
                resolutionIndex,
                0,
                uniqueResolutions.Count - 1
            );


        resolutionDropDown.SetValueWithoutNotify(
            resolutionIndex
        );


        Resolution resolution =
            uniqueResolutions[resolutionIndex];


        Screen.SetResolution(
            resolution.width,
            resolution.height,
            Screen.fullScreen
        );
    }

    #endregion
}