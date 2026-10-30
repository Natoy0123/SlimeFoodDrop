import os

path = r'C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. SlimePlayer mouth sprites assignment
target1 = '''        slimeCtrl.hurtFrames = LoadSpritesInFolder("Assets/Sprites/Slime/04-Hurt");
        slimeCtrl.deadFrames = LoadSpritesInFolder("Assets/Sprites/Slime/05-Dead");
        slimeCtrl.slimeTrailSprite = LoadSpriteEnsured("Assets/Sprites/Slime/slime_trail_droplet.png");'''

repl1 = '''        slimeCtrl.hurtFrames = LoadSpritesInFolder("Assets/Sprites/Slime/04-Hurt");
        slimeCtrl.deadFrames = LoadSpritesInFolder("Assets/Sprites/Slime/05-Dead");
        slimeCtrl.slimeTrailSprite = LoadSpriteEnsured("Assets/Sprites/Slime/slime_trail_droplet.png");
        slimeCtrl.mouthOpenSprite = LoadSpriteEnsured("Assets/Sprites/Slime/slime_mouth_open.png");
        slimeCtrl.mouthChewSprite = LoadSpriteEnsured("Assets/Sprites/Slime/slime_mouth_chew.png");
        slimeCtrl.mouthSmileSprite = LoadSpriteEnsured("Assets/Sprites/Slime/slime_mouth_smile.png");'''

# 2. Food Rush Clock Widget & Power-up bars
target2 = '''        // =========================================================================
        // 1B. POWER-UP DURATION BARS CONTAINER (Directly Below TopBar_HUD)
        // =========================================================================
        GameObject puContainer = CreateUIElement("PowerUp_Bars_Container", safeAreaObj.transform);
        RectTransform puRect = puContainer.GetComponent<RectTransform>();
        puRect.anchorMin = new Vector2(0f, 1f);
        puRect.anchorMax = new Vector2(1f, 1f);
        puRect.pivot = new Vector2(0.5f, 1f);
        puRect.anchoredPosition = new Vector2(0f, -176f);
        puRect.sizeDelta = new Vector2(-44f, 148f);
        uiMgr.powerUpBarsContainer = puContainer;'''

repl2 = '''        // Load Clock Dial and Needle Sprites for Food Rush HUD
        Sprite clockFaceSprite = LoadSpriteEnsured("Assets/Sprites/UI/ui_clock_face.png");
        Sprite clockHandSprite = LoadSpriteEnsured("Assets/Sprites/UI/ui_clock_hand.png");

        // =========================================================================
        // 1B. FOOD RUSH CLOCK WIDGET (Top-Left HUD below Hearts/Difficulty Badge)
        // =========================================================================
        GameObject foodRushClock = CreateUIElement("FoodRush_Clock_Widget", safeAreaObj.transform);
        RectTransform frcRect = foodRushClock.GetComponent<RectTransform>();
        frcRect.anchorMin = new Vector2(0f, 1f);
        frcRect.anchorMax = new Vector2(0f, 1f);
        frcRect.pivot = new Vector2(0f, 1f);
        frcRect.anchoredPosition = new Vector2(24f, -170f);
        frcRect.sizeDelta = new Vector2(188f, 62f);

        Image frcBg = foodRushClock.AddComponent<Image>();
        frcBg.color = new Color(0.12f, 0.09f, 0.18f, 0.94f);
        frcBg.raycastTarget = false;
        if (roundedFrameSprite != null)
        {
            frcBg.sprite = roundedFrameSprite;
            frcBg.type = Image.Type.Sliced;
        }
        Outline frcOutline = foodRushClock.AddComponent<Outline>();
        frcOutline.effectColor = new Color(1.0f, 0.82f, 0.28f, 0.95f);
        frcOutline.effectDistance = new Vector2(2f, -2f);

        // Circular Clock Face Dial (Left side of widget)
        Image clockFaceImg = CreateUISpriteIcon("Clock_Face", foodRushClock.transform, clockFaceSprite, new Vector2(34f, -31f), new Vector2(44f, 44f));
        clockFaceImg.raycastTarget = false;

        // Rotating Clock Hand Needle
        GameObject clockHandObj = CreateUIElement("Clock_Hand", clockFaceImg.transform);
        RectTransform handRect = clockHandObj.GetComponent<RectTransform>();
        handRect.anchorMin = new Vector2(0.5f, 0.5f);
        handRect.anchorMax = new Vector2(0.5f, 0.5f);
        handRect.pivot = new Vector2(0.5f, 0.5f);
        handRect.anchoredPosition = Vector2.zero;
        handRect.sizeDelta = new Vector2(44f, 44f);
        Image handImg = clockHandObj.AddComponent<Image>();
        handImg.sprite = clockHandSprite;
        handImg.preserveAspect = true;
        handImg.raycastTarget = false;

        // Header: "FOOD RUSH"
        Text frBadge = CreateUIText("Text_RushBadge", foodRushClock.transform, defaultBodyFont, "FOOD RUSH", 14, TextAnchor.MiddleLeft, new Color(1.0f, 0.85f, 0.32f, 1f));
        RectTransform frbRect = frBadge.GetComponent<RectTransform>();
        frbRect.anchorMin = new Vector2(0f, 1f);
        frbRect.anchorMax = new Vector2(0f, 1f);
        frbRect.pivot = new Vector2(0f, 1f);
        frbRect.anchoredPosition = new Vector2(66f, -12f);
        frbRect.sizeDelta = new Vector2(114f, 18f);

        // Digital timer countdown: "15.0s"
        Text frTimer = CreateUIText("Text_RushTimer", foodRushClock.transform, defaultBodyFont, "15.0s", 24, TextAnchor.MiddleLeft, new Color(1f, 0.98f, 0.85f, 1f));
        RectTransform frtRect = frTimer.GetComponent<RectTransform>();
        frtRect.anchorMin = new Vector2(0f, 1f);
        frtRect.anchorMax = new Vector2(0f, 1f);
        frtRect.pivot = new Vector2(0f, 1f);
        frtRect.anchoredPosition = new Vector2(66f, -32f);
        frtRect.sizeDelta = new Vector2(114f, 24f);

        foodRushClock.SetActive(false);
        uiMgr.foodRushClockWidget = foodRushClock;
        uiMgr.foodRushClockHand = handRect;
        uiMgr.textFoodRushClockTimer = frTimer;

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
        uiMgr.powerUpBarsContainer = puContainer;'''

