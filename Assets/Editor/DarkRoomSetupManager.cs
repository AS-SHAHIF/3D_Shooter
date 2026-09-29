using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class DarkRoomSetupManager : EditorWindow
{
    private const string SourceScenePath = "Assets/Asset/Dark Room/TestDemoScene.unity";
    private const string DemoTestScenePath = "Assets/Scenes/DemoTest.unity";

    [MenuItem("Tools/Dark Room/1. Reset DemoTest (Undo All Gameplay Changes)")]
    public static void ResetDemoTestScene()
    {
        if (!File.Exists(SourceScenePath))
        {
            Debug.LogError($"Source scene not found at {SourceScenePath}");
            return;
        }

        if (!Directory.Exists("Assets/Scenes"))
        {
            Directory.CreateDirectory("Assets/Scenes");
        }

        // Clean copy of original Dark Room scene to DemoTest
        File.Copy(SourceScenePath, DemoTestScenePath, true);
        AssetDatabase.Refresh();

        var scene = EditorSceneManager.OpenScene(DemoTestScenePath, OpenSceneMode.Single);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Successfully RESET Assets/Scenes/DemoTest.unity to the clean original Dark Room scene. Original scenes (MainMenu, LoadingScene, SampleScene) remain untouched!");
    }

    [MenuItem("Tools/Dark Room/2. Setup Gameplay in DemoTest (Exact Table & Floor Alignment)")]
    public static void SetupDemoTestGameplay()
    {
        // 1. Ensure clean scene copy first
        ResetDemoTestScene();

        var scene = EditorSceneManager.OpenScene(DemoTestScenePath, OpenSceneMode.Single);

        // Ensure required tags exist
        EnsureTagsExist("Player", "Waypoints", "ZombieHand");

        // 2. Load Prefabs & Assets
        GameObject crystalPrefab = SetupCrystalPrefab();
        GameObject ammoBoxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/AmmoBox.prefab");
        GameObject zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Zombie.prefab");
        GameObject grenadePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Grenade.prefab");
        GameObject smokeGrenadePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Smoke_Grenade.prefab");
        GameObject bulletImpactPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Effects/BulletImpactStoneEffect.prefab");
        GameObject bloodSprayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/BloodSprayFX.prefab");
        GameObject pistolPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/M1911.prefab");
        GameObject m16Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Machine_Gun_01.prefab");
        if (m16Prefab == null)
        {
            m16Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/M16A4/M16a4_prefab.prefab");
        }

        AudioMixer mainMixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/AudioMixer/MainAudioMixer.mixer");
        AudioClip m16Shot = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/M16.mp3");
        AudioClip pistolShot = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Pistol.mp3");
        AudioClip zombieWalk = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Zombie-Walking.mp3");
        AudioClip zombieChase = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Zombie-Chase.mp3");
        AudioClip zombieAttack = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Zombie-Attack.mp3");
        AudioClip zombieHurt = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Zombie-Hurt.mp3");
        AudioClip zombieDeath = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Zombie-Death.mp3");
        AudioClip playerHurt = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Player-Hurt.mp3");
        AudioClip playerDie = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Player-Die.mp3");
        AudioClip gameOverMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/GameOver.mp3");
        AudioClip bgMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Background-Music.mp3");

        // 3. Find and disable/remove any default scene camera to use our Player FPS camera
        foreach (Camera cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            if (cam.gameObject.name.ToLower().Contains("main") || cam.gameObject.name.ToLower().Contains("camera"))
            {
                DestroyImmediate(cam.gameObject);
            }
        }

        // 4. Scan Environment for Actual Tables, Display Cases & Rooms
        List<GameObject> tableObjects = new List<GameObject>();
        List<GameObject> allRenderers = new List<GameObject>();

        foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            string name = r.gameObject.name.ToLower();
            if (name.Contains("table") || name.Contains("desk") || name.Contains("displaycase") || name.Contains("chest"))
            {
                if (!tableObjects.Contains(r.gameObject))
                {
                    tableObjects.Add(r.gameObject);
                }
            }
        }

        Debug.Log($"Found {tableObjects.Count} table/furniture surfaces in Dark Room scene.");

        // 5. Build HUD Canvas
        GameObject canvasObj = CreateHUDCanvas();
        HUDManager hudMgr = canvasObj.GetComponent<HUDManager>();

        // 6. Setup Player Spawn Position (Center open area on floor)
        Vector3 playerSpawnPos = new Vector3(0f, 0.5f, 0f);
        if (tableObjects.Count > 0)
        {
            // Position player slightly offset from the first table
            Vector3 tPos = tableObjects[0].transform.position;
            playerSpawnPos = new Vector3(tPos.x - 2.5f, 0.5f, tPos.z - 2.5f);
        }

        GameObject playerObj = CreatePlayer(hudMgr, grenadePrefab, smokeGrenadePrefab, playerSpawnPos);

        // 7. Setup Game Managers (SoundManager, GlobalReferences, etc.)
        GameObject managersRoot = new GameObject("Gameplay_Managers");

        SoundManager soundMgr = managersRoot.AddComponent<SoundManager>();
        soundMgr.audioMixer = mainMixer;
        soundMgr.m16Shot = m16Shot;
        soundMgr.pistolShot = pistolShot;
        soundMgr.zombieWalking = zombieWalk;
        soundMgr.zombieChase = zombieChase;
        soundMgr.zombieAttack = zombieAttack;
        soundMgr.zombieHurt = zombieHurt;
        soundMgr.zombieDeath = zombieDeath;
        soundMgr.playerHurt = playerHurt;
        soundMgr.playerDie = playerDie;
        soundMgr.gameOverMusic = gameOverMusic;
        soundMgr.gameplayMusic = bgMusic;

        soundMgr.ShootingChannel = managersRoot.AddComponent<AudioSource>();
        soundMgr.reloadingSoundpistol = managersRoot.AddComponent<AudioSource>();
        soundMgr.reloadingSoundM16 = managersRoot.AddComponent<AudioSource>();
        soundMgr.empty_pistol_sound = managersRoot.AddComponent<AudioSource>();
        soundMgr.throwableChannel = managersRoot.AddComponent<AudioSource>();
        soundMgr.zombieChannel = managersRoot.AddComponent<AudioSource>();
        soundMgr.zombieChannel2 = managersRoot.AddComponent<AudioSource>();
        soundMgr.playerChannel = managersRoot.AddComponent<AudioSource>();
        soundMgr.musicChannel = managersRoot.AddComponent<AudioSource>();

        GlobalReferences globalRefs = managersRoot.AddComponent<GlobalReferences>();
        globalRefs.bulletImpactEffectPrefab = bulletImpactPrefab;
        globalRefs.bloodSprayEffect = bloodSprayPrefab;
        globalRefs.grenadeExplosionEffect = grenadePrefab;
        globalRefs.smokeGreandeEffect = smokeGrenadePrefab;

        managersRoot.AddComponent<AmmoManager>();
        managersRoot.AddComponent<SaveLoadManager>();

        // 8. Place Guns on Actual Tables
        GameObject weaponsRoot = new GameObject("Placed_Weapons");
        int tableIndex = 0;
        foreach (GameObject table in tableObjects)
        {
            Renderer rend = table.GetComponent<Renderer>();
            Bounds bounds = rend != null ? rend.bounds : new Bounds(table.transform.position, Vector3.one);
            float tableTopY = bounds.max.y + 0.05f;

            // Place Pistol on Left side of table
            if (pistolPrefab != null && tableIndex < 4)
            {
                Vector3 pistolPos = new Vector3(bounds.center.x - 0.25f, tableTopY, bounds.center.z);
                GameObject pistol = (GameObject)PrefabUtility.InstantiatePrefab(pistolPrefab);
                pistol.transform.position = pistolPos;
                pistol.transform.rotation = Quaternion.Euler(0, 90f, 0);
                pistol.transform.SetParent(weaponsRoot.transform);
                Weapon w = pistol.GetComponent<Weapon>();
                if (w != null)
                {
                    w.isActive = false;
                    w.UpdateStateVisuals();
                }
                if (pistol.GetComponent<Outline>() == null) pistol.AddComponent<Outline>();
            }

            // Place M16 on Right side of table
            if (m16Prefab != null && tableIndex < 4)
            {
                Vector3 m16Pos = new Vector3(bounds.center.x + 0.25f, tableTopY, bounds.center.z);
                GameObject m16 = (GameObject)PrefabUtility.InstantiatePrefab(m16Prefab);
                m16.transform.position = m16Pos;
                m16.transform.rotation = Quaternion.Euler(0, 90f, 0);
                m16.transform.SetParent(weaponsRoot.transform);
                Weapon w = m16.GetComponent<Weapon>();
                if (w != null)
                {
                    w.isActive = false;
                    w.UpdateStateVisuals();
                }
                if (m16.GetComponent<Outline>() == null) m16.AddComponent<Outline>();
            }

            tableIndex++;
        }

        // 9. Setup Ammo Spawn Points on Tables/Furniture
        GameObject ammoSpawnRoot = new GameObject("Ammo_SpawnPoints");
        List<Transform> ammoSpawns = new List<Transform>();

        int ammoIdx = 0;
        foreach (GameObject table in tableObjects)
        {
            Renderer rend = table.GetComponent<Renderer>();
            Bounds bounds = rend != null ? rend.bounds : new Bounds(table.transform.position, Vector3.one);
            float tableTopY = bounds.max.y + 0.05f;

            GameObject sp = new GameObject($"AmmoSpawn_{++ammoIdx}");
            sp.transform.SetParent(ammoSpawnRoot.transform);
            sp.transform.position = new Vector3(bounds.center.x, tableTopY, bounds.center.z + 0.2f);
            ammoSpawns.Add(sp.transform);
        }

        // If fewer than 6 tables, add additional surrounding ammo spawns
        while (ammoSpawns.Count < 8)
        {
            int i = ammoSpawns.Count;
            GameObject sp = new GameObject($"AmmoSpawn_{i + 1}");
            sp.transform.SetParent(ammoSpawnRoot.transform);
            sp.transform.position = new Vector3((i % 2 == 0 ? 3f : -3f) * (i + 1) * 0.5f, 0.4f, (i % 3 == 0 ? 3f : -3f));
            ammoSpawns.Add(sp.transform);
        }

        AmmoSpawner ammoSpawner = managersRoot.AddComponent<AmmoSpawner>();
        ammoSpawner.ammoBoxPrefab = ammoBoxPrefab;
        ammoSpawner.spawnPoints = ammoSpawns;
        ammoSpawner.targetActiveAmmoBoxes = Mathf.Min(5, ammoSpawns.Count);
        ammoSpawner.respawnCooldown = 20f;

        // 10. Setup Crystal Spawn Points
        GameObject crystalSpawnRoot = new GameObject("Crystal_SpawnPoints");
        List<Transform> crystalSpawns = new List<Transform>();

        // Create distributed crystal spawn points across room quadrants
        List<Vector3> crystalLocations = new List<Vector3>();
        foreach (GameObject table in tableObjects)
        {
            Renderer rend = table.GetComponent<Renderer>();
            Bounds bounds = rend != null ? rend.bounds : new Bounds(table.transform.position, Vector3.one);
            crystalLocations.Add(new Vector3(bounds.center.x + 1.2f, 0.8f, bounds.center.z + 1.2f));
            crystalLocations.Add(new Vector3(bounds.center.x - 1.2f, 0.8f, bounds.center.z - 1.2f));
        }

        if (crystalLocations.Count == 0)
        {
            crystalLocations.AddRange(new[]
            {
                new Vector3(3f, 0.8f, 3f),
                new Vector3(-3f, 0.8f, -3f),
                new Vector3(5f, 0.8f, -4f),
                new Vector3(-5f, 0.8f, 4f),
                new Vector3(0f, 0.8f, 6f),
                new Vector3(0f, 0.8f, -6f)
            });
        }

        int cIdx = 0;
        foreach (var loc in crystalLocations)
        {
            GameObject sp = new GameObject($"CrystalSpawn_{++cIdx}");
            sp.transform.SetParent(crystalSpawnRoot.transform);
            sp.transform.position = loc;
            crystalSpawns.Add(sp.transform);
            if (crystalSpawns.Count >= 10) break;
        }

        CrystalManager crystalMgr = managersRoot.AddComponent<CrystalManager>();
        crystalMgr.crystalPrefab = crystalPrefab;
        crystalMgr.spawnPoints = crystalSpawns;

        // 11. Setup Waypoints Cluster & Zombie Spawner
        GameObject waypointsCluster = new GameObject("WaypointsCluster");
        waypointsCluster.tag = "Waypoints";

        int wpIdx = 0;
        foreach (var sp in crystalSpawns)
        {
            GameObject wp = new GameObject($"Waypoint_{++wpIdx}");
            wp.transform.SetParent(waypointsCluster.transform);
            wp.transform.position = new Vector3(sp.position.x, 0.1f, sp.position.z);
        }

        GameObject spawnerObj = new GameObject("ZombieSpawner");
        spawnerObj.transform.position = new Vector3(0f, 0.1f, 8f);
        ZombieSpawner spawner = spawnerObj.AddComponent<ZombieSpawner>();
        spawner.zombiePrefab = zombiePrefab;
        spawner.initialZombiePerWave = 4;
        spawner.spawnDelay = 0.5f;
        spawner.waveCoolDown = 10f;
        spawner.currentZombieAlive = new List<Enemy>();

        Transform canvasTrans = canvasObj.transform;
        spawner.currentWaveUI = canvasTrans.Find("WaveText").GetComponent<TextMeshProUGUI>();
        spawner.waveOverUI = canvasTrans.Find("WaveOverText").GetComponent<TextMeshProUGUI>();
        spawner.counterUI = canvasTrans.Find("WaveCountdownText").GetComponent<TextMeshProUGUI>();

        // 12. Ensure all static geometry has colliders for NavMesh & Player Collision
        foreach (MeshFilter mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
        {
            if (mf.GetComponent<Collider>() == null && mf.sharedMesh != null)
            {
                mf.gameObject.AddComponent<MeshCollider>();
            }
        }

        // 13. Bake NavMesh
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

        // 14. Save Scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Successfully configured Assets/Scenes/DemoTest.unity with accurate table bounds, gun placement, crystal spawners, ammo spawners, and NavMesh!");
    }

    private static void EnsureTagsExist(params string[] tags)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        foreach (string tag in tags)
        {
            bool exists = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            }
        }

        tagManager.ApplyModifiedProperties();
    }

    private static GameObject SetupCrystalPrefab()
    {
        string prefabPath = "Assets/Prefab/Crystal.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existing != null) return existing;

        GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tempCube.name = "Crystal";
        tempCube.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
        tempCube.transform.rotation = Quaternion.Euler(45f, 45f, 0f);

        Material crystalMat = new Material(Shader.Find("Standard"));
        crystalMat.color = new Color(0f, 0.9f, 1f, 1f);
        crystalMat.EnableKeyword("_EMISSION");
        crystalMat.SetColor("_EmissionColor", new Color(0f, 0.6f, 0.9f) * 2f);
        AssetDatabase.CreateAsset(crystalMat, "Assets/Prefab/Crystal_Mat.mat");

        tempCube.GetComponent<MeshRenderer>().material = crystalMat;
        tempCube.GetComponent<BoxCollider>().isTrigger = true;
        tempCube.AddComponent<Crystal>();

        Outline outline = tempCube.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = Color.cyan;
        outline.OutlineWidth = 3f;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tempCube, prefabPath);
        DestroyImmediate(tempCube);
        return prefab;
    }

    private static GameObject CreateHUDCanvas()
    {
        GameObject canvasObj = new GameObject("HUD Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        HUDManager hud = canvasObj.AddComponent<HUDManager>();

        // Middle Dot / Crosshair
        GameObject middleDot = new GameObject("MiddleDot");
        middleDot.transform.SetParent(canvasObj.transform, false);
        Image dotImg = middleDot.AddComponent<Image>();
        dotImg.rectTransform.sizeDelta = new Vector2(6, 6);
        dotImg.color = Color.white;
        hud.middleDot = middleDot;

        // Health Text
        GameObject healthObj = new GameObject("HealthText");
        healthObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI healthText = healthObj.AddComponent<TextMeshProUGUI>();
        healthText.rectTransform.anchorMin = new Vector2(0, 0);
        healthText.rectTransform.anchorMax = new Vector2(0, 0);
        healthText.rectTransform.pivot = new Vector2(0, 0);
        healthText.rectTransform.anchoredPosition = new Vector2(30, 30);
        healthText.fontSize = 28;
        healthText.text = "Health: 100";
        healthText.color = Color.green;

        // Ammo Magazine & Total
        GameObject magAmmoObj = new GameObject("MagazineAmmoText");
        magAmmoObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI magAmmo = magAmmoObj.AddComponent<TextMeshProUGUI>();
        magAmmo.rectTransform.anchorMin = new Vector2(1, 0);
        magAmmo.rectTransform.anchorMax = new Vector2(1, 0);
        magAmmo.rectTransform.pivot = new Vector2(1, 0);
        magAmmo.rectTransform.anchoredPosition = new Vector2(-120, 30);
        magAmmo.fontSize = 32;
        magAmmo.text = "";
        hud.magazineAmmoUI = magAmmo;

        GameObject totAmmoObj = new GameObject("TotalAmmoText");
        totAmmoObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI totAmmo = totAmmoObj.AddComponent<TextMeshProUGUI>();
        totAmmo.rectTransform.anchorMin = new Vector2(1, 0);
        totAmmo.rectTransform.anchorMax = new Vector2(1, 0);
        totAmmo.rectTransform.pivot = new Vector2(1, 0);
        totAmmo.rectTransform.anchoredPosition = new Vector2(-30, 30);
        totAmmo.fontSize = 24;
        totAmmo.text = "";
        hud.totalAmmoUI = totAmmo;

        // Weapon Images
        GameObject activeWpnObj = new GameObject("ActiveWeaponUI");
        activeWpnObj.transform.SetParent(canvasObj.transform, false);
        Image activeWpnImg = activeWpnObj.AddComponent<Image>();
        activeWpnImg.rectTransform.anchorMin = new Vector2(1, 0);
        activeWpnImg.rectTransform.anchorMax = new Vector2(1, 0);
        activeWpnImg.rectTransform.anchoredPosition = new Vector2(-200, 45);
        activeWpnImg.rectTransform.sizeDelta = new Vector2(60, 40);
        hud.activeWeaponUI = activeWpnImg;

        GameObject inActiveWpnObj = new GameObject("InActiveWeaponUI");
        inActiveWpnObj.transform.SetParent(canvasObj.transform, false);
        Image inActiveWpnImg = inActiveWpnObj.AddComponent<Image>();
        inActiveWpnImg.rectTransform.anchorMin = new Vector2(1, 0);
        inActiveWpnImg.rectTransform.anchorMax = new Vector2(1, 0);
        inActiveWpnImg.rectTransform.anchoredPosition = new Vector2(-270, 45);
        inActiveWpnImg.rectTransform.sizeDelta = new Vector2(50, 35);
        hud.inActiveWeaponUI = inActiveWpnImg;

        // Ammo Type Icon
        GameObject ammoTypeObj = new GameObject("AmmoTypeUI");
        ammoTypeObj.transform.SetParent(canvasObj.transform, false);
        Image ammoTypeImg = ammoTypeObj.AddComponent<Image>();
        ammoTypeImg.rectTransform.anchorMin = new Vector2(1, 0);
        ammoTypeImg.rectTransform.anchorMax = new Vector2(1, 0);
        ammoTypeImg.rectTransform.anchoredPosition = new Vector2(-160, 45);
        ammoTypeImg.rectTransform.sizeDelta = new Vector2(30, 30);
        hud.ammoTypeUI = ammoTypeImg;

        // Throwables
        GameObject lethalObj = new GameObject("LethalUI");
        lethalObj.transform.SetParent(canvasObj.transform, false);
        Image lethalImg = lethalObj.AddComponent<Image>();
        lethalImg.rectTransform.anchorMin = new Vector2(0.5f, 0);
        lethalImg.rectTransform.anchorMax = new Vector2(0.5f, 0);
        lethalImg.rectTransform.anchoredPosition = new Vector2(-30, 40);
        lethalImg.rectTransform.sizeDelta = new Vector2(35, 35);
        hud.lethalUI = lethalImg;

        GameObject lethalAmtObj = new GameObject("LethalAmountUI");
        lethalAmtObj.transform.SetParent(lethalObj.transform, false);
        TextMeshProUGUI lethalAmt = lethalAmtObj.AddComponent<TextMeshProUGUI>();
        lethalAmt.rectTransform.anchoredPosition = new Vector2(20, -10);
        lethalAmt.fontSize = 18;
        lethalAmt.text = "0";
        hud.lathelAmountUI = lethalAmt;

        GameObject tacticalObj = new GameObject("TacticalUI");
        tacticalObj.transform.SetParent(canvasObj.transform, false);
        Image tacticalImg = tacticalObj.AddComponent<Image>();
        tacticalImg.rectTransform.anchorMin = new Vector2(0.5f, 0);
        tacticalImg.rectTransform.anchorMax = new Vector2(0.5f, 0);
        tacticalImg.rectTransform.anchoredPosition = new Vector2(30, 40);
        tacticalImg.rectTransform.sizeDelta = new Vector2(35, 35);
        hud.tacticalUI = tacticalImg;

        GameObject tacticalAmtObj = new GameObject("TacticalAmountUI");
        tacticalAmtObj.transform.SetParent(tacticalObj.transform, false);
        TextMeshProUGUI tacticalAmt = tacticalAmtObj.AddComponent<TextMeshProUGUI>();
        tacticalAmt.rectTransform.anchoredPosition = new Vector2(20, -10);
        tacticalAmt.fontSize = 18;
        tacticalAmt.text = "0";
        hud.tacticalAmountUI = tacticalAmt;

        // Wave Info (Top Center)
        GameObject waveObj = new GameObject("WaveText");
        waveObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI waveText = waveObj.AddComponent<TextMeshProUGUI>();
        waveText.rectTransform.anchorMin = new Vector2(0.5f, 1);
        waveText.rectTransform.anchorMax = new Vector2(0.5f, 1);
        waveText.rectTransform.anchoredPosition = new Vector2(0, -35);
        waveText.fontSize = 32;
        waveText.alignment = TextAlignmentOptions.Center;
        waveText.text = "Wave: 1";

        GameObject waveOverObj = new GameObject("WaveOverText");
        waveOverObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI waveOverText = waveOverObj.AddComponent<TextMeshProUGUI>();
        waveOverText.rectTransform.anchorMin = new Vector2(0.5f, 0.6f);
        waveOverText.rectTransform.anchorMax = new Vector2(0.5f, 0.6f);
        waveOverText.fontSize = 40;
        waveOverText.alignment = TextAlignmentOptions.Center;
        waveOverText.color = Color.yellow;
        waveOverText.text = "WAVE COMPLETED!";
        waveOverObj.SetActive(false);

        GameObject counterObj = new GameObject("WaveCountdownText");
        counterObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI counterText = counterObj.AddComponent<TextMeshProUGUI>();
        counterText.rectTransform.anchorMin = new Vector2(0.5f, 1);
        counterText.rectTransform.anchorMax = new Vector2(0.5f, 1);
        counterText.rectTransform.anchoredPosition = new Vector2(0, -70);
        counterText.fontSize = 24;
        counterText.alignment = TextAlignmentOptions.Center;
        counterText.text = "";

        // Crystals Collected Text (Top Right)
        GameObject crystalUIObj = new GameObject("CrystalCountText");
        crystalUIObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI crystalText = crystalUIObj.AddComponent<TextMeshProUGUI>();
        crystalText.rectTransform.anchorMin = new Vector2(1, 1);
        crystalText.rectTransform.anchorMax = new Vector2(1, 1);
        crystalText.rectTransform.pivot = new Vector2(1, 1);
        crystalText.rectTransform.anchoredPosition = new Vector2(-30, -30);
        crystalText.fontSize = 26;
        crystalText.color = new Color(0.3f, 0.9f, 1f, 1f);
        crystalText.text = "Crystals: 0";
        hud.crystalsCollectedUI = crystalText;

        // Bloody Screen Overlay
        GameObject bloodyScreen = new GameObject("BloodyScreen");
        bloodyScreen.transform.SetParent(canvasObj.transform, false);
        Image bloodImg = bloodyScreen.AddComponent<Image>();
        bloodImg.rectTransform.anchorMin = Vector2.zero;
        bloodImg.rectTransform.anchorMax = Vector2.one;
        bloodImg.rectTransform.sizeDelta = Vector2.zero;
        bloodImg.color = new Color(0.8f, 0f, 0f, 0f);
        bloodyScreen.SetActive(false);

        // Game Over UI
        GameObject gameOverObj = new GameObject("GameOverText");
        gameOverObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI gameOverText = gameOverObj.AddComponent<TextMeshProUGUI>();
        gameOverText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        gameOverText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        gameOverText.fontSize = 54;
        gameOverText.alignment = TextAlignmentOptions.Center;
        gameOverText.color = Color.red;
        gameOverText.text = "YOU DIED";
        gameOverObj.SetActive(false);

        // Screen Fader Image
        GameObject fadeObj = new GameObject("ScreenFadeImage");
        fadeObj.transform.SetParent(canvasObj.transform, false);
        Image fadeImg = fadeObj.AddComponent<Image>();
        fadeImg.rectTransform.anchorMin = Vector2.zero;
        fadeImg.rectTransform.anchorMax = Vector2.one;
        fadeImg.rectTransform.sizeDelta = Vector2.zero;
        fadeImg.color = new Color(0f, 0f, 0f, 0f);
        fadeObj.SetActive(false);

        return canvasObj;
    }

    private static GameObject CreatePlayer(HUDManager hud, GameObject grenadePrefab, GameObject smokeGrenadePrefab, Vector3 spawnPosition)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Default");
        player.transform.position = spawnPosition;

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.4f;
        cc.center = new Vector3(0, 0.9f, 0);

        PlayerMovement movement = player.AddComponent<PlayerMovement>();
        MouseMovement mouseMovement = player.AddComponent<MouseMovement>();
        Player playerScript = player.AddComponent<Player>();
        ScreenFader fader = player.AddComponent<ScreenFader>();

        // Ground Check
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform, false);
        groundCheck.transform.localPosition = new Vector3(0, 0.1f, 0);
        movement.groundCheck = groundCheck.transform;
        movement.GrounLayerMask = ~0;

        // Camera
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";
        cameraObj.transform.SetParent(player.transform, false);
        cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0);
        Camera cam = cameraObj.AddComponent<Camera>();
        cam.nearClipPlane = 0.05f;
        cameraObj.AddComponent<AudioListener>();

        cameraObj.AddComponent<InteractionManager>();

        WeaponManager wpnMgr = cameraObj.AddComponent<WeaponManager>();
        wpnMgr.grenadePrefab = grenadePrefab;
        wpnMgr.smokeGrenadePrefab = smokeGrenadePrefab;

        GameObject wpnSlot1 = new GameObject("WeaponSlot_1");
        wpnSlot1.transform.SetParent(cameraObj.transform, false);
        GameObject wpnSlot2 = new GameObject("WeaponSlot_2");
        wpnSlot2.transform.SetParent(cameraObj.transform, false);

        wpnMgr.weaponSlots = new List<GameObject> { wpnSlot1, wpnSlot2 };
        wpnMgr.activeWeaponSlot = wpnSlot1;

        GameObject throwableSpawn = new GameObject("ThrowableSpawn");
        throwableSpawn.transform.SetParent(cameraObj.transform, false);
        throwableSpawn.transform.localPosition = new Vector3(0.2f, -0.2f, 0.5f);
        wpnMgr.throwableSpawn = throwableSpawn;

        Transform canvasTrans = hud.transform;
        playerScript.playerHealthUI = canvasTrans.Find("HealthText").GetComponent<TextMeshProUGUI>();
        playerScript.gameOverUI = canvasTrans.Find("GameOverText").GetComponent<TextMeshProUGUI>();
        playerScript.bloodyScreen = canvasTrans.Find("BloodyScreen").gameObject;
        fader.fadeImage = canvasTrans.Find("ScreenFadeImage").GetComponent<Image>();

        return player;
    }
}
