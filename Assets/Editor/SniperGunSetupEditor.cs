using UnityEditor;
using UnityEngine;

public class SniperGunSetupEditor : EditorWindow
{
    [Header("Target Sniper")]
    [Tooltip("Drag the Sniper GameObject from the Scene or Prefab from the Project window")]
    public GameObject targetSniper;

    [Header("Settings & Functionalities")]
    public int weaponDamage = 100;
    public Weapon.ShootingMode shootingMode = Weapon.ShootingMode.Single;
    public int bulletsPerBurst = 1;
    public float shootingDelay = 1.0f;
    public float bulletVelocity = 900f;
    public int magazineSize = 5;
    public float reloadTime = 2.5f;
    public float reloadSpeedMultiplier = 1.5f;
    public float hipSpread = 0.02f;
    public float adsSpread = 0.0005f;

    [Header("Asset References (Auto-Loaded)")]
    public RuntimeAnimatorController m16AnimatorController;
    public GameObject fpsHandsPrefab;
    public GameObject bulletPrefab;
    public GameObject muzzleFlashPrefab;

    private Vector2 scrollPos;
    private string statusMessage = "";
    private MessageType statusType = MessageType.Info;

    [MenuItem("Tools/Guns/Sniper Gun Setup")]
    [MenuItem("Window/Guns/Sniper Gun Setup")]
    public static void ShowWindow()
    {
        var window = GetWindow<SniperGunSetupEditor>("Sniper Setup");
        window.minSize = new Vector2(400, 520);
        window.AutoLoadDefaultAssets();
        window.Show();
    }

    private void OnEnable()
    {
        AutoLoadDefaultAssets();
    }

    private void AutoLoadDefaultAssets()
    {
        if (m16AnimatorController == null)
        {
            m16AnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/m16.controller");
        }

        if (fpsHandsPrefab == null)
        {
            fpsHandsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Hands/M16_FPS_Hands.prefab");
        }

        if (bulletPrefab == null)
        {
            bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/bullet.prefab");
        }

        if (muzzleFlashPrefab == null)
        {
            muzzleFlashPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EffectExamples/WeaponEffects/Prefabs/MuzzleFlashEffect.prefab");
        }
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Sniper Weapon Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Drag your Sniper 3D model / prefab here. This tool will configure it with all M16 features " +
            "(idle/shoot/reload animations, FPS hands, pickup outline, collider, muzzle flash) " +
            "with single-shot sniper behavior and 100 damage.", MessageType.Info);

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
        targetSniper = (GameObject)EditorGUILayout.ObjectField("Sniper GameObject/Prefab", targetSniper, typeof(GameObject), true);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Weapon Configuration", EditorStyles.boldLabel);
        weaponDamage = EditorGUILayout.IntField("Damage (User: 100)", weaponDamage);
        shootingMode = (Weapon.ShootingMode)EditorGUILayout.EnumPopup("Shooting Mode", shootingMode);
        bulletsPerBurst = EditorGUILayout.IntField("Bullets Per Shot", bulletsPerBurst);
        shootingDelay = EditorGUILayout.FloatField("Shot Delay (Seconds)", shootingDelay);
        bulletVelocity = EditorGUILayout.FloatField("Bullet Velocity", bulletVelocity);
        magazineSize = EditorGUILayout.IntField("Magazine Size", magazineSize);
        reloadTime = EditorGUILayout.FloatField("Reload Time", reloadTime);
        reloadSpeedMultiplier = EditorGUILayout.FloatField("Reload Speed Multiplier", reloadSpeedMultiplier);
        hipSpread = EditorGUILayout.FloatField("Hip Spread Intensity", hipSpread);
        adsSpread = EditorGUILayout.FloatField("ADS Spread Intensity", adsSpread);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Assets & References", EditorStyles.boldLabel);
        m16AnimatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Animator Controller", m16AnimatorController, typeof(RuntimeAnimatorController), false);
        fpsHandsPrefab = (GameObject)EditorGUILayout.ObjectField("FPS Hands Prefab", fpsHandsPrefab, typeof(GameObject), false);
        bulletPrefab = (GameObject)EditorGUILayout.ObjectField("Bullet Prefab", bulletPrefab, typeof(GameObject), false);
        muzzleFlashPrefab = (GameObject)EditorGUILayout.ObjectField("Muzzle Flash Prefab", muzzleFlashPrefab, typeof(GameObject), false);

        EditorGUILayout.Space(15);

