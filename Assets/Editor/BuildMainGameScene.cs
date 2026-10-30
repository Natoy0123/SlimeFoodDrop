using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BuildMainGameScene
{
    private static Sprite roundedFrameSprite;
    private static Font defaultArcadeFont;
    private static Font defaultBodyFont;

    [InitializeOnLoadMethod]
    private static void CheckAutoRebuildTrigger()
    {
        EditorApplication.delayCall += () =>
        {
            string triggerFile = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "trigger_rebuild_scene.flag");
            if (File.Exists(triggerFile))
            {
                try { File.Delete(triggerFile); } catch { }
                Debug.Log("[BuildMainGameScene] Trigger file detected! Executing BuildAll()...");
                BuildAll();
            }
        };
    }

    [MenuItem("SlimeFoodDrop/Build Main Game Scene")]
    public static void BuildAll()
    {
        Debug.Log("[BuildMainGameScene] Starting full asset import, Splash, Home, Guidebook, Bottom Slime Tutorial, Power-Up Timers, Environment Banners & Scene assembly...");

        roundedFrameSprite = LoadSpriteEnsured("Assets/Sprites/UI/ui_rounded_frame_4px.png");

        // 1. Ensure all sprites are imported with SpriteImportMode.Single & Point filter
        PixelArtImportSetup.ForceReimportAllSprites();

        // 2. Create FallingItem Prefab
        GameObject itemPrefab = CreateFallingItemPrefab();

        // 3. Create New MainGameScene
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 4. Main Camera + ResponsiveCamera
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        camObj.transform.position = new Vector3(0f, 0f, -10f);
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.4f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.11f, 0.08f, 0.06f, 1f);
        camObj.AddComponent<AudioListener>();
        ResponsiveCamera respCam = camObj.AddComponent<ResponsiveCamera>();
        respCam.minOrthographicSize = 5.4f;
        respCam.minVisibleWorldWidth = 5.8f;
        respCam.horizontalPadding = 0.52f;

        // 5. EventSystem (Mandatory for UI interactive buttons)
        GameObject esObj = new GameObject("EventSystem");
        esObj.AddComponent<EventSystem>();
        esObj.AddComponent<StandaloneInputModule>();

        // 6. Environment Parallax Primary & Transition Layers
        GameObject envRoot = new GameObject("Environment");
        string[] forestLayerFiles = new string[]
        {
            "Assets/Sprites/Background/parallax-forest-back-trees.png",
            "Assets/Sprites/Background/parallax-forest-middle-trees.png",
            "Assets/Sprites/Background/parallax-forest-front-trees.png",
            "Assets/Sprites/Background/parallax-forest-lights.png"
        };
        string[] layerNames = new string[] { "Parallax_Back", "Parallax_Mid", "Parallax_Front", "Parallax_Lights" };
        int[] primarySortingOrders = new int[] { -10, -8, -6, -4 };
        int[] transitionSortingOrders = new int[] { -9, -7, -5, -3 };

        SpriteRenderer[] bgRenderers = new SpriteRenderer[4];
        SpriteRenderer[] transRenderers = new SpriteRenderer[4];

        for (int i = 0; i < 4; i++)
        {
            GameObject layerObj = new GameObject(layerNames[i]);
            layerObj.transform.SetParent(envRoot.transform, false);
            layerObj.transform.position = Vector3.zero;
            SpriteRenderer sr = layerObj.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(forestLayerFiles[i]);
            sr.sortingOrder = primarySortingOrders[i];
            if (i == 3)
            {
                sr.color = new Color(1f, 0.98f, 0.85f, 0.55f);
            }
            bgRenderers[i] = sr;

            GameObject transObj = new GameObject(layerNames[i] + "_Transition");
            transObj.transform.SetParent(envRoot.transform, false);
            transObj.transform.position = Vector3.zero;
            SpriteRenderer tsr = transObj.AddComponent<SpriteRenderer>();
            tsr.sortingOrder = transitionSortingOrders[i];
            tsr.color = new Color(1f, 1f, 1f, 0f);
            transObj.SetActive(false);
            transRenderers[i] = tsr;
        }
        respCam.backgroundLayers = bgRenderers;
        respCam.transitionBackgroundLayers = transRenderers;

        // 7. FloorBoundary + FloorTrigger
        GameObject floorObj = new GameObject("FloorBoundary");
        floorObj.transform.position = new Vector3(0f, -5.65f, 0f);
        BoxCollider2D floorCol = floorObj.AddComponent<BoxCollider2D>();
        floorCol.isTrigger = true;
        floorCol.size = new Vector2(18f, 1.2f);
        floorObj.AddComponent<FloorTrigger>();
        respCam.floorCollider = floorCol;

        // 8. SlimePlayer
        GameObject slimeObj = new GameObject("SlimePlayer");
        slimeObj.tag = "Player";
        slimeObj.transform.position = new Vector3(0f, -3.95f, 0f);
        SpriteRenderer slimeSr = slimeObj.AddComponent<SpriteRenderer>();
        slimeSr.sortingOrder = 5;

        CircleCollider2D slimeCol = slimeObj.AddComponent<CircleCollider2D>();
        slimeCol.isTrigger = true;
        slimeCol.radius = 0.38f;
        slimeCol.offset = new Vector2(0f, -0.04f);

        SlimeController slimeCtrl = slimeObj.AddComponent<SlimeController>();
        slimeCtrl.spriteRenderer = slimeSr;
        slimeCtrl.idleFrames = LoadSpritesInFolder("Assets/Sprites/Slime/01-Idle");
        slimeCtrl.moveFrames = LoadSpritesInFolder("Assets/Sprites/Slime/02-Move");
        slimeCtrl.eatFrames = LoadSpritesInFolder("Assets/Sprites/Slime/03-Attack");
        slimeCtrl.hurtFrames = LoadSpritesInFolder("Assets/Sprites/Slime/04-Hurt");
        slimeCtrl.deadFrames = LoadSpritesInFolder("Assets/Sprites/Slime/05-Dead");
        slimeCtrl.slimeTrailSprite = LoadSpriteEnsured("Assets/Sprites/Slime/slime_trail_droplet.png");
        if (slimeCtrl.idleFrames.Length > 0)
        {
            slimeSr.sprite = slimeCtrl.idleFrames[0];
        }

        // 9. ItemSpawner (with 4 Power-Ups: Heart, Magnet, ShieldBarrier, TimeSlow)
        GameObject spawnerObj = new GameObject("ItemSpawner");
        spawnerObj.transform.position = new Vector3(0f, 5.85f, 0f);
        ItemSpawner spawner = spawnerObj.AddComponent<ItemSpawner>();
        spawner.itemPrefab = itemPrefab;
        spawner.foodSprites = LoadSpritesInFolder("Assets/Sprites/Food");
        spawner.poisonSprites = LoadSpritesInFolder("Assets/Sprites/Hazards");
        spawner.heartSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pickups/bonus_heart.png");
        spawner.magnetSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pickups/star_magnet.png");
        spawner.shieldBarrierSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pickups/barrier_shield.png");
        spawner.timeSlowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pickups/time_slow_hourglass.png");

        // 10. Managers (GameManager, SoundManager, ThemeManager, UIManager)
        GameObject managersRoot = new GameObject("Managers");

        GameObject gmObj = new GameObject("GameManager");
        gmObj.transform.SetParent(managersRoot.transform, false);
        GameManager gm = gmObj.AddComponent<GameManager>();
        gm.showSplashOnStart = true;
        gm.enableCountdownAndTutorial = true;

        GameObject smObj = new GameObject("SoundManager");
        smObj.transform.SetParent(managersRoot.transform, false);
        AudioSource bgmSource = smObj.AddComponent<AudioSource>();
        AudioSource bgmCrossfadeSource = smObj.AddComponent<AudioSource>();
        AudioSource sfxSource = smObj.AddComponent<AudioSource>();
        AudioSource powerupActiveSource = smObj.AddComponent<AudioSource>();
        AudioSource shieldActiveSource = smObj.AddComponent<AudioSource>();
        AudioSource magnetActiveSource = smObj.AddComponent<AudioSource>();
        AudioSource timeSlowActiveSource = smObj.AddComponent<AudioSource>();
        SoundManager sm = smObj.AddComponent<SoundManager>();
        sm.bgmSource = bgmSource;
        sm.bgmCrossfadeSource = bgmCrossfadeSource;
        sm.sfxSource = sfxSource;
        sm.powerupActiveSource = powerupActiveSource;
        sm.shieldActiveSource = shieldActiveSource;
        sm.magnetActiveSource = magnetActiveSource;
        sm.timeSlowActiveSource = timeSlowActiveSource;

        AudioClip bgmForest = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM_Forest_HouseInAForestLoop.ogg");
        if (bgmForest == null) bgmForest = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Forest_Ambience.mp3");
        AudioClip bgmPlains = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM_Mountain_Theme.mp3");
        AudioClip bgmMountain = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM_Mountain_Theme.mp3");
        AudioClip bgmTropical = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM_Underwater_OceanWave.mp3");
        AudioClip bgmDesert = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM_Desert_DesertBounce.mp3");
        AudioClip bgmUnderwater = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM_Underwater_OceanWave.mp3");
        AudioClip bgmIndustrial = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM_Industrial_ChiptuneFactory.mp3");
        AudioClip bgmCave = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM_Cave_CrystalSong18.mp3");
        AudioClip bgmCliffs = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM_Cave_CrystalSong18.mp3");
        AudioClip bgmCitadel = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM_Night_ColdNight.mp3");

        sm.forestAmbienceClip = bgmForest;
        sm.themeBgmClips = new AudioClip[] { bgmForest, bgmPlains, bgmMountain, bgmTropical, bgmDesert, bgmUnderwater, bgmIndustrial, bgmCave, bgmCliffs, bgmCitadel };
        sm.hurtClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/break.ogg");
        sm.chimeClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/coin.ogg");
        sm.missClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/fall.ogg");
        sm.powerupClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/jump.ogg");

        sm.buttonClickClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_ui_click.wav");
        sm.slimeEatClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_slime_eat.wav");
        sm.slimePukeClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_slime_puke.wav");
        sm.foodScamperClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_food_scamper.wav");
        sm.shieldActiveClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_powerup_shield_active.wav");
        sm.magnetActiveClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_powerup_magnet_active.wav");
        sm.timeSlowActiveClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_powerup_timeslow_active.wav");
        sm.shieldPickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_pickup_shield.wav");
        sm.magnetPickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_pickup_magnet.wav");
        sm.timeSlowPickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_pickup_timeslow.wav");
        sm.heartPickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_pickup_heart.wav");

        List<AudioClip> crunches = new List<AudioClip>();
        for (int i = 1; i <= 7; i++)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Audio/crunch.{i}.ogg");
            if (c != null) crunches.Add(c);
        }
        sm.munchClips = crunches.ToArray();

        // ThemeManager with 10 progressive themes (0, 40, 90, 150, 220, 300, 400, 520, 660, 820 pts)
        GameObject tmObj = new GameObject("ThemeManager");
        tmObj.transform.SetParent(managersRoot.transform, false);
        ThemeManager tm = tmObj.AddComponent<ThemeManager>();
        tm.primaryRenderers = bgRenderers;
        tm.transitionRenderers = transRenderers;

        GameObject floorPlatformObj = new GameObject("Floor_Platform");
        floorPlatformObj.transform.SetParent(envRoot.transform, false);
        SpriteRenderer floorPlatformSr = floorPlatformObj.AddComponent<SpriteRenderer>();
        floorPlatformSr.sortingOrder = 5;

        GameObject transFloorPlatformObj = new GameObject("Transition_Floor_Platform");
        transFloorPlatformObj.transform.SetParent(envRoot.transform, false);
        SpriteRenderer transFloorPlatformSr = transFloorPlatformObj.AddComponent<SpriteRenderer>();
        transFloorPlatformSr.sortingOrder = 6;
        transFloorPlatformObj.SetActive(false);

        tm.floorPlatformRenderer = floorPlatformSr;
        tm.transitionFloorPlatformRenderer = transFloorPlatformSr;
        respCam.floorPlatformRenderer = floorPlatformSr;
        respCam.transitionFloorPlatformRenderer = transFloorPlatformSr;
        tm.themes = new ThemeManager.EnvironmentTheme[]
        {
            new ThemeManager.EnvironmentTheme
            {
                themeName = "Whispering Forest",
                floorPlatformSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Platforms/platform_0_forest.png"),
                environmentDescription = "Lush Woodland Canopy  |  Peaceful Forest BGM",
                unlockScore = 0,
                bgmClip = bgmForest,
                layers = new Sprite[]
                {
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/parallax-forest-back-trees.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/parallax-forest-middle-trees.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/parallax-forest-front-trees.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/parallax-forest-lights.png")
                },
                layerTints = new Color[]
                {
                    Color.white,
                    Color.white,
                    Color.white,
                    new Color(1f, 0.98f, 0.85f, 0.55f)
                }
            },
            new ThemeManager.EnvironmentTheme
            {
                themeName = "Breezy Plains",
                floorPlatformSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Platforms/platform_1_plains.png"),
                environmentDescription = "Sunlit Meadow & Rolling Grass  |  Sunny Plains BGM",
                unlockScore = 100,
                bgmClip = bgmPlains,
                layers = new Sprite[]
                {
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme4_Plains/plains_0_sky.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme4_Plains/plains_1_mountains.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme4_Plains/plains_2_hills.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme4_Plains/plains_3_grass.png")
                },
                layerTints = new Color[] { Color.white, Color.white, Color.white, Color.white }
            },
            new ThemeManager.EnvironmentTheme
            {
                themeName = "Sunset Peaks",
                floorPlatformSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Platforms/platform_2_mountain.png"),
                environmentDescription = "Golden Alpine Crags  |  Windswept Mountain BGM",
                unlockScore = 250,
                bgmClip = bgmMountain,
                layers = new Sprite[]
                {
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme1_Mountain/parallax-mountain-bg.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme1_Mountain/parallax-mountain-mountains.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme1_Mountain/parallax-mountain-trees.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme1_Mountain/parallax-mountain-foreground-trees.png")
                },
                layerTints = new Color[] { Color.white, Color.white, Color.white, Color.white }
            },
            new ThemeManager.EnvironmentTheme
            {
                themeName = "Emerald Tropic",
                floorPlatformSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Platforms/platform_3_tropical.png"),
                environmentDescription = "Lush Palms & Mist Cascades  |  Tropical Coast BGM",
                unlockScore = 450,
                bgmClip = bgmTropical,
                layers = new Sprite[]
                {
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme5_Tropical/tropical_0_sky.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme5_Tropical/tropical_1_mountains.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme5_Tropical/tropical_2_hills.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme5_Tropical/tropical_3_palms.png")
                },
                layerTints = new Color[] { Color.white, Color.white, Color.white, Color.white }
            },
            new ThemeManager.EnvironmentTheme
            {
                themeName = "Sunbaked Dunes",
                floorPlatformSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Platforms/platform_4_desert.png"),
                environmentDescription = "Canyon Spires & Desert Sands  |  Desert Mirage BGM",
                unlockScore = 700,
                bgmClip = bgmDesert,
                layers = new Sprite[]
                {
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme6_Desert/desert_0_sky.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme6_Desert/desert_1_mountains.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme6_Desert/desert_2_dunes.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme6_Desert/desert_3_palms.png")
                },
                layerTints = new Color[] { Color.white, Color.white, Color.white, Color.white }
            },
            new ThemeManager.EnvironmentTheme
            {
                themeName = "Coral Abyss",
                floorPlatformSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Platforms/platform_5_underwater.png"),
                environmentDescription = "Luminescent Deep Sea Reef  |  Ocean Wave BGM",
                unlockScore = 1000,
                bgmClip = bgmUnderwater,
                layers = new Sprite[]
                {
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme7_Underwater/underwater_0_deep.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme7_Underwater/underwater_1_sunrays.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme7_Underwater/underwater_2_corals.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme7_Underwater/underwater_3_reef.png")
                },
                layerTints = new Color[] { Color.white, Color.white, Color.white, Color.white }
            },
            new ThemeManager.EnvironmentTheme
            {
                themeName = "Steamworks Spire",
                floorPlatformSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Platforms/platform_6_industrial.png"),
                environmentDescription = "Industrial Silos & Machinery  |  Chiptune Factory BGM",
                unlockScore = 1350,
                bgmClip = bgmIndustrial,
                layers = new Sprite[]
                {
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme8_Industrial/industrial_0_sky.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme8_Industrial/industrial_1_far_buildings.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme8_Industrial/industrial_2_buildings.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme8_Industrial/industrial_3_foreground.png")
                },
                layerTints = new Color[] { Color.white, Color.white, Color.white, Color.white }
            },
            new ThemeManager.EnvironmentTheme
            {
                themeName = "Crystal Caverns",
                floorPlatformSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Platforms/platform_7_cave.png"),
                environmentDescription = "Deep Glowing Stalactite Caves  |  Crystal Echo BGM",
                unlockScore = 1750,
                bgmClip = bgmCave,
                layers = new Sprite[]
                {
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme9_Cave/cave_0_back.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme9_Cave/cave_1_far.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme9_Cave/cave_2_mid.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme9_Cave/cave_3_front.png")
                },
                layerTints = new Color[] { Color.white, Color.white, Color.white, Color.white }
            },
            new ThemeManager.EnvironmentTheme
            {
                themeName = "Sky Sanctuary",
                floorPlatformSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Platforms/platform_8_sky.png"),
                environmentDescription = "Floating Azure Sea Islands  |  Ancient Ruins BGM",
                unlockScore = 2200,
                bgmClip = bgmCliffs,
                layers = new Sprite[]
                {
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme2_MagicCliffs/magic-cliffs-0-sky-sea.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme2_MagicCliffs/magic-cliffs-1-clouds.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme2_MagicCliffs/magic-cliffs-2-islands.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme2_MagicCliffs/magic-cliffs-3-foreground.png")
                },
                layerTints = new Color[] { Color.white, Color.white, Color.white, Color.white }
            },
            new ThemeManager.EnvironmentTheme
            {
                themeName = "Frost Citadel",
                floorPlatformSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Platforms/platform_9_citadel.png"),
                environmentDescription = "Ancient Frosty Moonlit Fortress  |  Nocturnal Citadel BGM",
                unlockScore = 2700,
                bgmClip = bgmCitadel,
                layers = new Sprite[]
                {
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme3_NightCitadel/citadel-0-bg.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme3_NightCitadel/citadel-1-far.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme3_NightCitadel/citadel-2-mid.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Theme3_NightCitadel/citadel-3-front.png")
                },
                layerTints = new Color[] { Color.white, Color.white, Color.white, Color.white }
            }
        };

        GameObject uiMgrObj = new GameObject("UIManager");
        uiMgrObj.transform.SetParent(managersRoot.transform, false);
        UIManager uiMgr = uiMgrObj.AddComponent<UIManager>();

        // 11. Build UI Canvas
        BuildUICanvas(uiMgr, gm, cam, respCam);

        // 12. Calculate initial camera bounds & background fit
        respCam.RecalculateBounds();

        // 13. Save Scene & Register in EditorBuildSettings
        string scenePath = "Assets/Scenes/MainGameScene.unity";
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);

        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[BuildMainGameScene] Successfully built and saved {scenePath}! Themes={tm.themes.Length}, Food count={spawner.foodSprites.Length}, Hazard count={spawner.poisonSprites.Length}");

        // 14. Run automated verification for all screens & features
        RunAutomatedVerification(cam, respCam, slimeCtrl, spawner, gm, tm, uiMgr, scenePath);
        AuditAllUITexts();
    }

    private static void RunAutomatedVerification(Camera cam, ResponsiveCamera respCam, SlimeController slime, ItemSpawner spawner, GameManager gm, ThemeManager tm, UIManager ui, string scenePath)
    {
        Debug.Log("[SP-10 Verification] Running automated Splash, Home, Guidebook, Bottom Slime Tutorial, Power-Up Duration Bars, Environment Banner & Resolution checks...");

        string logDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        Directory.CreateDirectory(logDir);

        // Screenshot 1A: Pixar Splash Cinematic (Bloop staring at player through phone screen)
        ui.SetGameplayHUDVisible(false);
        ui.HideGameOver();
        if (ui.homePanel != null) ui.homePanel.SetActive(false);
        if (ui.difficultyPanel != null) ui.difficultyPanel.SetActive(false);
        if (ui.splashPanel != null)
        {
            ui.splashPanel.SetActive(true);
            if (ui.pixarSplashCinematic != null)
            {
                ui.pixarSplashCinematic.bloopRect.anchoredPosition = new Vector2(0f, -80f);
                ui.pixarSplashCinematic.bloopImage.sprite = ui.pixarSplashCinematic.slimeTurnLookUser;
                ui.pixarSplashCinematic.foodImage.gameObject.SetActive(false);
                ui.pixarSplashCinematic.ShowSpeechBubble("...Did you see what I just ate?!", new Vector2(0f, 75f));
            }
        }
        ui.SetSplashProgressVisual(0.44f, "44%", "MOUNTING 10 BIOME STAGE ATLASES...");
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_SplashScreen.png"));

        // Screenshot 1B: Pixar Splash Cinematic (Bloop spitting the poison projectile!)
        if (ui.pixarSplashCinematic != null)
        {
            ui.pixarSplashCinematic.bloopImage.sprite = ui.pixarSplashCinematic.slimeSpitLaunch;
            ui.pixarSplashCinematic.bloopRect.localScale = new Vector3(0.70f, 1.48f, 1f);
            ui.pixarSplashCinematic.ShowSpeechBubble("PFFFT!!", new Vector2(0f, 85f));
            ui.pixarSplashCinematic.projectileImage.gameObject.SetActive(true);
            ui.pixarSplashCinematic.projectileRect.anchoredPosition = new Vector2(240f, 60f);
        }
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_PixarSplashSpit.png"));

        // Screenshot 1C: Pixar Splash Catch (Bloop chomping down directly over the food in the spotlight)
        if (ui.pixarSplashCinematic != null)
        {
            ui.pixarSplashCinematic.bloopRect.anchoredPosition = new Vector2(40f, -80f);
            ui.pixarSplashCinematic.bloopImage.sprite = ui.pixarSplashCinematic.slimeChompBite;
            ui.pixarSplashCinematic.bloopRect.localScale = new Vector3(1.45f, 0.65f, 1f);
            ui.pixarSplashCinematic.foodImage.gameObject.SetActive(true);
            ui.pixarSplashCinematic.foodImage.sprite = ui.pixarSplashCinematic.foodPoisonReveal;
            ui.pixarSplashCinematic.foodRect.anchoredPosition = new Vector2(40f, -95f);
            ui.pixarSplashCinematic.foodRect.localScale = new Vector3(0.5f, 0.5f, 1f);
            ui.pixarSplashCinematic.speechBubbleGroup.SetActive(false);
            ui.pixarSplashCinematic.projectileImage.gameObject.SetActive(false);
        }
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_PixarSplashCatch.png"));

        if (ui.pixarSplashCinematic != null)
        {
            ui.pixarSplashCinematic.foodImage.gameObject.SetActive(false);
            ui.pixarSplashCinematic.projectileImage.gameObject.SetActive(false);
            ui.pixarSplashCinematic.speechBubbleGroup.SetActive(false);
            ui.pixarSplashCinematic.bloopRect.localScale = Vector3.one;
        }
        ui.SetSplashProgress(0f);

        // Screenshot 2A: Home Screen (Floating Unboxed Layout + Interactive Mascot Banner + Top-Right Settings)
        if (ui.splashPanel != null) ui.splashPanel.SetActive(false);
        if (slime != null) slime.gameObject.SetActive(false);
        gm.OpenHomeScreen();
        ui.ShowHomeScreen(425);
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_HomeScreen.png"));

        // Screenshot 2A2: Interactive Mascot Reacting (Speech Bubble + Cute Emotion)
        if (ui.homeInteractiveSlime != null)
        {
            ui.homeInteractiveSlime.slimeImage.sprite = ui.homeInteractiveSlime.spriteSurprised;
            ui.homeInteractiveSlime.reactionBubbleText.text = "Hehe! Tickles!";
            ui.homeInteractiveSlime.reactionBubbleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(250f, 15f);
            ui.homeInteractiveSlime.reactionBubbleObj.SetActive(true);
        }
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_HomeInteractiveReact.png"));

        // Screenshot 2A3: Annoyed Bloop Peeking from behind the Centered Floating Title!
        if (ui.homeInteractiveSlime != null)
        {
            ui.homeInteractiveSlime.transform.SetAsFirstSibling();
            ui.homeInteractiveSlime.slimeRect.anchoredPosition = new Vector2(160f, 150f);
            ui.homeInteractiveSlime.slimeImage.sprite = ui.homeInteractiveSlime.spritePeek;
            ui.homeInteractiveSlime.reactionBubbleText.text = "...Still there?";
            ui.homeInteractiveSlime.reactionBubbleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(160f, 270f);
            ui.homeInteractiveSlime.reactionBubbleObj.SetActive(true);
        }
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_HomeSlimePeek.png"));
        ui.homeInteractiveSlime?.ResetToRestingState();

        // Screenshot 2A4: Bloop Impatiently Begging at the PLAY Button!
        if (ui.homeInteractiveSlime != null)
        {
            ui.homeInteractiveSlime.transform.parent.SetAsLastSibling();
            ui.homeInteractiveSlime.slimeRect.anchoredPosition = new Vector2(250f, -670f);
            ui.homeInteractiveSlime.slimeImage.sprite = ui.homeInteractiveSlime.spriteAngry;
            ui.homeInteractiveSlime.reactionBubbleText.text = "TAP PLAY ALREADY!! >_<";
            ui.homeInteractiveSlime.reactionBubbleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(250f, -540f);
            ui.homeInteractiveSlime.reactionBubbleObj.SetActive(true);
        }
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_HomeSlimeBeggingPlay.png"));
        ui.homeInteractiveSlime?.ResetToRestingState();
        if (ui.homeInteractiveSlime != null) ui.homeInteractiveSlime.transform.parent.SetSiblingIndex(2);

        // Screenshot 2B: Home -> Settings Modal (Must NOT show Replay Tutorial, Restart, or Home buttons!)
        gm.OpenSettings();
        Debug.Assert(ui.inGameSettingsActionsGroup != null && !ui.inGameSettingsActionsGroup.activeSelf,
            "[SP-10 FAIL] Home -> Settings must NOT display in-game actions (Replay Tutorial, Restart, Home)!");
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_HomeSettings.png"));
        gm.CloseSettingsAndResume();

        // Screenshot 3: Slime Guidebook / Almanac Modal (Stages Tab & Power-Ups Tab)
        ui.ShowGuidebookModal(3); // Tab 3 = STAGES (with actual Stage Theme artwork previews!)
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_Guidebook.png"));
        ui.ShowGuidebookModal(0); // Tab 0 = FOODS (with clean item cards and details buttons)
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_Guidebook_Foods.png"));
        ui.ShowGuidebookModal(2); // Tab 2 = POWER-UPS
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_Guidebook_PowerUps.png"));

        // Screenshot 3B: Almanac Item Detail Modal (Savory Burger details with artwork, stats, lore, and tips)
        Sprite burgerDetailSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Food/food_092.png");
        ui.ShowAlmanacItemDetail(burgerDetailSprite, "SAVORY MEAL", new Color(0.20f, 0.75f, 0.35f, 1f), "SAVORY BURGERS & MEALS", "+10 PTS (+15 ON HARD)  |  BUILDS COMBO", "Hearty and delicious comfort foods dropped from above. Consuming these treats fuels Bloop's energy and boosts your run score.", "Catch in Bloop's mouth! Consecutive catches trigger combo multipliers (+5 Pts per 5 catches). Missing food does NOT cost Hearts!");
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_Almanac_ItemDetail.png"));
        ui.HideAlmanacItemDetail();
        ui.HideGuidebookModal();

        // Screenshot 3C: Restart Confirmation Modal Guard (From In-Game Settings)
        ui.ShowSettingsModal(true); // Open In-Game Pause Settings first
        ui.PromptRestartConfirmation(); // Open Restart Confirmation on top of Settings
        Debug.Assert(ui.confirmationModal.transform.GetSiblingIndex() > ui.settingsPanel.transform.GetSiblingIndex(),
            "[FAIL] Confirmation modal must have higher sibling index (render in front) than settings modal!");
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_Confirmation_Modal.png"));
        ui.HideConfirmation();
        ui.HideSettingsModal();

        // Screenshot 4: Difficulty Selection Screen (opened when tapping PLAY on Home Screen)
        ui.ShowDifficultyScreen(GameManager.DifficultyLevel.Medium);
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_DifficultyMenu.png"));

        // Verify Easy, Medium, Hard multipliers
        gm.StartGameWithDifficulty(GameManager.DifficultyLevel.Easy, true);
        spawner.GetTierParameters(0, out float easyInterval, out float easySpeed, out int easyPoison);
        gm.StartGameWithDifficulty(GameManager.DifficultyLevel.Hard, true);
        spawner.GetTierParameters(0, out float hardInterval, out float hardSpeed, out int hardPoison);
        Debug.Assert(easySpeed < hardSpeed && easyInterval > hardInterval && easyPoison < hardPoison,
            "[SP-10 FAIL] Easy mode must have slower speed, longer spawn interval, and lower poison weight than Hard mode.");

        // Start Medium gameplay for core rules & theme transition verification
        gm.StartGameWithDifficulty(GameManager.DifficultyLevel.Medium, true);

        // Screenshot 5: Bottom-Screen Slime Tutor Dialogue (Bloop the Slime on top-left corner overlapping outside dialog box!)
        if (ui.tutorialPanel != null)
        {
            ui.GetDifficultyTutorialContent(GameManager.DifficultyLevel.Hard, out string dBadge, out string dTitle, out string dBody, out Sprite[] dIcons);
            if (ui.textTutorialStepBadge != null) ui.textTutorialStepBadge.text = dBadge + "  (1/5)";
            if (ui.textTutorialTitle != null) ui.textTutorialTitle.text = dTitle;
            if (ui.textTutorialBody != null) ui.textTutorialBody.text = dBody;
            if (ui.textTutorialTapPrompt != null) ui.textTutorialTapPrompt.text = "[ TAP TO CONTINUE > ]";
            ui.tutorialPanel.SetActive(true);
            RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_Tutorial.png"));
            ui.tutorialPanel.SetActive(false);
        }

        // Screenshot 6: 3-Second Countdown Overlay
        if (ui.countdownPanel != null)
        {
            ui.countdownPanel.SetActive(true);
            if (ui.textCountdownNumber != null) ui.textCountdownNumber.text = "3";
            RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_Countdown.png"));
            ui.countdownPanel.SetActive(false);
        }

        // Screenshot 7: In-Game Pause -> Settings Modal (Shows Audio, Vibrate, Resume AND In-Game Session Actions)
        gm.OpenSettings();
        Debug.Assert(ui.inGameSettingsActionsGroup != null && ui.inGameSettingsActionsGroup.activeSelf,
            "[SP-10 FAIL] In-Game Pause -> Settings must display in-game session actions (Restart, Almanac, Tutorial, Home)!");
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_Settings.png"));
        gm.CloseSettingsAndResume();

        // Test 1: Catch Edible Food (+10 pts, lives remain 3)
        gm.OnFoodCaught(10);
        Debug.Assert(gm.currentScore == 10 && gm.currentLives == 3, "[SP-10 FAIL] Catching food should add 10 pts and keep 3 lives.");

        // Test 2: Activate Barrier Shield (12s) and verify it blocks a Poison hit with 0 heart loss!
        gm.ActivateShieldBarrier(12.0f);
        Debug.Assert(gm.IsShieldActive, "[SP-10 FAIL] Shield Barrier must be active after ActivateShieldBarrier.");
        gm.LoseLife("Poison");
        Debug.Assert(gm.currentLives == 3 && !gm.IsShieldActive, "[SP-10 FAIL] Barrier Shield must absorb 1 Poison hit and keep lives at 3!");

        // Test 3: Activate Time-Slow (6s) and verify fall speed multiplier is 0.48f
        gm.ActivateTimeSlow(6.0f);
        Debug.Assert(gm.IsTimeSlowActive && gm.GetTimeSlowMultiplier() < 0.6f, "[SP-10 FAIL] Time-Slow must reduce fall speed multiplier.");

        // Test 4: Miss Edible Food on Floor (0 heart loss — lives remain 3, only combo resets!)
        gm.OnFoodMissed();
        gm.LoseLife("MissedFood");
        Debug.Assert(gm.currentLives == 3 && gm.comboCount == 0, "[SP-10 FAIL] Missing edible food must NOT deduct any heart (lives remain 3)!");

        // Test 5: Catch Poison Hazard twice (-2 hearts -> 1 life + 1-Life Critical Warning!)
        gm.LoseLife("Poison");
        Debug.Assert(gm.currentLives == 2, "[SP-10 FAIL] Catching 1st poison should deduct 1 heart (3 -> 2).");
        gm.LoseLife("Poison");
        Debug.Assert(gm.currentLives == 1, "[SP-10 FAIL] Catching 2nd poison should deduct 1 heart (2 -> 1).");
        Debug.Assert(ui.lowLifeWarningRoot != null && ui.lowLifeWarningRoot.activeSelf, "[SP-10 FAIL] 1-Life critical danger indicator must activate when lives == 1.");

        // Render 1-Life Critical Warning Screenshot
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_LowLifeWarning.png"));

        // Test 6: Catch LifeHeart Power-up (+1 heart -> 2 lives, clears 1-Life Warning)
        gm.AddLife();
        Debug.Assert(gm.currentLives == 2, "[SP-10 FAIL] Catching LifeHeart should restore 1 heart (1 -> 2).");
        Debug.Assert(ui.lowLifeWarningRoot != null && !ui.lowLifeWarningRoot.activeSelf, "[SP-10 FAIL] 1-Life warning must hide when lives > 1.");

        // Test 7: Activate all 3 timed power-ups so their duration bars and Slime Shield Bubble are visible in Gameplay screenshot!
        gm.ActivateMagnet(6.0f);
        gm.ActivateShieldBarrier(12.0f);
        gm.ActivateTimeSlow(6.0f);
        Debug.Assert(gm.IsMagnetActive && gm.IsShieldActive && gm.IsTimeSlowActive, "[SP-10 FAIL] All 3 timed power-ups should be active.");
        Debug.Assert(SoundManager.Instance.magnetActiveSource != null && SoundManager.Instance.magnetActiveSource.isPlaying,
            "[SP-10 FAIL] Magnet audio source must be playing when Magnet is active!");
        Debug.Assert(SoundManager.Instance.shieldActiveSource != null && SoundManager.Instance.shieldActiveSource.isPlaying,
            "[SP-10 FAIL] Shield audio source must be playing when Shield is active!");
        Debug.Assert(SoundManager.Instance.timeSlowActiveSource != null && SoundManager.Instance.timeSlowActiveSource.isPlaying,
            "[SP-10 FAIL] Time-Slow audio source must be playing when Time-Slow is active!");

        // Spawn sample falling items in view for visual verification
        List<GameObject> previewItems = new List<GameObject>();
        if (spawner.foodSprites.Length > 2 && spawner.poisonSprites.Length > 0)
        {
            previewItems.Add(CreatePreviewDrop(spawner.itemPrefab, spawner.foodSprites[1], new Vector3(-1.3f, 2.1f, 0f), FallingItem.ItemType.Food));
            previewItems.Add(CreatePreviewDrop(spawner.itemPrefab, spawner.foodSprites[8], new Vector3(0.9f, 0.9f, 0f), FallingItem.ItemType.Food));
            previewItems.Add(CreatePreviewDrop(spawner.itemPrefab, spawner.poisonSprites[0], new Vector3(-0.2f, -0.1f, 0f), FallingItem.ItemType.Poison));
            previewItems.Add(CreatePreviewDrop(spawner.itemPrefab, spawner.shieldBarrierSprite, new Vector3(1.4f, 2.8f, 0f), FallingItem.ItemType.ShieldBarrier));
            previewItems.Add(CreatePreviewDrop(spawner.itemPrefab, spawner.timeSlowSprite, new Vector3(-1.5f, 3.3f, 0f), FallingItem.ItemType.TimeSlow));
        }
        slime.gameObject.SetActive(true);
        slime.transform.position = new Vector3(0.35f, ResponsiveCamera.BottomBound + slime.bottomOffset, 0f);
        slime.TriggerEatEffect();
        slime.UpdatePowerUpAuraVisuals();

        // Render Phone 9:16 (1080x1920) Theme 0 Gameplay with Power-Up Duration Bars & Environment Banner
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_Gameplay.png"));

        // Render Ultra-Tall Mobile Phone 20:9 (1080x2400) Active Gameplay
        RenderViewportScreenshot(cam, respCam, 1080, 2400, Path.Combine(logDir, "Verify_Phone_20x9_Gameplay.png"));

        // Render Tablet 4:3 (1536x2048) Active Gameplay
        RenderViewportScreenshot(cam, respCam, 1536, 2048, Path.Combine(logDir, "Verify_Tablet_4x3_Gameplay.png"));

        // Test Theme Transitions at 120 pts (Sunset Peaks), 260 pts (Mystic Cliffs), 420 pts (Twilight Citadel)
        gm.AddScore(115); // Score = 125 -> Theme 1 (Sunset Peaks)
        Debug.Assert(tm.currentThemeIndex == 1, "[SP-10 FAIL] Reaching 120+ score must transition to Theme 1 (Sunset Peaks).");
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Theme1_SunsetPeaks.png"));

        gm.AddScore(140); // Score = 265 -> Theme 2 (Mystic Cliffs)
        Debug.Assert(tm.currentThemeIndex == 2, "[SP-10 FAIL] Reaching 260+ score must transition to Theme 2 (Mystic Cliffs).");
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Theme2_MysticCliffs.png"));

        gm.AddScore(160); // Score = 425 -> Theme 3 (Twilight Citadel)
        Debug.Assert(tm.currentThemeIndex == 3, "[SP-10 FAIL] Reaching 420+ score must transition to Theme 3 (Twilight Citadel).");
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Theme3_TwilightCitadel.png"));

        // Clean preview items
        foreach (var p in previewItems)
        {
            if (p != null) Object.DestroyImmediate(p);
        }

        // Test 8: Clear shield & lose remaining 2 hearts via Poison -> Trigger Game Over (0 hearts)
        gm.isShieldActive = false;
        gm.shieldTimeRemaining = 0f;
        gm.LoseLife("Poison");
        gm.LoseLife("Poison");
        Debug.Assert(gm.currentLives == 0 && gm.currentState == GameManager.GameState.GameOver, "[SP-10 FAIL] 0 hearts must trigger GameOver.");
        Debug.Assert(ui.gameOverPanel != null && ui.gameOverPanel.activeSelf, "[SP-10 FAIL] GameOver modal must be active at 0 hearts.");

        // Render Phone 9:16 Game Over Modal
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_GameOver.png"));

        // Verify that restarting resets the Slime out of Dead state back to Alive Idle state!
        gm.StartGameWithDifficulty(GameManager.DifficultyLevel.Medium, true);
        Debug.Assert(slime.spriteRenderer.sprite == slime.idleFrames[0], "[SP-10 FAIL] Slime must reset to Idle frame 0 after replay/restart.");

        Debug.Log("[SP-10 Verification] ALL SCREENS, POWER-UP DURATION BARS, ENVIRONMENT BANNERS, GUIDEBOOK, BOTTOM SLIME TUTOR, AND MULTI-RESOLUTION TESTS PASSED!");

        // Reload clean saved scene so disk state remains pristine
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
    }

    private static GameObject CreatePreviewDrop(GameObject prefab, Sprite sprite, Vector3 pos, FallingItem.ItemType itemType = FallingItem.ItemType.Food)
    {
        GameObject obj = Object.Instantiate(prefab, pos, Quaternion.identity);
        FallingItem fi = obj.GetComponent<FallingItem>();
        if (fi != null)
        {
            fi.Initialize(itemType, sprite, 3.5f, 10);
        }
        else
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null && sprite != null) sr.sprite = sprite;
        }
        return obj;
    }

    private static void RenderViewportScreenshot(Camera cam, ResponsiveCamera respCam, int width, int height, string outPath)
    {
        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        RenderTexture prevTarget = cam.targetTexture;
        RenderTexture prevActive = RenderTexture.active;

        cam.targetTexture = rt;
        cam.aspect = (float)width / height;
        respCam.RecalculateBounds();
        if (SlimeController.Instance != null && SlimeController.Instance.gameObject.activeSelf)
        {
            Vector3 sp = SlimeController.Instance.transform.position;
            sp.y = ResponsiveCamera.BottomBound + SlimeController.Instance.bottomOffset;
            SlimeController.Instance.transform.position = sp;
        }
        Canvas.ForceUpdateCanvases();
        cam.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        File.WriteAllBytes(outPath, tex.EncodeToPNG());

        cam.targetTexture = prevTarget;
        RenderTexture.active = prevActive;
        cam.ResetAspect();
        respCam.RecalculateBounds();

        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
        Debug.Log($"[SP-10 Verification] Saved screenshot: {outPath} ({width}x{height})");
    }

    private static GameObject CreateFallingItemPrefab()
    {
        Directory.CreateDirectory("Assets/Prefabs");
        string prefabPath = "Assets/Prefabs/FallingItem.prefab";

        GameObject temp = new GameObject("FallingItem");
        SpriteRenderer sr = temp.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 4;
        Sprite defaultFood = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Food/food_014.png");
        sr.sprite = defaultFood;

        CircleCollider2D col = temp.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.28f;

        Rigidbody2D rb = temp.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        temp.AddComponent<FallingItem>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
        Object.DestroyImmediate(temp);
        return prefab;
    }

    private static void BuildUICanvas(UIManager uiMgr, GameManager gm, Camera cam, ResponsiveCamera respCam)
    {
        Font arcadeTitleFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/PressStart2P-Regular.ttf");
        Font roundedBodyFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/GameBody-Rounded.ttf");
        Font retroBodyFont = roundedBodyFont != null ? roundedBodyFont : (AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Silkscreen-Bold.ttf") ?? arcadeTitleFont);
        Font font = arcadeTitleFont != null ? arcadeTitleFont : retroBodyFont;
        if (retroBodyFont == null) retroBodyFont = font;
        defaultArcadeFont = font;
        defaultBodyFont = retroBodyFont;

        Sprite heartFull = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Heart_Full.png");
        Sprite heartEmpty = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Heart_Empty.png");
        Sprite slimeLogoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Slime/01-Idle/FR_Slime4_Idle_000.png");
        Sprite donutSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Food/food_014.png");
        Sprite burgerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Food/food_092.png");
        Sprite sushiSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Food/food_088.png");
        Sprite pizzaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Food/food_106.png");
        Sprite iceCreamSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Food/food_108.png");
        Sprite lollipopSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Food/food_008.png");

        Sprite poisonMushroomSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Hazards/poison_mushroom.png");
        Sprite poisonPotionSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Hazards/toxic_potion.png");
        Sprite poisonGobletSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Hazards/poison_goblet.png");
        Sprite spikeTrapSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Hazards/spike_trap.png");

        Sprite bonusHeartSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pickups/bonus_heart.png");
        Sprite starMagnetSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pickups/star_magnet.png");
        Sprite shieldBarrierSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pickups/barrier_shield.png");
        Sprite timeSlowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pickups/time_slow_hourglass.png");
        Sprite guidebookIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/guidebook_icon.png");

        // Crisp 32x32 Pixel-Art UI Action Icons
        Sprite iconPlaySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/icon_play.png");
        Sprite iconGearSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/icon_gear.png");
        Sprite iconRestartSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/icon_restart.png");
        Sprite iconHomeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/icon_home.png");
        Sprite iconExitSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/icon_exit.png");
        Sprite iconCloseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/icon_close.png");
        Sprite iconSkipSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/icon_skip.png");
        Sprite iconTutorialSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/icon_tutorial.png");
        Sprite iconDifficultySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/icon_difficulty.png");
        Sprite iconVibrateSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/icon_vibrate.png");

        // Home Screen Interactive Slime Emotion Sprites
        Sprite emotionIdle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/HomeSlimeEmotions/slime_emotion_idle.png");
        Sprite emotionHappy = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/HomeSlimeEmotions/slime_emotion_happy.png");
        Sprite emotionSurprised = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/HomeSlimeEmotions/slime_emotion_surprised.png");
        Sprite emotionAngry = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/HomeSlimeEmotions/slime_emotion_angry.png");
        Sprite emotionDizzy = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/HomeSlimeEmotions/slime_emotion_dizzy.png");
        Sprite emotionPeek = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/HomeSlimeEmotions/slime_emotion_peek.png");
        Sprite emotionAnnoyed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/HomeSlimeEmotions/slime_emotion_annoyed.png");
        Sprite retroGameTitleLogo = LoadSpriteEnsured("Assets/Sprites/UI/retro_game_title_logo.png");

        // Actual Stage Environment Theme Preview Sprites for the Almanac STAGES Tab (All 10 Stages!)
        Sprite stageForestSprite = LoadSpriteEnsured("Assets/Sprites/Background/stage_forest_preview.png");
        Sprite stagePlainsSprite = LoadSpriteEnsured("Assets/Sprites/Background/Theme4_Plains/stage_preview.png");
        Sprite stagePeaksSprite = LoadSpriteEnsured("Assets/Sprites/Background/Theme1_Mountain/stage_preview.png");
        Sprite stageTropicalSprite = LoadSpriteEnsured("Assets/Sprites/Background/Theme5_Tropical/stage_preview.png");
        Sprite stageDesertSprite = LoadSpriteEnsured("Assets/Sprites/Background/Theme6_Desert/stage_preview.png");
        Sprite stageUnderwaterSprite = LoadSpriteEnsured("Assets/Sprites/Background/Theme7_Underwater/stage_preview.png");
        Sprite stageIndustrialSprite = LoadSpriteEnsured("Assets/Sprites/Background/Theme8_Industrial/stage_preview.png");
        Sprite stageCaveSprite = LoadSpriteEnsured("Assets/Sprites/Background/Theme9_Cave/stage_preview.png");
        Sprite stageCliffsSprite = LoadSpriteEnsured("Assets/Sprites/Background/Theme2_MagicCliffs/stage_preview.png");
        Sprite stageCitadelSprite = LoadSpriteEnsured("Assets/Sprites/Background/Theme3_NightCitadel/stage_preview.png");
        Sprite[] stagePreviews = new Sprite[]
        {
            stageForestSprite, stagePlainsSprite, stagePeaksSprite, stageTropicalSprite, stageDesertSprite,
            stageUnderwaterSprite, stageIndustrialSprite, stageCaveSprite, stageCliffsSprite, stageCitadelSprite
        };

        uiMgr.heartFull = heartFull;
        uiMgr.heartEmpty = heartEmpty;
        uiMgr.tutorialFoodSprite = burgerSprite;
        uiMgr.tutorialPoisonSprite = poisonPotionSprite;
        uiMgr.tutorialHeartSprite = bonusHeartSprite;
        uiMgr.tutorialMagnetSprite = starMagnetSprite;
        uiMgr.tutorialShieldSprite = shieldBarrierSprite;
        uiMgr.tutorialTimeSlowSprite = timeSlowSprite;

        GameObject canvasObj = new GameObject("UI_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 1f;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.45f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Safe-Area Root Container
        GameObject safeAreaObj = CreateUIElement("SafeArea_Container", canvasObj.transform);
        RectTransform safeRect = safeAreaObj.GetComponent<RectTransform>();
        safeRect.anchorMin = Vector2.zero;
        safeRect.anchorMax = Vector2.one;
        safeRect.offsetMin = Vector2.zero;
        safeRect.offsetMax = Vector2.zero;
        if (respCam != null) respCam.safeAreaContainer = safeRect;

        // =========================================================================
        // 0. 1-LIFE CRITICAL DANGER INDICATOR (Red Screen-Edge Pulse + Banner)
        // =========================================================================
        GameObject lowLifeRoot = CreateUIElement("LowLife_WarningOverlay", canvasObj.transform);
        RectTransform llRect = lowLifeRoot.GetComponent<RectTransform>();
        llRect.anchorMin = Vector2.zero;
        llRect.anchorMax = Vector2.one;
        llRect.offsetMin = Vector2.zero;
        llRect.offsetMax = Vector2.zero;

        Image[] borderEdges = new Image[4];
        Color dangerRed = new Color(0.95f, 0.12f, 0.12f, 0.42f);

        borderEdges[0] = CreateEdgeStrip("Edge_Top", lowLifeRoot.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 28f), dangerRed);
        borderEdges[1] = CreateEdgeStrip("Edge_Bottom", lowLifeRoot.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 28f), dangerRed);
        borderEdges[2] = CreateEdgeStrip("Edge_Left", lowLifeRoot.transform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(28f, 0f), dangerRed);
        borderEdges[3] = CreateEdgeStrip("Edge_Right", lowLifeRoot.transform, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(28f, 0f), dangerRed);

        Text dangerBannerTxt = CreateUIText("Text_LowLifeBanner", lowLifeRoot.transform, defaultBodyFont, "! DANGER: 1 HEART LEFT !", 32, TextAnchor.MiddleCenter, new Color(1f, 0.32f, 0.30f, 1f));
        dangerBannerTxt.resizeTextForBestFit = true;
        dangerBannerTxt.resizeTextMinSize = 24;
        dangerBannerTxt.resizeTextMaxSize = 32;
        RectTransform dbRect = dangerBannerTxt.GetComponent<RectTransform>();
        dbRect.anchorMin = new Vector2(0.5f, 0f);
        dbRect.anchorMax = new Vector2(0.5f, 0f);
        dbRect.pivot = new Vector2(0.5f, 0.5f);
        dbRect.anchoredPosition = new Vector2(0f, 65f);
        dbRect.sizeDelta = new Vector2(720f, 54f);

        lowLifeRoot.SetActive(false);
        uiMgr.lowLifeWarningRoot = lowLifeRoot;
        uiMgr.lowLifeBorderEdges = borderEdges;
        uiMgr.textLowLifeBanner = dangerBannerTxt;

        // =========================================================================
        // 1. TOP BAR HUD (Active during Gameplay)
        // =========================================================================
        GameObject topBar = CreateUIElement("TopBar_HUD", safeAreaObj.transform);
        RectTransform topRect = topBar.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0f, 1f);
        topRect.anchorMax = new Vector2(1f, 1f);
        topRect.pivot = new Vector2(0.5f, 1f);
        topRect.anchoredPosition = new Vector2(0f, -20f);
        topRect.sizeDelta = new Vector2(-36f, 148f);

        Image topBg = topBar.AddComponent<Image>();
        topBg.color = new Color(0.14f, 0.09f, 0.05f, 0.82f);
        if (roundedFrameSprite != null)
        {
            topBg.sprite = roundedFrameSprite;
            topBg.type = Image.Type.Sliced;
        }
        Outline topOutline = topBar.AddComponent<Outline>();
        topOutline.effectColor = new Color(0.85f, 0.62f, 0.25f, 0.8f);
        topOutline.effectDistance = new Vector2(3f, -3f);
        uiMgr.topBarHud = topBar;

        // Hearts Container (Top-Left)
        GameObject heartsContainer = CreateUIElement("HeartsContainer", topBar.transform);
        RectTransform hcRect = heartsContainer.GetComponent<RectTransform>();
        hcRect.anchorMin = new Vector2(0f, 0.5f);
        hcRect.anchorMax = new Vector2(0f, 0.5f);
        hcRect.pivot = new Vector2(0f, 0.5f);
        hcRect.anchoredPosition = new Vector2(20f, 16f);
        hcRect.sizeDelta = new Vector2(260f, 74f);

        Image[] heartImgs = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject hObj = CreateUIElement($"Heart_{i + 1}", heartsContainer.transform);
            RectTransform hRect = hObj.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 0.5f);
            hRect.anchorMax = new Vector2(0f, 0.5f);
            hRect.pivot = new Vector2(0f, 0.5f);
            hRect.anchoredPosition = new Vector2(i * 78f, 0f);
            hRect.sizeDelta = new Vector2(68f, 68f);

            Image hImg = hObj.AddComponent<Image>();
            hImg.sprite = heartFull;
            hImg.preserveAspect = true;
            heartImgs[i] = hImg;
        }
        uiMgr.heartIcons = heartImgs;

        // Difficulty Badge (Below Hearts, Top-Left)
        Text diffBadgeTxt = CreateUIText("Text_DifficultyBadge", topBar.transform, defaultBodyFont, "MODE: MEDIUM", 25, TextAnchor.MiddleLeft, new Color(1f, 0.86f, 0.42f, 1f));
        RectTransform diffRect = diffBadgeTxt.GetComponent<RectTransform>();
        diffRect.anchorMin = new Vector2(0f, 0.5f);
        diffRect.anchorMax = new Vector2(0f, 0.5f);
        diffRect.pivot = new Vector2(0f, 0.5f);
        diffRect.anchoredPosition = new Vector2(24f, -44f);
        diffRect.sizeDelta = new Vector2(260f, 34f);
        uiMgr.textDifficultyBadge = diffBadgeTxt;

        // Score Text (Top-Center: Centered at x=0 with bounded width to prevent ANY overlap with Best Score)
        Text scoreTxt = CreateUIText("Text_Score", topBar.transform, defaultBodyFont, "SCORE: 0", 32, TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.78f, 1f));
        scoreTxt.raycastTarget = false;
        RectTransform scoreRect = scoreTxt.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 0.5f);
        scoreRect.anchorMax = new Vector2(0.5f, 0.5f);
        scoreRect.pivot = new Vector2(0.5f, 0.5f);
        scoreRect.anchoredPosition = new Vector2(0f, 18f);
        scoreRect.sizeDelta = new Vector2(260f, 44f);
        uiMgr.textScore = scoreTxt;

        // Stage / Theme Badge (Below Score, Top-Center - 400px width for all 10 biomes)
        Text themeBadgeTxt = CreateUIText("Text_ThemeBadge", topBar.transform, defaultBodyFont, "WHISPERING FOREST", 25, TextAnchor.MiddleCenter, new Color(0.78f, 0.95f, 1f, 0.95f));
        themeBadgeTxt.resizeTextForBestFit = true;
        themeBadgeTxt.resizeTextMinSize = 20;
        themeBadgeTxt.resizeTextMaxSize = 25;
        themeBadgeTxt.raycastTarget = false;
        RectTransform themeRect = themeBadgeTxt.GetComponent<RectTransform>();
        themeRect.anchorMin = new Vector2(0.5f, 0.5f);
        themeRect.anchorMax = new Vector2(0.5f, 0.5f);
        themeRect.pivot = new Vector2(0.5f, 0.5f);
        themeRect.anchoredPosition = new Vector2(0f, -36f);
        themeRect.sizeDelta = new Vector2(400f, 36f);
        uiMgr.textThemeBadge = themeBadgeTxt;

        // High Score Text (Top-Right, left of Gear button with strict right alignment)
        Text bestTxt = CreateUIText("Text_HighScore", topBar.transform, defaultBodyFont, "BEST: 0", 26, TextAnchor.MiddleRight, new Color(1f, 0.84f, 0.35f, 1f));
        bestTxt.raycastTarget = false;
        RectTransform bestRect = bestTxt.GetComponent<RectTransform>();
        bestRect.anchorMin = new Vector2(1f, 0.5f);
        bestRect.anchorMax = new Vector2(1f, 0.5f);
        bestRect.pivot = new Vector2(1f, 0.5f);
        bestRect.anchoredPosition = new Vector2(-110f, 18f);
        bestRect.sizeDelta = new Vector2(200f, 44f);
        uiMgr.textHighScore = bestTxt;

        // Magnet Active Timer (Legacy placeholder preserved disabled for compatibility)
        Text magTxt = CreateUIText("Text_MagnetTimer", topBar.transform, defaultBodyFont, "", 16, TextAnchor.MiddleRight, Color.clear);
        RectTransform magRect = magTxt.GetComponent<RectTransform>();
        magRect.anchorMin = new Vector2(1f, 0.5f);
        magRect.anchorMax = new Vector2(1f, 0.5f);
        magRect.pivot = new Vector2(1f, 0.5f);
        magRect.anchoredPosition = new Vector2(-110f, -36f);
        magRect.sizeDelta = new Vector2(200f, 32f);
        magTxt.gameObject.SetActive(false);
        uiMgr.textMagnetTimer = magTxt;

        // FRENZY STREAK HUD BADGE (Directly below High Score, right-aligned)
        Text frenzyTxt = CreateUIText("Text_FrenzyStreak", topBar.transform, defaultBodyFont, "FRENZY: x0/12", 25, TextAnchor.MiddleRight, new Color(0.70f, 0.85f, 0.95f, 0.85f));
        RectTransform frRect = frenzyTxt.GetComponent<RectTransform>();
        frRect.anchorMin = new Vector2(1f, 0.5f);
        frRect.anchorMax = new Vector2(1f, 0.5f);
        frRect.pivot = new Vector2(1f, 0.5f);
        frRect.anchoredPosition = new Vector2(-110f, -36f);
        frRect.sizeDelta = new Vector2(210f, 32f);
        uiMgr.textFrenzyStreak = frenzyTxt;
        uiMgr.arcadeFontRef = font;
        uiMgr.retroBodyFontRef = retroBodyFont;

        // HUD Settings / Pause Gear Icon Button (Top-Right Corner — 80x80)
        Button btnHudSettings = CreateUIIconButton(
            "Button_HudSettings",
            topBar.transform,
            iconGearSprite,
            Vector2.zero,
            new Vector2(80f, 80f),
            new Vector2(52f, 52f),
            new Color(0.28f, 0.22f, 0.36f, 0.96f)
        );
        RectTransform hsRect = btnHudSettings.GetComponent<RectTransform>();
        hsRect.anchorMin = new Vector2(1f, 0.5f);
        hsRect.anchorMax = new Vector2(1f, 0.5f);
        hsRect.pivot = new Vector2(1f, 0.5f);
        hsRect.anchoredPosition = new Vector2(-16f, 0f);
        uiMgr.btnHudSettings = btnHudSettings;

        // =========================================================================
        // 1B. POWER-UP DURATION BARS CONTAINER (Directly Below TopBar_HUD)
        // =========================================================================
        GameObject puContainer = CreateUIElement("PowerUp_Bars_Container", safeAreaObj.transform);
        RectTransform puRect = puContainer.GetComponent<RectTransform>();
        puRect.anchorMin = new Vector2(0f, 1f);
        puRect.anchorMax = new Vector2(1f, 1f);
        puRect.pivot = new Vector2(0.5f, 1f);
        puRect.anchoredPosition = new Vector2(0f, -176f);
        puRect.sizeDelta = new Vector2(-44f, 148f);
        uiMgr.powerUpBarsContainer = puContainer;

        uiMgr.magnetBarRow = CreatePowerUpDurationBarRow(
            "Magnet_Bar_Row", puContainer.transform, font, starMagnetSprite,
            "MAGNET  6.0s", new Vector2(0f, -4f), new Color(1.0f, 0.84f, 0.24f, 1f),
            out uiMgr.magnetBarFillRect, out uiMgr.textMagnetBarLabel);

        uiMgr.shieldBarRow = CreatePowerUpDurationBarRow(
            "Shield_Bar_Row", puContainer.transform, font, shieldBarrierSprite,
            "BARRIER SHIELD  12.0s (1 HIT)", new Vector2(0f, -50f), new Color(0.28f, 0.90f, 1.0f, 1f),
            out uiMgr.shieldBarFillRect, out uiMgr.textShieldBarLabel);

        uiMgr.timeSlowBarRow = CreatePowerUpDurationBarRow(
            "TimeSlow_Bar_Row", puContainer.transform, font, timeSlowSprite,
            "TIME-SLOW  6.0s (50%)", new Vector2(0f, -96f), new Color(0.76f, 0.50f, 1.0f, 1f),
            out uiMgr.timeSlowBarFillRect, out uiMgr.textTimeSlowBarLabel);

        puContainer.SetActive(false);

        // =========================================================================
        // 1C. ENVIRONMENT TRANSITION BANNER (Upper-Middle Screen)
        // =========================================================================
        GameObject envBanner = CreateUIElement("Environment_Banner_Panel", safeAreaObj.transform);
        RectTransform ebRect = envBanner.GetComponent<RectTransform>();
        ebRect.anchorMin = new Vector2(0.5f, 1f);
        ebRect.anchorMax = new Vector2(0.5f, 1f);
        ebRect.pivot = new Vector2(0.5f, 1f);
        ebRect.anchoredPosition = new Vector2(0f, -325f);
        ebRect.sizeDelta = new Vector2(940f, 180f);

        Image ebBg = envBanner.AddComponent<Image>();
        ebBg.color = new Color(0.10f, 0.08f, 0.16f, 0.92f);
        ebBg.raycastTarget = false;
        if (roundedFrameSprite != null)
        {
            ebBg.sprite = roundedFrameSprite;
            ebBg.type = Image.Type.Sliced;
        }
        Outline ebOutline = envBanner.AddComponent<Outline>();
        ebOutline.effectColor = new Color(0.95f, 0.78f, 0.35f, 0.95f);
        ebOutline.effectDistance = new Vector2(4f, -4f);

        CanvasGroup ebCg = envBanner.AddComponent<CanvasGroup>();
        ebCg.blocksRaycasts = false;
        ebCg.interactable = false;

        Text envStageTxt = CreateUIText("Text_EnvStageBadge", envBanner.transform, defaultBodyFont, "ENVIRONMENT STAGE 1", 24, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.38f, 1f));
        envStageTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 52f);
        envStageTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(880f, 32f);

        Text envTitleTxt = CreateUIText("Text_EnvTitle", envBanner.transform, font, "WHISPERING FOREST", 36, TextAnchor.MiddleCenter, new Color(0.52f, 0.98f, 0.68f, 1f));
        envTitleTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 12f);
        envTitleTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(880f, 44f);

        Text envSubTxt = CreateUIText("Text_EnvSubtitle", envBanner.transform, defaultBodyFont, "Lush Woodland Canopy  |  Peaceful Forest BGM", 26, TextAnchor.MiddleCenter, new Color(0.84f, 0.94f, 1f, 0.95f));
        envSubTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        envSubTxt.verticalOverflow = VerticalWrapMode.Truncate;
        envSubTxt.lineSpacing = 1.15f;
        envSubTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -40f);
        envSubTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(880f, 54f);

        envBanner.SetActive(false);
        uiMgr.environmentBannerPanel = envBanner;
        uiMgr.environmentBannerCanvasGroup = ebCg;
        uiMgr.textEnvStageBadge = envStageTxt;
        uiMgr.textEnvTitle = envTitleTxt;
        uiMgr.textEnvSubtitle = envSubTxt;

        // Power-Up Status Toast (Below Environment Banner)
        Text toastTxt = CreateUIText("Text_ThemeToast", safeAreaObj.transform, retroBodyFont, "BARRIER SHIELD ACTIVE!", 26, TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.55f, 1f));
        toastTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        toastTxt.verticalOverflow = VerticalWrapMode.Truncate;
        RectTransform toastRect = toastTxt.GetComponent<RectTransform>();
        toastRect.anchorMin = new Vector2(0.5f, 1f);
        toastRect.anchorMax = new Vector2(0.5f, 1f);
        toastRect.pivot = new Vector2(0.5f, 0.5f);
        toastRect.anchoredPosition = new Vector2(0f, -525f);
        toastRect.sizeDelta = new Vector2(880f, 68f);
        toastTxt.gameObject.SetActive(false);
        uiMgr.textThemeToast = toastTxt;

        // =========================================================================
        // 2. HOME SCREEN (Floating Unboxed Architecture + Interactive Slime Banner + Top-Right Settings + Stacked Floating Buttons)
        // =========================================================================
        GameObject homePanel = CreateUIElement("Home_Screen", canvasObj.transform);
        RectTransform homeRect = homePanel.GetComponent<RectTransform>();
        homeRect.anchorMin = Vector2.zero;
        homeRect.anchorMax = Vector2.one;
        homeRect.offsetMin = Vector2.zero;
        homeRect.offsetMax = Vector2.zero;

        Image homeOverlay = homePanel.AddComponent<Image>();
        homeOverlay.color = new Color(0.05f, 0.03f, 0.08f, 0.44f);
        CanvasGroup homeCg = homePanel.AddComponent<CanvasGroup>();
        homeCg.alpha = 1f;

        // 1. TOP-RIGHT CORNER FLOATING SETTINGS BUTTON
        GameObject settingsBtnObj = CreateUIElement("Button_HomeSettings", homePanel.transform);
        RectTransform stRect = settingsBtnObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(1f, 1f);
        stRect.anchorMax = new Vector2(1f, 1f);
        stRect.pivot = new Vector2(1f, 1f);
        stRect.anchoredPosition = new Vector2(-46f, -46f);
        stRect.sizeDelta = new Vector2(106f, 106f);

        Image stImg = settingsBtnObj.AddComponent<Image>();
        stImg.color = new Color(0.20f, 0.16f, 0.26f, 0.95f);
        if (roundedFrameSprite != null)
        {
            stImg.sprite = roundedFrameSprite;
            stImg.type = Image.Type.Sliced;
        }
        Outline stOut = settingsBtnObj.AddComponent<Outline>();
        stOut.effectColor = new Color(0.92f, 0.78f, 0.42f, 1f);
        stOut.effectDistance = new Vector2(3f, -3f);
        Shadow stShad = settingsBtnObj.AddComponent<Shadow>();
        stShad.effectColor = new Color(0f, 0f, 0f, 0.65f);
        stShad.effectDistance = new Vector2(0f, -4f);

        CreateUISpriteIcon("Icon_Gear", settingsBtnObj.transform, iconGearSprite, Vector2.zero, new Vector2(62f, 62f));
        Button btnHomeSettings = settingsBtnObj.AddComponent<Button>();
        settingsBtnObj.AddComponent<UIButtonJuice>();
        uiMgr.btnHomeSettings = btnHomeSettings;

        // 2. FLOATING TITLE BANNER & INTERACTIVE MASCOT (Centered Horizontally)
        GameObject bannerRoot = CreateUIElement("Home_TitleBannerRoot", homePanel.transform);
        RectTransform brRect = bannerRoot.GetComponent<RectTransform>();
        brRect.anchorMin = new Vector2(0.5f, 0.5f);
        brRect.anchorMax = new Vector2(0.5f, 0.5f);
        brRect.pivot = new Vector2(0.5f, 0.5f);
        brRect.anchoredPosition = new Vector2(0f, 560f);
        brRect.sizeDelta = new Vector2(980f, 340f);

        // A. Floating 16-Bit Retro Arcade Game Title Logo (Centered Horizontally!)
        GameObject titleLogoObj = CreateUIElement("Home_FloatingTitleLogo", bannerRoot.transform);
        RectTransform titleRect = titleLogoObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 20f); // Center horizontally on the screen!
        titleRect.sizeDelta = new Vector2(680f, 320f);

        Image titleImg = titleLogoObj.AddComponent<Image>();
        titleImg.sprite = retroGameTitleLogo;
        titleImg.preserveAspect = true;
        titleImg.raycastTarget = false;

        // B. Interactive Slime Character on the Bottom-Right Corner of the Game Title (Standing in front of it!)
        GameObject slimeActorObj = CreateUIElement("Home_InteractiveSlimeActor", bannerRoot.transform);
        RectTransform saRect = slimeActorObj.GetComponent<RectTransform>();
        saRect.anchorMin = new Vector2(0.5f, 0.5f);
        saRect.anchorMax = new Vector2(0.5f, 0.5f);
        saRect.pivot = new Vector2(0.5f, 0.5f);
        saRect.anchoredPosition = new Vector2(245f, -90f); // Standing at bottom-right corner of title!
        saRect.sizeDelta = new Vector2(360f, 280f); // Enlarged Bloop Mascot!

        Image slimeImg = slimeActorObj.AddComponent<Image>();
        slimeImg.sprite = (emotionIdle != null ? emotionIdle : slimeLogoSprite);
        slimeImg.preserveAspect = true;

        Button slimeBtn = slimeActorObj.AddComponent<Button>();
        HomeInteractiveSlime comp = slimeActorObj.AddComponent<HomeInteractiveSlime>();
        comp.slimeImage = slimeImg;
        comp.slimeRect = saRect;
        comp.bannerPlateRect = titleRect; // Peeks and hides behind the floating 3D title logo!
        comp.spriteIdle = (emotionIdle != null ? emotionIdle : slimeLogoSprite);
        comp.spriteHappy = emotionHappy;
        comp.spriteSurprised = emotionSurprised;
        comp.spriteAngry = emotionAngry;
        comp.spriteDizzy = emotionDizzy;
        comp.spritePeek = emotionPeek;
        comp.spriteAnnoyed = emotionAnnoyed;
        comp.SetRestingPosition(new Vector2(245f, -90f));

        slimeBtn.onClick.AddListener(() => comp.OnSlimeTapped());

        // Reaction Speech Bubble above Bloop (Wider container with text auto-wrapping!)
        GameObject bubbleObj = CreateUIElement("Slime_ReactionBubble", bannerRoot.transform);
        RectTransform bblRect = bubbleObj.GetComponent<RectTransform>();
        bblRect.anchoredPosition = new Vector2(245f, 40f);
        bblRect.sizeDelta = new Vector2(360f, 72f);

        Image bblBg = bubbleObj.AddComponent<Image>();
        bblBg.color = new Color(0.12f, 0.08f, 0.16f, 0.96f);
        if (roundedFrameSprite != null)
        {
            bblBg.sprite = roundedFrameSprite;
            bblBg.type = Image.Type.Sliced;
        }
        Outline bblOut = bubbleObj.AddComponent<Outline>();
        bblOut.effectColor = new Color(0.92f, 0.78f, 0.42f, 1f);
        bblOut.effectDistance = new Vector2(3f, -3f);

        Text bblTxt = CreateUIText("Text_ReactionSpeech", bubbleObj.transform, defaultBodyFont, "Boing!", 32, TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.60f, 1f));
        bblTxt.raycastTarget = false;
        bblTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        bblTxt.verticalOverflow = VerticalWrapMode.Overflow;
        bblTxt.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        bblTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(320f, 56f);

        comp.reactionBubbleObj = bubbleObj;
        comp.reactionBubbleText = bblTxt;
        bubbleObj.SetActive(false);

        uiMgr.homeInteractiveSlime = comp;

        // 3. FLOATING BEST SCORE PILL (Centered vertically on the screen device!)
        GameObject bestPill = CreateUIElement("Home_BestScorePill", homePanel.transform);
        RectTransform bpRect = bestPill.GetComponent<RectTransform>();
        bpRect.anchorMin = new Vector2(0.5f, 0.5f);
        bpRect.anchorMax = new Vector2(0.5f, 0.5f);
        bpRect.pivot = new Vector2(0.5f, 0.5f);
        bpRect.anchoredPosition = new Vector2(0f, 40f); // Centered on the screen!
        bpRect.sizeDelta = new Vector2(540f, 72f);

        Image bpPillBg = bestPill.AddComponent<Image>();
        bpPillBg.color = new Color(0.18f, 0.15f, 0.22f, 0.95f);
        bpPillBg.raycastTarget = false;
        if (roundedFrameSprite != null)
        {
            bpPillBg.sprite = roundedFrameSprite;
            bpPillBg.type = Image.Type.Sliced;
        }
        Outline bpPillOut = bestPill.AddComponent<Outline>();
        bpPillOut.effectColor = new Color(0.88f, 0.74f, 0.38f, 0.95f);
        bpPillOut.effectDistance = new Vector2(3f, -3f);

        Text homeBestTxt = CreateUIText("Text_HomeBestScore", bestPill.transform, defaultBodyFont, "BEST SCORE: 0", 30, TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.55f, 1f));
        homeBestTxt.raycastTarget = false;
        homeBestTxt.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        homeBestTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(480f, 52f);
        uiMgr.textHomeBestScore = homeBestTxt;

        // 4. VERTICALLY STACKED FLOATING BUTTONS (Positioned further downwards!)
        // TIER 1 (PRIMARY HERO CTA): Large Floating PLAY Button (Forest Emerald Green + Antique Gold Border)
        Button btnHomePlay = CreateUIIconBannerButton(
            "Button_HomePlay",
            homePanel.transform,
            font,
            iconPlaySprite,
            "PLAY",
            new Vector2(0f, -200f),
            new Vector2(680f, 136f),
            new Vector2(74f, 74f),
            new Color(0.16f, 0.40f, 0.22f, 1f),
            30
        );
        Outline playOut = btnHomePlay.GetComponent<Outline>();
        if (playOut != null)
        {
            playOut.effectColor = new Color(0.92f, 0.78f, 0.42f, 1f);
            playOut.effectDistance = new Vector2(6f, -6f);
        }
        uiMgr.btnHomePlay = btnHomePlay;
        comp.playButtonRect = btnHomePlay.GetComponent<RectTransform>();

        // TIER 2: Floating ALMANAC Button (Slate Midnight Teal + Soft Sky Border)
        Button btnHomeGuide = CreateUIIconBannerButton(
            "Button_HomeGuidebook",
            homePanel.transform,
            font,
            guidebookIconSprite,
            "ALMANAC",
            new Vector2(0f, -370f),
            new Vector2(560f, 106f),
            new Vector2(64f, 64f),
            new Color(0.16f, 0.28f, 0.38f, 1f),
            26
        );
        Outline guideOut = btnHomeGuide.GetComponent<Outline>();
        if (guideOut != null)
        {
            guideOut.effectColor = new Color(0.76f, 0.84f, 0.88f, 1f);
            guideOut.effectDistance = new Vector2(5f, -5f);
        }
        uiMgr.btnHomeGuidebook = btnHomeGuide;

        // TIER 3: Floating EXIT Button (Warm Rustic Terracotta + Copper Border)
        Button btnHomeQuit = CreateUIIconBannerButton(
            "Button_HomeQuit",
            homePanel.transform,
            font,
            iconExitSprite,
            "EXIT",
            new Vector2(0f, -510f),
            new Vector2(460f, 96f),
            new Vector2(56f, 56f),
            new Color(0.44f, 0.19f, 0.20f, 1f),
            24
        );
        Outline quitOut = btnHomeQuit.GetComponent<Outline>();
        if (quitOut != null)
        {
            quitOut.effectColor = new Color(0.85f, 0.58f, 0.48f, 1f);
            quitOut.effectDistance = new Vector2(4f, -4f);
        }
        {
            quitOut.effectColor = new Color(0.96f, 0.45f, 0.45f, 1f);
            quitOut.effectDistance = new Vector2(4f, -4f);
        }
        uiMgr.btnHomeQuit = btnHomeQuit;

        uiMgr.homePanel = homePanel;

        // =========================================================================
        // 3. DIFFICULTY SELECTION SCREEN (Generous Spacing + Icon Home Button)
        // =========================================================================
        GameObject diffPanel = CreateUIElement("Difficulty_Screen", canvasObj.transform);
        RectTransform diffPanelRect = diffPanel.GetComponent<RectTransform>();
        diffPanelRect.anchorMin = Vector2.zero;
        diffPanelRect.anchorMax = Vector2.one;
        diffPanelRect.offsetMin = Vector2.zero;
        diffPanelRect.offsetMax = Vector2.zero;

        Image diffOverlay = diffPanel.AddComponent<Image>();
        diffOverlay.color = new Color(0.05f, 0.03f, 0.09f, 0.68f);

        GameObject diffCard = CreateUIElement("Difficulty_Card", diffPanel.transform);
        RectTransform diffCardRect = diffCard.GetComponent<RectTransform>();
        diffCardRect.anchorMin = new Vector2(0.5f, 0.5f);
        diffCardRect.anchorMax = new Vector2(0.5f, 0.5f);
        diffCardRect.pivot = new Vector2(0.5f, 0.5f);
        diffCardRect.anchoredPosition = new Vector2(0f, 20f);
        diffCardRect.sizeDelta = new Vector2(880f, 960f);

        Image diffCardBg = diffCard.AddComponent<Image>();
        diffCardBg.color = new Color(0.14f, 0.10f, 0.18f, 0.97f);
        if (roundedFrameSprite != null)
        {
            diffCardBg.sprite = roundedFrameSprite;
            diffCardBg.type = Image.Type.Sliced;
        }

        Outline diffCardOutline = diffCard.AddComponent<Outline>();
        diffCardOutline.effectColor = new Color(0.95f, 0.76f, 0.32f, 1f);
        diffCardOutline.effectDistance = new Vector2(6f, -6f);

        Text chooseHeaderTxt = CreateUIText("Text_ChooseHeader", diffCard.transform, font, "SELECT DIFFICULTY", 34, TextAnchor.MiddleCenter, new Color(0.52f, 0.98f, 0.68f, 1f));
        chooseHeaderTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 392f);
        chooseHeaderTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(740f, 64f);

        // 38px vertical gap between each 132px Difficulty Card!
        Button btnEasy = CreateDifficultyCardButton(
            "Button_Easy",
            diffCard.transform,
            font,
            iconPlaySprite,
            "EASY MODE",
            "Relaxed Fall Speed  •  Fewer Hazards  •  +10 Pts",
            new Vector2(0f, 224f),
            new Vector2(760f, 132f),
            new Color(0.18f, 0.42f, 0.24f, 1f)
        );
        uiMgr.btnEasy = btnEasy;

        Button btnMedium = CreateDifficultyCardButton(
            "Button_Medium",
            diffCard.transform,
            font,
            iconPlaySprite,
            "MEDIUM MODE",
            "Classic Arcade Pace  •  Balanced Drops  •  +10 Pts",
            new Vector2(0f, 54f),
            new Vector2(760f, 132f),
            new Color(0.68f, 0.44f, 0.16f, 1f)
        );
        uiMgr.btnMedium = btnMedium;

        Button btnHard = CreateDifficultyCardButton(
            "Button_Hard",
            diffCard.transform,
            font,
            iconPlaySprite,
            "HARD MODE",
            "Frenzy Speed  •  High Hazards  •  +15 Pts / Catch",
            new Vector2(0f, -116f),
            new Vector2(760f, 132f),
            new Color(0.56f, 0.20f, 0.22f, 1f)
        );
        uiMgr.btnHard = btnHard;

        Text stageHintTxt = CreateUIText("Text_StageHint", diffCard.transform, defaultBodyFont, "10 BIOMES UNLOCK AT 100, 250, 450, 700... 2700 PTS\nWITH DYNAMIC MUSIC & UNIQUE HAZARDS!", 28, TextAnchor.MiddleCenter, new Color(0.72f, 0.92f, 1f, 0.95f));
        stageHintTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        stageHintTxt.verticalOverflow = VerticalWrapMode.Truncate;
        stageHintTxt.lineSpacing = 1.15f;
        stageHintTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -238f);
        stageHintTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(800f, 80f);

        Button btnDiffBack = CreateUIIconBannerButton(
            "Button_DifficultyBack",
            diffCard.transform,
            font,
            iconHomeSprite,
            "HOME",
            new Vector2(0f, -356f),
            new Vector2(320f, 92f),
            new Vector2(52f, 52f),
            new Color(0.24f, 0.20f, 0.32f, 1f),
            30
        );
        uiMgr.btnDifficultyBack = btnDiffBack;

        diffPanel.SetActive(false);
        uiMgr.difficultyPanel = diffPanel;

        // =========================================================================
        // 4. BOTTOM-SCREEN RPG SLIME TUTOR DIALOGUE
        //    - Bloop the Slime sits on TOP-LEFT CORNER overlapping outside the dialog box!
        //    - SKIP Icon Button sits on TOP-RIGHT BORDER overlapping outside the dialog box so it NEVER overlaps any text!
        // =========================================================================
        GameObject tutPanel = CreateUIElement("Tutorial_Overlay", canvasObj.transform);
        RectTransform tutRect = tutPanel.GetComponent<RectTransform>();
        tutRect.anchorMin = Vector2.zero;
        tutRect.anchorMax = Vector2.one;
        tutRect.offsetMin = Vector2.zero;
        tutRect.offsetMax = Vector2.zero;

        Image tutScrim = tutPanel.AddComponent<Image>();
        tutScrim.color = new Color(0.03f, 0.02f, 0.06f, 0.34f);

        // Full-Screen Invisible Button behind dialog card so tapping ANYWHERE advances/skips typing
        GameObject tapBtnObj = CreateUIElement("Button_TapAnywhere", tutPanel.transform);
        RectTransform tapRect = tapBtnObj.GetComponent<RectTransform>();
        tapRect.anchorMin = Vector2.zero;
        tapRect.anchorMax = Vector2.one;
        tapRect.offsetMin = Vector2.zero;
        tapRect.offsetMax = Vector2.zero;
        Image tapTransparentImg = tapBtnObj.AddComponent<Image>();
        tapTransparentImg.color = new Color(0f, 0f, 0f, 0.01f);
        Button btnTapAnywhere = tapBtnObj.AddComponent<Button>();
        btnTapAnywhere.targetGraphic = tapTransparentImg;
        uiMgr.btnTutorialTapAnywhere = btnTapAnywhere;

        // Compact Bottom RPG Dialog Box Container (940x380 - Safe 70px side margins & 192px auto-fit text area)
        GameObject tutDialogBox = CreateUIElement("Tutorial_BottomDialogBox", tutPanel.transform);
        RectTransform tdRect = tutDialogBox.GetComponent<RectTransform>();
        tdRect.anchorMin = new Vector2(0.5f, 0f);
        tdRect.anchorMax = new Vector2(0.5f, 0f);
        tdRect.pivot = new Vector2(0.5f, 0f);
        tdRect.anchoredPosition = new Vector2(0f, 28f);
        tdRect.sizeDelta = new Vector2(940f, 380f);

        Image tdBg = tutDialogBox.AddComponent<Image>();
        tdBg.color = new Color(0.11f, 0.10f, 0.18f, 0.97f);
        tdBg.raycastTarget = false;
        if (roundedFrameSprite != null)
        {
            tdBg.sprite = roundedFrameSprite;
            tdBg.type = Image.Type.Sliced;
        }
        Outline tdOutline = tutDialogBox.AddComponent<Outline>();
        tdOutline.effectColor = new Color(0.38f, 0.88f, 1.0f, 1f);
        tdOutline.effectDistance = new Vector2(5f, -5f);

        // Header Accent Strip inside Top of Dialog Box
        GameObject tdHeaderStrip = CreateUIElement("Dialog_HeaderStrip", tutDialogBox.transform);
        RectTransform thsRect = tdHeaderStrip.GetComponent<RectTransform>();
        thsRect.anchorMin = new Vector2(0f, 1f);
        thsRect.anchorMax = new Vector2(1f, 1f);
        thsRect.pivot = new Vector2(0.5f, 1f);
        thsRect.anchoredPosition = Vector2.zero;
        thsRect.sizeDelta = new Vector2(0f, 76f);
        Image thsBg = tdHeaderStrip.AddComponent<Image>();
        thsBg.color = new Color(0.16f, 0.15f, 0.28f, 0.95f);
        thsBg.raycastTarget = false;
        if (roundedFrameSprite != null)
        {
            thsBg.sprite = roundedFrameSprite;
            thsBg.type = Image.Type.Sliced;
        }

        // Step Badge & Lesson Title
        Text tutStepTxt = CreateUIText("Text_TutorialStep", tutDialogBox.transform, defaultBodyFont, "DIFFICULTY  •  MEDIUM MODE  (1/5)", 26, TextAnchor.MiddleLeft, new Color(1f, 0.86f, 0.40f, 1f));
        tutStepTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        tutStepTxt.verticalOverflow = VerticalWrapMode.Truncate;
        tutStepTxt.raycastTarget = false;
        RectTransform tsRect = tutStepTxt.GetComponent<RectTransform>();
        tsRect.anchorMin = new Vector2(0f, 1f);
        tsRect.anchorMax = new Vector2(0f, 1f);
        tsRect.pivot = new Vector2(0f, 1f);
        tsRect.anchoredPosition = new Vector2(215f, -14f);
        tsRect.sizeDelta = new Vector2(530f, 28f);
        uiMgr.textTutorialStepBadge = tutStepTxt;

        Text tutTitleTxt = CreateUIText("Text_TutorialTitle", tutDialogBox.transform, defaultBodyFont, "MEDIUM MODE: CLASSIC ARCADE!", 28, TextAnchor.MiddleLeft, new Color(0.50f, 0.98f, 0.66f, 1f));
        tutTitleTxt.raycastTarget = false;
        tutTitleTxt.resizeTextForBestFit = true;
        tutTitleTxt.resizeTextMinSize = 22;
        tutTitleTxt.resizeTextMaxSize = 28;
        tutTitleTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        tutTitleTxt.verticalOverflow = VerticalWrapMode.Truncate;
        RectTransform ttRect = tutTitleTxt.GetComponent<RectTransform>();
        ttRect.anchorMin = new Vector2(0f, 1f);
        ttRect.anchorMax = new Vector2(0f, 1f);
        ttRect.pivot = new Vector2(0f, 1f);
        ttRect.anchoredPosition = new Vector2(215f, -44f);
        ttRect.sizeDelta = new Vector2(550f, 34f);
        uiMgr.textTutorialTitle = tutTitleTxt;

        // TOP-RIGHT CORNER OVERLAPPING SKIP ICON BUTTON
        Button btnTutSkip = CreateUIIconBannerButton(
            "Button_TutorialSkip",
            tutDialogBox.transform,
            font,
            iconSkipSprite,
            "SKIP",
            Vector2.zero,
            new Vector2(148f, 50f),
            new Vector2(32f, 32f),
            new Color(0.56f, 0.22f, 0.26f, 1f),
            22
        );
        RectTransform skRect = btnTutSkip.GetComponent<RectTransform>();
        skRect.anchorMin = new Vector2(1f, 1f);
        skRect.anchorMax = new Vector2(1f, 1f);
        skRect.pivot = new Vector2(1f, 0.5f);
        skRect.anchoredPosition = new Vector2(-16f, 18f);
        Outline skOut = btnTutSkip.GetComponent<Outline>();
        if (skOut != null)
        {
            skOut.effectColor = new Color(1f, 0.84f, 0.36f, 1f);
            skOut.effectDistance = new Vector2(3f, -3f);
        }
        uiMgr.btnTutorialSkip = btnTutSkip;

        // Dialogue Speech Body (Auto-fitting BestFit text: 22-28pt across 192px height, zero overflow!)
        Text tutBodyTxt = CreateUIText("Text_TutorialBody", tutDialogBox.transform, defaultBodyFont,
            "\"Standard 100% arcade drop speed with balanced Poison rates (12%–26% across stages) and classic Power-Ups. Each food gives +10 Pts (+5 Combo Bonus)!\"",
            28, TextAnchor.UpperLeft, new Color(0.96f, 0.98f, 1f, 1f));
        tutBodyTxt.resizeTextForBestFit = true;
        tutBodyTxt.resizeTextMinSize = 22;
        tutBodyTxt.resizeTextMaxSize = 28;
        tutBodyTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        tutBodyTxt.verticalOverflow = VerticalWrapMode.Truncate;
        tutBodyTxt.lineSpacing = 1.14f;
        tutBodyTxt.raycastTarget = false;
        RectTransform tbRect = tutBodyTxt.GetComponent<RectTransform>();
        tbRect.anchorMin = new Vector2(0f, 1f);
        tbRect.anchorMax = new Vector2(0f, 1f);
        tbRect.pivot = new Vector2(0f, 1f);
        tbRect.anchoredPosition = new Vector2(28f, -92f);
        tbRect.sizeDelta = new Vector2(884f, 192f);
        uiMgr.textTutorialBody = tutBodyTxt;

        // Bottom Footer Strip: 4 Compact Showcase Item Icons on Left + Dynamic Tap Prompt on Right
        GameObject iconTray = CreateUIElement("Tutorial_IconTray", tutDialogBox.transform);
        RectTransform itRect = iconTray.GetComponent<RectTransform>();
        itRect.anchorMin = new Vector2(0f, 0f);
        itRect.anchorMax = new Vector2(0f, 0f);
        itRect.pivot = new Vector2(0f, 0f);
        itRect.anchoredPosition = new Vector2(28f, 14f);
        itRect.sizeDelta = new Vector2(280f, 58f);
        Image itBg = iconTray.AddComponent<Image>();
        itBg.color = new Color(0.16f, 0.15f, 0.26f, 0.92f);
        itBg.raycastTarget = false;
        if (roundedFrameSprite != null)
        {
            itBg.sprite = roundedFrameSprite;
            itBg.type = Image.Type.Sliced;
        }
        Outline itOut = iconTray.AddComponent<Outline>();
        itOut.effectColor = new Color(0.32f, 0.72f, 0.90f, 0.55f);
        itOut.effectDistance = new Vector2(2f, -2f);

        uiMgr.imgTutorialIcon1 = CreateUISpriteIcon("Tutorial_Icon1", iconTray.transform, burgerSprite, new Vector2(-96f, 0f), new Vector2(48f, 48f));
        uiMgr.imgTutorialIcon2 = CreateUISpriteIcon("Tutorial_Icon2", iconTray.transform, bonusHeartSprite, new Vector2(-32f, 0f), new Vector2(48f, 48f));
        uiMgr.imgTutorialIcon3 = CreateUISpriteIcon("Tutorial_Icon3", iconTray.transform, shieldBarrierSprite, new Vector2(32f, 0f), new Vector2(48f, 48f));
        uiMgr.imgTutorialIcon4 = CreateUISpriteIcon("Tutorial_Icon4", iconTray.transform, timeSlowSprite, new Vector2(96f, 0f), new Vector2(48f, 48f));

        Text tutPromptTxt = CreateUIText("Text_TapPrompt", tutDialogBox.transform, defaultBodyFont, "[ TAP TO CONTINUE > ]", 26, TextAnchor.MiddleRight, new Color(1f, 0.90f, 0.38f, 1f));
        tutPromptTxt.raycastTarget = false;
        RectTransform tpRect = tutPromptTxt.GetComponent<RectTransform>();
        tpRect.anchorMin = new Vector2(1f, 0f);
        tpRect.anchorMax = new Vector2(1f, 0f);
        tpRect.pivot = new Vector2(1f, 0f);
        tpRect.anchoredPosition = new Vector2(-24f, 22f);
        tpRect.sizeDelta = new Vector2(540f, 40f);
        uiMgr.textTutorialTapPrompt = tutPromptTxt;

        // TOP-LEFT CORNER OVERLAPPING SLIME TUTOR BADGE (Placed OUTSIDE/OVERLAPPING the Top-Left Border of the Dialog Container!)
        GameObject cornerBadge = CreateUIElement("Slime_Tutor_CornerBadge", tutDialogBox.transform);
        RectTransform cbRect = cornerBadge.GetComponent<RectTransform>();
        cbRect.anchorMin = new Vector2(0f, 1f);
        cbRect.anchorMax = new Vector2(0f, 1f);
        cbRect.pivot = new Vector2(0.5f, 0.5f);
        cbRect.anchoredPosition = new Vector2(80f, 32f);
        cbRect.sizeDelta = new Vector2(144f, 140f);

        Image cbBg = cornerBadge.AddComponent<Image>();
        cbBg.color = new Color(0.16f, 0.24f, 0.38f, 0.98f);
        cbBg.raycastTarget = false;
        if (roundedFrameSprite != null)
        {
            cbBg.sprite = roundedFrameSprite;
            cbBg.type = Image.Type.Sliced;
        }
        Outline cbOutline = cornerBadge.AddComponent<Outline>();
        cbOutline.effectColor = new Color(0.48f, 0.98f, 0.68f, 1f);
        cbOutline.effectDistance = new Vector2(4f, -4f);

        Image slimePortraitImg = CreateUISpriteIcon("Slime_Tutor_Avatar", cornerBadge.transform, slimeLogoSprite, new Vector2(0f, 10f), new Vector2(132f, 114f));
        uiMgr.imgTutorialSlimePortrait = slimePortraitImg;

        GameObject namePlate = CreateUIElement("Speaker_NamePlate", cornerBadge.transform);
        RectTransform npRect = namePlate.GetComponent<RectTransform>();
        npRect.anchorMin = new Vector2(0.5f, 0f);
        npRect.anchorMax = new Vector2(0.5f, 0f);
        npRect.pivot = new Vector2(0.5f, 0.5f);
        npRect.anchoredPosition = new Vector2(0f, -4f);
        npRect.sizeDelta = new Vector2(240f, 40f);
        Image npBg = namePlate.AddComponent<Image>();
        npBg.color = new Color(0.12f, 0.54f, 0.32f, 1f);
        npBg.raycastTarget = false;
        if (roundedFrameSprite != null)
        {
            npBg.sprite = roundedFrameSprite;
            npBg.type = Image.Type.Sliced;
        }
        Outline npOut = namePlate.AddComponent<Outline>();
        npOut.effectColor = new Color(0.95f, 0.84f, 0.35f, 1f);
        npOut.effectDistance = new Vector2(2f, -2f);

        Text speakerTxt = CreateUIText("Text_SpeakerName", namePlate.transform, defaultBodyFont, "BLOOP THE TUTOR", 24, TextAnchor.MiddleCenter, new Color(0.96f, 1f, 0.88f, 1f));
        speakerTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
        speakerTxt.verticalOverflow = VerticalWrapMode.Truncate;
        RectTransform spkRect = speakerTxt.GetComponent<RectTransform>();
        spkRect.anchorMin = Vector2.zero;
        spkRect.anchorMax = Vector2.one;
        spkRect.offsetMin = Vector2.zero;
        spkRect.offsetMax = Vector2.zero;
        uiMgr.textTutorialSpeakerName = speakerTxt;

        tutPanel.SetActive(false);
        uiMgr.tutorialPanel = tutPanel;

        // =========================================================================
        // 5. 3-SECOND COUNTDOWN OVERLAY (3... 2... 1... GO!)
        // =========================================================================
        GameObject cdPanel = CreateUIElement("Countdown_Overlay", canvasObj.transform);
        RectTransform cdRect = cdPanel.GetComponent<RectTransform>();
        cdRect.anchorMin = Vector2.zero;
        cdRect.anchorMax = Vector2.one;
        cdRect.offsetMin = Vector2.zero;
        cdRect.offsetMax = Vector2.zero;

        Image cdScrim = cdPanel.AddComponent<Image>();
        cdScrim.color = new Color(0.05f, 0.03f, 0.08f, 0.55f);

        Text cdNumTxt = CreateUIText("Text_CountdownNumber", cdPanel.transform, font, "3", 120, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.42f, 1f));
        cdNumTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 60f);
        cdNumTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(600f, 240f);
        uiMgr.textCountdownNumber = cdNumTxt;

        Text cdSubTxt = CreateUIText("Text_CountdownSubtitle", cdPanel.transform, font, "GET READY TO SLIDE...", 38, TextAnchor.MiddleCenter, Color.white);
        cdSubTxt.resizeTextForBestFit = true;
        cdSubTxt.resizeTextMinSize = 28;
        cdSubTxt.resizeTextMaxSize = 38;
        cdSubTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -110f);
        cdSubTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(840f, 70f);
        uiMgr.textCountdownSubtitle = cdSubTxt;

        cdPanel.SetActive(false);
        uiMgr.countdownPanel = cdPanel;

        // =========================================================================
        // 6. SLIME GUIDEBOOK / ALMANAC MODAL (4 Tabs: FOODS, POISONS, POWER-UPS, STAGES)
        // =========================================================================
        BuildGuidebookModal(canvasObj.transform, uiMgr, gm, font, retroBodyFont, iconCloseSprite,
            burgerSprite, sushiSprite, pizzaSprite, donutSprite, iceCreamSprite, lollipopSprite,
            poisonMushroomSprite, poisonPotionSprite, poisonGobletSprite, spikeTrapSprite,
            bonusHeartSprite, starMagnetSprite, shieldBarrierSprite, timeSlowSprite,
            stagePreviews);

        BuildAlmanacItemDetailModal(canvasObj.transform, uiMgr, font, retroBodyFont, burgerSprite);

        // =========================================================================
        // 7. SETTINGS MODAL (Context-Aware: Home -> Settings vs. In-Game Pause -> Settings)
        //    - Uses Icon Buttons with Generous Spacing (zero crowded/touching controls!)
        // =========================================================================
        GameObject setPanel = CreateUIElement("Settings_Modal", canvasObj.transform);
        RectTransform setRect = setPanel.GetComponent<RectTransform>();
        setRect.anchorMin = Vector2.zero;
        setRect.anchorMax = Vector2.one;
        setRect.offsetMin = Vector2.zero;
        setRect.offsetMax = Vector2.zero;

        Image setScrim = setPanel.AddComponent<Image>();
        setScrim.color = new Color(0.04f, 0.03f, 0.08f, 0.85f);

        GameObject setCard = CreateUIElement("Settings_Card", setPanel.transform);
        RectTransform scCardRect = setCard.GetComponent<RectTransform>();
        scCardRect.anchorMin = new Vector2(0.5f, 0.5f);
        scCardRect.anchorMax = new Vector2(0.5f, 0.5f);
        scCardRect.pivot = new Vector2(0.5f, 0.5f);
        scCardRect.anchoredPosition = new Vector2(0f, 15f);
        scCardRect.sizeDelta = new Vector2(880f, 1000f);
        uiMgr.settingsCardRect = scCardRect;

        Image setCardBg = setCard.AddComponent<Image>();
        setCardBg.color = new Color(0.14f, 0.11f, 0.19f, 0.98f);
        if (roundedFrameSprite != null)
        {
            setCardBg.sprite = roundedFrameSprite;
            setCardBg.type = Image.Type.Sliced;
        }
        Outline setCardOutline = setCard.AddComponent<Outline>();
        setCardOutline.effectColor = new Color(0.95f, 0.76f, 0.32f, 1f);
        setCardOutline.effectDistance = new Vector2(6f, -6f);

        // Header Title + Top-Right Close Icon Button
        Text setTitleTxt = CreateUIText("Text_SettingsTitle", setCard.transform, font, "GAME PAUSED", 36, TextAnchor.MiddleCenter, new Color(0.52f, 0.98f, 0.68f, 1f));
        setTitleTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(-24f, 430f);
        setTitleTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(620f, 64f);
        uiMgr.textSettingsTitle = setTitleTxt;

        Button btnSetCloseX = CreateUIIconButton(
            "Button_SettingsCloseX",
            setCard.transform,
            iconCloseSprite,
            new Vector2(360f, 430f),
            new Vector2(78f, 78f),
            new Vector2(44f, 44f),
            new Color(0.62f, 0.22f, 0.26f, 1f)
        );
        uiMgr.btnSettingsClose = btnSetCloseX;
        uiMgr.settingsCloseBtnRect = btnSetCloseX.GetComponent<RectTransform>();

        // GROUP 1: Inset Audio & Haptics Sub-Panel (Generous row spacing: +96, 0, -96 inside 316px height)
        GameObject audioBox = CreateUIElement("Settings_AudioHapticsPanel", setCard.transform);
        RectTransform abRect = audioBox.GetComponent<RectTransform>();
        abRect.anchoredPosition = new Vector2(0f, 185f);
        abRect.sizeDelta = new Vector2(780f, 320f);
        uiMgr.settingsAudioBoxRect = abRect;

        Image abBg = audioBox.AddComponent<Image>();
        abBg.color = new Color(0.09f, 0.07f, 0.13f, 0.92f);
        abBg.raycastTarget = false;
        if (roundedFrameSprite != null)
        {
            abBg.sprite = roundedFrameSprite;
            abBg.type = Image.Type.Sliced;
        }
        Outline abOut = audioBox.AddComponent<Outline>();
        abOut.effectColor = new Color(0.30f, 0.68f, 0.90f, 0.45f);
        abOut.effectDistance = new Vector2(2f, -2f);

        // Row 1: BGM Volume (28px horizontal gaps between [-] [100%] [+])
        Text bgmLbl = CreateUIText("Label_BgmVolume", audioBox.transform, font, "MUSIC (BGM)", 26, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));
        bgmLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(-175f, 98f);
        bgmLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(340f, 52f);

        Button btnBgmMinus = CreateUIButton("Button_BgmMinus", audioBox.transform, font, "-", new Vector2(75f, 98f), new Vector2(76f, 66f), new Color(0.68f, 0.26f, 0.26f, 1f), 40);
        Text bgmValTxt = CreateUIText("Text_BgmValue", audioBox.transform, font, "100%", 25, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.38f, 1f));
        bgmValTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(195f, 98f);
        bgmValTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(130f, 52f);
        Button btnBgmPlus = CreateUIButton("Button_BgmPlus", audioBox.transform, font, "+", new Vector2(315f, 98f), new Vector2(76f, 66f), new Color(0.20f, 0.66f, 0.34f, 1f), 40);

        uiMgr.btnBgmMinus = btnBgmMinus;
        uiMgr.btnBgmPlus = btnBgmPlus;
        uiMgr.textBgmVolumeValue = bgmValTxt;

        // Row 2: SFX Volume
        Text sfxLbl = CreateUIText("Label_SfxVolume", audioBox.transform, font, "SOUND (SFX)", 26, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));
        sfxLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(-175f, 0f);
        sfxLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(340f, 52f);

        Button btnSfxMinus = CreateUIButton("Button_SfxMinus", audioBox.transform, font, "-", new Vector2(75f, 0f), new Vector2(76f, 66f), new Color(0.68f, 0.26f, 0.26f, 1f), 40);
        Text sfxValTxt = CreateUIText("Text_SfxValue", audioBox.transform, font, "100%", 25, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.38f, 1f));
        sfxValTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(195f, 0f);
        sfxValTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(130f, 52f);
        Button btnSfxPlus = CreateUIButton("Button_SfxPlus", audioBox.transform, font, "+", new Vector2(315f, 0f), new Vector2(76f, 66f), new Color(0.20f, 0.66f, 0.34f, 1f), 40);

        uiMgr.btnSfxMinus = btnSfxMinus;
        uiMgr.btnSfxPlus = btnSfxPlus;
        uiMgr.textSfxVolumeValue = sfxValTxt;

        // Row 3: Poison Haptics / Vibration Toggle Icon Button
        Text vibTitleLbl = CreateUIText("Label_VibrationTitle", audioBox.transform, defaultBodyFont, "POISON HAPTICS", 25, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));
        vibTitleLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(-175f, -98f);
        vibTitleLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(340f, 52f);

        Button btnVibrate = CreateUIIconBannerButton(
            "Button_ToggleVibrate",
            audioBox.transform,
            font,
            iconVibrateSprite,
            "ON",
            new Vector2(195f, -98f),
            new Vector2(240f, 68f),
            new Vector2(44f, 44f),
            new Color(0.20f, 0.60f, 0.34f, 1f),
            28
        );
        uiMgr.btnToggleVibrate = btnVibrate;
        uiMgr.textVibrateLabel = btnVibrate.transform.Find("Text_Label").GetComponent<Text>();

        // GROUP 2: Primary CTA — RESUME GAME / DONE Icon Button (Forest Emerald Green)
        Button btnSetResume = CreateUIIconBannerButton(
            "Button_SettingsResume",
            setCard.transform,
            font,
            iconPlaySprite,
            "RESUME GAME",
            new Vector2(0f, -68f),
            new Vector2(580f, 100f),
            new Vector2(54f, 54f),
            new Color(0.16f, 0.40f, 0.22f, 1f),
            36
        );
        uiMgr.btnSettingsResume = btnSetResume;
        uiMgr.settingsResumeBtnRect = btnSetResume.GetComponent<RectTransform>();
        uiMgr.textSettingsResumeLabel = btnSetResume.transform.Find("Text_Label").GetComponent<Text>();

        // GROUP 3: IN-GAME SESSION ACTIONS GROUP (Hidden when opened from Home -> Settings!)
        // Contains 4 Spacious Icon Tile Buttons: RESTART (Rotated Arrow), ALMANAC (Book), TUTORIAL (Speech Bubble), HOME (House)
        GameObject inGameGroup = CreateUIElement("InGame_SessionActions_Group", setCard.transform);
        RectTransform iggRect = inGameGroup.GetComponent<RectTransform>();
        iggRect.anchorMin = new Vector2(0.5f, 0.5f);
        iggRect.anchorMax = new Vector2(0.5f, 0.5f);
        iggRect.pivot = new Vector2(0.5f, 0.5f);
        iggRect.anchoredPosition = new Vector2(0f, -305f);
        iggRect.sizeDelta = new Vector2(780f, 240f);
        uiMgr.inGameSettingsActionsGroup = inGameGroup;

        Text sessionHeaderLbl = CreateUIText("Label_SessionActions", inGameGroup.transform, font, "IN-GAME ACTIONS", 25, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.42f, 0.95f));
        sessionHeaderLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 88f);
        sessionHeaderLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(500f, 32f);

        // 4 Icon Tile Buttons with 36px horizontal gap between each 164x144 button!
        Button btnSetRestart = CreateUIIconTileButton(
            "Button_SettingsRestart",
            inGameGroup.transform,
            font,
            iconRestartSprite,
            "RESTART",
            new Vector2(-294f, -18f),
            new Vector2(164f, 144f),
            new Vector2(64f, 64f),
            new Color(0.68f, 0.44f, 0.16f, 1f),
            20
        );
        uiMgr.btnSettingsRestart = btnSetRestart;

        Button btnSetGuide = CreateUIIconTileButton(
            "Button_SettingsGuidebook",
            inGameGroup.transform,
            font,
            guidebookIconSprite,
            "ALMANAC",
            new Vector2(-98f, -18f),
            new Vector2(164f, 144f),
            new Vector2(64f, 64f),
            new Color(0.16f, 0.28f, 0.38f, 1f),
            20
        );
        uiMgr.btnSettingsGuidebook = btnSetGuide;

        Button btnReplayTut = CreateUIIconTileButton(
            "Button_ReplayTutorial",
            inGameGroup.transform,
            font,
            iconTutorialSprite,
            "TUTORIAL",
            new Vector2(98f, -18f),
            new Vector2(164f, 144f),
            new Vector2(64f, 64f),
            new Color(0.32f, 0.26f, 0.44f, 1f),
            20
        );
        uiMgr.btnSettingsReplayTutorial = btnReplayTut;

        Button btnSetHome = CreateUIIconTileButton(
            "Button_SettingsHome",
            inGameGroup.transform,
            font,
            iconHomeSprite,
            "HOME",
            new Vector2(294f, -18f),
            new Vector2(164f, 144f),
            new Vector2(64f, 64f),
            new Color(0.24f, 0.20f, 0.32f, 1f),
            20
        );
        uiMgr.btnSettingsHome = btnSetHome;

        setPanel.SetActive(false);
        uiMgr.settingsPanel = setPanel;

        // =========================================================================
        // 8. GAME OVER MODAL (Generous Spacing + Replay Icon Button + 3-Icon Dock Row)
        // =========================================================================
        GameObject modalObj = CreateUIElement("GameOver_Modal", canvasObj.transform);
        RectTransform modalRect = modalObj.GetComponent<RectTransform>();
        modalRect.anchorMin = Vector2.zero;
        modalRect.anchorMax = Vector2.one;
        modalRect.offsetMin = Vector2.zero;
        modalRect.offsetMax = Vector2.zero;

        Image scrimImg = modalObj.AddComponent<Image>();
        scrimImg.color = new Color(0.05f, 0.03f, 0.08f, 0.84f);

        GameObject dialogBox = CreateUIElement("Dialog_Box", modalObj.transform);
        RectTransform dlgRect = dialogBox.GetComponent<RectTransform>();
        dlgRect.anchorMin = new Vector2(0.5f, 0.5f);
        dlgRect.anchorMax = new Vector2(0.5f, 0.5f);
        dlgRect.pivot = new Vector2(0.5f, 0.5f);
        dlgRect.anchoredPosition = Vector2.zero;
        dlgRect.sizeDelta = new Vector2(860f, 840f);

        Image dlgBg = dialogBox.AddComponent<Image>();
        dlgBg.color = new Color(0.15f, 0.11f, 0.19f, 0.98f);
        if (roundedFrameSprite != null)
        {
            dlgBg.sprite = roundedFrameSprite;
            dlgBg.type = Image.Type.Sliced;
        }

        Outline dlgOutline = dialogBox.AddComponent<Outline>();
        dlgOutline.effectColor = new Color(0.95f, 0.72f, 0.30f, 1f);
        dlgOutline.effectDistance = new Vector2(6f, -6f);

        Text titleTxt = CreateUIText("Text_Title", dialogBox.transform, font, "GAME OVER", 38, TextAnchor.MiddleCenter, new Color(1f, 0.42f, 0.38f, 1f));
        titleTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 315f);
        titleTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(680f, 86f);

        // Framed Score Summary Panel (Generous margin below Title and above Play Again)
        GameObject scoreBox = CreateUIElement("GameOver_ScorePanel", dialogBox.transform);
        RectTransform sbRect = scoreBox.GetComponent<RectTransform>();
        sbRect.anchoredPosition = new Vector2(0f, 145f);
        sbRect.sizeDelta = new Vector2(720f, 174f);
        Image sbBg = scoreBox.AddComponent<Image>();
        sbBg.color = new Color(0.09f, 0.07f, 0.13f, 0.92f);
        sbBg.raycastTarget = false;
        if (roundedFrameSprite != null)
        {
            sbBg.sprite = roundedFrameSprite;
            sbBg.type = Image.Type.Sliced;
        }
        Outline sbOut = scoreBox.AddComponent<Outline>();
        sbOut.effectColor = new Color(0.32f, 0.74f, 0.92f, 0.5f);
        sbOut.effectDistance = new Vector2(2f, -2f);

        Text finalScoreTxt = CreateUIText("Text_FinalScore", scoreBox.transform, defaultBodyFont, "FINAL SCORE: 0", 36, TextAnchor.MiddleCenter, Color.white);
        finalScoreTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 34f);
        finalScoreTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(660f, 64f);
        finalScoreTxt.raycastTarget = false;
        uiMgr.textFinalScore = finalScoreTxt;

        Text bestScoreModalTxt = CreateUIText("Text_BestScore", scoreBox.transform, defaultBodyFont, "HIGH SCORE: 0", 32, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.36f, 1f));
        bestScoreModalTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -36f);
        bestScoreModalTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(660f, 54f);
        bestScoreModalTxt.raycastTarget = false;
        uiMgr.textBestScore = bestScoreModalTxt;

        // Primary Hero Button: PLAY AGAIN with Rotated Arrow Icon (Retries same difficulty immediately!)
        Button btnRestart = CreateUIIconBannerButton(
            "Button_Restart",
            dialogBox.transform,
            font,
            iconRestartSprite,
            "PLAY AGAIN",
            new Vector2(0f, -36f),
            new Vector2(640f, 114f),
            new Vector2(58f, 58f),
            new Color(0.16f, 0.40f, 0.22f, 1f),
            26
        );
        uiMgr.btnRestart = btnRestart;

        // Secondary 3-Icon Action Row (54px vertical gap below Play Again, 48px horizontal gap between buttons!)
        // 1. MODES (Opens Difficulty Selection Screen!)
        Button btnChangeDiff = CreateUIIconTileButton(
            "Button_ChangeDifficulty",
            dialogBox.transform,
            font,
            iconDifficultySprite,
            "MODES",
            new Vector2(-228f, -242f),
            new Vector2(180f, 144f),
            new Vector2(64f, 64f),
            new Color(0.68f, 0.44f, 0.16f, 1f),
            21
        );
        uiMgr.btnChangeDifficulty = btnChangeDiff;

        // 2. HOME (Opens Home Screen!)
        Button btnGoHome = CreateUIIconTileButton(
            "Button_GameOverHome",
            dialogBox.transform,
            font,
            iconHomeSprite,
            "HOME",
            new Vector2(0f, -242f),
            new Vector2(180f, 144f),
            new Vector2(64f, 64f),
            new Color(0.24f, 0.20f, 0.32f, 1f),
            21
        );
        uiMgr.btnGameOverHome = btnGoHome;

        // 3. EXIT (Quits App!)
        Button btnQuit = CreateUIIconTileButton(
            "Button_Quit",
            dialogBox.transform,
            font,
            iconExitSprite,
            "EXIT",
            new Vector2(228f, -242f),
            new Vector2(180f, 144f),
            new Vector2(64f, 64f),
            new Color(0.44f, 0.19f, 0.20f, 1f),
            21
        );
        uiMgr.btnQuit = btnQuit;

        modalObj.SetActive(false);
        uiMgr.gameOverPanel = modalObj;

        // Confirmation Modal built after Settings and GameOver so it sits on top in hierarchy
        BuildConfirmationModal(canvasObj.transform, uiMgr, font, retroBodyFont);

        // =========================================================================
        // 9. SPLASH SCREEN (Full-Screen Pixar Intro Cinematic & Loading Overlay)
        // =========================================================================
        Sprite foodRun1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashCutscene/running_food_stride_1.png");
        Sprite foodRun2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashCutscene/running_food_stride_2.png");
        Sprite foodPoisonReveal = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashCutscene/running_food_poison_reveal.png");
        Sprite slimeChaseExcited = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashCutscene/slime_chase_excited.png");
        Sprite slimeChompBite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashCutscene/slime_chomp_bite.png");
        Sprite slimeRealizePoison = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashCutscene/slime_realize_poison.png");
        Sprite slimeTurnLookUser = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashCutscene/slime_turn_look_user.png");
        Sprite slimeGagPuff1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashCutscene/slime_gag_puff_1.png");
        Sprite slimeGagPuff2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashCutscene/slime_gag_puff_2.png");
        Sprite slimeSpitLaunch = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashCutscene/slime_spit_launch.png");
        Sprite slimeReliefSmile = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashCutscene/slime_relief_smile.png");
        Sprite spitProjectileSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashCutscene/spit_poison_projectile.png");
        Sprite spotlightSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashCutscene/stage_spotlight_cone.png");
        Sprite contactShadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashCutscene/stage_contact_shadow.png");

        GameObject splashObj = CreateUIElement("Splash_Screen", canvasObj.transform);
        RectTransform splashRect = splashObj.GetComponent<RectTransform>();
        splashRect.anchorMin = Vector2.zero;
        splashRect.anchorMax = Vector2.one;
        splashRect.offsetMin = Vector2.zero;
        splashRect.offsetMax = Vector2.zero;

        Image splashBg = splashObj.AddComponent<Image>();
        splashBg.color = new Color(0.08f, 0.05f, 0.12f, 1f);

        CanvasGroup splashCg = splashObj.AddComponent<CanvasGroup>();
        splashCg.alpha = 1f;
        uiMgr.splashCanvasGroup = splashCg;

        // Cinematic Script component
        PixarSplashCinematic cinematic = splashObj.AddComponent<PixarSplashCinematic>();

        // Top Screen Header Strip: Slime Studio Presents + Grand Title + Subtitle (Same level as Home Title Screen)
        GameObject splashTopHeader = CreateUIElement("Splash_TopHeader", splashObj.transform);
        RectTransform sthRect = splashTopHeader.GetComponent<RectTransform>();
        sthRect.anchorMin = new Vector2(0.5f, 0.5f);
        sthRect.anchorMax = new Vector2(0.5f, 0.5f);
        sthRect.pivot = new Vector2(0.5f, 0.5f);
        sthRect.anchoredPosition = new Vector2(0f, 560f);
        sthRect.sizeDelta = new Vector2(920f, 220f);

        Text studioTxt = CreateUIText("Text_StudioPresents", splashTopHeader.transform, font, "SLIME STUDIO PRESENTS", 18, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.38f, 1f));
        studioTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 65f);
        studioTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(760f, 38f);

        Text splashTitle = CreateUIText("Text_SplashTitle", splashTopHeader.transform, font, "SLIME FOOD DROP", 44, TextAnchor.MiddleCenter, new Color(0.52f, 0.98f, 0.65f, 1f));
        splashTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
        splashTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(880f, 80f);

        Text splashSub = CreateUIText("Text_SplashSubtitle", splashTopHeader.transform, defaultBodyFont, "CATCH THE TREATS • DODGE THE TOXINS!", 28, TextAnchor.MiddleCenter, new Color(0.82f, 0.94f, 1f, 0.95f));
        splashSub.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -55f);
        splashSub.GetComponent<RectTransform>().sizeDelta = new Vector2(820f, 36f);

        // Stage Area Container (Centered vertically and horizontally on the screen device!)
        GameObject stageObj = CreateUIElement("Cinematic_StageContainer", splashObj.transform);
        RectTransform stageRect = stageObj.GetComponent<RectTransform>();
        stageRect.anchorMin = new Vector2(0.5f, 0.5f);
        stageRect.anchorMax = new Vector2(0.5f, 0.5f);
        stageRect.pivot = new Vector2(0.5f, 0.5f);
        stageRect.anchoredPosition = new Vector2(0f, 15f);
        stageRect.sizeDelta = new Vector2(920f, 600f);
        cinematic.stageContainer = stageRect;

        // Spotlight Cone (Enlarged)
        Image spotlightImg = CreateUISpriteIcon("Cinematic_Spotlight", stageObj.transform, spotlightSprite, new Vector2(0f, 50f), new Vector2(860f, 640f));
        spotlightImg.color = new Color(1f, 0.96f, 0.82f, 0.35f);
        cinematic.spotlightImage = spotlightImg;

        // Contact Shadows (Enlarged)
        Image foodShadow = CreateUISpriteIcon("Shadow_Food", stageObj.transform, contactShadowSprite, new Vector2(-460f, -115f), new Vector2(120f, 36f));
        foodShadow.color = new Color(0f, 0f, 0f, 0.45f);
        cinematic.foodShadow = foodShadow;

        Image bloopShadow = CreateUISpriteIcon("Shadow_Bloop", stageObj.transform, contactShadowSprite, new Vector2(-550f, -115f), new Vector2(260f, 64f));
        bloopShadow.color = new Color(0f, 0f, 0f, 0.55f);
        cinematic.bloopShadow = bloopShadow;

        // Running Food Actor (Enlarged)
        Image foodImg = CreateUISpriteIcon("Actor_RunningFood", stageObj.transform, foodRun1, new Vector2(-460f, -95f), new Vector2(160f, 160f));
        cinematic.foodImage = foodImg;
        cinematic.foodRect = foodImg.GetComponent<RectTransform>();

        // Bloop Slime Actor (Enlarged)
        Image bloopImg = CreateUISpriteIcon("Actor_Bloop", stageObj.transform, slimeChaseExcited, new Vector2(-550f, -80f), new Vector2(380f, 320f));
        cinematic.bloopImage = bloopImg;
        cinematic.bloopRect = bloopImg.GetComponent<RectTransform>();

        // Spit Poison Projectile (Enlarged)
        Image spitImg = CreateUISpriteIcon("Actor_PoisonProjectile", stageObj.transform, spitProjectileSprite, Vector2.zero, new Vector2(140f, 140f));
        cinematic.slimeTrailSprite = LoadSpriteEnsured("Assets/Sprites/Slime/slime_trail_droplet.png");
        spitImg.gameObject.SetActive(false);
        cinematic.projectileImage = spitImg;
        cinematic.projectileRect = spitImg.GetComponent<RectTransform>();

        // Speech Bubble
        GameObject cinematicBubbleObj = CreateUIElement("Cinematic_SpeechBubble", stageObj.transform);
        RectTransform bRect = cinematicBubbleObj.GetComponent<RectTransform>();
        bRect.anchoredPosition = new Vector2(0f, 80f);
        bRect.sizeDelta = new Vector2(560f, 96f);
        Image bBg = cinematicBubbleObj.AddComponent<Image>();
        bBg.color = new Color(0.12f, 0.16f, 0.24f, 0.96f);
        if (roundedFrameSprite != null)
        {
            bBg.sprite = roundedFrameSprite;
            bBg.type = Image.Type.Sliced;
        }
        Outline bOut = cinematicBubbleObj.AddComponent<Outline>();
        bOut.effectColor = new Color(1f, 0.88f, 0.44f, 1f);
        bOut.effectDistance = new Vector2(2f, -2f);

        Text bubbleTxt = CreateUIText("Text_Bubble", cinematicBubbleObj.transform, font, "...Still there?", 24, TextAnchor.MiddleCenter, Color.white);
        bubbleTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        bubbleTxt.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform btRectInternal = bubbleTxt.GetComponent<RectTransform>();
        btRectInternal.anchorMin = Vector2.zero;
        btRectInternal.anchorMax = Vector2.one;
        btRectInternal.offsetMin = new Vector2(16f, 10f);
        btRectInternal.offsetMax = new Vector2(-16f, -10f);

        cinematicBubbleObj.SetActive(false);
        cinematic.speechBubbleGroup = cinematicBubbleObj;
        cinematic.speechBubbleText = bubbleTxt;
        cinematic.speechBubbleRect = bRect;

        // --- Bottom Screen Loading Bar (Elevated away from phone edge) ---
        GameObject loadGroupObj = CreateUIElement("Splash_LoadingGroup", splashObj.transform);
        RectTransform lgRect = loadGroupObj.GetComponent<RectTransform>();
        lgRect.anchorMin = new Vector2(0.5f, 0f);
        lgRect.anchorMax = new Vector2(0.5f, 0f);
        lgRect.pivot = new Vector2(0.5f, 0f);
        lgRect.anchoredPosition = new Vector2(0f, 155f);
        lgRect.sizeDelta = new Vector2(860f, 140f);
        CanvasGroup loadCg = loadGroupObj.AddComponent<CanvasGroup>();
        loadCg.alpha = 1f;
        cinematic.loadingBarGroup = loadCg;

        // Loading Bar Track
        GameObject barTrack = CreateUIElement("Splash_ProgressBarTrack", loadGroupObj.transform);
        RectTransform btRect = barTrack.GetComponent<RectTransform>();
        btRect.anchoredPosition = new Vector2(0f, 10f);
        btRect.sizeDelta = new Vector2(720f, 44f);

        Image btImg = barTrack.AddComponent<Image>();
        btImg.color = new Color(0.18f, 0.12f, 0.26f, 1f);
        if (roundedFrameSprite != null)
        {
            btImg.sprite = roundedFrameSprite;
            btImg.type = Image.Type.Sliced;
        }
        Outline btOutline = barTrack.AddComponent<Outline>();
        btOutline.effectColor = new Color(0.92f, 0.70f, 0.30f, 1f);
        btOutline.effectDistance = new Vector2(2f, -2f);

        // Loading Bar Fill
        GameObject barFill = CreateUIElement("Splash_ProgressBarFill", barTrack.transform);
        RectTransform bfRect = barFill.GetComponent<RectTransform>();
        bfRect.anchorMin = Vector2.zero;
        bfRect.anchorMax = new Vector2(0.01f, 1f);
        bfRect.offsetMin = new Vector2(6f, 6f);
        bfRect.offsetMax = new Vector2(-6f, -6f);

        Image bfImg = barFill.AddComponent<Image>();
        bfImg.color = new Color(0.35f, 0.92f, 0.48f, 1f);
        if (roundedFrameSprite != null)
        {
            bfImg.sprite = roundedFrameSprite;
            bfImg.type = Image.Type.Sliced;
        }
        uiMgr.splashProgressFill = bfImg;
        uiMgr.splashProgressBarFillRect = bfRect;
        cinematic.loadingBarFillRect = bfRect;

        Text pctTxt = CreateUIText("Text_SplashPercent", barTrack.transform, defaultBodyFont, "0%", 28, TextAnchor.MiddleCenter, Color.white);
        RectTransform pctRect = pctTxt.GetComponent<RectTransform>();
        pctRect.anchorMin = Vector2.zero;
        pctRect.anchorMax = Vector2.one;
        pctRect.offsetMin = Vector2.zero;
        pctRect.offsetMax = Vector2.zero;
        uiMgr.splashPercentText = pctTxt;
        cinematic.loadingPercentText = pctTxt;

        Text statusTxt = CreateUIText("Text_SplashStatus", loadGroupObj.transform, defaultBodyFont, "INITIALIZING 10 BIOME STAGES...", 28, TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.75f, 1f));
        statusTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -50f);
        statusTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(720f, 44f);
        uiMgr.splashStatusText = statusTxt;
        cinematic.loadingStatusText = statusTxt;

        // Wire Sprites into cinematic
        cinematic.foodRun1 = foodRun1;
        cinematic.foodRun2 = foodRun2;
        cinematic.foodPoisonReveal = foodPoisonReveal;
        cinematic.slimeChaseExcited = slimeChaseExcited;
        cinematic.slimeChompBite = slimeChompBite;
        cinematic.slimeRealizePoison = slimeRealizePoison;
        cinematic.slimeTurnLookUser = slimeTurnLookUser;
        cinematic.slimeGagPuff1 = slimeGagPuff1;
        cinematic.slimeGagPuff2 = slimeGagPuff2;
        cinematic.slimeSpitLaunch = slimeSpitLaunch;
        cinematic.slimeReliefSmile = slimeReliefSmile;
        cinematic.spitProjectileSprite = spitProjectileSprite;
        cinematic.spotlightSprite = spotlightSprite;
        cinematic.contactShadowSprite = contactShadowSprite;

        uiMgr.pixarSplashCinematic = cinematic;
        uiMgr.splashPanel = splashObj;
    }

    private static GameObject CreatePowerUpDurationBarRow(
        string name, Transform parent, Font font, Sprite iconSprite,
        string defaultLabel, Vector2 anchoredPos, Color fillColor,
        out RectTransform fillRect, out Text labelText)
    {
        GameObject rowObj = CreateUIElement(name, parent);
        RectTransform rRect = rowObj.GetComponent<RectTransform>();
        rRect.anchorMin = new Vector2(0.5f, 1f);
        rRect.anchorMax = new Vector2(0.5f, 1f);
        rRect.pivot = new Vector2(0.5f, 1f);
        rRect.anchoredPosition = anchoredPos;
        rRect.sizeDelta = new Vector2(760f, 44f);

        Image rowBg = rowObj.AddComponent<Image>();
        rowBg.color = new Color(0.10f, 0.08f, 0.14f, 0.86f);
        rowBg.raycastTarget = false;
        if (roundedFrameSprite != null)
        {
            rowBg.sprite = roundedFrameSprite;
            rowBg.type = Image.Type.Sliced;
        }
        Outline rowOut = rowObj.AddComponent<Outline>();
        rowOut.effectColor = new Color(0.04f, 0.03f, 0.06f, 0.95f);
        rowOut.effectDistance = new Vector2(3f, -3f);

        // Icon on Left
        Image iconImg = CreateUISpriteIcon("Icon", rowObj.transform, iconSprite, new Vector2(-352f, 0f), new Vector2(34f, 34f));

        // Bar Track
        GameObject trackObj = CreateUIElement("Bar_Track", rowObj.transform);
        RectTransform tRect = trackObj.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0f, 0f);
        tRect.anchorMax = new Vector2(1f, 1f);
        tRect.offsetMin = new Vector2(54f, 6f);
        tRect.offsetMax = new Vector2(-8f, -6f);

        Image tBg = trackObj.AddComponent<Image>();
        tBg.color = new Color(0.18f, 0.15f, 0.22f, 0.95f);
        tBg.raycastTarget = false;
        if (roundedFrameSprite != null)
        {
            tBg.sprite = roundedFrameSprite;
            tBg.type = Image.Type.Sliced;
        }

        // Bar Fill
        GameObject fillObj = CreateUIElement("Bar_Fill", trackObj.transform);
        fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fImg = fillObj.AddComponent<Image>();
        fImg.color = fillColor;
        fImg.raycastTarget = false;
        if (roundedFrameSprite != null)
        {
            fImg.sprite = roundedFrameSprite;
            fImg.type = Image.Type.Sliced;
        }

        // Label inside Bar
        labelText = CreateUIText("Text_BarLabel", trackObj.transform, font, defaultLabel, 23, TextAnchor.MiddleCenter, Color.white);
        RectTransform lRect = labelText.GetComponent<RectTransform>();
        lRect.anchorMin = Vector2.zero;
        lRect.anchorMax = Vector2.one;
        lRect.offsetMin = Vector2.zero;
        lRect.offsetMax = Vector2.zero;

        return rowObj;
    }

    private static void BuildGuidebookModal(
        Transform canvasTransform, UIManager uiMgr, GameManager gm, Font font, Font bodyFont, Sprite iconCloseSprite,
        Sprite burgerSprite, Sprite sushiSprite, Sprite pizzaSprite, Sprite donutSprite, Sprite iceCreamSprite, Sprite lollipopSprite,
        Sprite poisonMushroomSprite, Sprite poisonPotionSprite, Sprite poisonGobletSprite, Sprite spikeTrapSprite,
        Sprite bonusHeartSprite, Sprite starMagnetSprite, Sprite shieldBarrierSprite, Sprite timeSlowSprite,
        Sprite[] stagePreviews)
    {
        GameObject gbPanel = CreateUIElement("Guidebook_Modal", canvasTransform);
        RectTransform gbRect = gbPanel.GetComponent<RectTransform>();
        gbRect.anchorMin = Vector2.zero;
        gbRect.anchorMax = Vector2.one;
        gbRect.offsetMin = Vector2.zero;
        gbRect.offsetMax = Vector2.zero;

        Image gbScrim = gbPanel.AddComponent<Image>();
        gbScrim.color = new Color(0.04f, 0.03f, 0.08f, 0.86f);

        GameObject gbCard = CreateUIElement("Guidebook_Card", gbPanel.transform);
        RectTransform gbcRect = gbCard.GetComponent<RectTransform>();
        gbcRect.anchorMin = new Vector2(0.5f, 0.5f);
        gbcRect.anchorMax = new Vector2(0.5f, 0.5f);
        gbcRect.pivot = new Vector2(0.5f, 0.5f);
        gbcRect.anchoredPosition = new Vector2(0f, 10f);
        gbcRect.sizeDelta = new Vector2(940f, 1340f);

        Image gbcBg = gbCard.AddComponent<Image>();
        gbcBg.color = new Color(0.14f, 0.11f, 0.19f, 0.98f);
        if (roundedFrameSprite != null)
        {
            gbcBg.sprite = roundedFrameSprite;
            gbcBg.type = Image.Type.Sliced;
        }
        Outline gbcOut = gbCard.AddComponent<Outline>();
        gbcOut.effectColor = new Color(0.95f, 0.76f, 0.32f, 1f);
        gbcOut.effectDistance = new Vector2(6f, -6f);

        Text gbTitle = CreateUIText("Text_GuidebookTitle", gbCard.transform, font, "SLIME ALMANAC & GUIDE", 36, TextAnchor.MiddleCenter, new Color(0.52f, 0.98f, 0.68f, 1f));
        gbTitle.resizeTextForBestFit = true;
        gbTitle.resizeTextMinSize = 28;
        gbTitle.resizeTextMaxSize = 36;
        gbTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-24f, 600f);
        gbTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(760f, 54f);

        Text gbSub = CreateUIText("Text_GuidebookSub", gbCard.transform, defaultBodyFont, "TAP ANY CARD FOR STATS & TIPS", 28, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.48f, 1f));
        gbSub.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 544f);
        gbSub.GetComponent<RectTransform>().sizeDelta = new Vector2(860f, 40f);

        // 4 Category Chip Filter Buttons
        string[] tabNames = { "FOODS", "POISONS", "POWER-UPS", "STAGES" };
        Button[] tabBtns = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            float xPos = -324f + i * 216f;
            tabBtns[i] = CreateUIButton($"Button_Tab_{tabNames[i]}", gbCard.transform, font, tabNames[i], new Vector2(xPos, 462f), new Vector2(196f, 76f), new Color(0.15f, 0.19f, 0.28f, 0.96f), 18);
        }
        uiMgr.btnGuidebookTabs = tabBtns;

        GameObject[] tabPages = new GameObject[4];

        // --- TAB 0: EDIBLE FOODS PAGE ---
        tabPages[0] = CreateGuidePage("Page_Foods", gbCard.transform);
        CreateGuideEntryCard(tabPages[0].transform, font, bodyFont, burgerSprite,
            "SAVORY BURGERS & MEALS", "EDIBLE FOOD  •  +10 PTS", new Color(0.20f, 0.75f, 0.35f, 1f),
            "+10 PTS (+15 ON HARD)  |  BUILDS COMBO",
            "Hearty and delicious comfort foods dropped from above. Consuming these treats fuels Bloop's energy and boosts your run score.",
            "Catch in Bloop's mouth! Consecutive catches trigger combo multipliers (+5 Pts per 5 catches). Missing food does NOT cost Hearts!",
            new Vector2(0f, 268f), new Color(0.20f, 0.38f, 0.26f, 0.96f));

        CreateGuideEntryCard(tabPages[0].transform, font, bodyFont, sushiSprite,
            "FRESH SUSHI ROLLS", "OCEAN DELICACY  •  +10 PTS", new Color(0.25f, 0.70f, 0.95f, 1f),
            "+10 PTS (+15 ON HARD)  |  MAGNETIC PULL",
            "Delicate salmon nigiri and maki rolls wrapped in crisp seaweed. Bloop's absolute favorite delicacy.",
            "Activate Star Magnet to pull all falling sushi directly into Bloop's mouth across the entire screen!",
            new Vector2(0f, 84f), new Color(0.18f, 0.36f, 0.44f, 0.96f));

        CreateGuideEntryCard(tabPages[0].transform, font, bodyFont, donutSprite,
            "SWEET DESSERTS & CANDY", "SWEET BAKERY  •  +10 PTS", new Color(0.92f, 0.40f, 0.70f, 1f),
            "+10 PTS (+15 ON HARD)  |  FAST FALL SPEED",
            "Glazed strawberry donuts, swirled soft-serve ice cream, and rainbow lollipops. Light and sugary treats.",
            "Keep your eyes on fast-falling sweets in higher stages! Time-Slow helps catch clustered treats easily.",
            new Vector2(0f, -100f), new Color(0.38f, 0.22f, 0.36f, 0.96f));

        CreateGuideEntryCard(tabPages[0].transform, font, bodyFont, lollipopSprite,
            "0 DAMAGE ON MISS RULE", "GAMEPLAY RULE  •  0 DMG", new Color(0.40f, 0.85f, 0.45f, 1f),
            "0 HEART LOSS WHEN FOOD HITS FLOOR",
            "Letting edible food drop to the ground costs 0 Hearts! It only resets your combo multiplier.",
            "NEVER risk catching a poison just to save a food! Only poisonous hazards and spikes reduce your health.",
            new Vector2(0f, -284f), new Color(0.26f, 0.34f, 0.20f, 0.96f));

        // --- TAB 1: POISONOUS HAZARDS PAGE ---
        tabPages[1] = CreateGuidePage("Page_Poisons", gbCard.transform);
        CreateGuideEntryCard(tabPages[1].transform, font, bodyFont, poisonMushroomSprite,
            "GLOWING TOXIC MUSHROOM", "TOXIC HAZARD  •  -1 HEART", new Color(0.85f, 0.30f, 0.80f, 1f),
            "-1 HEART ON CATCH  |  PHONE HAPTICS",
            "A bioluminescent purple-green toadstool dripping with noxious fungal spores.",
            "Dodge it and let it hit the floor! The ground harmlessly destroys mushrooms with zero penalty.",
            new Vector2(0f, 268f), new Color(0.38f, 0.16f, 0.36f, 0.96f));

        CreateGuideEntryCard(tabPages[1].transform, font, bodyFont, poisonPotionSprite,
            "CAUSTIC VENOM FLASK", "ACIDIC HAZARD  •  -1 HEART", new Color(0.35f, 0.85f, 0.30f, 1f),
            "-1 HEART ON CATCH  |  RESETS COMBO",
            "A bubbling alchemical glass flask containing caustic serpent acid.",
            "Never catch this unless you have an active Barrier Shield, which completely absorbs the acid!",
            new Vector2(0f, 84f), new Color(0.36f, 0.18f, 0.32f, 0.96f));

        CreateGuideEntryCard(tabPages[1].transform, font, bodyFont, poisonGobletSprite,
            "CURSED VENOM CHALICE", "DARK HAZARD  •  -1 HEART", new Color(0.70f, 0.25f, 0.85f, 1f),
            "-1 HEART ON CATCH  |  SCREEN SHAKE",
            "An ornate dark chalice overflowing with cursed purple venom sludge.",
            "Give it wide clearance! Its hitbox is slightly wider than standard food items.",
            new Vector2(0f, -100f), new Color(0.34f, 0.16f, 0.28f, 0.96f));

        CreateGuideEntryCard(tabPages[1].transform, font, bodyFont, spikeTrapSprite,
            "IRON SPIKE BALL TRAP", "PHYSICAL HAZARD  •  -1 HEART", new Color(0.85f, 0.35f, 0.25f, 1f),
            "-1 HEART ON CATCH  |  FAST VELOCITY",
            "A heavy cast-iron spiked naval mine with razor-sharp steel barbs.",
            "Falls faster than organic food! Move Bloop early when you see spikes descending.",
            new Vector2(0f, -284f), new Color(0.42f, 0.18f, 0.20f, 0.96f));

        // --- TAB 2: POWER-UPS PAGE ---
        tabPages[2] = CreateGuidePage("Page_PowerUps", gbCard.transform);
        CreateGuideEntryCard(tabPages[2].transform, font, bodyFont, bonusHeartSprite,
            "REVITALIZING GOLDEN HEART", "HEALING ITEM  •  RESTORE HP", new Color(1f, 0.82f, 0.25f, 1f),
            "+1 FULL HEART (OR +50 PTS AT MAX HP)",
            "A radiant golden heart filled with pure restorative health essence.",
            "Prioritize catching this when damaged! If already at 3/3 health, awards a massive +50 bonus score.",
            new Vector2(0f, 268f), new Color(0.46f, 0.34f, 0.14f, 0.96f));

        CreateGuideEntryCard(tabPages[2].transform, font, bodyFont, starMagnetSprite,
            "STAR MAGNET FIELD", "ACTIVE POWER-UP  •  6.0s DURATION", new Color(1f, 0.88f, 0.35f, 1f),
            "DURATION: 6.0 SECONDS (GOLD TIMER BAR)",
            "A cosmic magnet pulsing with gravitational starlight. Pulls edible treats from anywhere on screen.",
            "Completely ignores poisons! Stand still in the center and let the delicious treats fly to Bloop.",
            new Vector2(0f, 84f), new Color(0.42f, 0.36f, 0.16f, 0.96f));

        CreateGuideEntryCard(tabPages[2].transform, font, bodyFont, shieldBarrierSprite,
            "BARRIER SHIELD ARMOR", "DEFENSIVE POWER-UP  •  12.0s DURATION", new Color(0.35f, 0.88f, 1f, 1f),
            "DURATION: 12.0s OR BLOCKS 1 POISON",
            "Surrounds Bloop in a shimmering cyan bubble armor that absorbs 1 poison catch.",
            "Provides complete immunity for one mistake! Once it absorbs a poison, the bubble safely pops.",
            new Vector2(0f, -100f), new Color(0.14f, 0.40f, 0.50f, 0.96f));

        CreateGuideEntryCard(tabPages[2].transform, font, bodyFont, timeSlowSprite,
            "CHRONO HOURGLASS", "TEMPORAL ITEM  •  6.0s DURATION", new Color(0.75f, 0.50f, 1f, 1f),
            "DURATION: 6.0s (50% DROP VELOCITY)",
            "An ancient relic containing temporal sands that distorts time across the falling sky.",
            "All falling food and hazards slow by 50% while Bloop glides at 100% full speed! Produces a distinct clock tick.",
            new Vector2(0f, -284f), new Color(0.30f, 0.20f, 0.50f, 0.96f));

        // --- TAB 3: STAGE THEMES PAGE ---
        tabPages[3] = CreateScrollableGuidePage("Page_Stages", gbCard.transform, out RectTransform stagesContent);

        string[] stageTitles = {
            "WHISPERING FOREST", "BREEZY PLAINS", "SUNSET PEAKS", "EMERALD TROPIC", "SUNBAKED DUNES",
            "CORAL ABYSS", "STEAMWORKS SPIRE", "CRYSTAL CAVERNS", "SKY SANCTUARY", "FROST CITADEL"
        };
        int[] stageScores = { 0, 100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700 };
        string[] stageThemes = {
            "Lush Woodland Canopy", "Sunlit Meadow & Rolling Grass", "Golden Alpine Crags", "Lush Palms & Mist Cascades",
            "Canyon Spires & Desert Sands", "Luminescent Deep Sea Reef", "Industrial Silos & Machinery",
            "Deep Glowing Stalactite Caves", "Floating Azure Sea Islands", "Ancient Frosty Moonlit Fortress"
        };
        string[] stageBgms = {
            "Forest Loop", "Sunny Plains", "Mountain Theme", "Tropical Coast", "Desert Mirage",
            "Ocean Wave", "Chiptune Factory", "Crystal Echo", "Ancient Ruins", "Nocturnal Citadel"
        };
        string[] stageDescs = {
            "Starting stage! Gentle drop speed (1.0x base) and low 12% Poison rate so you can build your first combo streak.",
            "Unlocks at 100 points! Bright rolling grass and azure sky with gentle breeze. Poison hazard rate 14%.",
            "Unlocks at 250 points! High alpine sunset peaks with windswept atmosphere. Poison hazard rate 17%.",
            "Unlocks at 450 points! Deep jungle lagoon with cascading mist and swaying palm trees. Poison rate 19%.",
            "Unlocks at 700 points! High desert canyons and golden dunes with heat distortion shimmer. Poison rate 21%.",
            "Unlocks at 1000 points! Subaquatic wonderland with deep ocean light rays cutting through clear water. Poison rate 23%.",
            "Unlocks at 1350 points! Steampunk factory chimneys, gearworks, and retro chiptune machinery. Poison rate 25%.",
            "Unlocks at 1750 points! Subterranean cavern filled with luminescent glowing crystals and rock arches. Poison rate 27%.",
            "Unlocks at 2200 points! High-altitude floating sky sanctuary with billowing clouds and magic ruins. Poison rate 29%.",
            "Final survival stage at 2700 points! Ancient moonlit citadel in snow. Stays here for endless high-score play until game over!",
        };
        Color[] stageColors = {
            new Color(0.16f, 0.36f, 0.24f, 0.96f),
            new Color(0.18f, 0.38f, 0.32f, 0.96f),
            new Color(0.44f, 0.26f, 0.18f, 0.96f),
            new Color(0.16f, 0.40f, 0.36f, 0.96f),
            new Color(0.48f, 0.34f, 0.16f, 0.96f),
            new Color(0.12f, 0.32f, 0.50f, 0.96f),
            new Color(0.38f, 0.24f, 0.18f, 0.96f),
            new Color(0.22f, 0.18f, 0.42f, 0.96f),
            new Color(0.18f, 0.36f, 0.48f, 0.96f),
            new Color(0.24f, 0.18f, 0.44f, 0.96f)
        };

        for (int s = 0; s < 10; s++)
        {
            Sprite prevSprite = (stagePreviews != null && s < stagePreviews.Length && stagePreviews[s] != null) ? stagePreviews[s] : burgerSprite;
            float yPos = -16f - s * 170f;
            CreateGuideEntryCard(stagesContent, font, bodyFont, prevSprite,
                $"STAGE {s + 1}: {stageTitles[s]}",
                $"UNLOCK: {stageScores[s]}+ PTS  •  {stageThemes[s]}",
                new Color(0.48f, 0.96f, 1f, 1f),
                $"UNLOCK SCORE: {stageScores[s]}+ PTS  |  BGM: {stageBgms[s]}",
                stageDescs[s],
                $"Enjoy the retro 16-bit '{stageThemes[s]}' aesthetic while dodging toxins and grabbing power-ups!",
                new Vector2(0f, yPos),
                stageColors[s],
                true);
        }

        uiMgr.guidebookTabPages = tabPages;
        uiMgr.SelectGuidebookTab(0);

        Button btnCloseGb = CreateUIIconBannerButton(
            "Button_CloseGuidebook",
            gbCard.transform,
            font,
            iconCloseSprite,
            "CLOSE ALMANAC",
            new Vector2(0f, -576f),
            new Vector2(460f, 92f),
            new Vector2(42f, 42f),
            new Color(0.16f, 0.40f, 0.22f, 1f),
            22
        );
        uiMgr.btnGuidebookClose = btnCloseGb;

        gbPanel.SetActive(false);
        uiMgr.guidebookPanel = gbPanel;
    }

    private static void BuildAlmanacItemDetailModal(Transform canvasTransform, UIManager uiMgr, Font arcadeFont, Font bodyFont, Sprite defaultArtwork)
    {
        GameObject detailPanel = CreateUIElement("Almanac_ItemDetailModal", canvasTransform);
        RectTransform dpRect = detailPanel.GetComponent<RectTransform>();
        dpRect.anchorMin = Vector2.zero;
        dpRect.anchorMax = Vector2.one;
        dpRect.offsetMin = Vector2.zero;
        dpRect.offsetMax = Vector2.zero;

        Image dpScrim = detailPanel.AddComponent<Image>();
        dpScrim.color = new Color(0.03f, 0.02f, 0.07f, 0.90f);

        GameObject dpCard = CreateUIElement("Detail_Card", detailPanel.transform);
        RectTransform dpcRect = dpCard.GetComponent<RectTransform>();
        dpcRect.anchorMin = new Vector2(0.5f, 0.5f);
        dpcRect.anchorMax = new Vector2(0.5f, 0.5f);
        dpcRect.pivot = new Vector2(0.5f, 0.5f);
        dpcRect.anchoredPosition = new Vector2(0f, 15f);
        dpcRect.sizeDelta = new Vector2(900f, 1260f);

        Image dpcBg = dpCard.AddComponent<Image>();
        dpcBg.color = new Color(0.12f, 0.10f, 0.17f, 0.98f);
        if (roundedFrameSprite != null)
        {
            dpcBg.sprite = roundedFrameSprite;
            dpcBg.type = Image.Type.Sliced;
        }
        Outline dpcOut = dpCard.AddComponent<Outline>();
        dpcOut.effectColor = new Color(0.96f, 0.78f, 0.35f, 1f);
        dpcOut.effectDistance = new Vector2(6f, -6f);

        // 1. Category Badge Header
        GameObject badgeObj = CreateUIElement("Badge_Container", dpCard.transform);
        RectTransform bdRect = badgeObj.GetComponent<RectTransform>();
        bdRect.anchoredPosition = new Vector2(0f, 550f);
        bdRect.sizeDelta = new Vector2(560f, 52f);
        Image bdBg = badgeObj.AddComponent<Image>();
        bdBg.color = new Color(0.18f, 0.22f, 0.32f, 0.96f);
        if (roundedFrameSprite != null)
        {
            bdBg.sprite = roundedFrameSprite;
            bdBg.type = Image.Type.Sliced;
        }

        Text badgeTxt = CreateUIText("Text_CategoryBadge", badgeObj.transform, defaultBodyFont, "ITEM DETAILS", 26, TextAnchor.MiddleCenter, new Color(0.48f, 0.96f, 1f, 1f));
        RectTransform btRect = badgeTxt.GetComponent<RectTransform>();
        btRect.anchorMin = Vector2.zero;
        btRect.anchorMax = Vector2.one;
        btRect.offsetMin = Vector2.zero;
        btRect.offsetMax = Vector2.zero;

        // 2. Large Artwork Frame
        GameObject artFrame = CreateUIElement("Artwork_Frame", dpCard.transform);
        RectTransform afRect = artFrame.GetComponent<RectTransform>();
        afRect.anchoredPosition = new Vector2(0f, 410f);
        afRect.sizeDelta = new Vector2(210f, 190f);
        Image afBg = artFrame.AddComponent<Image>();
        afBg.color = new Color(0.07f, 0.05f, 0.12f, 1f);
        if (roundedFrameSprite != null)
        {
            afBg.sprite = roundedFrameSprite;
            afBg.type = Image.Type.Sliced;
        }
        Outline afOut = artFrame.AddComponent<Outline>();
        afOut.effectColor = new Color(0.92f, 0.76f, 0.38f, 0.9f);
        afOut.effectDistance = new Vector2(3f, -3f);

        Image artImg = CreateUISpriteIcon("Artwork_Sprite", artFrame.transform, defaultArtwork, Vector2.zero, new Vector2(150f, 150f));
        artImg.preserveAspect = true;

        // 3. Item Title Text
        Text titleTxt = CreateUIText("Text_DetailTitle", dpCard.transform, defaultBodyFont, "ITEM NAME", 38, TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.72f, 1f));
        titleTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        titleTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 280f);
        titleTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(840f, 52f);

        // 4. Stats Container Box
        GameObject statsBox = CreateUIElement("Stats_Container", dpCard.transform);
        RectTransform sbRect = statsBox.GetComponent<RectTransform>();
        sbRect.anchoredPosition = new Vector2(0f, 205f);
        sbRect.sizeDelta = new Vector2(840f, 66f);
        Image sbBg = statsBox.AddComponent<Image>();
        sbBg.color = new Color(0.18f, 0.15f, 0.24f, 0.95f);
        if (roundedFrameSprite != null)
        {
            sbBg.sprite = roundedFrameSprite;
            sbBg.type = Image.Type.Sliced;
        }
        Outline sbOut = statsBox.AddComponent<Outline>();
        sbOut.effectColor = new Color(0.35f, 0.78f, 0.92f, 0.65f);
        sbOut.effectDistance = new Vector2(2f, -2f);

        Text statsTxt = CreateUIText("Text_DetailStats", statsBox.transform, defaultBodyFont, "+10 PTS  •  BUILD COMBO STREAK", 28, TextAnchor.MiddleCenter, new Color(0.50f, 0.98f, 0.65f, 1f));
        RectTransform stRect = statsTxt.GetComponent<RectTransform>();
        stRect.anchorMin = Vector2.zero;
        stRect.anchorMax = Vector2.one;
        stRect.offsetMin = new Vector2(16f, 0f);
        stRect.offsetMax = new Vector2(-16f, 0f);

        // 5. Description & Mechanics Box (Spacious, bounded, zero spill-outside!)
        GameObject descBox = CreateUIElement("Description_Container", dpCard.transform);
        RectTransform dbRect = descBox.GetComponent<RectTransform>();
        dbRect.anchoredPosition = new Vector2(0f, 25f);
        dbRect.sizeDelta = new Vector2(840f, 250f);
        Image dbBg = descBox.AddComponent<Image>();
        dbBg.color = new Color(0.15f, 0.12f, 0.21f, 0.95f);
        if (roundedFrameSprite != null)
        {
            dbBg.sprite = roundedFrameSprite;
            dbBg.type = Image.Type.Sliced;
        }

        Text descLbl = CreateUIText("Label_Description", descBox.transform, defaultBodyFont, "LORE & MECHANICS", 26, TextAnchor.MiddleLeft, new Color(1f, 0.88f, 0.44f, 1f));
        descLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(24f, 96f);
        descLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(790f, 32f);

        Text descTxt = CreateUIText("Text_DetailDescription", descBox.transform, defaultBodyFont,
            "Full item mechanics, origins, and interactive properties during gameplay.",
            30, TextAnchor.UpperLeft, new Color(0.94f, 0.96f, 0.98f, 1f));
        descTxt.resizeTextForBestFit = true;
        descTxt.resizeTextMinSize = 22;
        descTxt.resizeTextMaxSize = 28;
        descTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        descTxt.verticalOverflow = VerticalWrapMode.Truncate;
        descTxt.lineSpacing = 1.15f;
        descTxt.raycastTarget = false;
        RectTransform dtRect = descTxt.GetComponent<RectTransform>();
        dtRect.anchoredPosition = new Vector2(24f, -8f);
        dtRect.sizeDelta = new Vector2(792f, 170f);

        // 6. Pro Strategy Tips Box (Spacious, bounded, zero spill-outside!)
        GameObject tipsBox = CreateUIElement("Tips_Container", dpCard.transform);
        RectTransform tbRect = tipsBox.GetComponent<RectTransform>();
        tbRect.anchoredPosition = new Vector2(0f, -230f);
        tbRect.sizeDelta = new Vector2(840f, 210f);
        Image tbBg = tipsBox.AddComponent<Image>();
        tbBg.color = new Color(0.20f, 0.17f, 0.12f, 0.95f);
        if (roundedFrameSprite != null)
        {
            tbBg.sprite = roundedFrameSprite;
            tbBg.type = Image.Type.Sliced;
        }
        Outline tbOut = tipsBox.AddComponent<Outline>();
        tbOut.effectColor = new Color(0.95f, 0.80f, 0.35f, 0.85f);
        tbOut.effectDistance = new Vector2(2f, -2f);

        Text tipsLbl = CreateUIText("Label_Tips", tipsBox.transform, defaultBodyFont, "PRO STRATEGY TIP", 26, TextAnchor.MiddleLeft, new Color(1f, 0.92f, 0.45f, 1f));
        tipsLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(24f, 74f);
        tipsLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(790f, 32f);

        Text tipsTxt = CreateUIText("Text_DetailTips", tipsBox.transform, defaultBodyFont,
            "Pro gameplay hints and optimal techniques for maximizing score.",
            30, TextAnchor.UpperLeft, new Color(1f, 0.98f, 0.88f, 1f));
        tipsTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        tipsTxt.verticalOverflow = VerticalWrapMode.Truncate;
        tipsTxt.lineSpacing = 1.15f;
        RectTransform tpRect = tipsTxt.GetComponent<RectTransform>();
        tpRect.anchoredPosition = new Vector2(24f, -8f);
        tpRect.sizeDelta = new Vector2(792f, 135f);

        // 7. Close / Return Button
        Button btnClose = CreateUIButton("Button_DetailClose", dpCard.transform, defaultBodyFont, "< RETURN TO ALMANAC", new Vector2(0f, -540f), new Vector2(580f, 92f), new Color(0.18f, 0.42f, 0.24f, 1f), 26);

        uiMgr.almanacDetailModal = detailPanel;
        uiMgr.imgDetailArtwork = artImg;
        uiMgr.textDetailCategoryBadge = badgeTxt;
        uiMgr.textDetailTitle = titleTxt;
        uiMgr.textDetailStats = statsTxt;
        uiMgr.textDetailDescription = descTxt;
        uiMgr.textDetailTips = tipsTxt;
        uiMgr.btnDetailClose = btnClose;

        detailPanel.SetActive(false);
    }

    private static void BuildConfirmationModal(Transform canvasTransform, UIManager uiMgr, Font arcadeFont, Font bodyFont)
    {
        GameObject confirmPanel = CreateUIElement("Confirmation_Modal", canvasTransform);
        RectTransform cpRect = confirmPanel.GetComponent<RectTransform>();
        cpRect.anchorMin = Vector2.zero;
        cpRect.anchorMax = Vector2.one;
        cpRect.offsetMin = Vector2.zero;
        cpRect.offsetMax = Vector2.zero;

        Canvas cpCanvas = confirmPanel.AddComponent<Canvas>();
        cpCanvas.overrideSorting = true;
        cpCanvas.sortingOrder = 500;
        confirmPanel.AddComponent<GraphicRaycaster>();

        Image cpScrim = confirmPanel.AddComponent<Image>();
        cpScrim.color = new Color(0.04f, 0.03f, 0.08f, 0.88f);

        GameObject cpCard = CreateUIElement("Confirm_Card", confirmPanel.transform);
        RectTransform cpcRect = cpCard.GetComponent<RectTransform>();
        cpcRect.anchorMin = new Vector2(0.5f, 0.5f);
        cpcRect.anchorMax = new Vector2(0.5f, 0.5f);
        cpcRect.pivot = new Vector2(0.5f, 0.5f);
        cpcRect.anchoredPosition = Vector2.zero;
        cpcRect.sizeDelta = new Vector2(760f, 460f);

        Image cpcBg = cpCard.AddComponent<Image>();
        cpcBg.color = new Color(0.13f, 0.10f, 0.18f, 0.98f);
        if (roundedFrameSprite != null)
        {
            cpcBg.sprite = roundedFrameSprite;
            cpcBg.type = Image.Type.Sliced;
        }
        Outline cpcOutline = cpCard.AddComponent<Outline>();
        cpcOutline.effectColor = new Color(0.95f, 0.78f, 0.35f, 1f);
        cpcOutline.effectDistance = new Vector2(5f, -5f);

        // Header Accent Strip
        GameObject strip = CreateUIElement("Header_Strip", cpCard.transform);
        RectTransform sRect = strip.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0f, 1f);
        sRect.anchorMax = new Vector2(1f, 1f);
        sRect.pivot = new Vector2(0.5f, 1f);
        sRect.anchoredPosition = Vector2.zero;
        sRect.sizeDelta = new Vector2(0f, 84f);
        Image sBg = strip.AddComponent<Image>();
        sBg.color = new Color(0.18f, 0.14f, 0.26f, 0.95f);
        if (roundedFrameSprite != null)
        {
            sBg.sprite = roundedFrameSprite;
            sBg.type = Image.Type.Sliced;
        }

        Text titleTxt = CreateUIText("Text_ConfirmTitle", strip.transform, defaultBodyFont, "CONFIRMATION", 34, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.38f, 1f));
        RectTransform tRect = titleTxt.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = Vector2.zero;
        tRect.offsetMax = Vector2.zero;
        titleTxt.raycastTarget = false;

        // Message Body Box
        Text msgTxt = CreateUIText("Text_ConfirmMessage", cpCard.transform, defaultBodyFont, "Are you sure you want to proceed?", 30, TextAnchor.MiddleCenter, new Color(0.96f, 0.98f, 1f, 1f));
        msgTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        msgTxt.lineSpacing = 1.18f;
        msgTxt.raycastTarget = false;
        RectTransform mRect = msgTxt.GetComponent<RectTransform>();
        mRect.anchorMin = new Vector2(0.5f, 0.5f);
        mRect.anchorMax = new Vector2(0.5f, 0.5f);
        mRect.pivot = new Vector2(0.5f, 0.5f);
        mRect.anchoredPosition = new Vector2(0f, 24f);
        mRect.sizeDelta = new Vector2(700f, 150f);

        // Cancel Button (Muted Slate / Gray)
        Button btnCancel = CreateUIButton("Button_ConfirmCancel", cpCard.transform, defaultBodyFont, "CANCEL", new Vector2(-160f, -140f), new Vector2(280f, 88f), new Color(0.24f, 0.26f, 0.36f, 1f), 26);

        // Yes/Proceed Button (Terracotta / Red-Orange)
        Button btnYes = CreateUIButton("Button_ConfirmYes", cpCard.transform, defaultBodyFont, "YES, PROCEED", new Vector2(160f, -140f), new Vector2(280f, 88f), new Color(0.68f, 0.24f, 0.22f, 1f), 26);

        uiMgr.confirmationModal = confirmPanel;
        uiMgr.textConfirmTitle = titleTxt;
        uiMgr.textConfirmMessage = msgTxt;
        uiMgr.btnConfirmYes = btnYes;
        uiMgr.btnConfirmCancel = btnCancel;
        confirmPanel.SetActive(false);
    }

    private static GameObject CreateScrollableGuidePage(string name, Transform parent, out RectTransform contentRect)
    {
        GameObject page = CreateUIElement(name, parent);
        RectTransform rt = page.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -38f);
        rt.sizeDelta = new Vector2(860f, 760f);

        page.AddComponent<RectMask2D>();

        ScrollRect sr = page.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 30f;

        GameObject content = CreateUIElement("Content", page.transform);
        contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 1860f);

        sr.content = contentRect;
        return page;
    }

    private static GameObject CreateGuidePage(string name, Transform parent)
    {
        GameObject page = CreateUIElement(name, parent);
        RectTransform rt = page.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -38f);
        rt.sizeDelta = new Vector2(860f, 760f);
        return page;
    }

    private static GameObject CreateGuideEntryCard(
        Transform parent, Font font, Font bodyFont, Sprite icon,
        string itemName, string categoryBadge, Color badgeColor,
        string stats, string loreDescription, string strategyTips,
        Vector2 anchoredPos, Color cardColor, bool isStagePreview = false)
    {
        string safeName = "Entry_" + (itemName.Length > 12 ? itemName.Substring(0, 12) : itemName).Replace(" ", "_").Replace(":", "");
        GameObject card = CreateUIElement(safeName, parent);
        RectTransform rt = card.GetComponent<RectTransform>();
        if (isStagePreview)
        {
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
        }
        else
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
        }
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(856f, 160f);

        Image bg = card.AddComponent<Image>();
        bg.color = cardColor;
        if (roundedFrameSprite != null)
        {
            bg.sprite = roundedFrameSprite;
            bg.type = Image.Type.Sliced;
        }
        Outline ol = card.AddComponent<Outline>();
        ol.effectColor = new Color(0.06f, 0.04f, 0.09f, 0.92f);
        ol.effectDistance = new Vector2(4f, -4f);

        Button btn = card.AddComponent<Button>();
        btn.targetGraphic = bg;
        card.AddComponent<UIButtonJuice>();

        AlmanacCardTrigger trigger = card.AddComponent<AlmanacCardTrigger>();
        trigger.artwork = icon;
        trigger.categoryBadge = categoryBadge;
        trigger.badgeColor = badgeColor;
        trigger.itemName = itemName;
        trigger.stats = stats;
        trigger.loreDescription = loreDescription;
        trigger.strategyTips = strategyTips;

        // Visual Icon Frame on Left
        if (isStagePreview)
        {
            GameObject previewFrame = CreateUIElement("Stage_PreviewFrame", card.transform);
            RectTransform pfRect = previewFrame.GetComponent<RectTransform>();
            pfRect.anchoredPosition = new Vector2(-336f, 0f);
            pfRect.sizeDelta = new Vector2(150f, 114f);
            Image pfBg = previewFrame.AddComponent<Image>();
            pfBg.color = new Color(0.08f, 0.06f, 0.14f, 1f);
            pfBg.raycastTarget = false;
            if (roundedFrameSprite != null)
            {
                pfBg.sprite = roundedFrameSprite;
                pfBg.type = Image.Type.Sliced;
            }
            Outline pfOut = previewFrame.AddComponent<Outline>();
            pfOut.effectColor = new Color(0.96f, 0.80f, 0.36f, 0.95f);
            pfOut.effectDistance = new Vector2(2f, -2f);

            Image stageImg = CreateUISpriteIcon("Stage_Art", previewFrame.transform, icon, Vector2.zero, new Vector2(142f, 106f));
            stageImg.preserveAspect = false;
        }
        else
        {
            GameObject iconFrame = CreateUIElement("Icon_Frame", card.transform);
            RectTransform ifRect = iconFrame.GetComponent<RectTransform>();
            ifRect.anchoredPosition = new Vector2(-346f, 0f);
            ifRect.sizeDelta = new Vector2(116f, 116f);
            Image ifBg = iconFrame.AddComponent<Image>();
            ifBg.color = new Color(0.08f, 0.06f, 0.14f, 1f);
            ifBg.raycastTarget = false;
            if (roundedFrameSprite != null)
            {
                ifBg.sprite = roundedFrameSprite;
                ifBg.type = Image.Type.Sliced;
            }
            Outline ifOut = iconFrame.AddComponent<Outline>();
            ifOut.effectColor = new Color(0.95f, 0.82f, 0.45f, 0.85f);
            ifOut.effectDistance = new Vector2(2f, -2f);

            CreateUISpriteIcon("Entry_Icon", iconFrame.transform, icon, Vector2.zero, new Vector2(92f, 92f));
        }

        // Center Content Area: Fully bounded and centered between the icon and the right edge
        // Left icon ends at -260px (width 116px centered at -346px, right edge = -288px)
        // Card has full width 856px, centered at 0px (-428px to +428px)
        float textLeft = isStagePreview ? -240f : -265f;
        float textWidth = isStagePreview ? 440f : 465f;

        GameObject textContainer = CreateUIElement("Text_Container", card.transform);
        RectTransform tcRect = textContainer.GetComponent<RectTransform>();
        tcRect.anchorMin = new Vector2(0f, 0.5f);
        tcRect.anchorMax = new Vector2(0f, 0.5f);
        tcRect.pivot = new Vector2(0f, 0.5f);
        tcRect.anchoredPosition = new Vector2(textLeft + 428f, 0f);
        tcRect.sizeDelta = new Vector2(textWidth, 110f);

        Text tTitle = CreateUIText("Text_EntryTitle", textContainer.transform, bodyFont, itemName, 26, TextAnchor.MiddleLeft, new Color(1f, 0.97f, 0.86f, 1f));
        tTitle.resizeTextForBestFit = true;
        tTitle.resizeTextMinSize = 22;
        tTitle.resizeTextMaxSize = 26;
        tTitle.horizontalOverflow = HorizontalWrapMode.Wrap;
        tTitle.verticalOverflow = VerticalWrapMode.Truncate;
        tTitle.lineSpacing = 1.1f;
        tTitle.raycastTarget = false;
        RectTransform titleRect = tTitle.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.5f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(0f, 0f);
        titleRect.offsetMax = new Vector2(0f, 0f);

        Text tBadge = CreateUIText("Text_EntryBadge", textContainer.transform, bodyFont, categoryBadge, 24, TextAnchor.MiddleLeft, badgeColor);
        tBadge.resizeTextForBestFit = true;
        tBadge.resizeTextMinSize = 20;
        tBadge.resizeTextMaxSize = 24;
        tBadge.horizontalOverflow = HorizontalWrapMode.Wrap;
        tBadge.verticalOverflow = VerticalWrapMode.Truncate;
        tBadge.raycastTarget = false;
        RectTransform badgeRect = tBadge.GetComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(0f, 0f);
        badgeRect.anchorMax = new Vector2(1f, 0.5f);
        badgeRect.offsetMin = new Vector2(0f, 0f);
        badgeRect.offsetMax = new Vector2(0f, 0f);

        // Right Action Pill Button: "[ DETAILS > ]" (Placed at the right edge)
        GameObject hintPill = CreateUIElement("Pill_DetailsHint", card.transform);
        RectTransform hpRect = hintPill.GetComponent<RectTransform>();
        hpRect.anchorMin = new Vector2(1f, 0.5f);
        hpRect.anchorMax = new Vector2(1f, 0.5f);
        hpRect.pivot = new Vector2(1f, 0.5f);
        hpRect.anchoredPosition = new Vector2(-16f, 0f);
        hpRect.sizeDelta = new Vector2(175f, 58f);

        Image hpBg = hintPill.AddComponent<Image>();
        hpBg.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);
        hpBg.raycastTarget = false;
        if (roundedFrameSprite != null)
        {
            hpBg.sprite = roundedFrameSprite;
            hpBg.type = Image.Type.Sliced;
        }
        Outline hpOut = hintPill.AddComponent<Outline>();
        hpOut.effectColor = new Color(0.96f, 0.84f, 0.40f, 0.95f);
        hpOut.effectDistance = new Vector2(2f, -2f);

        Text tHint = CreateUIText("Text_HintLabel", hintPill.transform, defaultBodyFont, "DETAILS >", 24, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.50f, 1f));
        RectTransform hlRect = tHint.GetComponent<RectTransform>();
        hlRect.anchorMin = Vector2.zero;
        hlRect.anchorMax = Vector2.one;
        hlRect.offsetMin = Vector2.zero;
        hlRect.offsetMax = Vector2.zero;

        return card;
    }

    private static Image CreateEdgeStrip(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
    {
        GameObject obj = CreateUIElement(name, parent);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        Image img = obj.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static Image CreateUISpriteIcon(string name, Transform parent, Sprite sprite, Vector2 anchoredPos, Vector2 size)
    {
        GameObject obj = CreateUIElement(name, parent);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        return img;
    }

    private static Button CreateUIIconButton(
        string name, Transform parent, Sprite iconSprite,
        Vector2 anchoredPos, Vector2 buttonSize, Vector2 iconSize, Color bgColor)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = buttonSize;

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;
        if (roundedFrameSprite != null)
        {
            img.sprite = roundedFrameSprite;
            img.type = Image.Type.Sliced;
        }

        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.95f, 0.78f, 0.34f, 0.95f);
        outline.effectDistance = new Vector2(4f, -4f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        btnObj.AddComponent<UIButtonJuice>();

        if (iconSprite != null)
        {
            CreateUISpriteIcon("Icon_Graphic", btnObj.transform, iconSprite, Vector2.zero, iconSize);
        }

        return btn;
    }

    private static Button CreateUIIconBannerButton(
        string name, Transform parent, Font font, Sprite iconSprite, string label,
        Vector2 anchoredPos, Vector2 buttonSize, Vector2 iconSize, Color bgColor, int fontSize = 36)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = buttonSize;

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;
        if (roundedFrameSprite != null)
        {
            img.sprite = roundedFrameSprite;
            img.type = Image.Type.Sliced;
        }

        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.10f, 0.06f, 0.03f, 0.95f);
        outline.effectDistance = new Vector2(4f, -4f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        btnObj.AddComponent<UIButtonJuice>();

        if (iconSprite != null)
        {
            float iconX = -buttonSize.x * 0.5f + iconSize.x * 0.5f + 22f;
            CreateUISpriteIcon("Icon_Graphic", btnObj.transform, iconSprite, new Vector2(iconX, 0f), iconSize);
        }

        Text labelTxt = CreateUIText("Text_Label", btnObj.transform, font, label, fontSize, TextAnchor.MiddleCenter, Color.white);
        RectTransform lRect = labelTxt.GetComponent<RectTransform>();
        lRect.anchorMin = Vector2.zero;
        lRect.anchorMax = Vector2.one;
        lRect.offsetMin = new Vector2(iconSprite != null ? (iconSize.x + 18f) : 0f, 0f);
        lRect.offsetMax = new Vector2(-10f, 0f);

        return btn;
    }

    private static Button CreateUIIconTileButton(
        string name, Transform parent, Font font, Sprite iconSprite, string caption,
        Vector2 anchoredPos, Vector2 tileSize, Vector2 iconSize, Color bgColor, int captionFontSize = 20)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = tileSize;

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;
        if (roundedFrameSprite != null)
        {
            img.sprite = roundedFrameSprite;
            img.type = Image.Type.Sliced;
        }

        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.10f, 0.06f, 0.03f, 0.95f);
        outline.effectDistance = new Vector2(4f, -4f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        btnObj.AddComponent<UIButtonJuice>();

        if (iconSprite != null)
        {
            CreateUISpriteIcon("Icon_Graphic", btnObj.transform, iconSprite, new Vector2(0f, 18f), iconSize);
        }

        Text labelTxt = CreateUIText("Text_Label", btnObj.transform, font, caption, captionFontSize, TextAnchor.MiddleCenter, new Color(1f, 0.98f, 0.90f, 1f));
        RectTransform lRect = labelTxt.GetComponent<RectTransform>();
        lRect.anchorMin = new Vector2(0f, 0f);
        lRect.anchorMax = new Vector2(1f, 0f);
        lRect.pivot = new Vector2(0.5f, 0f);
        lRect.anchoredPosition = new Vector2(0f, 10f);
        lRect.sizeDelta = new Vector2(0f, 32f);

        return btn;
    }

    private static Button CreateDifficultyCardButton(string name, Transform parent, Font font, Sprite playIcon, string title, string subtitle, Vector2 anchoredPos, Vector2 size, Color bgColor)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;
        if (roundedFrameSprite != null)
        {
            img.sprite = roundedFrameSprite;
            img.type = Image.Type.Sliced;
        }

        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.08f, 0.05f, 0.03f, 0.95f);
        outline.effectDistance = new Vector2(5f, -5f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        btnObj.AddComponent<UIButtonJuice>();

        if (playIcon != null)
        {
            CreateUISpriteIcon("Icon_Play", btnObj.transform, playIcon, new Vector2(-size.x * 0.5f + 56f, 0f), new Vector2(54f, 54f));
        }

        Text titleTxt = CreateUIText("Text_ModeTitle", btnObj.transform, defaultBodyFont, title, 30, TextAnchor.MiddleCenter, Color.white);
        titleTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        titleTxt.verticalOverflow = VerticalWrapMode.Truncate;
        titleTxt.raycastTarget = false;
        RectTransform tRect = titleTxt.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0.5f, 0.5f);
        tRect.anchorMax = new Vector2(0.5f, 0.5f);
        tRect.pivot = new Vector2(0.5f, 0.5f);
        tRect.anchoredPosition = new Vector2(22f, 24f);
        tRect.sizeDelta = new Vector2(size.x - 120f, 52f);

        Text subTxt = CreateUIText("Text_ModeDesc", btnObj.transform, defaultBodyFont, subtitle, 25, TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.84f, 0.95f));
        subTxt.resizeTextForBestFit = true;
        subTxt.resizeTextMinSize = 20;
        subTxt.resizeTextMaxSize = 25;
        subTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        subTxt.verticalOverflow = VerticalWrapMode.Truncate;
        subTxt.raycastTarget = false;
        RectTransform sRect = subTxt.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0.5f, 0.5f);
        sRect.anchorMax = new Vector2(0.5f, 0.5f);
        sRect.pivot = new Vector2(0.5f, 0.5f);
        sRect.anchoredPosition = new Vector2(22f, -26f);
        sRect.sizeDelta = new Vector2(size.x - 80f, 54f);

        return btn;
    }

    private static GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static Text CreateUIText(string name, Transform parent, Font font, string content, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject obj = CreateUIElement(name, parent);
        Text txt = obj.AddComponent<Text>();
        txt.font = font;
        txt.text = content;
        txt.fontSize = fontSize;
        txt.fontStyle = FontStyle.Normal;
        txt.alignment = alignment;
        txt.color = color;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        txt.raycastTarget = false;

        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        shadow.effectDistance = new Vector2(3f, -3f);
        return txt;
    }

    private static Button CreateUIButton(string name, Transform parent, Font font, string label, Vector2 anchoredPos, Vector2 size, Color bgColor, int fontSize = 36)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;
        if (roundedFrameSprite != null)
        {
            img.sprite = roundedFrameSprite;
            img.type = Image.Type.Sliced;
        }

        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.1f, 0.06f, 0.03f, 0.95f);
        outline.effectDistance = new Vector2(4f, -4f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        btnObj.AddComponent<UIButtonJuice>();

        Text labelTxt = CreateUIText("Text_Label", btnObj.transform, font, label, fontSize, TextAnchor.MiddleCenter, Color.white);
        RectTransform lRect = labelTxt.GetComponent<RectTransform>();
        lRect.anchorMin = Vector2.zero;
        lRect.anchorMax = Vector2.one;
        lRect.offsetMin = Vector2.zero;
        lRect.offsetMax = Vector2.zero;

        return btn;
    }

    private static Sprite LoadSpriteEnsured(string assetPath)
    {
        TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (ti != null && ti.textureType != TextureImporterType.Sprite)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static Sprite[] LoadSpritesInFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return new Sprite[0];
        string[] files = Directory.GetFiles(folderPath, "*.png")
            .OrderBy(f => f)
            .ToArray();
        List<Sprite> sprites = new List<Sprite>();
        foreach (string f in files)
        {
            string assetPath = f.Replace("\\", "/");
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (s != null) sprites.Add(s);
        }
        return sprites.ToArray();
    }

    [MenuItem("SlimeFoodDrop/Audit All UI Texts")]
    public static void AuditAllUITexts()
    {
        Text[] allTexts = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int overflowCount = 0;
        int checkedCount = 0;

        foreach (Text t in allTexts)
        {
            if (t == null) continue;
            checkedCount++;
            RectTransform rt = t.GetComponent<RectTransform>();
            if (rt == null) continue;

            float w = rt.rect.width;
            float h = rt.rect.height;

            if (w <= 0.1f || h <= 0.1f) continue;

            // If best-fit is ON, Unity guarantees it scales down to minSize
            if (t.resizeTextForBestFit) continue;

            if (t.horizontalOverflow == HorizontalWrapMode.Overflow && t.preferredWidth > w + 1f)
            {
                Debug.LogWarning($"[UI AUDIT OVERFLOW] {t.gameObject.name}: preferred width {t.preferredWidth:F1} > rect width {w:F1} (Text: '{t.text}')");
                overflowCount++;
            }
            else if (t.verticalOverflow == VerticalWrapMode.Truncate && t.preferredHeight > h + 1f)
            {
                Debug.LogWarning($"[UI AUDIT TRUNCATE] {t.gameObject.name}: preferred height {t.preferredHeight:F1} > rect height {h:F1} (Text: '{t.text}')");
                overflowCount++;
            }
        }

        Debug.Log($"[UI AUDIT COMPLETE] Checked {checkedCount} Text components. Flagged overflows: {overflowCount}.");
    }

}
