import os

path = r'C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs'
with open(path, 'rb') as f:
    data = f.read()

# 1. SlimePlayer mouth sprites assignment
target1 = b'''        slimeCtrl.hurtFrames = LoadSpritesInFolder("Assets/Sprites/Slime/04-Hurt");\r
        slimeCtrl.deadFrames = LoadSpritesInFolder("Assets/Sprites/Slime/05-Dead");\r
        slimeCtrl.slimeTrailSprite = LoadSpriteEnsured("Assets/Sprites/Slime/slime_trail_droplet.png");'''

repl1 = b'''        slimeCtrl.hurtFrames = LoadSpritesInFolder("Assets/Sprites/Slime/04-Hurt");\r
        slimeCtrl.deadFrames = LoadSpritesInFolder("Assets/Sprites/Slime/05-Dead");\r
        slimeCtrl.slimeTrailSprite = LoadSpriteEnsured("Assets/Sprites/Slime/slime_trail_droplet.png");\r
        slimeCtrl.mouthOpenSprite = LoadSpriteEnsured("Assets/Sprites/Slime/slime_mouth_open.png");\r
        slimeCtrl.mouthChewSprite = LoadSpriteEnsured("Assets/Sprites/Slime/slime_mouth_chew.png");\r
        slimeCtrl.mouthSmileSprite = LoadSpriteEnsured("Assets/Sprites/Slime/slime_mouth_smile.png");'''

# 2. Food Rush Clock Widget & Power-up bars
target2 = b'''        // =========================================================================\r
        // 1B. POWER-UP DURATION BARS CONTAINER (Directly Below TopBar_HUD)\r
        // =========================================================================\r
        GameObject puContainer = CreateUIElement("PowerUp_Bars_Container", safeAreaObj.transform);\r
        RectTransform puRect = puContainer.GetComponent<RectTransform>();\r
        puRect.anchorMin = new Vector2(0f, 1f);\r
        puRect.anchorMax = new Vector2(1f, 1f);\r
        puRect.pivot = new Vector2(0.5f, 1f);\r
        puRect.anchoredPosition = new Vector2(0f, -176f);\r
        puRect.sizeDelta = new Vector2(-44f, 148f);\r
        uiMgr.powerUpBarsContainer = puContainer;'''

