import os

path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs"
with open(path, "r", encoding="utf-8") as f:
    c = f.read()

# 1. Update Slime's Y position in BuildMainGameScene to match new bottomOffset (1.25f)
c = c.replace(
    "slime.transform.position = new Vector3(0.35f, ResponsiveCamera.BottomBound + slime.bottomOffset, 0f);",
    "slime.transform.position = new Vector3(0.35f, ResponsiveCamera.BottomBound + slime.bottomOffset, 0f);"
)
c = c.replace(
    "sp.y = ResponsiveCamera.BottomBound + SlimeController.Instance.bottomOffset;",
    "sp.y = ResponsiveCamera.BottomBound + SlimeController.Instance.bottomOffset;"
)

# 2. Add Floor_Platform and Transition_Floor_Platform in BuildMainGameScene.cs
old_tm_creation = """        ThemeManager tm = tmObj.AddComponent<ThemeManager>();
        tm.primaryRenderers = bgRenderers;
        tm.transitionRenderers = transRenderers;"""

new_tm_creation = """        ThemeManager tm = tmObj.AddComponent<ThemeManager>();
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
        respCam.transitionFloorPlatformRenderer = transFloorPlatformSr;"""

if old_tm_creation in c:
    c = c.replace(old_tm_creation, new_tm_creation, 1)
    print("BuildMainGameScene.cs: Added Floor_Platform GameObjects")

# 3. Wire floorPlatformSprite for all 10 themes in BuildMainGameScene.cs
theme_platforms = [
    ("Whispering Forest", "platform_0_forest.png"),
    ("Breezy Plains", "platform_1_plains.png"),
    ("Sunset Peaks", "platform_2_mountain.png"),
    ("Emerald Tropic", "platform_3_tropical.png"),
    ("Sunbaked Dunes", "platform_4_desert.png"),
    ("Coral Abyss", "platform_5_underwater.png"),
    ("Steamworks Spire", "platform_6_industrial.png"),
    ("Crystal Caverns", "platform_7_cave.png"),
    ("Sky Sanctuary", "platform_8_sky.png"),
    ("Frost Citadel", "platform_9_citadel.png"),
]

for t_name, p_sprite in theme_platforms:
    target = f'themeName = "{t_name}",'
    replacement = f'themeName = "{t_name}",\n                floorPlatformSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background/Platforms/{p_sprite}"),'
    if target in c and replacement not in c:
        c = c.replace(target, replacement, 1)
        print(f"BuildMainGameScene.cs: Wired platform for {t_name}")

# 4. Enlarge Tutorial Dialog Box to 980x440 and bump font sizes
old_tut_box = """        tdRect.anchoredPosition = new Vector2(0f, 34f);
        tdRect.sizeDelta = new Vector2(960f, 316f);"""

new_tut_box = """        tdRect.anchoredPosition = new Vector2(0f, 34f);
        tdRect.sizeDelta = new Vector2(980f, 440f);"""

if old_tut_box in c:
    c = c.replace(old_tut_box, new_tut_box, 1)
    print("BuildMainGameScene.cs: Enlarged tutorial dialog box to 980x440")

# Update tutorial body text container
old_tut_body = """        Text tutBodyTxt = CreateUIText("Text_TutorialBody", tutDialogBox.transform, defaultBodyFont,
            "\\\"Standard 100% arcade drop speed with balanced Poison rates (12%–26% across stages) and classic Power-Up drop frequency. Clean competitive rules!\\\"",
            21, TextAnchor.UpperLeft, new Color(0.96f, 0.98f, 1f, 1f));
        tutBodyTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        tutBodyTxt.lineSpacing = 1.10f;
        RectTransform tbRect = tutBodyTxt.GetComponent<RectTransform>();
        tbRect.anchorMin = new Vector2(0f, 1f);
        tbRect.anchorMax = new Vector2(0f, 1f);
        tbRect.pivot = new Vector2(0f, 1f);
        tbRect.anchoredPosition = new Vector2(34f, -108f);
        tbRect.sizeDelta = new Vector2(892f, 130f);"""

new_tut_body = """        Text tutBodyTxt = CreateUIText("Text_TutorialBody", tutDialogBox.transform, defaultBodyFont,
            "\\\"Standard 100% arcade drop speed with balanced Poison rates (12%–26% across stages) and classic Power-Up drop frequency. Clean competitive rules!\\\"",
            26, TextAnchor.UpperLeft, new Color(0.96f, 0.98f, 1f, 1f));
        tutBodyTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        tutBodyTxt.lineSpacing = 1.15f;
        RectTransform tbRect = tutBodyTxt.GetComponent<RectTransform>();
        tbRect.anchorMin = new Vector2(0f, 1f);
        tbRect.anchorMax = new Vector2(0f, 1f);
        tbRect.pivot = new Vector2(0f, 1f);
        tbRect.anchoredPosition = new Vector2(34f, -112f);
        tbRect.sizeDelta = new Vector2(912f, 210f);"""

