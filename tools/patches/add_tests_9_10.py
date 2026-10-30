runner_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\InEnginePlayTestRunner.cs"
with open(runner_path, 'r', encoding='utf-8') as f:
    text = f.read()

target = """        ui.splashPanel.SetActive(true);
        if (ui.splashCanvasGroup != null) ui.splashCanvasGroup.alpha = 1f;
        RenderCapture(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_InEngine_Play_Splash.png"));
        ui.splashPanel.SetActive(false);

        Debug.Log("==================================================================");
        Debug.Log("[IN-ENGINE PLAYTEST SUCCEEDED] All in-engine mechanics, SFX audio, container constraints, and z-orders verified!");
        Debug.Log("==================================================================");"""

replacement = """        ui.splashPanel.SetActive(true);
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

        Debug.Log("==================================================================");
        Debug.Log("[IN-ENGINE PLAYTEST SUCCEEDED] All in-engine mechanics, SFX audio, container constraints, and z-orders verified!");
        Debug.Log("==================================================================");"""

assert target in text, "target not found in InEnginePlayTestRunner.cs"
text = text.replace(target, replacement, 1)

with open(runner_path, 'w', encoding='utf-8') as f:
    f.write(text)

print("InEnginePlayTestRunner.cs updated with Test 9 and 10!")
