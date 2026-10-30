import os, re, sys

# ==============================================================================
# 1. Update GameManager.cs: Add Food Rush timer to Update() loop
# ==============================================================================
gm_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\GameManager.cs"
with open(gm_path, 'r', encoding='utf-8') as f:
    gm_text = f.read()

target_update = """        if (isTimeSlowActive)
        {
            timeSlowTimeRemaining -= Time.deltaTime;
            if (timeSlowTimeRemaining <= 0f)
            {
                timeSlowTimeRemaining = 0f;
                isTimeSlowActive = false;
            }
            anyChanged = true;
        }

        if (anyChanged)"""

new_update = """        if (isTimeSlowActive)
        {
            timeSlowTimeRemaining -= Time.deltaTime;
            if (timeSlowTimeRemaining <= 0f)
            {
                timeSlowTimeRemaining = 0f;
                isTimeSlowActive = false;
            }
            anyChanged = true;
        }

        // Active Food Rush countdown timer
        if (isFoodRushActive)
        {
            foodRushTimeRemaining -= Time.deltaTime;
            if (foodRushTimeRemaining <= 0f)
            {
                EndFoodRush();
            }
            else if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateFoodRushHUD(foodRushTimeRemaining, foodRushMaxDuration);
            }
        }
        else if (frenzyStreak > 0)
        {
            // 4.0-second streak sustain window
            frenzyStreakTimer -= Time.deltaTime;
            if (frenzyStreakTimer <= 0f)
            {
                frenzyStreak = 0;
                frenzyStreakTimer = 0f;
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateFrenzyStreakHUD(0, FrenzyTargetForCurrentDifficulty);
                }
            }
        }

        if (anyChanged)"""

assert target_update in gm_text, "target_update not found in GameManager.cs"
gm_text = gm_text.replace(target_update, new_update, 1)

with open(gm_path, 'w', encoding='utf-8') as f:
    f.write(gm_text)
print("[OK] Updated GameManager.cs: FoodRush timer added to Update()")

# ==============================================================================
# 2. Update ThemeManager.cs: Authoritative unlock scores array in Awake & Start
# ==============================================================================
tm_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\ThemeManager.cs"
with open(tm_path, 'r', encoding='utf-8') as f:
    tm_text = f.read()

target_tm_awake = """        PrewarmAllThemeAssets();
        currentThemeIndex = 0;
        ApplyThemeImmediate(0);"""

new_tm_awake = """        EnforceAuthoritativeUnlockScores();
        PrewarmAllThemeAssets();
        currentThemeIndex = 0;
        ApplyThemeImmediate(0);"""

tm_methods = """    private static readonly int[] AuthoritativeUnlockScores = { 0, 100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700 };

    public void EnforceAuthoritativeUnlockScores()
    {
        if (themes == null) return;
        for (int i = 0; i < themes.Length && i < AuthoritativeUnlockScores.Length; i++)
        {
            themes[i].unlockScore = AuthoritativeUnlockScores[i];
        }
    }

    private void PrewarmAllThemeAssets()"""

assert target_tm_awake in tm_text, "target_tm_awake not found in ThemeManager.cs"
tm_text = tm_text.replace(target_tm_awake, new_tm_awake, 1)

assert "    private void PrewarmAllThemeAssets()" in tm_text, "PrewarmAllThemeAssets not found in ThemeManager.cs"
tm_text = tm_text.replace("    private void PrewarmAllThemeAssets()", tm_methods, 1)

with open(tm_path, 'w', encoding='utf-8') as f:
    f.write(tm_text)
print("[OK] Updated ThemeManager.cs: EnforceAuthoritativeUnlockScores added")

# ==============================================================================
# 3. Update UIManager.cs: Reliable Floating Indicators (WorldToScreen with canvas.worldCamera)
# ==============================================================================
ui_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\UIManager.cs"
with open(ui_path, 'r', encoding='utf-8') as f:
    ui_text = f.read()

