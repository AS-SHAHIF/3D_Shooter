using UnityEngine;

/// <summary>
/// Allows the game to auto-rotate between all orientations (Portrait, Portrait Upside Down,
/// Landscape Left, and Landscape Right) so the screen follows how the user holds the device.
/// </summary>
public static class ScreenOrientationController
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnforceAutoRotation()
    {
        // Allow all orientations so the screen follows device rotation
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        // Let Unity auto-rotate based on device orientation
        Screen.orientation = ScreenOrientation.AutoRotation;
    }
}
