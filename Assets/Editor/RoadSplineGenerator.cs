using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Road Spline Generator - Editor Tool
/// Draw roads on your terrain/ground by placing control points in the Scene view.
/// The tool procedurally generates a road mesh along a Catmull-Rom spline.
///
/// USAGE:
///   1. Menu: GameObject > 3D Object > Road Spline   (creates a road object)
///   2. In Scene view: Left-click to add control points
///   3. Drag control points (yellow spheres) to reshape the road
///   4. Hold Shift + Left-click on a point to delete it
///   5. Use the window to adjust width, segments, height, material
///   6. Click "Bake Mesh" to save the generated mesh as an asset
/// </summary>
public class RoadSplineGenerator : EditorWindow
{
    private const float HandleSize = 0.15f;
    private const float PickDistance = 0.25f;

    private GameObject _roadObject;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private RoadSettings _settings;
    private List<Vector3> _points = new List<Vector3>();
    private int _selectedPointIndex = -1;
    private bool _isDragging = false;
    private int _dragStartIndex = -1;
    private Vector3 _dragStartPosition;
    private Mesh _generatedMesh;
    private bool _needsRegeneration = true;

    [MenuItem("GameObject/3D Object/Road Spline", false, 10)]
    private static void CreateRoadSplineMenu()
    {
        var window = GetWindow<RoadSplineGenerator>("Road Spline");
        window.CreateNewRoad();
    }

    [MenuItem("Tools/Road Spline Generator", false, 1)]
    private static void OpenWindow()
    {
        GetWindow<RoadSplineGenerator>("Road Spline");
    }

    private void CreateNewRoad()
    {
        if (_roadObject != null)
        {
            Undo.DestroyObjectImmediate(_roadObject);
        }

        _roadObject = new GameObject("Road Spline");
        Undo.RegisterCreatedObjectUndo(_roadObject, "Create Road Spline");

        _meshFilter = _roadObject.AddComponent<MeshFilter>();
        _meshRenderer = _roadObject.AddComponent<MeshRenderer>();

        _settings = new RoadSettings();
        _points.Clear();
        _points.Add(new Vector3(0, 0, 0));
        _points.Add(new Vector3(0, 0, 10));
        _points.Add(new Vector3(0, 0, 20));

        _selectedPointIndex = -1;
        _generatedMesh = new Mesh();
        _generatedMesh.name = "Road Mesh";
        _meshFilter.sharedMesh = _generatedMesh;
        _needsRegeneration = true;

        Selection.activeObject = _roadObject;
        SceneView.RepaintAll();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.update += OnUpdate;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        EditorApplication.update -= OnUpdate;
    }