old_floating_methods = """    public void SpawnFloatingStreakIndicator(int streakCount, Vector3 worldPos)
    {
        if (topBarHud == null && topBarHud == null) return;
        Transform parentTransform = topBarHud != null ? topBarHud.transform.parent : topBarHud.transform;

        GameObject floatObj = new GameObject($"FloatStreak_x{streakCount}", typeof(RectTransform));
        floatObj.transform.SetParent(parentTransform, false);
        RectTransform rt = floatObj.GetComponent<RectTransform>();

        Vector2 screenPoint = Camera.main != null ? (Vector2)Camera.main.WorldToScreenPoint(worldPos) : new Vector2(Screen.width * 0.5f, Screen.height * 0.35f);
        RectTransform canvasRt = parentTransform.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPoint, null, out Vector2 localPoint))
        {
            rt.anchoredPosition = localPoint + new Vector2(0f, 50f);
        }
        else
        {
            rt.anchoredPosition = new Vector2(0f, -120f);
        }
        rt.sizeDelta = new Vector2(360f, 80f);

        Text txt = floatObj.AddComponent<Text>();
        txt.font = (retroBodyFontRef != null ? retroBodyFontRef : (arcadeFontRef != null ? arcadeFontRef : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")));
        txt.text = (streakCount >= 15) ? $"x{streakCount} FRENZY!" : $"x{streakCount}";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;

        Color streakColor = GetStreakColor(streakCount);
        txt.color = streakColor;
        txt.fontSize = (streakCount >= 15 ? 46 : (streakCount >= 10 ? 40 : (streakCount >= 7 ? 36 : (streakCount >= 4 ? 32 : 28))));

        Outline ol = floatObj.AddComponent<Outline>();
        ol.effectColor = new Color(0.06f, 0.04f, 0.10f, 0.95f);
        ol.effectDistance = new Vector2(3f, -3f);

        CanvasGroup cg = floatObj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        if (Application.isPlaying)
        {
            StartCoroutine(AnimateFloatingIndicator(rt, cg, floatObj));
        }
    }

    public void SpawnFloatingRushBonus(Vector3 worldPos, int points)
    {
        if (topBarHud == null && topBarHud == null) return;
        Transform parentTransform = topBarHud != null ? topBarHud.transform.parent : topBarHud.transform;

        GameObject floatObj = new GameObject("Float_RushBonus", typeof(RectTransform));
        floatObj.transform.SetParent(parentTransform, false);
        RectTransform rt = floatObj.GetComponent<RectTransform>();

        Vector2 screenPoint = Camera.main != null ? (Vector2)Camera.main.WorldToScreenPoint(worldPos) : new Vector2(Screen.width * 0.5f, Screen.height * 0.35f);
        RectTransform canvasRt = parentTransform.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPoint, null, out Vector2 localPoint))
        {
            rt.anchoredPosition = localPoint + new Vector2(0f, 50f);
        }
        else
        {
            rt.anchoredPosition = new Vector2(0f, -120f);
        }
        rt.sizeDelta = new Vector2(360f, 80f);

        Text txt = floatObj.AddComponent<Text>();
        txt.font = (retroBodyFontRef != null ? retroBodyFontRef : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
        txt.text = $"+{points} RUSH!";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(1.0f, 0.88f, 0.28f, 1f);
        txt.fontSize = 38;

        Outline ol = floatObj.AddComponent<Outline>();
        ol.effectColor = new Color(0.12f, 0.08f, 0.02f, 0.95f);
        ol.effectDistance = new Vector2(3f, -3f);

        CanvasGroup cg = floatObj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        if (Application.isPlaying)
        {
            StartCoroutine(AnimateFloatingIndicator(rt, cg, floatObj));
        }
    }

    public void SpawnStreakBrokenIndicator()
    {
        if ((topBarHud == null && topBarHud == null) || SlimeController.Instance == null) return;
        Transform parentTransform = topBarHud != null ? topBarHud.transform.parent : topBarHud.transform;

        GameObject floatObj = new GameObject("Float_StreakLost", typeof(RectTransform));
        floatObj.transform.SetParent(parentTransform, false);
        RectTransform rt = floatObj.GetComponent<RectTransform>();

        Vector2 screenPoint = Camera.main != null ? (Vector2)Camera.main.WorldToScreenPoint(SlimeController.Instance.transform.position) : new Vector2(Screen.width * 0.5f, Screen.height * 0.25f);
        RectTransform canvasRt = parentTransform.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPoint, null, out Vector2 localPoint))
        {
            rt.anchoredPosition = localPoint + new Vector2(0f, 80f);
        }
        else
        {
            rt.anchoredPosition = new Vector2(0f, -400f);
        }
        rt.sizeDelta = new Vector2(380f, 70f);

        Text txt = floatObj.AddComponent<Text>();
        txt.font = (retroBodyFontRef != null ? retroBodyFontRef : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
        txt.text = "STREAK LOST!";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(0.95f, 0.38f, 0.38f, 0.95f);
        txt.fontSize = 28;

        Outline ol = floatObj.AddComponent<Outline>();
        ol.effectColor = new Color(0.10f, 0.04f, 0.04f, 0.95f);
        ol.effectDistance = new Vector2(3f, -3f);

        CanvasGroup cg = floatObj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        if (Application.isPlaying)
        {
            StartCoroutine(AnimateFloatingIndicator(rt, cg, floatObj));
        }
    }"""

