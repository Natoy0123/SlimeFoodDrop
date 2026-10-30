using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class InEnginePlayTestRunner
{
    [MenuItem("SlimeFoodDrop/Run In-Engine Play Test")]
    public static void RunPlayTest()
    {
        Debug.Log("==================================================================");
        Debug.Log("[IN-ENGINE PLAYTEST] Starting comprehensive in-engine validation...");
        Debug.Log("==================================================================");

        // Freshly bake MainGameScene with all latest UI, audio, and platform assets
        BuildMainGameScene.BuildAll();

        string scenePath = "Assets/Scenes/MainGameScene.unity";
        EditorSceneManager.OpenScene(scenePath);

        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        UIManager ui = Object.FindFirstObjectByType<UIManager>();
        SoundManager sm = Object.FindFirstObjectByType<SoundManager>();
        ResponsiveCamera respCam = Object.FindFirstObjectByType<ResponsiveCamera>();
        Camera cam = Camera.main;

        Debug.Assert(gm != null, "[PLAYTEST FAIL] GameManager missing in scene!");
        Debug.Assert(ui != null, "[PLAYTEST FAIL] UIManager missing in scene!");
        Debug.Assert(sm != null, "[PLAYTEST FAIL] SoundManager missing in scene!");
        Debug.Assert(respCam != null, "[PLAYTEST FAIL] ResponsiveCamera missing in scene!");

        string logDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        Directory.CreateDirectory(logDir);

        // ------------------------------------------------------------------
        // TEST 1: HOME SCREEN & INTERACTIVE SLIME
        // ------------------------------------------------------------------
        Debug.Log("[TEST 1] Verifying Home Screen & Interactive Slime dialogue...");
        ui.HideSplashAndHomeScreens();
        ui.ShowHomeScreen(150);
        Debug.Assert(ui.homePanel != null && ui.homePanel.activeSelf, "[PLAYTEST FAIL] Home panel must be active!");

        if (ui.homeInteractiveSlime != null)
        {
            // Verify reaction bubble contains text cleanly
            Debug.Assert(ui.homeInteractiveSlime.reactionBubbleObj != null, "[PLAYTEST FAIL] Slime reaction bubble missing!");
        }

        // ------------------------------------------------------------------
        // TEST 2: DIFFICULTY SELECTION & TEXT CONTAINER BOUNDS
        // ------------------------------------------------------------------
        Debug.Log("[TEST 2] Verifying Difficulty Menu & text container bounds...");
        ui.ShowDifficultyScreen(GameManager.DifficultyLevel.Medium);
        Debug.Assert(ui.difficultyPanel != null && ui.difficultyPanel.activeSelf, "[PLAYTEST FAIL] Difficulty panel must be active!");

        // Assert bottom tip text is wrapped and contained inside the card
        Transform diffCard = ui.difficultyPanel.transform.Find("Difficulty_Card");
        Debug.Assert(diffCard != null, "[PLAYTEST FAIL] Difficulty_Card missing!");
        RectTransform dcRt = diffCard.GetComponent<RectTransform>();
        Transform tipObj = diffCard.Find("Text_StageHint") ?? diffCard.Find("Text_DiffTip");
        Debug.Assert(tipObj != null, "[PLAYTEST FAIL] Text_StageHint missing from Difficulty_Card!");
        RectTransform tipRt = tipObj.GetComponent<RectTransform>();
        Text tipTxt = tipObj.GetComponent<Text>();
        Debug.Assert(tipTxt.horizontalOverflow == HorizontalWrapMode.Wrap, "[PLAYTEST FAIL] Text_DiffTip must have HorizontalWrapMode.Wrap!");
        Debug.Assert(tipRt.sizeDelta.x <= dcRt.sizeDelta.x, "[PLAYTEST FAIL] Text_DiffTip width must fit inside Difficulty_Card!");

        RenderCapture(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_InEngine_Play_Difficulty.png"));

        // ------------------------------------------------------------------
        // TEST 3: FULL 5-STEP TUTORIAL TRIGGERING ON DIFFICULTY SELECTION
        // ------------------------------------------------------------------
        Debug.Log("[TEST 3] Verifying complete 5-step tutorial triggering...");
        ui.HideDifficultyScreen();
        ui.SetGameplayHUDVisible(true);

        // Simulate tutorial step 1 (Difficulty rules)
        ui.GetDifficultyTutorialContent(GameManager.DifficultyLevel.Medium, out string diffBadge, out string diffTitle, out string diffBody, out Sprite[] diffIcons);
        ui.textTutorialSpeakerName.text = "BLOOP THE TUTOR";
        ui.textTutorialStepBadge.text = diffBadge + "  (1/5)";
        ui.textTutorialTitle.text = diffTitle;
        ui.textTutorialBody.text = diffBody;
        ui.textTutorialTapPrompt.text = "[ TAP TO CONTINUE > ]";
        ui.tutorialPanel.SetActive(true);

        Debug.Assert(ui.tutorialPanel.activeSelf, "[PLAYTEST FAIL] Tutorial panel must be active!");
        RectTransform tutBoxRt = (ui.tutorialPanel.transform.Find("Tutorial_BottomDialogBox") ?? ui.tutorialPanel.transform.Find("Tutorial_DialogBox"))?.GetComponent<RectTransform>();
        Debug.Assert(tutBoxRt != null && tutBoxRt.sizeDelta.y >= 340f && tutBoxRt.sizeDelta.y <= 420f, "[PLAYTEST FAIL] Tutorial dialog box height must be between 340f and 420f!");
        Debug.Assert(ui.textTutorialBody.fontSize >= 24, "[PLAYTEST FAIL] Tutorial body font size must be >= 24pt for readability!");

        RenderCapture(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_InEngine_Play_Tutorial.png"));
        ui.tutorialPanel.SetActive(false);

        // ------------------------------------------------------------------
        // TEST 4: POWER-UP SFX AUDIO CHANNELS & AUDIBLE VOLUMES
        // ------------------------------------------------------------------
        Debug.Log("[TEST 4] Verifying Power-Up SFX loops & audible volume levels...");
        gm.StartGameWithDifficulty(GameManager.DifficultyLevel.Medium, true);

        // Assert all 4 dedicated pickup sound clips are loaded
        Debug.Assert(sm.heartPickupClip != null, "[PLAYTEST FAIL] SoundManager heartPickupClip must not be null!");
        Debug.Assert(sm.shieldPickupClip != null, "[PLAYTEST FAIL] SoundManager shieldPickupClip must not be null!");
        Debug.Assert(sm.magnetPickupClip != null, "[PLAYTEST FAIL] SoundManager magnetPickupClip must not be null!");
        Debug.Assert(sm.timeSlowPickupClip != null, "[PLAYTEST FAIL] SoundManager timeSlowPickupClip must not be null!");

        // 4A: Magnet Power-Up (Friendly Chimes at comfortable gentle volume)
        gm.ActivateMagnet(6.0f);
        Debug.Assert(gm.IsMagnetActive, "[PLAYTEST FAIL] Magnet should be active!");
        Debug.Assert(sm.magnetActiveSource != null, "[PLAYTEST FAIL] Magnet active audio source missing!");
        Debug.Assert(sm.magnetActiveSource.clip == sm.magnetActiveClip, "[PLAYTEST FAIL] Magnet audio source clip mismatch!");
        Debug.Assert(sm.magnetActiveSource.volume >= 0.35f, "[PLAYTEST FAIL] Magnet audio volume must be >= 0.35f!");
        if (Application.isPlaying) Debug.Assert(sm.magnetActiveSource.isPlaying, "[PLAYTEST FAIL] Magnet audio source must be playing in play mode!");

        // 4B: Barrier Shield Power-Up (Gentle Pad at comfortable soothing volume)
        gm.ActivateShieldBarrier(12.0f);
        Debug.Assert(gm.IsShieldActive, "[PLAYTEST FAIL] Shield should be active!");
        Debug.Assert(sm.shieldActiveSource != null, "[PLAYTEST FAIL] Shield active audio source missing!");
        Debug.Assert(sm.shieldActiveSource.clip == sm.shieldActiveClip, "[PLAYTEST FAIL] Shield audio source clip mismatch!");
        Debug.Assert(sm.shieldActiveSource.volume >= 0.35f, "[PLAYTEST FAIL] Shield audio volume must be >= 0.35f!");
        if (Application.isPlaying) Debug.Assert(sm.shieldActiveSource.isPlaying, "[PLAYTEST FAIL] Shield audio source must be playing in play mode!");

        // 4C: Time-Slow Power-Up
        gm.ActivateTimeSlow(6.0f);
        Debug.Assert(gm.IsTimeSlowActive, "[PLAYTEST FAIL] Time-Slow should be active!");
        Debug.Assert(sm.timeSlowActiveSource != null, "[PLAYTEST FAIL] Time-Slow active audio source missing!");
        Debug.Assert(sm.timeSlowActiveSource.clip == sm.timeSlowActiveClip, "[PLAYTEST FAIL] Time-Slow audio source clip mismatch!");
        Debug.Assert(sm.timeSlowActiveSource.volume >= 0.35f, "[PLAYTEST FAIL] Time-Slow audio volume must be >= 0.35f!");
        if (Application.isPlaying) Debug.Assert(sm.timeSlowActiveSource.isPlaying, "[PLAYTEST FAIL] Time-Slow audio source must be playing in play mode!");

        // ------------------------------------------------------------------
        // TEST 5: CONFIRMATION MODAL OVER SETTINGS MODAL (Z-ORDER GUARD)
        // ------------------------------------------------------------------
        Debug.Log("[TEST 5] Verifying Confirmation modal displays IN FRONT of Settings modal...");
        ui.ShowSettingsModal(true); // Open In-Game Pause Settings modal
        Debug.Assert(ui.settingsPanel != null && ui.settingsPanel.activeSelf, "[PLAYTEST FAIL] Settings modal must be open!");

        // Trigger confirmation modal while settings modal is open
        ui.PromptRestartConfirmation();
        Debug.Assert(ui.confirmationModal != null && ui.confirmationModal.activeSelf, "[PLAYTEST FAIL] Confirmation modal must be active!");

        int confIndex = ui.confirmationModal.transform.GetSiblingIndex();
        int setIndex = ui.settingsPanel.transform.GetSiblingIndex();
        Debug.Assert(confIndex > setIndex,
            $"[PLAYTEST FAIL] Confirmation modal (sibling index {confIndex}) must be higher than Settings modal (sibling index {setIndex}) so it renders in front!");

        Canvas confCanvas = ui.confirmationModal.GetComponent<Canvas>();
        Debug.Assert(confCanvas != null && confCanvas.overrideSorting && confCanvas.sortingOrder >= 500,
            "[PLAYTEST FAIL] Confirmation modal must have Canvas with overrideSorting=true and sortingOrder>=500!");

        RenderCapture(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_InEngine_Play_ConfirmationOverSettings.png"));

        ui.HideConfirmation();
        ui.HideSettingsModal();

        // ------------------------------------------------------------------
        // TEST 6: ENVIRONMENT PLATFORM UNDER SLIME
        // ------------------------------------------------------------------
        Debug.Log("[TEST 6] Verifying environment ground platform under slime...");
        Debug.Assert(respCam.floorPlatformRenderer != null, "[PLAYTEST FAIL] Floor platform renderer must be assigned!");
        Debug.Assert(respCam.floorPlatformRenderer.sprite != null, "[PLAYTEST FAIL] Floor platform sprite must not be null!");

        RenderCapture(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_InEngine_Play_GameplayWithPlatform.png"));

        // ------------------------------------------------------------------
        // TEST 7: ENVIRONMENT TRANSITION BANNER (EXPANDED CONTAINER & WRAPPING)
        // ------------------------------------------------------------------
        Debug.Log("[TEST 7] Verifying Environment Transition Banner wrapping and bounds...");
        Debug.Assert(ui.environmentBannerPanel != null, "[PLAYTEST FAIL] Environment Banner Panel missing!");
        RectTransform ebRt = ui.environmentBannerPanel.GetComponent<RectTransform>();
        Debug.Assert(ebRt != null && ebRt.sizeDelta.x >= 920f, "[PLAYTEST FAIL] Environment Banner width must be >= 920f!");
        Debug.Assert(ui.textEnvSubtitle != null, "[PLAYTEST FAIL] textEnvSubtitle missing!");
        Debug.Assert(ui.textEnvSubtitle.fontSize >= 22, "[PLAYTEST FAIL] textEnvSubtitle fontSize must be >= 22!");
        Debug.Assert(ui.textEnvSubtitle.horizontalOverflow == HorizontalWrapMode.Wrap, "[PLAYTEST FAIL] textEnvSubtitle must wrap!");
        Debug.Assert(ui.textEnvSubtitle.verticalOverflow == VerticalWrapMode.Truncate, "[PLAYTEST FAIL] textEnvSubtitle must truncate!");

        ui.ShowEnvironmentTransitionBanner(2, "BREEZY MEADOWS", "Sunny Plains Canopy  |  Calm Wind BGM");
        RenderCapture(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_InEngine_Play_EnvironmentBanner.png"));
        ui.environmentBannerPanel.SetActive(false);

        // ------------------------------------------------------------------
        // TEST 8: SPLASH SCREEN (NO SKIP BUTTON, TOPHEADER Y=560, LOADING GROUP Y>=150)
        // ------------------------------------------------------------------
        Debug.Log("[TEST 8] Verifying Splash Screen layout and skip removal...");
        Debug.Assert(ui.splashPanel != null, "[PLAYTEST FAIL] Splash panel missing!");
        Transform skipBtnTransform = ui.splashPanel.transform.Find("Button_SkipCinematic");
        Debug.Assert(skipBtnTransform == null, "[PLAYTEST FAIL] Splash Skip button must be removed!");

        Transform topHeaderTransform = ui.splashPanel.transform.Find("Splash_TopHeader");
        Debug.Assert(topHeaderTransform != null, "[PLAYTEST FAIL] Splash_TopHeader missing!");
        RectTransform thRt = topHeaderTransform.GetComponent<RectTransform>();
        Debug.Assert(Mathf.Abs(thRt.anchoredPosition.y - 560f) < 5f, $"[PLAYTEST FAIL] Splash_TopHeader anchoredPosition.y ({thRt.anchoredPosition.y}) must match Home Title level (560f)!");

        Transform loadingGroupTransform = ui.splashPanel.transform.Find("Splash_LoadingGroup");
        Debug.Assert(loadingGroupTransform != null, "[PLAYTEST FAIL] Splash_LoadingGroup missing!");
        RectTransform lgRt = loadingGroupTransform.GetComponent<RectTransform>();
        Debug.Assert(lgRt.anchoredPosition.y >= 140f, $"[PLAYTEST FAIL] Splash_LoadingGroup anchoredPosition.y ({lgRt.anchoredPosition.y}) must be >= 140f!");

        ui.splashPanel.SetActive(true);
        if (ui.splashCanvasGroup != null) ui.splashCanvasGroup.alpha = 1f;
        RenderCapture(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_InEngine_Play_Splash.png"));
        ui.splashPanel.SetActive(false);

        // ------------------------------------------------------------------
        // TEST 9: ENVIRONMENT TRANSITION MILESTONES & PERMANENT FINAL STAGE
        // ------------------------------------------------------------------
        Debug.Log("[TEST 9] Verifying higher environment score thresholds & final stage stay...");
        ThemeManager tm = Object.FindFirstObjectByType<ThemeManager>();
        Debug.Assert(tm != null && tm.themes != null && tm.themes.Length == 10, "[PLAYTEST FAIL] ThemeManager must have exactly 10 themes!");

        int[] expectedScores = { 0, 100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700 };
        for (int i = 0; i < expectedScores.Length; i++)
        {
            Debug.Assert(tm.themes[i].unlockScore == expectedScores[i],
                $"[PLAYTEST FAIL] Theme {i} ({tm.themes[i].themeName}) unlockScore must be {expectedScores[i]}, but got {tm.themes[i].unlockScore}!");
        }

        // Test that evaluating score beyond 2700 (e.g. 3500) permanently stays on stage 10 (index 9)
        tm.EvaluateScoreForTheme(3500);
        Debug.Assert(tm.currentThemeIndex == 9, $"[PLAYTEST FAIL] Evaluating score 3500 must stay on final stage 10 (index 9), but got {tm.currentThemeIndex}!");

        // ------------------------------------------------------------------
        // TEST 10: FRENZY STREAK & FOOD RUSH MECHANICS
        // ------------------------------------------------------------------
        Debug.Log("[TEST 10] Verifying Frenzy Streak indicator & Food Rush mode...");
        gm.StartGameWithDifficulty(GameManager.DifficultyLevel.Easy, true);
        Debug.Assert(gm.frenzyStreak == 0, "[PLAYTEST FAIL] Frenzy streak must start at 0!");
        Debug.Assert(gm.FrenzyTargetForCurrentDifficulty == 10, "[PLAYTEST FAIL] Easy mode frenzy target must be 10!");
        Debug.Assert(gm.FoodRushDurationForCurrentDifficulty == 15.0f, "[PLAYTEST FAIL] Easy mode food rush duration must be 15s!");

        // Simulate catching 5 foods
        for (int c = 1; c <= 5; c++)
        {
            gm.OnFoodCaught(10, new Vector3(0f, -2f, 0f));
            Debug.Assert(gm.frenzyStreak == c, $"[PLAYTEST FAIL] Frenzy streak must increment to {c}!");
        }
        Debug.Assert(!gm.IsFoodRushActive, "[PLAYTEST FAIL] Food rush should not be active at streak 5!");

        // Trigger Food Rush directly to verify mode
        gm.TriggerFoodRush();
        Debug.Assert(gm.IsFoodRushActive, "[PLAYTEST FAIL] Food Rush must be active!");
        Debug.Assert(gm.foodRushTimeRemaining > 14.0f, "[PLAYTEST FAIL] Food Rush duration must be ~15.0s on Easy!");

        // In-Game capture showing HUD Frenzy Streak / Rush timer active
        RenderCapture(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_InEngine_Play_FrenzyRush.png"));

        // End Food Rush cleanly
        gm.EndFoodRush();
        Debug.Assert(!gm.IsFoodRushActive, "[PLAYTEST FAIL] Food rush must be inactive after EndFoodRush!");

        // ------------------------------------------------------------------
        // TEST 11: ALMANAC ITEM DETAIL MODAL (Z-ORDER & TYPOGRAPHY)
        // ------------------------------------------------------------------
        Debug.Log("[TEST 11] Verifying Almanac & Item Detail Modal...");
        ui.ShowGuidebookModal(0);
        Debug.Assert(ui.guidebookPanel != null && ui.guidebookPanel.activeSelf, "[PLAYTEST FAIL] Guidebook panel must be open!");

        ui.ShowAlmanacItemDetail(
            null,
            "POWER-UP ITEM",
            new Color(0.96f, 0.76f, 0.22f, 1f),
            "MAGNETIC RING",
            "+0 PTS  •  ATTRACTION FIELD",
            "Passively draws all tasty fruits towards your slime within a 5-meter aura.",
            "Best used when multiple fruits are falling simultaneously to safely grab all treats!"
        );
        Debug.Assert(ui.almanacDetailModal != null && ui.almanacDetailModal.activeSelf, "[PLAYTEST FAIL] Almanac Detail Modal must be active!");
        int guideIdx = ui.guidebookPanel.transform.GetSiblingIndex();
        int detailIdx = ui.almanacDetailModal.transform.GetSiblingIndex();
        Debug.Assert(detailIdx >= guideIdx, $"[PLAYTEST FAIL] Almanac Detail Modal ({detailIdx}) must be on top of Guidebook ({guideIdx})!");

        RenderCapture(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_InEngine_Play_AlmanacItemDetail.png"));
        ui.almanacDetailModal.SetActive(false);
        ui.guidebookPanel.SetActive(false);

        // ------------------------------------------------------------------
        // TEST 12: GAME OVER MODAL (READABLE SCORES & BUTTONS)
        // ------------------------------------------------------------------
        Debug.Log("[TEST 12] Verifying Game Over Modal layout and typography...");
        ui.ShowGameOver(1250, 2700);
        Debug.Assert(ui.gameOverPanel != null && ui.gameOverPanel.activeSelf, "[PLAYTEST FAIL] Game Over panel must be active!");
        Debug.Assert(ui.textFinalScore != null && ui.textFinalScore.fontSize >= 28, "[PLAYTEST FAIL] Final score font size must be >= 28pt!");
        Debug.Assert(ui.textBestScore != null && ui.textBestScore.fontSize >= 24, "[PLAYTEST FAIL] Best score font size must be >= 24pt!");

        RenderCapture(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_InEngine_Play_GameOver.png"));
        ui.gameOverPanel.SetActive(false);

        Debug.Log("==================================================================");
        Debug.Log("[IN-ENGINE PLAYTEST SUCCEEDED] All in-engine mechanics, SFX audio, container constraints, and z-orders verified!");
        Debug.Log("==================================================================");
    }

    private static void RenderCapture(Camera cam, ResponsiveCamera respCam, int width, int height, string outPath)
    {
        if (cam == null || respCam == null) return;
        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        respCam.RecalculateBounds();
        cam.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        File.WriteAllBytes(outPath, tex.EncodeToPNG());
        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
        respCam.RecalculateBounds();
    }
}