    private void OnUpdate()
    {
        if (_needsRegeneration && _roadObject != null && _meshFilter != null)
        {
            RegenerateMesh();
            _needsRegeneration = false;
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (_roadObject == null || _settings == null)
            return;

        Handles.BeginGUI();
        DrawToolbarGUI();
        Handles.EndGUI();

        DrawSplinePreview();
        DrawControlPoints();
        HandleSceneInput();

        if (_needsRegeneration)
        {
            RegenerateMesh();
            _needsRegeneration = false;
        }

        sceneView.Repaint();
    }

    private void DrawToolbarGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 280, 200));
        GUILayout.BeginVertical("box");
        GUILayout.Label("Road Spline Tool", EditorStyles.boldLabel);
        GUILayout.Space(5);

        if (GUILayout.Button("New Road"))
        {
            CreateNewRoad();
        }

        if (GUILayout.Button("Select Road Object"))
        {
            Selection.activeObject = _roadObject;
        }

        GUILayout.Space(5);
        GUILayout.Label("Controls:", EditorStyles.miniLabel);
        GUILayout.Label("LMB: Add / Move point", EditorStyles.miniLabel);
        GUILayout.Label("Shift+LMB: Delete point", EditorStyles.miniLabel);
        GUILayout.Label("Drag: Move selected point", EditorStyles.miniLabel);

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawSplinePreview()
    {
        if (_points.Count < 2)
            return;

        Handles.color = Color.cyan;
        var splinePoints = GetSplinePoints();
        for (int i = 0; i < splinePoints.Count - 1; i++)
        {
            Handles.DrawLine(splinePoints[i], splinePoints[i + 1]);
        }

        Handles.color = new Color(1f, 1f, 1f, 0.3f);
        for (int i = 0; i < _points.Count - 1; i++)
        {
            Handles.DrawLine(_points[i], _points[i + 1]);
        }

        if (_settings.showRoadWidth)
        {
            Handles.color = Color.green;
            var normals = GetSplineNormals();
            for (int i = 0; i < splinePoints.Count; i++)
            {
                if (i < normals.Count)
                {
                    var left = splinePoints[i] - normals[i] * (_settings.roadWidth * 0.5f);
                    var right = splinePoints[i] + normals[i] * (_settings.roadWidth * 0.5f);
                    Handles.DrawLine(left, right);
                    Handles.DrawLine(splinePoints[i], left);
                    Handles.DrawLine(splinePoints[i], right);
                }
            }
        }
    }

    private void DrawControlPoints()
    {
        for (int i = 0; i < _points.Count; i++)
        {
            bool isSelected = (i == _selectedPointIndex);
            Handles.color = isSelected ? Color.green : Color.yellow;

            var fmh_192_17_639289160808094760 = Quaternion.identity; var newPos = Handles.FreeMoveHandle(
                _points[i],
                HandleSize,
                Vector3.zero,
                Handles.SphereHandleCap
            );

            if (newPos != _points[i])
            {
                Undo.RecordObject(_roadObject, "Move Road Point");
                _points[i] = newPos;
                _selectedPointIndex = i;
                _needsRegeneration = true;
            }

            Handles.Label(_points[i] + Vector3.up * 0.5f, $"P{i}", isSelected ? EditorStyles.whiteLabel : EditorStyles.miniLabel);
        }
    }

    private void HandleSceneInput()
    {
        var e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            int clickedPoint = GetPointUnderMouse(ray);

            if (e.shift && clickedPoint != -1)
            {
                Undo.RecordObject(_roadObject, "Delete Road Point");
                _points.RemoveAt(clickedPoint);
                if (_selectedPointIndex >= clickedPoint)
                    _selectedPointIndex = Mathf.Max(-1, _selectedPointIndex - 1);
                _needsRegeneration = true;
                e.Use();
                return;
            }

            if (clickedPoint != -1)
            {
                _selectedPointIndex = clickedPoint;
                _isDragging = true;
                _dragStartIndex = clickedPoint;
                _dragStartPosition = _points[clickedPoint];
                e.Use();
                return;
            }

            var mousePos = GetMouseWorldPosition(e.mousePosition);
            if (mousePos.HasValue)
            {
                Undo.RecordObject(_roadObject, "Add Road Point");
                var insertIndex = _selectedPointIndex != -1 ? _selectedPointIndex + 1 : _points.Count;
                _points.Insert(insertIndex, mousePos.Value);
                _selectedPointIndex = insertIndex;
                _needsRegeneration = true;
                e.Use();
            }
        }

        if (e.type == EventType.MouseDrag && e.button == 0 && _isDragging && _dragStartIndex != -1)
        {
            var mousePos = GetMouseWorldPosition(e.mousePosition);
            if (mousePos.HasValue)
            {
                Undo.RecordObject(_roadObject, "Drag Road Point");
                _points[_dragStartIndex] = mousePos.Value;
                _selectedPointIndex = _dragStartIndex;
                _needsRegeneration = true;
                e.Use();
            }
        }

        if (e.type == EventType.MouseUp && e.button == 0)
        {
            _isDragging = false;
            _dragStartIndex = -1;
        }
    }

    private int GetPointUnderMouse(Ray ray)
    {
        for (int i = 0; i < _points.Count; i++)
        {
            var screenPoint = HandleUtility.WorldToGUIPoint(_points[i]);
            var mousePoint = Event.current.mousePosition;
            var dist = Vector2.Distance(screenPoint, mousePoint);
            if (dist < PickDistance * 50f)
                return i;
        }
        return -1;
    }

    private Vector3? GetMouseWorldPosition(Vector2 mousePosition)
    {
        var ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        var plane = new Plane(Vector3.up, 0f);
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        return null;
    }

    private void RegenerateMesh()
    {
        if (_points.Count < 2 || _meshFilter == null)
            return;

        if (_generatedMesh == null)
            _generatedMesh = new Mesh();

        _generatedMesh.Clear();

        var splinePoints = GetSplinePoints();
        var normals = GetSplineNormals();

        if (splinePoints.Count < 2 || normals.Count < 2)
            return;

        var vertices = new List<Vector3>();
        var normalsList = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        float halfWidth = _settings.roadWidth * 0.5f;
        float totalLength = 0f;

        float[] segmentLengths = new float[splinePoints.Count - 1];
        for (int i = 0; i < segmentLengths.Length; i++)
        {
            segmentLengths[i] = Vector3.Distance(splinePoints[i], splinePoints[i + 1]);
            totalLength += segmentLengths[i];
        }

        float accumulatedLength = 0f;

        for (int i = 0; i < splinePoints.Count; i++)
        {
            if (i > 0)
                accumulatedLength += segmentLengths[i - 1];

            var center = splinePoints[i] + Vector3.up * _settings.roadHeight;
            var normal = normals[i];

            var left = center - normal * halfWidth;
            var right = center + normal * halfWidth;

            float v = totalLength > 0 ? accumulatedLength / totalLength : 0f;

            vertices.Add(left);
            vertices.Add(right);
            normalsList.Add(Vector3.up);
            normalsList.Add(Vector3.up);
            uvs.Add(new Vector2(0f, v));
            uvs.Add(new Vector2(1f, v));
        }

        for (int i = 0; i < splinePoints.Count - 1; i++)
        {
            int v0 = i * 2;
            int v1 = i * 2 + 1;
            int v2 = (i + 1) * 2;
            int v3 = (i + 1) * 2 + 1;

            triangles.Add(v0);
            triangles.Add(v2);
            triangles.Add(v1);

            triangles.Add(v1);
            triangles.Add(v2);
            triangles.Add(v3);
        }

        _generatedMesh.SetVertices(vertices);
        _generatedMesh.SetNormals(normalsList);
        _generatedMesh.SetUVs(0, uvs);
        _generatedMesh.SetTriangles(triangles, 0);
        _generatedMesh.RecalculateBounds();

        _meshFilter.sharedMesh = _generatedMesh;

        EditorUtility.SetDirty(_meshFilter);
        EditorUtility.SetDirty(_meshRenderer);
    }

    private List<Vector3> GetSplinePoints()
    {
        var result = new List<Vector3>();

        if (_points.Count < 2)
            return result;

        for (int i = 0; i < _points.Count - 1; i++)
        {
            Vector3 p0 = i == 0 ? _points[0] : _points[i - 1];
            Vector3 p1 = _points[i];
            Vector3 p2 = _points[i + 1];
            Vector3 p3 = i + 2 >= _points.Count ? _points[_points.Count - 1] : _points[i + 2];

            for (int j = 0; j <= _settings.segmentsPerCurve; j++)
            {
                float t = j / (float)_settings.segmentsPerCurve;
                result.Add(CatmullRom(p0, p1, p2, p3, t));
            }
        }

        result.Add(_points[_points.Count - 1]);
        return result;
    }

    private List<Vector3> GetSplineNormals()
    {
        var result = new List<Vector3>();
        var splinePoints = GetSplinePoints();

        for (int i = 0; i < splinePoints.Count; i++)
        {
            Vector3 tangent;
            if (i == 0)
            {
                tangent = splinePoints[1] - splinePoints[0];
            }
            else if (i == splinePoints.Count - 1)
            {
                tangent = splinePoints[i] - splinePoints[i - 1];
            }
            else
            {
                tangent = splinePoints[i + 1] - splinePoints[i - 1];
            }

            tangent.Normalize();

            Vector3 normal = Vector3.Cross(tangent, Vector3.up).normalized;
            result.Add(normal);
        }

        return result;
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    private void OnGUI()
    {
        if (_settings == null)
        {
            _settings = new RoadSettings();
        }

        GUILayout.Label("Road Spline Generator", EditorStyles.boldLabel);
        GUILayout.Space(5);

        if (_roadObject == null)
        {
            if (GUILayout.Button("Create New Road"))
                CreateNewRoad();
            return;
        }

        EditorGUI.BeginChangeCheck();

        _settings.roadWidth = EditorGUILayout.FloatField("Road Width", _settings.roadWidth);
        _settings.roadHeight = EditorGUILayout.FloatField("Road Height", _settings.roadHeight);
        _settings.segmentsPerCurve = EditorGUILayout.IntField("Segments Per Curve", _settings.segmentsPerCurve);
        _settings.showRoadWidth = EditorGUILayout.Toggle("Show Width Gizmos", _settings.showRoadWidth);
        _settings.roadMaterial = (Material)EditorGUILayout.ObjectField("Material", _settings.roadMaterial, typeof(Material), false);

        GUILayout.Space(10);

        // Prefab Mode
        GUILayout.Label("Prefab Mode", EditorStyles.boldLabel);
        _settings.usePrefabMode = EditorGUILayout.Toggle("Use Road Prefab", _settings.usePrefabMode);

        if (_settings.usePrefabMode)
        {
            _settings.roadPrefab = (GameObject)EditorGUILayout.ObjectField("Road Prefab", _settings.roadPrefab, typeof(GameObject), false);
            _settings.prefabSpacing = EditorGUILayout.FloatField("Prefab Spacing", _settings.prefabSpacing);
            if (GUILayout.Button("Place Prefabs Along Spline"))
            {
                PlacePrefabsAlongSpline();
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            _needsRegeneration = true;
            if (_meshRenderer != null && _settings.roadMaterial != null)
                _meshRenderer.sharedMaterial = _settings.roadMaterial;
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Bake Mesh to Asset"))
        {
            BakeMeshToAsset();
        }

        if (GUILayout.Button("Delete Road"))
        {
            DeleteRoad();
        }

        GUILayout.Space(10);
        GUILayout.Label($"Points: {_points.Count}", EditorStyles.miniLabel);
        GUILayout.Label($"Selected: {_selectedPointIndex}", EditorStyles.miniLabel);
    }

    private void BakeMeshToAsset()
    {
        if (_generatedMesh == null)
            return;

        string path = EditorUtility.SaveFilePanel("Save Road Mesh", "Assets", "RoadMesh", "asset");
        if (!string.IsNullOrEmpty(path))
        {
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            var meshToSave = Instantiate(_generatedMesh);
            AssetDatabase.CreateAsset(meshToSave, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"Road mesh saved to: {path}");
        }
    }

    private void DeleteRoad()
    {
        if (_roadObject != null)
        {
            Undo.DestroyObjectImmediate(_roadObject);
            _roadObject = null;
            _meshFilter = null;
            _meshRenderer = null;
            _points.Clear();
            _selectedPointIndex = -1;
            _generatedMesh = null;
        }
    }

    private void PlacePrefabsAlongSpline()
    {
        if (_settings.roadPrefab == null || _points.Count < 2)
        {
            Debug.LogWarning("Assign a road prefab and add at least 2 points.");
            return;
        }

        // Clean up existing prefab instances
        var existingPrefabs = _roadObject.GetComponentsInChildren<Transform>();
        foreach (var child in existingPrefabs)
        {
            if (child != _roadObject.transform)
                Undo.DestroyObjectImmediate(child.gameObject);
        }

        var splinePoints = GetSplinePoints();
        float totalLength = 0f;
        float[] segmentLengths = new float[splinePoints.Count - 1];
        for (int i = 0; i < segmentLengths.Length; i++)
        {
            segmentLengths[i] = Vector3.Distance(splinePoints[i], splinePoints[i + 1]);
            totalLength += segmentLengths[i];
        }

        float distanceAlong = 0f;
        int segmentIndex = 0;

        while (distanceAlong <= totalLength)
        {
            // Find which segment we're on
            float segmentStart = 0f;
            for (int i = 0; i < segmentLengths.Length; i++)
            {
                if (distanceAlong >= segmentStart && distanceAlong < segmentStart + segmentLengths[i])
                {
                    segmentIndex = i;
                    break;
                }
                segmentStart += segmentLengths[i];
            }

            if (segmentIndex >= segmentLengths.Length)
                break;

            float t = (distanceAlong - segmentStart) / segmentLengths[segmentIndex];
            Vector3 p0 = segmentIndex == 0 ? splinePoints[0] : splinePoints[segmentIndex - 1];
            Vector3 p1 = splinePoints[segmentIndex];
            Vector3 p2 = splinePoints[segmentIndex + 1];
            Vector3 p3 = segmentIndex + 2 >= splinePoints.Count ? splinePoints[splinePoints.Count - 1] : splinePoints[segmentIndex + 2];

            Vector3 pos = CatmullRom(p0, p1, p2, p3, t);
            pos.y += _settings.roadHeight;

            // Calculate rotation to follow spline
            Vector3 tangent;
            if (segmentIndex == 0)
                tangent = splinePoints[1] - splinePoints[0];
            else if (segmentIndex >= segmentLengths.Length - 1)
                tangent = splinePoints[splinePoints.Count - 1] - splinePoints[splinePoints.Count - 2];
            else
                tangent = splinePoints[segmentIndex + 1] - splinePoints[segmentIndex - 1];

            tangent.Normalize();
            Quaternion rot = Quaternion.LookRotation(tangent, Vector3.up);

            var instance = PrefabUtility.InstantiatePrefab(_settings.roadPrefab) as GameObject;
            Undo.RegisterCreatedObjectUndo(instance, "Place Road Prefab");
            instance.transform.SetParent(_roadObject.transform);
            instance.transform.position = pos;
            instance.transform.rotation = rot;

            distanceAlong += _settings.prefabSpacing;
        }

        Debug.Log($"Placed prefabs along spline. Total length: {totalLength:F2}m");
    }
}

[System.Serializable]
public class RoadSettings
{
    public float roadWidth = 4f;
    public float roadHeight = 0f;
    public int segmentsPerCurve = 12;
    public bool showRoadWidth = true;
    public Material roadMaterial;

    // Prefab Mode
    public bool usePrefabMode = false;
    public GameObject roadPrefab;
    public float prefabSpacing = 5f;
}