new_floating_methods = """    public void SpawnFloatingStreakIndicator(int streakCount, Vector3 worldPos)
    {
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null && topBarHud != null) rootCanvas = topBarHud.GetComponentInParent<Canvas>();
        if (rootCanvas == null) return;

        Camera uiCam = (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;
        if (uiCam == null) uiCam = Camera.main;

        GameObject floatObj = new GameObject($"FloatStreak_x{streakCount}", typeof(RectTransform));
        floatObj.transform.SetParent(rootCanvas.transform, false);
        floatObj.transform.SetAsLastSibling();
        RectTransform rt = floatObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCam, worldPos);
        RectTransform canvasRt = rootCanvas.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPoint, uiCam, out Vector2 localPoint))
        {
            rt.anchoredPosition = localPoint + new Vector2(0f, 75f);
        }
        else
        {
            rt.anchoredPosition = new Vector2(0f, -280f);
        }
        rt.sizeDelta = new Vector2(380f, 90f);

        Text txt = floatObj.AddComponent<Text>();
        Font chosenFont = retroBodyFontRef != null ? retroBodyFontRef : (arcadeFontRef != null ? arcadeFontRef : (textScore != null ? textScore.font : null));
        if (chosenFont == null) chosenFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.font = chosenFont;
        txt.text = (streakCount >= 15) ? $"x{streakCount} FRENZY!" : $"x{streakCount}";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;

        Color streakColor = GetStreakColor(streakCount);
        txt.color = streakColor;
        txt.fontSize = (streakCount >= 15 ? 48 : (streakCount >= 10 ? 42 : (streakCount >= 7 ? 38 : (streakCount >= 4 ? 34 : 30))));

        Outline ol = floatObj.AddComponent<Outline>();
        ol.effectColor = new Color(0.04f, 0.02f, 0.08f, 0.98f);
        ol.effectDistance = new Vector2(3f, -3f);

        CanvasGroup cg = floatObj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        if (Application.isPlaying)
        {
            StartCoroutine(AnimateFloatingIndicator(rt, cg, floatObj));
        }
    }

    public void SpawnFloatingRushBonus(Vector3 worldPos, int points)
    {
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null && topBarHud != null) rootCanvas = topBarHud.GetComponentInParent<Canvas>();
        if (rootCanvas == null) return;

        Camera uiCam = (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;
        if (uiCam == null) uiCam = Camera.main;

        GameObject floatObj = new GameObject("Float_RushBonus", typeof(RectTransform));
        floatObj.transform.SetParent(rootCanvas.transform, false);
        floatObj.transform.SetAsLastSibling();
        RectTransform rt = floatObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCam, worldPos);
        RectTransform canvasRt = rootCanvas.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPoint, uiCam, out Vector2 localPoint))
        {
            rt.anchoredPosition = localPoint + new Vector2(0f, 75f);
        }
        else
        {
            rt.anchoredPosition = new Vector2(0f, -280f);
        }
        rt.sizeDelta = new Vector2(380f, 90f);

        Text txt = floatObj.AddComponent<Text>();
        Font chosenFont = retroBodyFontRef != null ? retroBodyFontRef : (arcadeFontRef != null ? arcadeFontRef : (textScore != null ? textScore.font : null));
        if (chosenFont == null) chosenFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.font = chosenFont;
        txt.text = $"+{points} RUSH!";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(1.0f, 0.88f, 0.28f, 1f);
        txt.fontSize = 40;

        Outline ol = floatObj.AddComponent<Outline>();
        ol.effectColor = new Color(0.12f, 0.08f, 0.02f, 0.95f);
        ol.effectDistance = new Vector2(3f, -3f);

        CanvasGroup cg = floatObj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        if (Application.isPlaying)
        {
            StartCoroutine(AnimateFloatingIndicator(rt, cg, floatObj));
        }
    }

    public void SpawnStreakBrokenIndicator()
    {
        if (SlimeController.Instance == null) return;
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null && topBarHud != null) rootCanvas = topBarHud.GetComponentInParent<Canvas>();
        if (rootCanvas == null) return;

        Camera uiCam = (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;
        if (uiCam == null) uiCam = Camera.main;

        GameObject floatObj = new GameObject("Float_StreakLost", typeof(RectTransform));
        floatObj.transform.SetParent(rootCanvas.transform, false);
        floatObj.transform.SetAsLastSibling();
        RectTransform rt = floatObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCam, SlimeController.Instance.transform.position);
        RectTransform canvasRt = rootCanvas.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPoint, uiCam, out Vector2 localPoint))
        {
            rt.anchoredPosition = localPoint + new Vector2(0f, 95f);
        }
        else
        {
            rt.anchoredPosition = new Vector2(0f, -320f);
        }
        rt.sizeDelta = new Vector2(380f, 70f);

        Text txt = floatObj.AddComponent<Text>();
        Font chosenFont = retroBodyFontRef != null ? retroBodyFontRef : (arcadeFontRef != null ? arcadeFontRef : (textScore != null ? textScore.font : null));
        if (chosenFont == null) chosenFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.font = chosenFont;
        txt.text = "STREAK LOST!";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(0.95f, 0.38f, 0.38f, 0.95f);
        txt.fontSize = 28;

        Outline ol = floatObj.AddComponent<Outline>();
        ol.effectColor = new Color(0.10f, 0.04f, 0.04f, 0.95f);
        ol.effectDistance = new Vector2(3f, -3f);

        CanvasGroup cg = floatObj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        if (Application.isPlaying)
        {
            StartCoroutine(AnimateFloatingIndicator(rt, cg, floatObj));
        }
    }"""

