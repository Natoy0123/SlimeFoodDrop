import re, sys

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
# 1. Update PixarSplashCinematic.cs
# -------------------------------------------------------------
pixar_file = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\PixarSplashCinematic.cs"
pixar_replacements = [
    (
        '''    private IEnumerator FastFinishRoutine()
    {
        if (loadingBarGroup != null) loadingBarGroup.alpha = 1f;
        UpdateLoadingBar(1f, "100%", "READY TO DROP!");
        yield return new WaitForSecondsRealtime(0.2f);
        currentOnComplete?.Invoke();
    }''',
        '''    private IEnumerator FastFinishRoutine()
    {
        if (loadingBarGroup != null) loadingBarGroup.alpha = 1f;
        UpdateLoadingBar(1f, "100%", "READY TO DROP!");
        yield return new WaitForSecondsRealtime(0.15f);

        // Smooth fade out before transitioning to Home Screen
        CanvasGroup splashCg = GetComponent<CanvasGroup>();
        if (splashCg != null)
        {
            float fadeDur = 0.35f;
            float elapsed = 0f;
            while (elapsed < fadeDur)
            {
                elapsed += Time.unscaledDeltaTime;
                splashCg.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDur);
                yield return null;
            }
            splashCg.alpha = 0f;
        }

        currentOnComplete?.Invoke();
    }''',
        "PixarSplashCinematic FastFinishRoutine smooth fade"
    ),
    (
        '''        UpdateLoadingBar(1f, "100%", "ALL 10 STAGES READY TO DROP!");
        yield return new WaitForSecondsRealtime(0.45f);

        HideSpeechBubble();
        currentOnComplete?.Invoke();
    }''',
        '''        UpdateLoadingBar(1f, "100%", "ALL 10 STAGES READY TO DROP!");
        yield return new WaitForSecondsRealtime(0.45f);

        // Smooth fade out before transitioning to Home Screen
        CanvasGroup splashCg = GetComponent<CanvasGroup>();
        if (splashCg != null)
        {
            float fadeDur = 0.45f;
            float elapsed = 0f;
            while (elapsed < fadeDur)
            {
                elapsed += Time.unscaledDeltaTime;
                splashCg.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDur);
                yield return null;
            }
            splashCg.alpha = 0f;
        }

        HideSpeechBubble();
        currentOnComplete?.Invoke();
    }''',
        "PixarSplashCinematic CinematicSequenceRoutine end smooth fade"
    )
]

# -------------------------------------------------------------
# 2. Update UIManager.cs
# -------------------------------------------------------------
ui_file = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\UIManager.cs"
ui_replacements = [
    (
        '''    public void ShowHomeScreen(int bestScore = 0)
    {
        if (splashPanel != null) splashPanel.SetActive(false);
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (guidebookPanel != null) guidebookPanel.SetActive(false);
        if (countdownPanel != null) countdownPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        if (textHomeBestScore != null)
        {
            textHomeBestScore.text = $"BEST SCORE: {bestScore}";
        }

        if (homePanel != null)
        {
            homePanel.SetActive(true);''',
        '''    public void ShowHomeScreen(int bestScore = 0)
    {
        if (splashPanel != null) splashPanel.SetActive(false);
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (guidebookPanel != null) guidebookPanel.SetActive(false);
        if (countdownPanel != null) countdownPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        if (textHomeBestScore != null)
        {
            textHomeBestScore.text = $"BEST SCORE: {bestScore}";
        }

        if (homePanel != null)
        {
            homePanel.SetActive(true);
            CanvasGroup homeCg = homePanel.GetComponent<CanvasGroup>();
            if (homeCg == null) homeCg = homePanel.AddComponent<CanvasGroup>();
            if (Application.isPlaying)
            {
                StartCoroutine(FadeInCanvasGroup(homeCg, 0.40f));
            }
            else
            {
                homeCg.alpha = 1f;
            }''',
        "UIManager ShowHomeScreen smooth fade-in"
    ),
    (
        '''        if (textEnvSubtitle != null)
        {
            textEnvSubtitle.text = envSubtitle;
        }''',
        '''        if (textEnvSubtitle != null)
        {
            textEnvSubtitle.text = envSubtitle;
            textEnvSubtitle.horizontalOverflow = HorizontalWrapMode.Wrap;
            textEnvSubtitle.verticalOverflow = VerticalWrapMode.Truncate;
        }''',
        "UIManager ShowEnvironmentTransitionBanner wrap/truncate"
    ),
    (
        '''    public void SetGameplayHUDVisible(bool visible)''',
        '''    private IEnumerator FadeInCanvasGroup(CanvasGroup cg, float duration)
    {
        if (cg == null) yield break;
        cg.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    public void SetGameplayHUDVisible(bool visible)''',
        "UIManager FadeInCanvasGroup helper coroutine"
    )
]