if old_tut_body in c:
    c = c.replace(old_tut_body, new_tut_body, 1)
    print("BuildMainGameScene.cs: Updated tutorial body font size to 26 and height to 210")

# Update tutorial step and title font sizes
c = c.replace(
    'Text tutStepTxt = CreateUIText("Text_TutorialStep", tutDialogBox.transform, font, "DIFFICULTY  •  MEDIUM MODE  (1/5)", 15, TextAnchor.UpperLeft, new Color(0.48f, 0.94f, 1f, 1f));',
    'Text tutStepTxt = CreateUIText("Text_TutorialStep", tutDialogBox.transform, font, "DIFFICULTY  •  MEDIUM MODE  (1/5)", 24, TextAnchor.UpperLeft, new Color(0.48f, 0.94f, 1f, 1f));'
)
c = c.replace(
    'Text tutTitleTxt = CreateUIText("Text_TutorialTitle", tutDialogBox.transform, font, "MEDIUM MODE: CLASSIC ARCADE!", 22, TextAnchor.UpperLeft, Color.white);',
    'Text tutTitleTxt = CreateUIText("Text_TutorialTitle", tutDialogBox.transform, font, "MEDIUM MODE: CLASSIC ARCADE!", 28, TextAnchor.UpperLeft, Color.white);'
)
c = c.replace(
    'Text tutPromptTxt = CreateUIText("Text_TapPrompt", tutDialogBox.transform, font, "[ TAP TO CONTINUE > ]", 17, TextAnchor.MiddleRight, new Color(1f, 0.90f, 0.45f, 1f));',
    'Text tutPromptTxt = CreateUIText("Text_TapPrompt", tutDialogBox.transform, font, "[ TAP TO CONTINUE > ]", 22, TextAnchor.MiddleRight, new Color(1f, 0.90f, 0.45f, 1f));'
)
c = c.replace(
    'Text speakerTxt = CreateUIText("Text_SpeakerName", namePlate.transform, font, "BLOOP THE TUTOR", 14, TextAnchor.MiddleCenter, Color.white);',
    'Text speakerTxt = CreateUIText("Text_SpeakerName", namePlate.transform, font, "BLOOP THE TUTOR", 18, TextAnchor.MiddleCenter, Color.white);'
)

# 5. Fix Cinematic Speech Bubble in Verification Screenshots
old_verif_splash = """            ui.pixarSplashCinematic.bloopImage.sprite = ui.pixarSplashCinematic.slimeTurnLookUser;
            ui.pixarSplashCinematic.foodImage.gameObject.SetActive(false);
            ui.pixarSplashCinematic.speechBubbleGroup.SetActive(true);
            ui.pixarSplashCinematic.speechBubbleText.text = "...Did you see what I just ate?!";
            ui.pixarSplashCinematic.speechBubbleRect.anchoredPosition = new Vector2(0f, 75f);"""

new_verif_splash = """            ui.pixarSplashCinematic.bloopImage.sprite = ui.pixarSplashCinematic.slimeTurnLookUser;
            ui.pixarSplashCinematic.foodImage.gameObject.SetActive(false);
            ui.pixarSplashCinematic.speechBubbleGroup.SetActive(true);
            ui.pixarSplashCinematic.speechBubbleText.text = "...Did you see what I just ate?!";
            ui.pixarSplashCinematic.speechBubbleRect.sizeDelta = new Vector2(560f, 96f);
            RectTransform sbtRect = ui.pixarSplashCinematic.speechBubbleText.GetComponent<RectTransform>();
            if (sbtRect != null) sbtRect.sizeDelta = new Vector2(520f, 70f);
            ui.pixarSplashCinematic.speechBubbleRect.anchoredPosition = new Vector2(0f, 75f);"""

if old_verif_splash in c:
    c = c.replace(old_verif_splash, new_verif_splash, 1)
    print("BuildMainGameScene.cs: Fixed verification splash speech bubble dimensions")

# 6. Reduce harsh outline distances on Splash speech bubble and loading bar to prevent pointy corners
c = c.replace(
    "bOut.effectDistance = new Vector2(3f, -3f);",
    "bOut.effectDistance = new Vector2(2f, -2f);"
)
c = c.replace(
    "btOutline.effectDistance = new Vector2(4f, -4f);",
    "btOutline.effectDistance = new Vector2(2f, -2f);"
)