assert old_floating_methods in ui_text, "old_floating_methods not found in UIManager.cs"
ui_text = ui_text.replace(old_floating_methods, new_floating_methods, 1)

with open(ui_path, 'w', encoding='utf-8') as f:
    f.write(ui_text)
print("[OK] Updated UIManager.cs: Floating indicators rewritten with correct UI camera & Canvas parent")

# ==============================================================================
# 4. Update BuildMainGameScene.cs:
#    - Update all 10 unlockScores to 0, 100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700
#    - Fix TopBar_HUD layout to eliminate ANY overlap between Score and Best Score
# ==============================================================================
bmgs_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs"
with open(bmgs_path, 'r', encoding='utf-8') as f:
    bmgs_text = f.read()

# Replace all unlockScore entries in BuildMainGameScene
old_scores = [
    ("unlockScore = 40,", "unlockScore = 100,"),
    ("unlockScore = 90,", "unlockScore = 250,"),
    ("unlockScore = 150,", "unlockScore = 450,"),
    ("unlockScore = 220,", "unlockScore = 700,"),
    ("unlockScore = 300,", "unlockScore = 1000,"),
    ("unlockScore = 400,", "unlockScore = 1350,"),
    ("unlockScore = 520,", "unlockScore = 1750,"),
    ("unlockScore = 660,", "unlockScore = 2200,"),
    ("unlockScore = 820,", "unlockScore = 2700,")
]

for old_s, new_s in old_scores:
    if old_s in bmgs_text:
        bmgs_text = bmgs_text.replace(old_s, new_s, 1)
        print(f"[OK] Replaced {old_s} -> {new_s} in BuildMainGameScene.cs")

