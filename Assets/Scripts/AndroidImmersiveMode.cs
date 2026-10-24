using UnityEngine;

public class AndroidImmersiveMode : MonoBehaviour
{
    private static AndroidImmersiveMode _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (_instance == null)
        {
            GameObject go = new GameObject("AndroidImmersiveMode");
            _instance = go.AddComponent<AndroidImmersiveMode>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        ApplyImmersiveMode();
    }

    private void Start()
    {
        ApplyImmersiveMode();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            ApplyImmersiveMode();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            ApplyImmersiveMode();
        }
    }

    public static void ApplyImmersiveMode()
    {
        Screen.fullScreen = true;
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                if (activity == null) return;

                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    try
                    {
                        using (AndroidJavaObject window = activity.Call<AndroidJavaObject>("getWindow"))
                        {
                            if (window == null) return;

                            // 1. Android Window Flags: FLAG_FULLSCREEN (1024)
                            int FLAG_FULLSCREEN = 0x00000400;
                            window.Call("addFlags", FLAG_FULLSCREEN);
                            window.Call("clearFlags", 0x00000800); // Clear FLAG_FORCE_NOT_FULLSCREEN

                            // 2. Allow extending into display cutout / notch (Android P+, API 28+)
                            using (AndroidJavaClass versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
                            {
                                int sdkInt = versionClass.GetStatic<int>("SDK_INT");
                                if (sdkInt >= 28)
                                {
                                    using (AndroidJavaObject layoutParams = window.Call<AndroidJavaObject>("getAttributes"))
                                    {
                                        if (layoutParams != null)
                                        {
                                            // LAYOUT_IN_DISPLAY_CUTOUT_MODE_SHORT_EDGES = 1
                                            layoutParams.Set("layoutInDisplayCutoutMode", 1);
                                            window.Call("setAttributes", layoutParams);
                                        }
                                    }
                                }

                                // 3. For Android 11+ (API 30+), use WindowInsetsController
                                if (sdkInt >= 30)
                                {
                                    using (AndroidJavaObject insetsController = window.Call<AndroidJavaObject>("getInsetsController"))
                                    {
                                        if (insetsController != null)
                                        {
                                            // WindowInsets.Type.statusBars() = 1, navigationBars() = 2 -> 3
                                            insetsController.Call("hide", 1 | 2);
                                            // BEHAVIOR_SHOW_TRANSIENT_BARS_BY_SWIPE = 2
                                            insetsController.Call("setSystemBarsBehavior", 2);
                                        }
                                    }
                                }
                            }

                            // 4. Immersive Sticky System UI Visibility Flags (All Android versions)
                            using (AndroidJavaObject decorView = window.Call<AndroidJavaObject>("getDecorView"))
                            {
                                if (decorView != null)
                                {
                                    int SYSTEM_UI_FLAG_LOW_PROFILE = 1;
                                    int SYSTEM_UI_FLAG_HIDE_NAVIGATION = 2;
                                    int SYSTEM_UI_FLAG_FULLSCREEN = 4;
                                    int SYSTEM_UI_FLAG_LAYOUT_STABLE = 256;
                                    int SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION = 512;
                                    int SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN = 1024;
                                    int SYSTEM_UI_FLAG_IMMERSIVE_STICKY = 4096;

                                    int uiOptions = SYSTEM_UI_FLAG_LOW_PROFILE
                                                  | SYSTEM_UI_FLAG_HIDE_NAVIGATION
                                                  | SYSTEM_UI_FLAG_FULLSCREEN
                                                  | SYSTEM_UI_FLAG_LAYOUT_STABLE
                                                  | SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
                                                  | SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN
                                                  | SYSTEM_UI_FLAG_IMMERSIVE_STICKY;

                                    decorView.Call("setSystemUiVisibility", uiOptions);
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning("AndroidImmersiveMode UI Thread Exception: " + ex.Message);
                    }
                }));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("AndroidImmersiveMode Exception: " + e.Message);
        }
#endif
    }
}