repl2 = b'''        // Load Clock Dial and Needle Sprites for Food Rush HUD\r
        Sprite clockFaceSprite = LoadSpriteEnsured("Assets/Sprites/UI/ui_clock_face.png");\r
        Sprite clockHandSprite = LoadSpriteEnsured("Assets/Sprites/UI/ui_clock_hand.png");\r
\r
        // =========================================================================\r
        // 1B. FOOD RUSH CLOCK WIDGET (Top-Left HUD below Hearts/Difficulty Badge)\r
        // =========================================================================\r
        GameObject foodRushClock = CreateUIElement("FoodRush_Clock_Widget", safeAreaObj.transform);\r
        RectTransform frcRect = foodRushClock.GetComponent<RectTransform>();\r
        frcRect.anchorMin = new Vector2(0f, 1f);\r
        frcRect.anchorMax = new Vector2(0f, 1f);\r
        frcRect.pivot = new Vector2(0f, 1f);\r
        frcRect.anchoredPosition = new Vector2(24f, -170f);\r
        frcRect.sizeDelta = new Vector2(188f, 62f);\r
\r
        Image frcBg = foodRushClock.AddComponent<Image>();\r
        frcBg.color = new Color(0.12f, 0.09f, 0.18f, 0.94f);\r
        frcBg.raycastTarget = false;\r
        if (roundedFrameSprite != null)\r
        {\r
            frcBg.sprite = roundedFrameSprite;\r
            frcBg.type = Image.Type.Sliced;\r
        }\r
        Outline frcOutline = foodRushClock.AddComponent<Outline>();\r
        frcOutline.effectColor = new Color(1.0f, 0.82f, 0.28f, 0.95f);\r
        frcOutline.effectDistance = new Vector2(2f, -2f);\r
\r
        // Circular Clock Face Dial (Left side of widget)\r
        Image clockFaceImg = CreateUISpriteIcon("Clock_Face", foodRushClock.transform, clockFaceSprite, new Vector2(34f, -31f), new Vector2(44f, 44f));\r
        clockFaceImg.raycastTarget = false;\r
\r
        // Rotating Clock Hand Needle\r
        GameObject clockHandObj = CreateUIElement("Clock_Hand", clockFaceImg.transform);\r
        RectTransform handRect = clockHandObj.GetComponent<RectTransform>();\r
        handRect.anchorMin = new Vector2(0.5f, 0.5f);\r
        handRect.anchorMax = new Vector2(0.5f, 0.5f);\r
        handRect.pivot = new Vector2(0.5f, 0.5f);\r
        handRect.anchoredPosition = Vector2.zero;\r
        handRect.sizeDelta = new Vector2(44f, 44f);\r
        Image handImg = clockHandObj.AddComponent<Image>();\r
        handImg.sprite = clockHandSprite;\r
        handImg.preserveAspect = true;\r
        handImg.raycastTarget = false;\r
\r
        // Header: "FOOD RUSH"\r
        Text frBadge = CreateUIText("Text_RushBadge", foodRushClock.transform, defaultBodyFont, "FOOD RUSH", 14, TextAnchor.MiddleLeft, new Color(1.0f, 0.85f, 0.32f, 1f));\r
        RectTransform frbRect = frBadge.GetComponent<RectTransform>();\r
        frbRect.anchorMin = new Vector2(0f, 1f);\r
        frbRect.anchorMax = new Vector2(0f, 1f);\r
        frbRect.pivot = new Vector2(0f, 1f);\r
        frbRect.anchoredPosition = new Vector2(66f, -12f);\r
        frbRect.sizeDelta = new Vector2(114f, 18f);\r
\r
        // Digital timer countdown: "15.0s"\r
        Text frTimer = CreateUIText("Text_RushTimer", foodRushClock.transform, defaultBodyFont, "15.0s", 24, TextAnchor.MiddleLeft, new Color(1f, 0.98f, 0.85f, 1f));\r
        RectTransform frtRect = frTimer.GetComponent<RectTransform>();\r
        frtRect.anchorMin = new Vector2(0f, 1f);\r
        frtRect.anchorMax = new Vector2(0f, 1f);\r
        frtRect.pivot = new Vector2(0f, 1f);\r
        frtRect.anchoredPosition = new Vector2(66f, -32f);\r
        frtRect.sizeDelta = new Vector2(114f, 24f);\r
\r
        foodRushClock.SetActive(false);\r
        uiMgr.foodRushClockWidget = foodRushClock;\r
        uiMgr.foodRushClockHand = handRect;\r
        uiMgr.textFoodRushClockTimer = frTimer;\r
\r
        // =========================================================================\r
        // 1B. POWER-UP DURATION BARS CONTAINER (Directly Below TopBar_HUD)\r
        // =========================================================================\r
        GameObject puContainer = CreateUIElement("PowerUp_Bars_Container", safeAreaObj.transform);\r
        RectTransform puRect = puContainer.GetComponent<RectTransform>();\r
        puRect.anchorMin = new Vector2(0f, 1f);\r
        puRect.anchorMax = new Vector2(1f, 1f);\r
        puRect.pivot = new Vector2(0.5f, 1f);\r
        puRect.anchoredPosition = new Vector2(0f, -176f);\r
        puRect.sizeDelta = new Vector2(-44f, 148f);\r
        uiMgr.powerUpBarsContainer = puContainer;'''

# 3. CreatePowerUpDurationBarRow sizing
target3 = b'''        rRect.sizeDelta = new Vector2(760f, 44f);\r
\r
        Image rowBg = rowObj.AddComponent<Image>();\r
        rowBg.color = new Color(0.10f, 0.08f, 0.14f, 0.86f);\r
        rowBg.raycastTarget = false;\r
        if (roundedFrameSprite != null)\r
        {\r
            rowBg.sprite = roundedFrameSprite;\r
            rowBg.type = Image.Type.Sliced;\r
        }\r
        Outline rowOut = rowObj.AddComponent<Outline>();\r
        rowOut.effectColor = new Color(0.04f, 0.03f, 0.06f, 0.95f);\r
        rowOut.effectDistance = new Vector2(3f, -3f);\r
\r
        // Icon on Left\r
        Image iconImg = CreateUISpriteIcon("Icon", rowObj.transform, iconSprite, new Vector2(-352f, 0f), new Vector2(34f, 34f));'''