# Replace TopBar_HUD layout
old_topbar = """        // Score Text (Top-Center)
        Text scoreTxt = CreateUIText("Text_Score", topBar.transform, font, "SCORE: 0", 26, TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.78f, 1f));
        RectTransform scoreRect = scoreTxt.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 0.5f);
        scoreRect.anchorMax = new Vector2(0.5f, 0.5f);
        scoreRect.pivot = new Vector2(0.5f, 0.5f);
        scoreRect.anchoredPosition = new Vector2(-15f, 16f);
        scoreRect.sizeDelta = new Vector2(340f, 76f);
        uiMgr.textScore = scoreTxt;

        // Stage / Theme Badge (Below Score, Top-Center)
        Text themeBadgeTxt = CreateUIText("Text_ThemeBadge", topBar.transform, defaultBodyFont, "STAGE: WHISPERING FOREST", 19, TextAnchor.MiddleCenter, new Color(0.78f, 0.95f, 1f, 0.95f));
        RectTransform themeRect = themeBadgeTxt.GetComponent<RectTransform>();
        themeRect.anchorMin = new Vector2(0.5f, 0.5f);
        themeRect.anchorMax = new Vector2(0.5f, 0.5f);
        themeRect.pivot = new Vector2(0.5f, 0.5f);
        themeRect.anchoredPosition = new Vector2(-15f, -44f);
        themeRect.sizeDelta = new Vector2(400f, 34f);
        uiMgr.textThemeBadge = themeBadgeTxt;

        // High Score Text (Top-Right, Left of Settings Button)
        Text bestTxt = CreateUIText("Text_HighScore", topBar.transform, font, "BEST: 0", 22, TextAnchor.MiddleRight, new Color(1f, 0.84f, 0.35f, 1f));
        RectTransform bestRect = bestTxt.GetComponent<RectTransform>();
        bestRect.anchorMin = new Vector2(1f, 0.5f);
        bestRect.anchorMax = new Vector2(1f, 0.5f);
        bestRect.pivot = new Vector2(1f, 0.5f);
        bestRect.anchoredPosition = new Vector2(-130f, 18f);
        bestRect.sizeDelta = new Vector2(240f, 46f);
        uiMgr.textHighScore = bestTxt;

        // Magnet Active Timer (Legacy mini text below High Score)
        Text magTxt = CreateUIText("Text_MagnetTimer", topBar.transform, defaultBodyFont, "MAGNET: 6.0s", 16, TextAnchor.MiddleRight, new Color(0.45f, 0.95f, 1f, 1f));
        RectTransform magRect = magTxt.GetComponent<RectTransform>();
        magRect.anchorMin = new Vector2(1f, 0.5f);
        magRect.anchorMax = new Vector2(1f, 0.5f);
        magRect.pivot = new Vector2(1f, 0.5f);
        magRect.anchoredPosition = new Vector2(-130f, -42f);
        magRect.sizeDelta = new Vector2(240f, 36f);
        magTxt.gameObject.SetActive(false);
        uiMgr.textMagnetTimer = magTxt;

        // FRENZY STREAK HUD BADGE (Directly below High Score)
        Text frenzyTxt = CreateUIText("Text_FrenzyStreak", topBar.transform, retroBodyFont, "FRENZY: x0/12", 18, TextAnchor.MiddleRight, new Color(0.70f, 0.85f, 0.95f, 0.85f));
        RectTransform frRect = frenzyTxt.GetComponent<RectTransform>();
        frRect.anchorMin = new Vector2(1f, 0.5f);
        frRect.anchorMax = new Vector2(1f, 0.5f);
        frRect.pivot = new Vector2(1f, 0.5f);
        frRect.anchoredPosition = new Vector2(-130f, -42f);
        frRect.sizeDelta = new Vector2(260f, 36f);
        uiMgr.textFrenzyStreak = frenzyTxt;
        uiMgr.arcadeFontRef = font;
        uiMgr.retroBodyFontRef = retroBodyFont;

        // HUD Settings / Pause Gear Icon Button (Top-Right Corner — Pure Gear Icon, No "SET" Text!)
        Button btnHudSettings = CreateUIIconButton(
            "Button_HudSettings",
            topBar.transform,
            iconGearSprite,
            Vector2.zero,
            new Vector2(92f, 92f),
            new Vector2(60f, 60f),
            new Color(0.28f, 0.22f, 0.36f, 0.96f)
        );
        RectTransform hsRect = btnHudSettings.GetComponent<RectTransform>();
        hsRect.anchorMin = new Vector2(1f, 1f);
        hsRect.anchorMax = new Vector2(1f, 1f);
        hsRect.pivot = new Vector2(1f, 1f);
        hsRect.anchoredPosition = new Vector2(-16f, -16f);"""

