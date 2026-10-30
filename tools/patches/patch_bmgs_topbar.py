import os

bmgs_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs"
with open(bmgs_path, "r", encoding="utf-8") as f:
    text = f.read()

start_marker = "// Score Text (Top-Center)"
end_marker = "uiMgr.btnHudSettings = btnHudSettings;"

start_idx = text.find(start_marker)
end_idx = text.find(end_marker)

print(f"start_idx: {start_idx}, end_idx: {end_idx}")
if start_idx != -1 and end_idx != -1:
    old_slice = text[start_idx : end_idx + len(end_marker)]
    new_slice = """// Score Text (Top-Center: Centered at x=0 with bounded width to prevent ANY overlap with Best Score)
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
        hsRect.anchoredPosition = new Vector2(-16f, 0f);
        uiMgr.btnHudSettings = btnHudSettings;"""

    text = text[:start_idx] + new_slice + text[end_idx + len(end_marker):]
    with open(bmgs_path, "w", encoding="utf-8") as f:
        f.write(text)
    print("[SUCCESS] BuildMainGameScene.cs updated successfully!")
else:
    print("[ERROR] Markers not found in BuildMainGameScene.cs")