repl3 = b'''        rRect.sizeDelta = new Vector2(620f, 44f);\r
\r
        Image rowBg = rowObj.AddComponent<Image>();\r
        rowBg.color = new Color(0.10f, 0.08f, 0.14f, 0.86f);\r
        rowBg.raycastTarget = false;\r
        if (roundedFrameSprite != null)\r
        {\r
            rowBg.sprite = roundedFrameSprite;\r
            rowBg.type = Image.Type.Sliced;\r
        }\r
        Outline rowOut = rowObj.AddComponent<Outline>();\r
        rowOut.effectColor = new Color(0.04f, 0.03f, 0.06f, 0.95f);\r
        rowOut.effectDistance = new Vector2(3f, -3f);\r
\r
        // Icon on Left\r
        Image iconImg = CreateUISpriteIcon("Icon", rowObj.transform, iconSprite, new Vector2(-282f, 0f), new Vector2(34f, 34f));'''

# 4. Splash Screen Title redesign (using retroGameTitleLogo) & Spotlight color
target4 = b'''        // Top Screen Header Strip: Slime Studio Presents + Grand Title + Subtitle (Same level as Home Title Screen)\r
        GameObject splashTopHeader = CreateUIElement("Splash_TopHeader", splashObj.transform);\r
        RectTransform sthRect = splashTopHeader.GetComponent<RectTransform>();\r
        sthRect.anchorMin = new Vector2(0.5f, 0.5f);\r
        sthRect.anchorMax = new Vector2(0.5f, 0.5f);\r
        sthRect.pivot = new Vector2(0.5f, 0.5f);\r
        sthRect.anchoredPosition = new Vector2(0f, 560f);\r
        sthRect.sizeDelta = new Vector2(920f, 220f);\r
\r
        Text studioTxt = CreateUIText("Text_StudioPresents", splashTopHeader.transform, font, "SLIME STUDIO PRESENTS", 18, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.38f, 1f));\r
        studioTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 65f);\r
        studioTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(760f, 38f);\r
\r
        Text splashTitle = CreateUIText("Text_SplashTitle", splashTopHeader.transform, font, "SLIME FOOD DROP", 44, TextAnchor.MiddleCenter, new Color(0.52f, 0.98f, 0.65f, 1f));\r
        splashTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);\r
        splashTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(880f, 80f);\r
\r
        Text splashSub = CreateUIText("Text_SplashSubtitle", splashTopHeader.transform, defaultBodyFont, "CATCH THE TREATS \xe2\x80\xa2 DODGE THE TOXINS!", 28, TextAnchor.MiddleCenter, new Color(0.82f, 0.94f, 1f, 0.95f));\r
        splashSub.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -55f);\r
        splashSub.GetComponent<RectTransform>().sizeDelta = new Vector2(820f, 36f);\r
\r
        // Stage Area Container (Centered vertically and horizontally on the screen device!)\r
        GameObject stageObj = CreateUIElement("Cinematic_StageContainer", splashObj.transform);\r
        RectTransform stageRect = stageObj.GetComponent<RectTransform>();\r
        stageRect.anchorMin = new Vector2(0.5f, 0.5f);\r
        stageRect.anchorMax = new Vector2(0.5f, 0.5f);\r
        stageRect.pivot = new Vector2(0.5f, 0.5f);\r
        stageRect.anchoredPosition = new Vector2(0f, 15f);\r
        stageRect.sizeDelta = new Vector2(920f, 600f);\r
        cinematic.stageContainer = stageRect;\r
\r
        // Spotlight Cone (Enlarged)\r
        Image spotlightImg = CreateUISpriteIcon("Cinematic_Spotlight", stageObj.transform, spotlightSprite, new Vector2(0f, 50f), new Vector2(860f, 640f));\r
        spotlightImg.color = new Color(1f, 0.96f, 0.82f, 0.35f);'''