new_topbar = """        // Score Text (Top-Center: Centered at x=0 with bounded width to prevent ANY overlap with Best Score)
        Text scoreTxt = CreateUIText("Text_Score", topBar.transform, font, "SCORE: 0", 24, TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.78f, 1f));
        RectTransform scoreRect = scoreTxt.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 0.5f);
        scoreRect.anchorMax = new Vector2(0.5f, 0.5f);
        scoreRect.pivot = new Vector2(0.5f, 0.5f);
        scoreRect.anchoredPosition = new Vector2(0f, 18f);
        scoreRect.sizeDelta = new Vector2(280f, 48f);
        uiMgr.textScore = scoreTxt;

        // Stage / Theme Badge (Below Score, Top-Center)
        Text themeBadgeTxt = CreateUIText("Text_ThemeBadge", topBar.transform, defaultBodyFont, "STAGE: WHISPERING FOREST", 18, TextAnchor.MiddleCenter, new Color(0.78f, 0.95f, 1f, 0.95f));
        RectTransform themeRect = themeBadgeTxt.GetComponent<RectTransform>();
        themeRect.anchorMin = new Vector2(0.5f, 0.5f);
        themeRect.anchorMax = new Vector2(0.5f, 0.5f);
        themeRect.pivot = new Vector2(0.5f, 0.5f);
        themeRect.anchoredPosition = new Vector2(0f, -36f);
        themeRect.sizeDelta = new Vector2(300f, 32f);
        uiMgr.textThemeBadge = themeBadgeTxt;

        // High Score Text (Top-Right, left of Gear button with strict right alignment)
        Text bestTxt = CreateUIText("Text_HighScore", topBar.transform, font, "BEST: 0", 20, TextAnchor.MiddleRight, new Color(1f, 0.84f, 0.35f, 1f));
        RectTransform bestRect = bestTxt.GetComponent<RectTransform>();
        bestRect.anchorMin = new Vector2(1f, 0.5f);
        bestRect.anchorMax = new Vector2(1f, 0.5f);
        bestRect.pivot = new Vector2(1f, 0.5f);
        bestRect.anchoredPosition = new Vector2(-110f, 18f);
        bestRect.sizeDelta = new Vector2(200f, 44f);
        uiMgr.textHighScore = bestTxt;

        // Magnet Active Timer (Legacy mini text below High Score)
        Text magTxt = CreateUIText("Text_MagnetTimer", topBar.transform, defaultBodyFont, "MAGNET: 6.0s", 16, TextAnchor.MiddleRight, new Color(0.45f, 0.95f, 1f, 1f));
        RectTransform magRect = magTxt.GetComponent<RectTransform>();
        magRect.anchorMin = new Vector2(1f, 0.5f);
        magRect.anchorMax = new Vector2(1f, 0.5f);
        magRect.pivot = new Vector2(1f, 0.5f);
        magRect.anchoredPosition = new Vector2(-110f, -36f);
        magRect.sizeDelta = new Vector2(200f, 32f);
        magTxt.gameObject.SetActive(false);
        uiMgr.textMagnetTimer = magTxt;

        // FRENZY STREAK HUD BADGE (Directly below High Score, right-aligned)
        Text frenzyTxt = CreateUIText("Text_FrenzyStreak", topBar.transform, retroBodyFont, "FRENZY: x0/12", 17, TextAnchor.MiddleRight, new Color(0.70f, 0.85f, 0.95f, 0.85f));
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
        hsRect.anchoredPosition = new Vector2(-16f, 0f);"""

assert old_topbar in bmgs_text, "old_topbar not found in BuildMainGameScene.cs"
bmgs_text = bmgs_text.replace(old_topbar, new_topbar, 1)

with open(bmgs_path, 'w', encoding='utf-8') as f:
    f.write(bmgs_text)
print("[OK] Updated BuildMainGameScene.cs: TopBar HUD layout refined and unlockScores updated")