# -------------------------------------------------------------
# 3. Update BuildMainGameScene.cs
# -------------------------------------------------------------
build_file = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs"
build_replacements = [
    # A. Environment Banner: expand container, use retroBodyFont, wrap/truncate, font size 22
    (
        '''        GameObject envBanner = CreateUIElement("Environment_Banner_Panel", safeAreaObj.transform);
        RectTransform ebRect = envBanner.GetComponent<RectTransform>();
        ebRect.anchorMin = new Vector2(0.5f, 1f);
        ebRect.anchorMax = new Vector2(0.5f, 1f);
        ebRect.pivot = new Vector2(0.5f, 1f);
        ebRect.anchoredPosition = new Vector2(0f, -330f);
        ebRect.sizeDelta = new Vector2(880f, 150f);

        Image ebBg = envBanner.AddComponent<Image>();
        ebBg.color = new Color(0.10f, 0.08f, 0.16f, 0.90f);
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

        Text envStageTxt = CreateUIText("Text_EnvStageBadge", envBanner.transform, font, "ENVIRONMENT STAGE 1", 22, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.38f, 1f));
        envStageTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 44f);
        envStageTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(820f, 34f);

        Text envTitleTxt = CreateUIText("Text_EnvTitle", envBanner.transform, font, "WHISPERING FOREST", 40, TextAnchor.MiddleCenter, new Color(0.52f, 0.98f, 0.68f, 1f));
        envTitleTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 2f);
        envTitleTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(820f, 52f);

        Text envSubTxt = CreateUIText("Text_EnvSubtitle", envBanner.transform, font, "Lush Woodland Canopy  |  Peaceful Forest BGM", 23, TextAnchor.MiddleCenter, new Color(0.84f, 0.94f, 1f, 0.95f));
        envSubTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -44f);
        envSubTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(820f, 36f);''',
        '''        GameObject envBanner = CreateUIElement("Environment_Banner_Panel", safeAreaObj.transform);
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

        Text envStageTxt = CreateUIText("Text_EnvStageBadge", envBanner.transform, font, "ENVIRONMENT STAGE 1", 20, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.38f, 1f));
        envStageTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 52f);
        envStageTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(880f, 32f);

        Text envTitleTxt = CreateUIText("Text_EnvTitle", envBanner.transform, font, "WHISPERING FOREST", 32, TextAnchor.MiddleCenter, new Color(0.52f, 0.98f, 0.68f, 1f));
        envTitleTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 12f);
        envTitleTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(880f, 44f);

        Text envSubTxt = CreateUIText("Text_EnvSubtitle", envBanner.transform, retroBodyFont, "Lush Woodland Canopy  |  Peaceful Forest BGM", 22, TextAnchor.MiddleCenter, new Color(0.84f, 0.94f, 1f, 0.95f));
        envSubTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        envSubTxt.verticalOverflow = VerticalWrapMode.Truncate;
        envSubTxt.lineSpacing = 1.15f;
        envSubTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -40f);
        envSubTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(880f, 54f);''',
        "BuildMainGameScene Environment Banner sizing and retroBodyFont wrap"
    ),

    # B. Home Screen CanvasGroup
    (
        '''        Image homeOverlay = homePanel.AddComponent<Image>();
        homeOverlay.color = new Color(0.05f, 0.03f, 0.08f, 0.44f);''',
        '''        Image homeOverlay = homePanel.AddComponent<Image>();
        homeOverlay.color = new Color(0.05f, 0.03f, 0.08f, 0.44f);
        CanvasGroup homeCg = homePanel.AddComponent<CanvasGroup>();
        homeCg.alpha = 1f;''',
        "BuildMainGameScene Home Screen CanvasGroup"
    ),

    # C. Difficulty Card Button font sizes (title 26, sub 22)
    (
        '''        Text titleTxt = CreateUIText("Text_ModeTitle", btnObj.transform, defaultBodyFont, title, 24, TextAnchor.MiddleCenter, Color.white);
        titleTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        titleTxt.verticalOverflow = VerticalWrapMode.Truncate;
        RectTransform tRect = titleTxt.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0.5f, 0.5f);
        tRect.anchorMax = new Vector2(0.5f, 0.5f);
        tRect.pivot = new Vector2(0.5f, 0.5f);
        tRect.anchoredPosition = new Vector2(22f, 22f);
        tRect.sizeDelta = new Vector2(size.x - 120f, 52f);

        Text subTxt = CreateUIText("Text_ModeDesc", btnObj.transform, defaultBodyFont, subtitle, 19, TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.84f, 0.95f));
        subTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        subTxt.verticalOverflow = VerticalWrapMode.Truncate;
        RectTransform sRect = subTxt.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0.5f, 0.5f);
        sRect.anchorMax = new Vector2(0.5f, 0.5f);
        sRect.pivot = new Vector2(0.5f, 0.5f);
        sRect.anchoredPosition = new Vector2(22f, -26f);
        sRect.sizeDelta = new Vector2(size.x - 120f, 42f);''',
        '''        Text titleTxt = CreateUIText("Text_ModeTitle", btnObj.transform, defaultBodyFont, title, 26, TextAnchor.MiddleCenter, Color.white);
        titleTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        titleTxt.verticalOverflow = VerticalWrapMode.Truncate;
        RectTransform tRect = titleTxt.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0.5f, 0.5f);
        tRect.anchorMax = new Vector2(0.5f, 0.5f);
        tRect.pivot = new Vector2(0.5f, 0.5f);
        tRect.anchoredPosition = new Vector2(22f, 24f);
        tRect.sizeDelta = new Vector2(size.x - 120f, 52f);

        Text subTxt = CreateUIText("Text_ModeDesc", btnObj.transform, defaultBodyFont, subtitle, 22, TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.84f, 0.95f));
        subTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        subTxt.verticalOverflow = VerticalWrapMode.Truncate;
        RectTransform sRect = subTxt.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0.5f, 0.5f);
        sRect.anchorMax = new Vector2(0.5f, 0.5f);
        sRect.pivot = new Vector2(0.5f, 0.5f);
        sRect.anchoredPosition = new Vector2(22f, -26f);
        sRect.sizeDelta = new Vector2(size.x - 120f, 48f);''',
        "BuildMainGameScene Difficulty Card Button fonts"
    ),

    # D. Difficulty Menu Stage Hint (font size 22)
    (
        '''        Text stageHintTxt = CreateUIText("Text_StageHint", diffCard.transform, retroBodyFont, "STAGE THEMES UNLOCK AT 120, 260 & 420 PTS\\nWITH DYNAMIC BGM & UNIQUE HAZARDS!", 20, TextAnchor.MiddleCenter, new Color(0.72f, 0.92f, 1f, 0.95f));
        stageHintTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        stageHintTxt.verticalOverflow = VerticalWrapMode.Truncate;
        stageHintTxt.lineSpacing = 1.15f;
        stageHintTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -238f);
        stageHintTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(780f, 64f);''',
        '''        Text stageHintTxt = CreateUIText("Text_StageHint", diffCard.transform, retroBodyFont, "STAGE THEMES UNLOCK AT 120, 260 & 420 PTS\\nWITH DYNAMIC BGM & UNIQUE HAZARDS!", 22, TextAnchor.MiddleCenter, new Color(0.72f, 0.92f, 1f, 0.95f));
        stageHintTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        stageHintTxt.verticalOverflow = VerticalWrapMode.Truncate;
        stageHintTxt.lineSpacing = 1.15f;
        stageHintTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -238f);
        stageHintTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(800f, 68f);''',
        "BuildMainGameScene Difficulty Stage Hint font size 22"
    ),

    # E. Splash Screen: Lower TopHeader to home title level (560f), remove skip button, raise loading bar
    (
        '''        // Top Screen Header Strip: Slime Studio Presents + Grand Title + Subtitle
        GameObject splashTopHeader = CreateUIElement("Splash_TopHeader", splashObj.transform);
        RectTransform sthRect = splashTopHeader.GetComponent<RectTransform>();
        sthRect.anchorMin = new Vector2(0.5f, 1f);
        sthRect.anchorMax = new Vector2(0.5f, 1f);
        sthRect.pivot = new Vector2(0.5f, 1f);
        sthRect.anchoredPosition = new Vector2(0f, -80f);
        sthRect.sizeDelta = new Vector2(920f, 220f);''',
        '''        // Top Screen Header Strip: Slime Studio Presents + Grand Title + Subtitle (Same level as Home Title Screen)
        GameObject splashTopHeader = CreateUIElement("Splash_TopHeader", splashObj.transform);
        RectTransform sthRect = splashTopHeader.GetComponent<RectTransform>();
        sthRect.anchorMin = new Vector2(0.5f, 0.5f);
        sthRect.anchorMax = new Vector2(0.5f, 0.5f);
        sthRect.pivot = new Vector2(0.5f, 0.5f);
        sthRect.anchoredPosition = new Vector2(0f, 560f);
        sthRect.sizeDelta = new Vector2(920f, 220f);''',
        "BuildMainGameScene Splash TopHeader lowered to match home title level (560f)"
    ),
    (
        '''        // Skip Button (Top Right)
        Button skipBtn = CreateUIButton("Button_SkipCinematic", splashObj.transform, font, "SKIP >", Vector2.zero, new Vector2(150f, 52f), new Color(0.20f, 0.24f, 0.36f, 0.92f), 20);
        RectTransform skipRt = skipBtn.GetComponent<RectTransform>();
        skipRt.anchorMin = new Vector2(1f, 1f);
        skipRt.anchorMax = new Vector2(1f, 1f);
        skipRt.pivot = new Vector2(1f, 1f);
        skipRt.anchoredPosition = new Vector2(-28f, -28f);
        cinematic.skipButton = skipBtn;

        // --- Bottom Screen Loading Bar ---
        GameObject loadGroupObj = CreateUIElement("Splash_LoadingGroup", splashObj.transform);
        RectTransform lgRect = loadGroupObj.GetComponent<RectTransform>();
        lgRect.anchorMin = new Vector2(0.5f, 0f);
        lgRect.anchorMax = new Vector2(0.5f, 0f);
        lgRect.pivot = new Vector2(0.5f, 0f);
        lgRect.anchoredPosition = new Vector2(0f, 75f);''',
        '''        // --- Bottom Screen Loading Bar (Elevated away from phone edge) ---
        GameObject loadGroupObj = CreateUIElement("Splash_LoadingGroup", splashObj.transform);
        RectTransform lgRect = loadGroupObj.GetComponent<RectTransform>();
        lgRect.anchorMin = new Vector2(0.5f, 0f);
        lgRect.anchorMax = new Vector2(0.5f, 0f);
        lgRect.pivot = new Vector2(0.5f, 0f);
        lgRect.anchoredPosition = new Vector2(0f, 155f);''',
        "BuildMainGameScene Splash remove skip button & raise loading bar to 155f"
    ),

    # F. Almanac Card Subtext & fonts
    (
        '''        Text gbSub = CreateUIText("Text_GuidebookSub", gbCard.transform, bodyFont, "TAP ANY CARD BELOW TO INSPECT FULL ITEM STATS, LORE & STRATEGY TIPS", 16, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.48f, 1f));
        gbSub.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 544f);
        gbSub.GetComponent<RectTransform>().sizeDelta = new Vector2(860f, 32f);''',
        '''        Text gbSub = CreateUIText("Text_GuidebookSub", gbCard.transform, bodyFont, "TAP ANY CARD BELOW TO INSPECT FULL ITEM STATS, LORE & STRATEGY TIPS", 22, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.48f, 1f));
        gbSub.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 544f);
        gbSub.GetComponent<RectTransform>().sizeDelta = new Vector2(860f, 40f);''',
        "BuildMainGameScene Almanac GuidebookSub font 22"
    ),
    (
        '''        Text tTitle = CreateUIText("Text_EntryTitle", textContainer.transform, font, itemName, 17, TextAnchor.MiddleLeft, new Color(1f, 0.97f, 0.86f, 1f));
        tTitle.horizontalOverflow = HorizontalWrapMode.Wrap;
        tTitle.verticalOverflow = VerticalWrapMode.Truncate;
        tTitle.lineSpacing = 1.1f;
        RectTransform titleRect = tTitle.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.5f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(0f, 0f);
        titleRect.offsetMax = new Vector2(0f, 0f);

        Text tBadge = CreateUIText("Text_EntryBadge", textContainer.transform, bodyFont, categoryBadge, 16, TextAnchor.MiddleLeft, badgeColor);
        tBadge.horizontalOverflow = HorizontalWrapMode.Wrap;
        tBadge.verticalOverflow = VerticalWrapMode.Truncate;
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
        hpRect.sizeDelta = new Vector2(165f, 54f);''',
        '''        Text tTitle = CreateUIText("Text_EntryTitle", textContainer.transform, font, itemName, 20, TextAnchor.MiddleLeft, new Color(1f, 0.97f, 0.86f, 1f));
        tTitle.horizontalOverflow = HorizontalWrapMode.Wrap;
        tTitle.verticalOverflow = VerticalWrapMode.Truncate;
        tTitle.lineSpacing = 1.1f;
        RectTransform titleRect = tTitle.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.5f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(0f, 0f);
        titleRect.offsetMax = new Vector2(0f, 0f);

        Text tBadge = CreateUIText("Text_EntryBadge", textContainer.transform, bodyFont, categoryBadge, 20, TextAnchor.MiddleLeft, badgeColor);
        tBadge.horizontalOverflow = HorizontalWrapMode.Wrap;
        tBadge.verticalOverflow = VerticalWrapMode.Truncate;
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
        hpRect.sizeDelta = new Vector2(175f, 58f);''',
        "BuildMainGameScene Almanac Entry Card font upgrades"
    ),
    (
        '''        Text tHint = CreateUIText("Text_HintLabel", hintPill.transform, font, "DETAILS >", 16, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.50f, 1f));''',
        '''        Text tHint = CreateUIText("Text_HintLabel", hintPill.transform, font, "DETAILS >", 18, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.50f, 1f));''',
        "BuildMainGameScene Almanac Details Pill font 18"
    ),

    # G. Almanac Item Detail Modal fonts (badge 20, title 28, stats 22, lore 23, tips 23)
    (
        '''        Text badgeTxt = CreateUIText("Text_CategoryBadge", badgeObj.transform, arcadeFont, "ITEM DETAILS", 18, TextAnchor.MiddleCenter, new Color(0.48f, 0.96f, 1f, 1f));''',
        '''        Text badgeTxt = CreateUIText("Text_CategoryBadge", badgeObj.transform, arcadeFont, "ITEM DETAILS", 20, TextAnchor.MiddleCenter, new Color(0.48f, 0.96f, 1f, 1f));''',
        "BuildMainGameScene Detail Modal CategoryBadge font 20"
    ),
    (
        '''        Text titleTxt = CreateUIText("Text_DetailTitle", dpCard.transform, arcadeFont, "ITEM NAME", 24, TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.72f, 1f));''',
        '''        Text titleTxt = CreateUIText("Text_DetailTitle", dpCard.transform, arcadeFont, "ITEM NAME", 28, TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.72f, 1f));''',
        "BuildMainGameScene Detail Modal Title font 28"
    ),
    (
        '''        Text statsTxt = CreateUIText("Text_DetailStats", statsBox.transform, bodyFont, "+10 PTS  •  BUILD COMBO STREAK", 20, TextAnchor.MiddleCenter, new Color(0.50f, 0.98f, 0.65f, 1f));''',
        '''        Text statsTxt = CreateUIText("Text_DetailStats", statsBox.transform, bodyFont, "+10 PTS  •  BUILD COMBO STREAK", 22, TextAnchor.MiddleCenter, new Color(0.50f, 0.98f, 0.65f, 1f));''',
        "BuildMainGameScene Detail Modal Stats font 22"
    ),
    (
        '''        Text descLbl = CreateUIText("Label_Description", descBox.transform, arcadeFont, "LORE & MECHANICS", 18, TextAnchor.MiddleLeft, new Color(1f, 0.88f, 0.44f, 1f));
        descLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(24f, 96f);
        descLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(790f, 28f);

        Text descTxt = CreateUIText("Text_DetailDescription", descBox.transform, bodyFont,
            "Full item mechanics, origins, and interactive properties during gameplay.",
            21, TextAnchor.UpperLeft, new Color(0.94f, 0.96f, 0.98f, 1f));''',
        '''        Text descLbl = CreateUIText("Label_Description", descBox.transform, arcadeFont, "LORE & MECHANICS", 20, TextAnchor.MiddleLeft, new Color(1f, 0.88f, 0.44f, 1f));
        descLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(24f, 96f);
        descLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(790f, 28f);

        Text descTxt = CreateUIText("Text_DetailDescription", descBox.transform, bodyFont,
            "Full item mechanics, origins, and interactive properties during gameplay.",
            23, TextAnchor.UpperLeft, new Color(0.94f, 0.96f, 0.98f, 1f));''',
        "BuildMainGameScene Detail Modal Lore font 23"
    ),
    (
        '''        Text tipsLbl = CreateUIText("Label_Tips", tipsBox.transform, arcadeFont, "PRO STRATEGY TIP", 18, TextAnchor.MiddleLeft, new Color(1f, 0.92f, 0.45f, 1f));
        tipsLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(24f, 74f);
        tipsLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(790f, 28f);

        Text tipsTxt = CreateUIText("Text_DetailTips", tipsBox.transform, bodyFont,
            "Pro gameplay hints and optimal techniques for maximizing score.",
            21, TextAnchor.UpperLeft, new Color(1f, 0.98f, 0.88f, 1f));''',
        '''        Text tipsLbl = CreateUIText("Label_Tips", tipsBox.transform, arcadeFont, "PRO STRATEGY TIP", 20, TextAnchor.MiddleLeft, new Color(1f, 0.92f, 0.45f, 1f));
        tipsLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(24f, 74f);
        tipsLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(790f, 28f);

        Text tipsTxt = CreateUIText("Text_DetailTips", tipsBox.transform, bodyFont,
            "Pro gameplay hints and optimal techniques for maximizing score.",
            23, TextAnchor.UpperLeft, new Color(1f, 0.98f, 0.88f, 1f));''',
        "BuildMainGameScene Detail Modal Tips font 23"
    ),

    # H. Settings Modal audio labels (24pt) and vibration label (22pt)
    (
        '''        Text bgmLbl = CreateUIText("Label_BgmVolume", audioBox.transform, font, "MUSIC (BGM)", 20, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));''',
        '''        Text bgmLbl = CreateUIText("Label_BgmVolume", audioBox.transform, font, "MUSIC (BGM)", 24, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));''',
        "BuildMainGameScene Settings BGM label font 24"
    ),
    (
        '''        Text sfxLbl = CreateUIText("Label_SfxVolume", audioBox.transform, font, "SOUND (SFX)", 20, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));''',
        '''        Text sfxLbl = CreateUIText("Label_SfxVolume", audioBox.transform, font, "SOUND (SFX)", 24, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));''',
        "BuildMainGameScene Settings SFX label font 24"
    ),
    (
        '''        Text vibTitleLbl = CreateUIText("Label_VibrationTitle", audioBox.transform, font, "POISON HAPTICS", 18, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));''',
        '''        Text vibTitleLbl = CreateUIText("Label_VibrationTitle", audioBox.transform, font, "POISON HAPTICS", 22, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));''',
        "BuildMainGameScene Settings Vibration label font 22"
    ),

    # I. PowerUp Duration Bar Row (fontSize 23, row height 44f)
    (
        '''        rRect.sizeDelta = new Vector2(760f, 40f);''',
        '''        rRect.sizeDelta = new Vector2(760f, 44f);''',
        "BuildMainGameScene PowerUp Duration Bar height 44"
    ),
    (
        '''        labelText = CreateUIText("Text_BarLabel", trackObj.transform, font, defaultLabel, 21, TextAnchor.MiddleCenter, Color.white);''',
        '''        labelText = CreateUIText("Text_BarLabel", trackObj.transform, font, defaultLabel, 23, TextAnchor.MiddleCenter, Color.white);''',
        "BuildMainGameScene PowerUp Duration Bar label font 23"
    )
]

ok1 = modify_file(pixar_file, pixar_replacements)
ok2 = modify_file(ui_file, ui_replacements)
ok3 = modify_file(build_file, build_replacements)

if ok1 and ok2 and ok3:
    print("\nALL SCRIPTS UPDATED SUCCESSFULLY!")
else:
    print("\n[ERROR] SOME UPDATES FAILED!")
    sys.exit(1)