# 7. Bump Splash Screen font sizes
c = c.replace(
    'Text subTitle = CreateUIText("Text_SplashSub", titleGroup.transform, font, "SLIME STUDIO PRESENTS", 18, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.40f, 1f));',
    'Text subTitle = CreateUIText("Text_SplashSub", titleGroup.transform, font, "SLIME STUDIO PRESENTS", 26, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.40f, 1f));'
)
c = c.replace(
    'Text mainTitle = CreateUIText("Text_SplashTitle", titleGroup.transform, font, "SLIME FOOD DROP", 40, TextAnchor.MiddleCenter, new Color(0.40f, 0.98f, 0.62f, 1f));',
    'Text mainTitle = CreateUIText("Text_SplashTitle", titleGroup.transform, font, "SLIME FOOD DROP", 52, TextAnchor.MiddleCenter, new Color(0.40f, 0.98f, 0.62f, 1f));'
)
c = c.replace(
    'Text descTxt = CreateUIText("Text_SplashTagline", titleGroup.transform, font, "CATCH THE TREATS • DODGE THE TOXINS!", 17, TextAnchor.MiddleCenter, new Color(0.85f, 0.92f, 1f, 0.95f));',
    'Text descTxt = CreateUIText("Text_SplashTagline", titleGroup.transform, font, "CATCH THE TREATS • DODGE THE TOXINS!", 24, TextAnchor.MiddleCenter, new Color(0.85f, 0.92f, 1f, 0.95f));'
)

# 8. Bump HUD font sizes
c = c.replace(
    'Text diffTxt = CreateUIText("Text_Mode", topBanner.transform, font, "MODE: MEDIUM", 16, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.35f, 1f));',
    'Text diffTxt = CreateUIText("Text_Mode", topBanner.transform, font, "MODE: MEDIUM", 24, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.35f, 1f));'
)
c = c.replace(
    'Text stageTxt = CreateUIText("Text_StageTheme", topBanner.transform, font, "STAGE: WHISPERING FOREST", 19, TextAnchor.MiddleCenter, new Color(0.60f, 0.92f, 1f, 1f));',
    'Text stageTxt = CreateUIText("Text_StageTheme", topBanner.transform, font, "STAGE: WHISPERING FOREST", 26, TextAnchor.MiddleCenter, new Color(0.60f, 0.92f, 1f, 1f));'
)
c = c.replace(
    'Text scoreTxt = CreateUIText("Text_Score", topBanner.transform, font, "SCORE: 0", 26, TextAnchor.MiddleCenter, Color.white);',
    'Text scoreTxt = CreateUIText("Text_Score", topBanner.transform, font, "SCORE: 0", 32, TextAnchor.MiddleCenter, Color.white);'
)
c = c.replace(
    'Text highTxt = CreateUIText("Text_HighScore", topBanner.transform, font, "BEST: 0", 22, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.28f, 1f));',
    'Text highTxt = CreateUIText("Text_HighScore", topBanner.transform, font, "BEST: 0", 26, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.28f, 1f));'
)
c = c.replace(
    'Text magTxt = CreateUIText("Text_MagnetTimer", magPill.transform, font, "MAGNET: 6.0s", 16, TextAnchor.MiddleCenter, Color.white);',
    'Text magTxt = CreateUIText("Text_MagnetTimer", magPill.transform, font, "MAGNET: 6.0s", 22, TextAnchor.MiddleCenter, Color.white);'
)
c = c.replace(
    'Text slowTxt = CreateUIText("Text_TimeSlowTimer", slowPill.transform, font, "SLOW-MO: 6.0s", 16, TextAnchor.MiddleCenter, Color.white);',
    'Text slowTxt = CreateUIText("Text_TimeSlowTimer", slowPill.transform, font, "SLOW-MO: 6.0s", 22, TextAnchor.MiddleCenter, Color.white);'
)

# 9. Bump Almanac font sizes
c = c.replace(
    'Text gbSub = CreateUIText("Text_GuidebookSub", gbCard.transform, font, "TAP ANY CARD BELOW TO INSPECT LORE & STRATEGY", 18, TextAnchor.MiddleCenter, new Color(0.95f, 0.88f, 0.40f, 1f));',
    'Text gbSub = CreateUIText("Text_GuidebookSub", gbCard.transform, font, "TAP ANY CARD BELOW TO INSPECT LORE & STRATEGY", 24, TextAnchor.MiddleCenter, new Color(0.95f, 0.88f, 0.40f, 1f));'
)
c = c.replace(
    'Text dTitle = CreateUIText("Text_DetailModalTitle", dpStrip.transform, font, "ITEM DETAILS", 18, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.38f, 1f));',
    'Text dTitle = CreateUIText("Text_DetailModalTitle", dpStrip.transform, font, "ITEM DETAILS", 26, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.38f, 1f));'
)

with open(path, "w", encoding="utf-8") as f:
    f.write(c)

print("BuildMainGameScene.cs successfully updated.")
