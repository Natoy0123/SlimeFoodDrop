import sys

def modify_file(filepath, replacements):
    with open(filepath, 'r', encoding='utf-8') as f:
        text = f.read()

    original_crlf = '\r\n' in text
    normalized = text.replace('\r\n', '\n')

    for old, new, desc in replacements:
        old_norm = old.replace('\r\n', '\n')
        new_norm = new.replace('\r\n', '\n')
        if old_norm not in normalized:
            print(f"[ERROR] Could not find target in {filepath}: {desc}")
            return False
        normalized = normalized.replace(old_norm, new_norm, 1)
        print(f"[OK] Applied: {desc}")

    if original_crlf:
        output = normalized.replace('\n', '\r\n')
    else:
        output = normalized

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(output)
    print(f"[SUCCESS] Saved {filepath}")
    return True

# -------------------------------------------------------------
# 1. Update GameManager.cs
# -------------------------------------------------------------
gm_file = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\GameManager.cs"
gm_replacements = [
    # Update() loop for FoodRush timer & Frenzy Streak window
    (
        '''        if (isTimeSlowActive)
        {
            timeSlowTimeRemaining -= Time.deltaTime;
            if (timeSlowTimeRemaining <= 0f)
            {
                timeSlowTimeRemaining = 0f;
                isTimeSlowActive = false;
            }
            anyChanged = true;
        }

        if (anyChanged)''',
        '''        if (isTimeSlowActive)
        {
            timeSlowTimeRemaining -= Time.deltaTime;
            if (timeSlowTimeRemaining <= 0f)
            {
                timeSlowTimeRemaining = 0f;
                isTimeSlowActive = false;
            }
            anyChanged = true;
        }

        // Food Rush duration timer
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
            // Streak window countdown: 4.0s without eating food decays the streak
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

        if (anyChanged)''',
        "GameManager FoodRush timer and streak window update"
    ),

    # OnFoodCaught: update streak, spawn floating indicator, trigger Food Rush
    (
        '''        comboCount++;
        int effectiveBase = Mathf.Max(basePoints, BaseFoodPoints);
        int bonus = (comboCount / 5) * 5;
        int totalPoints = effectiveBase + bonus;
        AddScore(totalPoints);''',
        '''        comboCount++;
        int effectiveBase = Mathf.Max(basePoints, BaseFoodPoints);
        if (IsFoodRushActive) effectiveBase += 10; // Extra +10 bonus points during Food Rush!
        int bonus = (comboCount / 5) * 5;
        int totalPoints = effectiveBase + bonus;
        AddScore(totalPoints);

        // Frenzy Streak logic (excluding poison / powerups)
        if (!IsFoodRushActive)
        {
            frenzyStreak++;
            frenzyStreakTimer = MaxFrenzyStreakWindow; // Reset 4-second streak window
            int target = FrenzyTargetForCurrentDifficulty;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.SpawnFloatingStreakIndicator(frenzyStreak, foodWorldPos);
                UIManager.Instance.UpdateFrenzyStreakHUD(frenzyStreak, target);
            }

            if (frenzyStreak >= target)
            {
                TriggerFoodRush();
            }
        }
        else
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SpawnFloatingRushBonus(foodWorldPos, totalPoints);
            }
        }''',
        "GameManager OnFoodCaught frenzy streak and food rush logic"
    ),

    # TriggerFoodRush and EndFoodRush methods
    (
        '''    public void ActivateMagnet(float duration)''',
        '''    public void TriggerFoodRush()
    {
        isFoodRushActive = true;
        foodRushMaxDuration = FoodRushDurationForCurrentDifficulty;
        foodRushTimeRemaining = foodRushMaxDuration;
        frenzyStreak = 0;
        frenzyStreakTimer = 0f;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayChimeSound();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFoodRushBanner(foodRushMaxDuration);
            UIManager.Instance.UpdateFrenzyStreakHUD(0, FrenzyTargetForCurrentDifficulty);
        }

        if (SlimeController.Instance != null)
        {
            SlimeController.Instance.TriggerCelebration();
        }
    }

    public void EndFoodRush()
    {
        isFoodRushActive = false;
        foodRushTimeRemaining = 0f;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideFoodRushBanner();
            UIManager.Instance.UpdateFrenzyStreakHUD(0, FrenzyTargetForCurrentDifficulty);
        }
    }

    public void ActivateMagnet(float duration)''',
        "GameManager TriggerFoodRush and EndFoodRush implementation"
    ),

    # Reset frenzy on poison hit
    (
        '''        comboCount = 0;
        currentLives = Mathf.Clamp(currentLives - 1, 0, maxLives);''',
        '''        comboCount = 0;
        frenzyStreak = 0;
        frenzyStreakTimer = 0f;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateFrenzyStreakHUD(0, FrenzyTargetForCurrentDifficulty);
            UIManager.Instance.SpawnStreakBrokenIndicator();
        }
        currentLives = Mathf.Clamp(currentLives - 1, 0, maxLives);''',
        "GameManager reset streak on poison hit"
    )
]