repl4 = b'''        // Top Screen Header Strip: Slime Studio Presents + Grand Title + Subtitle (Same level as Home Title Screen)\r
        GameObject splashTopHeader = CreateUIElement("Splash_TopHeader", splashObj.transform);\r
        RectTransform sthRect = splashTopHeader.GetComponent<RectTransform>();\r
        sthRect.anchorMin = new Vector2(0.5f, 0.5f);\r
        sthRect.anchorMax = new Vector2(0.5f, 0.5f);\r
        sthRect.pivot = new Vector2(0.5f, 0.5f);\r
        sthRect.anchoredPosition = new Vector2(0f, 560f);\r
        sthRect.sizeDelta = new Vector2(920f, 260f);\r
\r
        Text studioTxt = CreateUIText("Text_StudioPresents", splashTopHeader.transform, font, "SLIME STUDIO PRESENTS", 18, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.38f, 1f));\r
        studioTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 96f);\r
        studioTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(760f, 32f);\r
\r
        // 16-Bit Retro Arcade Title Logo Graphic (Matching Home Screen design!)\r
        GameObject titleGraphicObj = CreateUIElement("Image_SplashTitleLogo", splashTopHeader.transform);\r
        RectTransform tgRect = titleGraphicObj.GetComponent<RectTransform>();\r
        tgRect.anchorMin = new Vector2(0.5f, 0.5f);\r
        tgRect.anchorMax = new Vector2(0.5f, 0.5f);\r
        tgRect.pivot = new Vector2(0.5f, 0.5f);\r
        tgRect.anchoredPosition = new Vector2(0f, 10f);\r
        tgRect.sizeDelta = new Vector2(580f, 155f);\r
        Image tgImg = titleGraphicObj.AddComponent<Image>();\r
        tgImg.sprite = retroGameTitleLogo;\r
        tgImg.preserveAspect = true;\r
        tgImg.raycastTarget = false;\r
\r
        Text splashSub = CreateUIText("Text_SplashSubtitle", splashTopHeader.transform, defaultBodyFont, "CATCH THE TREATS \xe2\x80\xa2 DODGE THE TOXINS!", 26, TextAnchor.MiddleCenter, new Color(0.82f, 0.94f, 1f, 0.95f));\r
        splashSub.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -80f);\r
        splashSub.GetComponent<RectTransform>().sizeDelta = new Vector2(820f, 36f);\r
\r
        // Stage Area Container (Centered vertically and horizontally on the screen device!)\r
        GameObject stageObj = CreateUIElement("Cinematic_StageContainer", splashObj.transform);\r
        RectTransform stageRect = stageObj.GetComponent<RectTransform>();\r
        stageRect.anchorMin = new Vector2(0.5f, 0.5f);\r
        stageRect.anchorMax = new Vector2(0.5f, 0.5f);\r
        stageRect.pivot = new Vector2(0.5f, 0.5f);\r
        stageRect.anchoredPosition = new Vector2(0f, 15f);\r
        stageRect.sizeDelta = new Vector2(920f, 600f);\r
        cinematic.stageContainer = stageRect;\r
\r
        // Volumetric Theater Spotlight Cone with soft atmospheric falloff & floor illumination\r
        Image spotlightImg = CreateUISpriteIcon("Cinematic_Spotlight", stageObj.transform, spotlightSprite, new Vector2(0f, 50f), new Vector2(860f, 640f));\r
        spotlightImg.color = new Color(1f, 1f, 1f, 0.90f);'''

# 5. Verification screenshot Food Rush HUD activation
target5 = b'''        slime.gameObject.SetActive(true);\r
        slime.transform.position = new Vector3(0.35f, ResponsiveCamera.BottomBound + slime.bottomOffset, 0f);\r
        slime.TriggerEatEffect();\r
        slime.UpdatePowerUpAuraVisuals();'''

repl5 = b'''        slime.gameObject.SetActive(true);\r
        slime.transform.position = new Vector3(0.35f, ResponsiveCamera.BottomBound + slime.bottomOffset, 0f);\r
        slime.TriggerEatEffect();\r
        slime.UpdatePowerUpAuraVisuals();\r
        ui.ShowFoodRushBanner(15.0f);\r
        ui.UpdateFoodRushHUD(14.8f, 15.0f);'''

target6 = b'''        // Clean preview items\r
        foreach (var p in previewItems)'''

repl6 = b'''        ui.HideFoodRushBanner();\r
        // Clean preview items\r
        foreach (var p in previewItems)'''

for i, (t, r) in enumerate([(target1, repl1), (target2, repl2), (target3, repl3), (target4, repl4), (target5, repl5), (target6, repl6)]):
    if t not in data:
        print(f'FAILED TO FIND BINARY TARGET {i+1}')
    else:
        data = data.replace(t, r, 1)

with open(path, 'wb') as f:
    f.write(data)

print('BuildMainGameScene.cs successfully patched via binary!')
