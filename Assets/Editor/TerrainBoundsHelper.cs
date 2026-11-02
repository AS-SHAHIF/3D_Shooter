#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Helper to calculate terrain/map bounds.
/// 
/// USAGE:
/// 1. Select your ground/terrain objects in the Hierarchy
/// 2. Go to Tools > Calculate Terrain Bounds
/// 3. Read the Console for exact Width, Height, and Center
/// </summary>
public class TerrainBoundsHelper : EditorWindow
{
    [MenuItem("Tools/Calculate Terrain Bounds")]
    public static void CalculateBounds()
    {
        // Get selected objects
        GameObject[] selected = Selection.gameObjects;

        if (selected == null || selected.Length == 0)
        {
            Debug.LogWarning("[TerrainBoundsHelper] No objects selected. Select your ground/terrain objects first.");
            return;
        }

        // Collect all renderers/colliders from selected objects and their children
        List<Renderer> renderers = new List<Renderer>();
        List<Collider> colliders = new List<Collider>();

        foreach (var go in selected)
        {
            renderers.AddRange(go.GetComponentsInChildren<Renderer>());
            colliders.AddRange(go.GetComponentsInChildren<Collider>());
        }

        if (renderers.Count == 0 && colliders.Count == 0)
        {
            Debug.LogWarning("[TerrainBoundsHelper] No Renderer or Collider found on selected objects.");
            return;
        }

        // Calculate bounds
        Bounds bounds = new Bounds();

        bool initialized = false;

        foreach (var r in renderers)
        {
            if (!initialized)
            {
                bounds = r.bounds;
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(r.bounds);
            }
        }

        foreach (var c in colliders)
        {
            if (!initialized)
            {
                bounds = c.bounds;
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(c.bounds);
            }
        }

        // Print results
        float width = bounds.size.x;
        float height = bounds.size.z; // Z is "depth" on terrain
        Vector3 center = bounds.center;

        Debug.Log("================================================");
        Debug.Log("[TerrainBoundsHelper] TERRAIN BOUNDS CALCULATED");
        Debug.Log("================================================");
        Debug.Log($"Map Width (X):  {width:F2} units");
        Debug.Log($"Map Height (Z): {height:F2} units");
        Debug.Log($"Map Center:     {center:F2}");
        Debug.Log($"Min X/Z:        {bounds.min.x:F2}, {bounds.min.z:F2}");
        Debug.Log($"Max X/Z:        {bounds.max.x:F2}, {bounds.max.z:F2}");
        Debug.Log("================================================");
        Debug.Log($"--> Set MinimapManager: Map Width = {width:F2}, Map Height = {height:F2}, Map Center = ({center.x:F2}, 0, {center.z:F2})");
        Debug.Log("================================================");
    }

    [MenuItem("Tools/Calculate Terrain Bounds", true)]
    public static bool CanCalculate()
    {
        return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
    }
}
#endif