        GUI.backgroundColor = new Color(0.25f, 0.75f, 0.4f);
        if (GUILayout.Button("Apply Sniper Functionalities", GUILayout.Height(36)))
        {
            ApplySniperFunctionality();
        }
        GUI.backgroundColor = Color.white;

        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(statusMessage, statusType);
        }

        EditorGUILayout.EndScrollView();
    }

    private void ApplySniperFunctionality()
    {
        if (targetSniper == null)
        {
            statusMessage = "Please assign a Sniper GameObject or Prefab first!";
            statusType = MessageType.Error;
            return;
        }

        bool isAssetPrefab = EditorUtility.IsPersistent(targetSniper);
        GameObject rootToConfigure = targetSniper;
        string prefabPath = "";

        if (isAssetPrefab)
        {
            prefabPath = AssetDatabase.GetAssetPath(targetSniper);
            rootToConfigure = PrefabUtility.LoadPrefabContents(prefabPath);
        }
        else
        {
            Undo.RegisterFullObjectHierarchyUndo(rootToConfigure, "Configure Sniper Weapon");
        }

        try
        {
            // 1. Animator component
            Animator anim = rootToConfigure.GetComponent<Animator>();
            if (anim == null)
            {
                anim = rootToConfigure.AddComponent<Animator>();
            }
            if (m16AnimatorController != null)
            {
                anim.runtimeAnimatorController = m16AnimatorController;
            }

            // 2. Weapon component
            Weapon weapon = rootToConfigure.GetComponent<Weapon>();
            if (weapon == null)
            {
                weapon = rootToConfigure.AddComponent<Weapon>();
            }

            weapon.thisWeaponModel = Weapon.WeaponModel.sniper;
            weapon.weaponDamage = weaponDamage;
            weapon.currentShootMode = shootingMode;
            weapon.bulletsPerBurst = bulletsPerBurst;
            weapon.shootingDelay = shootingDelay;
            weapon.bulletVelocity = bulletVelocity;
            weapon.bulletPrefabLifeTime = 3f;
            weapon.magazineSize = magazineSize;
            weapon.bulletleft = magazineSize;
            weapon.reloadTime = reloadTime;
            weapon.reloadSpeedMultiplier = reloadSpeedMultiplier;
            weapon.hipSpreadIntensity = hipSpread;
            weapon.adsSpreadIntensity = adsSpread;
            weapon.spreadIntensity = hipSpread;

            if (bulletPrefab != null) weapon.bulletPrefab = bulletPrefab;
            if (fpsHandsPrefab != null) weapon.fpsHandsPrefab = fpsHandsPrefab;
            weapon.animator = anim;

            // 3. BulletSpawn & MuzzleEffect
            Transform bulletSpawn = FindChildRecursive(rootToConfigure.transform, "BulletSpawn");
            if (bulletSpawn == null)
            {
                GameObject bsGo = new GameObject("BulletSpawn");
                bsGo.transform.SetParent(rootToConfigure.transform, false);

                // Position BulletSpawn at front edge of gun mesh
                Bounds bounds = CalculateMeshBounds(rootToConfigure);
                bsGo.transform.localPosition = new Vector3(0, bounds.center.y, bounds.max.z);
                bulletSpawn = bsGo.transform;
            }
            weapon.bulletSpawn = bulletSpawn;

            // MuzzleFlash effect
            Transform muzzleT = FindChildRecursive(rootToConfigure.transform, "MuzzleFlashEffect");
            if (muzzleT == null && muzzleFlashPrefab != null)
            {
                GameObject mf = (GameObject)PrefabUtility.InstantiatePrefab(muzzleFlashPrefab, rootToConfigure.transform);
                if (mf == null) mf = Instantiate(muzzleFlashPrefab, rootToConfigure.transform);
                mf.name = "MuzzleFlashEffect";
                mf.transform.position = bulletSpawn.position;
                mf.transform.rotation = bulletSpawn.rotation;
                muzzleT = mf.transform;
            }
            if (muzzleT != null)
            {
                weapon.muzzleEffect = muzzleT.gameObject;
            }

            // 4. Box Collider for ground pickup
            BoxCollider boxCol = rootToConfigure.GetComponent<BoxCollider>();
            if (boxCol == null)
            {
                boxCol = rootToConfigure.AddComponent<BoxCollider>();
            }
            Bounds totalBounds = CalculateMeshBounds(rootToConfigure);
            boxCol.center = totalBounds.center;
            boxCol.size = Vector3.Max(totalBounds.size, new Vector3(0.2f, 0.2f, 0.6f));

            // 5. Outline component (QuickOutline)
            Outline outline = rootToConfigure.GetComponent<Outline>();
            if (outline == null)
            {
                outline = rootToConfigure.AddComponent<Outline>();
            }
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineColor = Color.white;
            outline.OutlineWidth = 2f;
            outline.enabled = false;

            // 6. Layer setup: WeaponRender (layer 7)
            int weaponLayer = LayerMask.NameToLayer("WeaponRender");
            if (weaponLayer != -1)
            {
                SetLayerRecursively(rootToConfigure, weaponLayer);
            }

            // Save changes
            if (isAssetPrefab)
            {
                PrefabUtility.SaveAsPrefabAsset(rootToConfigure, prefabPath);
                PrefabUtility.UnloadPrefabContents(rootToConfigure);
            }
            else
            {
                EditorUtility.SetDirty(rootToConfigure);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(rootToConfigure.scene);
            }

            statusMessage = $"Successfully applied M16 animations, FPS hands, and Sniper functionality (Damage: {weaponDamage}, Single-Shot) to '{targetSniper.name}'!";
            statusType = MessageType.Info;
            Debug.Log($"[SniperGunSetupEditor] {statusMessage}");
        }
        catch (System.Exception ex)
        {
            if (isAssetPrefab && !string.IsNullOrEmpty(prefabPath))
            {
                PrefabUtility.UnloadPrefabContents(rootToConfigure);
            }
            statusMessage = $"Error applying sniper functionality: {ex.Message}";
            statusType = MessageType.Error;
            Debug.LogError($"[SniperGunSetupEditor] {ex}");
        }
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent.name.Equals(childName, System.StringComparison.OrdinalIgnoreCase))
            return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindChildRecursive(parent.GetChild(i), childName);
            if (found != null) return found;
        }
        return null;
    }

    private Bounds CalculateMeshBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(Vector3.zero, Vector3.one * 0.5f);
        }

        Bounds bounds = new Bounds(obj.transform.InverseTransformPoint(renderers[0].bounds.center), Vector3.zero);
        foreach (Renderer r in renderers)
        {
            if (r is ParticleSystemRenderer) continue;
            bounds.Encapsulate(obj.transform.InverseTransformPoint(r.bounds.min));
            bounds.Encapsulate(obj.transform.InverseTransformPoint(r.bounds.max));
        }
        return bounds;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            SetLayerRecursively(obj.transform.GetChild(i).gameObject, newLayer);
        }
    }
}