# 3. CreatePowerUpDurationBarRow sizing
target3 = '''        rRect.sizeDelta = new Vector2(760f, 44f);

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
        Image iconImg = CreateUISpriteIcon("Icon", rowObj.transform, iconSprite, new Vector2(-352f, 0f), new Vector2(34f, 34f));'''

repl3 = '''        rRect.sizeDelta = new Vector2(620f, 44f);

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
        Image iconImg = CreateUISpriteIcon("Icon", rowObj.transform, iconSprite, new Vector2(-282f, 0f), new Vector2(34f, 34f));'''

# 4. Splash Screen Title redesign (using retroGameTitleLogo) & Spotlight color
target4 = '''        // Top Screen Header Strip: Slime Studio Presents + Grand Title + Subtitle (Same level as Home Title Screen)
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
        spotlightImg.color = new Color(1f, 0.96f, 0.82f, 0.35f);'''

repl4 = '''        // Top Screen Header Strip: Slime Studio Presents + Grand Title + Subtitle (Same level as Home Title Screen)
        GameObject splashTopHeader = CreateUIElement("Splash_TopHeader", splashObj.transform);
        RectTransform sthRect = splashTopHeader.GetComponent<RectTransform>();
        sthRect.anchorMin = new Vector2(0.5f, 0.5f);
        sthRect.anchorMax = new Vector2(0.5f, 0.5f);
        sthRect.pivot = new Vector2(0.5f, 0.5f);
        sthRect.anchoredPosition = new Vector2(0f, 560f);
        sthRect.sizeDelta = new Vector2(920f, 260f);

        Text studioTxt = CreateUIText("Text_StudioPresents", splashTopHeader.transform, font, "SLIME STUDIO PRESENTS", 18, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.38f, 1f));
        studioTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 96f);
        studioTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(760f, 32f);

        // 16-Bit Retro Arcade Title Logo Graphic (Matching Home Screen design!)
        GameObject titleGraphicObj = CreateUIElement("Image_SplashTitleLogo", splashTopHeader.transform);
        RectTransform tgRect = titleGraphicObj.GetComponent<RectTransform>();
        tgRect.anchorMin = new Vector2(0.5f, 0.5f);
        tgRect.anchorMax = new Vector2(0.5f, 0.5f);
        tgRect.pivot = new Vector2(0.5f, 0.5f);
        tgRect.anchoredPosition = new Vector2(0f, 10f);
        tgRect.sizeDelta = new Vector2(580f, 155f);
        Image tgImg = titleGraphicObj.AddComponent<Image>();
        tgImg.sprite = retroGameTitleLogo;
        tgImg.preserveAspect = true;
        tgImg.raycastTarget = false;

        Text splashSub = CreateUIText("Text_SplashSubtitle", splashTopHeader.transform, defaultBodyFont, "CATCH THE TREATS • DODGE THE TOXINS!", 26, TextAnchor.MiddleCenter, new Color(0.82f, 0.94f, 1f, 0.95f));
        splashSub.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -80f);
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

        // Volumetric Theater Spotlight Cone with soft atmospheric falloff & floor illumination
        Image spotlightImg = CreateUISpriteIcon("Cinematic_Spotlight", stageObj.transform, spotlightSprite, new Vector2(0f, 50f), new Vector2(860f, 640f));
        spotlightImg.color = new Color(1f, 1f, 1f, 0.90f);'''

content_nl = content.replace('\r\n', '\n')
for i, (t, r) in enumerate([(target1, repl1), (target2, repl2), (target3, repl3), (target4, repl4)]):
    if t not in content_nl:
        print(f'FAILED TO FIND TARGET {i+1}:', t[:40])
    else:
        content_nl = content_nl.replace(t, r, 1)

content_out = content_nl.replace('\n', '\r\n')
with open(path, 'w', encoding='utf-8') as f:
    f.write(content_out)

print('BuildMainGameScene.cs successfully patched!')