# -------------------------------------------------------------
# 2. Update UIManager.cs
# -------------------------------------------------------------
ui_file = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\UIManager.cs"
ui_replacements = [
    # Header fields
    (
        '''    [Header("Gameplay HUD References")]
    public GameObject topBarHud;
    public Image[] heartIcons;
    public Sprite heartFull;
    public Sprite heartEmpty;
    public Text textScore;
    public Text textHighScore;
    public Text textDifficultyBadge;
    public Text textThemeBadge;
    public Text textMagnetTimer;
    public Text textThemeToast;
    public Button btnHudSettings;''',
        '''    [Header("Gameplay HUD References")]
    public GameObject topBarHud;
    public Image[] heartIcons;
    public Sprite heartFull;
    public Sprite heartEmpty;
    public Text textScore;
    public Text textHighScore;
    public Text textDifficultyBadge;
    public Text textThemeBadge;
    public Text textMagnetTimer;
    public Text textThemeToast;
    public Button btnHudSettings;
    public Text textFrenzyStreak;
    public Font arcadeFontRef;
    public Font retroBodyFontRef;''',
        "UIManager Frenzy Streak & font refs"
    ),

    # Floating indicator methods
    (
        '''    public void SetGameplayHUDVisible(bool visible)''',
        '''    public void UpdateFrenzyStreakHUD(int streak, int target)
    {
        if (textFrenzyStreak == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsFoodRushActive)
        {
            textFrenzyStreak.text = $"RUSH: {GameManager.Instance.foodRushTimeRemaining:F1}s";
            textFrenzyStreak.color = new Color(1.0f, 0.86f, 0.25f, 1f);
            textFrenzyStreak.gameObject.SetActive(true);
            return;
        }

        if (streak <= 0)
        {
            textFrenzyStreak.text = $"FRENZY: x0/{target}";
            textFrenzyStreak.color = new Color(0.70f, 0.85f, 0.95f, 0.80f);
        }
        else
        {
            textFrenzyStreak.text = $"FRENZY: x{streak}/{target}";
            textFrenzyStreak.color = GetStreakColor(streak);
        }
        textFrenzyStreak.gameObject.SetActive(true);
    }

    public Color GetStreakColor(int streak)
    {
        if (streak >= 15) return new Color(1.0f, 0.20f, 0.65f, 1f); // Blazing Magenta
        if (streak >= 10) return new Color(1.0f, 0.50f, 0.15f, 1f); // Fiery Neon Orange
        if (streak >= 7)  return new Color(1.0f, 0.86f, 0.22f, 1f); // Radiant Amber Gold
        if (streak >= 4)  return new Color(0.28f, 0.94f, 1.0f, 1f);  // Electric Cyan
        return new Color(0.48f, 1.0f, 0.56f, 1f);                  // Fresh Mint Green
    }

    public void SpawnFloatingStreakIndicator(int streakCount, Vector3 worldPos)
    {
        if (topBarHud == null && hudPanel == null) return;
        Transform parentTransform = topBarHud != null ? topBarHud.transform.parent : hudPanel.transform;

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
        if (topBarHud == null && hudPanel == null) return;
        Transform parentTransform = topBarHud != null ? topBarHud.transform.parent : hudPanel.transform;

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
        if ((topBarHud == null && hudPanel == null) || SlimeController.Instance == null) return;
        Transform parentTransform = topBarHud != null ? topBarHud.transform.parent : hudPanel.transform;

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
    }

    private IEnumerator AnimateFloatingIndicator(RectTransform rt, CanvasGroup cg, GameObject targetObj)
    {
        float dur = 0.70f;
        float elapsed = 0f;
        Vector2 startPos = rt.anchoredPosition;
        Vector2 targetPos = startPos + new Vector2(0f, 100f);

        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(elapsed / dur);

            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, u));
            float scale = u < 0.25f ? Mathf.Lerp(1.35f, 1.0f, u / 0.25f) : 1.0f;
            rt.localScale = new Vector3(scale, scale, 1f);

            if (u > 0.50f)
            {
                cg.alpha = Mathf.Lerp(1f, 0f, (u - 0.50f) / 0.50f);
            }
            else
            {
                cg.alpha = 1f;
            }

            yield return null;
        }

        if (targetObj != null) Destroy(targetObj);
    }

    public void ShowFoodRushBanner(float duration)
    {
        if (textThemeToast != null)
        {
            textThemeToast.text = $"FOOD RUSH ACTIVE! ({duration:F0}s)";
            textThemeToast.color = new Color(1.0f, 0.90f, 0.30f, 1f);
            textThemeToast.gameObject.SetActive(true);
        }
    }

    public void UpdateFoodRushHUD(float remaining, float max)
    {
        if (textThemeToast != null && textThemeToast.gameObject.activeSelf)
        {
            textThemeToast.text = $"FOOD RUSH! {remaining:F1}s";
        }
        if (textFrenzyStreak != null)
        {
            textFrenzyStreak.text = $"RUSH: {remaining:F1}s";
            textFrenzyStreak.color = new Color(1.0f, 0.86f, 0.25f, 1f);
        }
    }

    public void HideFoodRushBanner()
    {
        if (textThemeToast != null)
        {
            textThemeToast.gameObject.SetActive(false);
        }
    }

    public void SetGameplayHUDVisible(bool visible)''',
        "UIManager Frenzy Floating Indicator & FoodRush methods"
    )
]

