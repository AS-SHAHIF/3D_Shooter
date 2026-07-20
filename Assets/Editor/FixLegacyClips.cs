using UnityEngine;
using UnityEditor;

public class FixLegacyClips : EditorWindow
{
    [MenuItem("Tools/Fix Legacy Clips")]
    static void Fix()
    {
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip");
        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null && clip.legacy)
            {
                clip.legacy = false;
                EditorUtility.SetDirty(clip);
                Debug.Log("Fixed: " + clip.name);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Total fixed: " + count + " clips");
    }
}