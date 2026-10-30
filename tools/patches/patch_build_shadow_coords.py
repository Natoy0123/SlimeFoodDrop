import os

path = r'C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs'
with open(path, 'r', encoding='utf-8') as f:
    text = f.read()

text_nl = text.replace('\r\n', '\n')

# 1. Update initial shadow and floor creation
target_floor = """        // 1. Theater Stage Wooden Floor Platform (Solid ground for Bloop and Food to stand and run on!)
        Image stageFloorImg = CreateUISpriteIcon("Stage_Floor", stageObj.transform, stageFloorSprite, new Vector2(0f, -205f), new Vector2(920f, 180f));
        stageFloorImg.raycastTarget = false;

        // 2. Volumetric Theater Spotlight Cone (Shining down onto the wooden stage floor)
        Image spotlightImg = CreateUISpriteIcon("Cinematic_Spotlight", stageObj.transform, spotlightSprite, new Vector2(0f, 120f), new Vector2(880f, 780f));
        spotlightImg.color = new Color(1f, 1f, 1f, 0.95f);
        cinematic.spotlightImage = spotlightImg;

        // 3. Contact Shadows (Cast with rich dark contrast directly on top of the illuminated stage floor!)
        Image foodShadow = CreateUISpriteIcon("Shadow_Food", stageObj.transform, contactShadowSprite, new Vector2(-460f, -115f), new Vector2(140f, 44f));
        foodShadow.color = new Color(0.08f, 0.05f, 0.12f, 0.85f);
        foodShadow.gameObject.SetActive(false);
        cinematic.foodShadow = foodShadow;

        Image bloopShadow = CreateUISpriteIcon("Shadow_Bloop", stageObj.transform, contactShadowSprite, new Vector2(-550f, -115f), new Vector2(280f, 74f));"""

repl_floor = """        // 1. Theater Stage Wooden Floor Platform (Solid ground for Bloop and Food to stand and run on!)
        Image stageFloorImg = CreateUISpriteIcon("Stage_Floor", stageObj.transform, stageFloorSprite, new Vector2(0f, -220f), new Vector2(960f, 200f));
        stageFloorImg.raycastTarget = false;

        // 2. Volumetric Theater Spotlight Cone (Shining down onto the wooden stage floor)
        Image spotlightImg = CreateUISpriteIcon("Cinematic_Spotlight", stageObj.transform, spotlightSprite, new Vector2(0f, 120f), new Vector2(880f, 780f));
        spotlightImg.color = new Color(1f, 1f, 1f, 0.95f);
        cinematic.spotlightImage = spotlightImg;

        // 3. Contact Shadows (Anchored to wooden plank surface at y = -180f with dynamic squash/stretch scale tracking)
        Image foodShadow = CreateUISpriteIcon("Shadow_Food", stageObj.transform, contactShadowSprite, new Vector2(-460f, -180f), new Vector2(140f, 44f));
        foodShadow.color = new Color(0.08f, 0.05f, 0.12f, 0.85f);
        foodShadow.gameObject.SetActive(false);
        cinematic.foodShadow = foodShadow;

        Image bloopShadow = CreateUISpriteIcon("Shadow_Bloop", stageObj.transform, contactShadowSprite, new Vector2(-550f, -180f), new Vector2(280f, 74f));"""

if target_floor in text_nl:
    text_nl = text_nl.replace(target_floor, repl_floor, 1)
    print("Patched floor and shadow creation in BuildMainGameScene.cs")
else:
    print("FAILED to find floor creation block")

# 2. Update screenshot preview setups
target_ss1a = """                ui.pixarSplashCinematic.bloopRect.anchoredPosition = new Vector2(0f, -80f);
                if (ui.pixarSplashCinematic.bloopShadow != null)
                {
                    ui.pixarSplashCinematic.bloopShadow.gameObject.SetActive(true);
                    ui.pixarSplashCinematic.bloopShadow.rectTransform.anchoredPosition = new Vector2(0f, -115f);
                }"""

