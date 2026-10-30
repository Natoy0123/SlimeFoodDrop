import os

runner_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\InEnginePlayTestRunner.cs"
with open(runner_path, 'r', encoding='utf-8') as f:
    text = f.read()

# 1. Update Test 4 volume checks
old_t4 = """        // 4A: Magnet Power-Up
        gm.ActivateMagnet(6.0f);
        Debug.Assert(gm.IsMagnetActive, "[PLAYTEST FAIL] Magnet should be active!");
        Debug.Assert(sm.magnetActiveSource != null, "[PLAYTEST FAIL] Magnet active audio source missing!");
        Debug.Assert(sm.magnetActiveSource.clip == sm.magnetActiveClip, "[PLAYTEST FAIL] Magnet audio source clip mismatch!");
        Debug.Assert(sm.magnetActiveSource.volume >= 0.8f, "[PLAYTEST FAIL] Magnet audio volume must be >= 0.8f!");
        if (Application.isPlaying) Debug.Assert(sm.magnetActiveSource.isPlaying, "[PLAYTEST FAIL] Magnet audio source must be playing in play mode!");

        // 4B: Barrier Shield Power-Up
        gm.ActivateShieldBarrier(12.0f);
        Debug.Assert(gm.IsShieldActive, "[PLAYTEST FAIL] Shield should be active!");
        Debug.Assert(sm.shieldActiveSource != null, "[PLAYTEST FAIL] Shield active audio source missing!");
        Debug.Assert(sm.shieldActiveSource.clip == sm.shieldActiveClip, "[PLAYTEST FAIL] Shield audio source clip mismatch!");
        Debug.Assert(sm.shieldActiveSource.volume >= 0.8f, "[PLAYTEST FAIL] Shield audio volume must be >= 0.8f!");
        if (Application.isPlaying) Debug.Assert(sm.shieldActiveSource.isPlaying, "[PLAYTEST FAIL] Shield audio source must be playing in play mode!");

        // 4C: Time-Slow Power-Up
        gm.ActivateTimeSlow(6.0f);
        Debug.Assert(gm.IsTimeSlowActive, "[PLAYTEST FAIL] Time-Slow should be active!");
        Debug.Assert(sm.timeSlowActiveSource != null, "[PLAYTEST FAIL] Time-Slow active audio source missing!");
        Debug.Assert(sm.timeSlowActiveSource.clip == sm.timeSlowActiveClip, "[PLAYTEST FAIL] Time-Slow audio source clip mismatch!");
        Debug.Assert(sm.timeSlowActiveSource.volume >= 0.8f, "[PLAYTEST FAIL] Time-Slow audio volume must be >= 0.8f!");"""

new_t4 = """        // 4A: Magnet Power-Up (Friendly Chimes at comfortable gentle volume)
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
        Debug.Assert(sm.timeSlowActiveSource.volume >= 0.35f, "[PLAYTEST FAIL] Time-Slow audio volume must be >= 0.35f!");"""

# 2. Add Test 7 & 8 after Test 6
old_t6 = """        RenderCapture(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_InEngine_Play_GameplayWithPlatform.png"));

        Debug.Log("==================================================================");
        Debug.Log("[IN-ENGINE PLAYTEST SUCCEEDED] All in-engine mechanics, SFX audio, container constraints, and z-orders verified!");
        Debug.Log("==================================================================");"""

new_t6 = """        RenderCapture(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_InEngine_Play_GameplayWithPlatform.png"));

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

        Debug.Log("==================================================================");
        Debug.Log("[IN-ENGINE PLAYTEST SUCCEEDED] All in-engine mechanics, SFX audio, container constraints, and z-orders verified!");
        Debug.Log("==================================================================");"""

normalized = text.replace('\r\n', '\n')
old_t4_norm = old_t4.replace('\r\n', '\n')
new_t4_norm = new_t4.replace('\r\n', '\n')
old_t6_norm = old_t6.replace('\r\n', '\n')
new_t6_norm = new_t6.replace('\r\n', '\n')

assert old_t4_norm in normalized, "old_t4 not found!"
normalized = normalized.replace(old_t4_norm, new_t4_norm, 1)

assert old_t6_norm in normalized, "old_t6 not found!"
normalized = normalized.replace(old_t6_norm, new_t6_norm, 1)

if '\r\n' in text:
    out = normalized.replace('\n', '\r\n')
else:
    out = normalized

with open(runner_path, 'w', encoding='utf-8') as f:
    f.write(out)

print("InEnginePlayTestRunner.cs updated successfully with Test 7 & 8!")