# -------------------------------------------------------------
# 3. Update BuildMainGameScene.cs
# -------------------------------------------------------------
build_file = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs"
build_replacements = [
    (
        '''        string[] stageDescs = {
            "Starting stage! Gentle drop speed (1.0x base) and low 12% Poison rate so you can build your first combo streak.",
            "Unlocks at 40 points! Bright rolling grass and azure sky with gentle breeze. Poison hazard rate 14%.",
            "Unlocks at 90 points! High alpine sunset peaks with windswept atmosphere. Poison hazard rate 17%.",
            "Unlocks at 150 points! Deep jungle lagoon with cascading mist and swaying palm trees. Poison rate 19%.",
            "Unlocks at 220 points! High desert canyons and golden dunes with heat distortion shimmer. Poison rate 21%.",
            "Unlocks at 300 points! Subaquatic wonderland with deep ocean light rays cutting through clear water. Poison rate 23%.",
            "Unlocks at 400 points! Steampunk factory chimneys, gearworks, and retro chiptune machinery. Poison rate 25%.",
            "Unlocks at 520 points! Subterranean cavern filled with luminescent glowing crystals and rock arches. Poison rate 27%.",
            "Unlocks at 660 points! High-altitude floating sky sanctuary with billowing clouds and magic ruins. Poison rate 29%.",
            "Unlocks at 820+ points! The ultimate nocturnal icy citadel with maximum arcade velocity and 32% Poison rate!"
        };''',
        '''        string[] stageDescs = {
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
        };''',
        "BuildMainGameScene Almanac stageDescs higher points"
    )
]

ok1 = modify_file(gm_file, gm_replacements)
ok2 = modify_file(ui_file, ui_replacements)
ok3 = modify_file(build_file, build_replacements)

if ok1 and ok2 and ok3:
    print("\nALL REMAINING FRENZY & THEME PROGRESSION UPDATES APPLIED SUCCESSFULLY!")
else:
    print("\n[ERROR] SOME UPDATES FAILED!")
    sys.exit(1)