repl_ss1a = """                ui.pixarSplashCinematic.bloopRect.anchoredPosition = new Vector2(0f, -80f);
                if (ui.pixarSplashCinematic.bloopShadow != null)
                {
                    ui.pixarSplashCinematic.bloopShadow.gameObject.SetActive(true);
                    ui.pixarSplashCinematic.bloopShadow.rectTransform.anchoredPosition = new Vector2(0f, -180f);
                    ui.pixarSplashCinematic.bloopShadow.rectTransform.localScale = Vector3.one;
                }"""

if target_ss1a in text_nl:
    text_nl = text_nl.replace(target_ss1a, repl_ss1a, 1)
    print("Patched Screenshot 1A bloopShadow in BuildMainGameScene.cs")
else:
    print("FAILED to find Screenshot 1A block")

target_ss1b = """        // Screenshot 1B: Pixar Splash Cinematic (Bloop spitting the poison projectile!)
        if (ui.pixarSplashCinematic != null)
        {
            ui.pixarSplashCinematic.bloopImage.sprite = ui.pixarSplashCinematic.slimeSpitLaunch;
            ui.pixarSplashCinematic.bloopRect.localScale = new Vector3(0.70f, 1.48f, 1f);"""

repl_ss1b = """        // Screenshot 1B: Pixar Splash Cinematic (Bloop spitting the poison projectile!)
        if (ui.pixarSplashCinematic != null)
        {
            ui.pixarSplashCinematic.bloopImage.sprite = ui.pixarSplashCinematic.slimeSpitLaunch;
            ui.pixarSplashCinematic.bloopRect.localScale = new Vector3(0.70f, 1.48f, 1f);
            if (ui.pixarSplashCinematic.bloopShadow != null)
            {
                ui.pixarSplashCinematic.bloopShadow.rectTransform.anchoredPosition = new Vector2(0f, -180f);
                ui.pixarSplashCinematic.bloopShadow.rectTransform.localScale = new Vector3(0.70f, 1.22f, 1f);
            }"""

if target_ss1b in text_nl:
    text_nl = text_nl.replace(target_ss1b, repl_ss1b, 1)
    print("Patched Screenshot 1B bloopShadow in BuildMainGameScene.cs")
else:
    print("FAILED to find Screenshot 1B block")

target_ss1c = """            if (ui.pixarSplashCinematic.foodShadow != null)
            {
                ui.pixarSplashCinematic.foodShadow.gameObject.SetActive(true);
                ui.pixarSplashCinematic.foodShadow.rectTransform.anchoredPosition = new Vector2(40f, -115f);
            }"""

repl_ss1c = """            if (ui.pixarSplashCinematic.foodShadow != null)
            {
                ui.pixarSplashCinematic.foodShadow.gameObject.SetActive(true);
                ui.pixarSplashCinematic.foodShadow.rectTransform.anchoredPosition = new Vector2(40f, -180f);
                ui.pixarSplashCinematic.foodShadow.rectTransform.localScale = new Vector3(0.5f, 0.5f, 1f);
            }
            if (ui.pixarSplashCinematic.bloopShadow != null)
            {
                ui.pixarSplashCinematic.bloopShadow.rectTransform.anchoredPosition = new Vector2(40f, -180f);
                ui.pixarSplashCinematic.bloopShadow.rectTransform.localScale = new Vector3(1.45f, 0.69f, 1f);
            }"""

if target_ss1c in text_nl:
    text_nl = text_nl.replace(target_ss1c, repl_ss1c, 1)
    print("Patched Screenshot 1C bloopShadow in BuildMainGameScene.cs")
else:
    print("FAILED to find Screenshot 1C block")

with open(path, 'wb') as f:
    f.write(text_nl.replace('\n', '\r\n').encode('utf-8'))
