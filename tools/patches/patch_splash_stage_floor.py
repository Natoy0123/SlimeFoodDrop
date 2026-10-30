import os

path = r'C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs'
with open(path, 'rb') as f:
    data = f.read()

# 1. Load stage floor sprite
target1 = b'        Sprite contactShadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashCutscene/stage_contact_shadow.png");'
repl1 = b'''        Sprite contactShadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashCutscene/stage_contact_shadow.png");\r
        Sprite stageFloorSprite = LoadSpriteEnsured("Assets/Sprites/SplashCutscene/stage_floor_planks.png");'''

# 2. Add stage floor platform, place spotlight illuminating it, and high-contrast contact shadows
target2 = b'''        // Volumetric Theater Spotlight Cone with soft atmospheric falloff & floor illumination\r
        Image spotlightImg = CreateUISpriteIcon("Cinematic_Spotlight", stageObj.transform, spotlightSprite, new Vector2(0f, 50f), new Vector2(860f, 640f));\r
        spotlightImg.color = new Color(1f, 1f, 1f, 0.90f);\r
        cinematic.spotlightImage = spotlightImg;\r
\r
        // Contact Shadows (Enlarged)\r
        Image foodShadow = CreateUISpriteIcon("Shadow_Food", stageObj.transform, contactShadowSprite, new Vector2(-460f, -115f), new Vector2(120f, 36f));\r
        foodShadow.color = new Color(0f, 0f, 0f, 0.45f);\r
        cinematic.foodShadow = foodShadow;\r
\r
        Image bloopShadow = CreateUISpriteIcon("Shadow_Bloop", stageObj.transform, contactShadowSprite, new Vector2(-550f, -115f), new Vector2(260f, 64f));\r
        bloopShadow.color = new Color(0f, 0f, 0f, 0.55f);\r
        cinematic.bloopShadow = bloopShadow;'''

repl2 = b'''        // 1. Theater Stage Wooden Floor Platform (Solid ground for Bloop and Food to stand and run on!)\r
        Image stageFloorImg = CreateUISpriteIcon("Stage_Floor", stageObj.transform, stageFloorSprite, new Vector2(0f, -205f), new Vector2(920f, 180f));\r
        stageFloorImg.raycastTarget = false;\r
\r
        // 2. Volumetric Theater Spotlight Cone (Shining down onto the wooden stage floor)\r
        Image spotlightImg = CreateUISpriteIcon("Cinematic_Spotlight", stageObj.transform, spotlightSprite, new Vector2(0f, 65f), new Vector2(860f, 620f));\r
        spotlightImg.color = new Color(1f, 1f, 1f, 0.95f);\r
        cinematic.spotlightImage = spotlightImg;\r
\r
        // 3. Contact Shadows (Cast with rich dark contrast directly on top of the illuminated stage floor!)\r
        Image foodShadow = CreateUISpriteIcon("Shadow_Food", stageObj.transform, contactShadowSprite, new Vector2(-460f, -115f), new Vector2(140f, 44f));\r
        foodShadow.color = new Color(0.08f, 0.05f, 0.12f, 0.85f);\r
        cinematic.foodShadow = foodShadow;\r
\r
        Image bloopShadow = CreateUISpriteIcon("Shadow_Bloop", stageObj.transform, contactShadowSprite, new Vector2(-550f, -115f), new Vector2(280f, 74f));\r
        bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, 0.88f);\r
        cinematic.bloopShadow = bloopShadow;'''

# 3. Verification setup: ensure shadows are explicitly positioned in splash screenshot
target3 = b'''                ui.pixarSplashCinematic.bloopRect.anchoredPosition = new Vector2(0f, -80f);\r
                ui.pixarSplashCinematic.bloopImage.sprite = ui.pixarSplashCinematic.slimeTurnLookUser;'''

repl3 = b'''                ui.pixarSplashCinematic.bloopRect.anchoredPosition = new Vector2(0f, -80f);\r
                if (ui.pixarSplashCinematic.bloopShadow != null)\r
                {\r
                    ui.pixarSplashCinematic.bloopShadow.gameObject.SetActive(true);\r
                    ui.pixarSplashCinematic.bloopShadow.rectTransform.anchoredPosition = new Vector2(0f, -115f);\r
                }\r
                ui.pixarSplashCinematic.bloopImage.sprite = ui.pixarSplashCinematic.slimeTurnLookUser;'''

if target1 in data and target2 in data and target3 in data:
    data = data.replace(target1, repl1, 1)
    data = data.replace(target2, repl2, 1)
    data = data.replace(target3, repl3, 1)
    with open(path, 'wb') as f:
        f.write(data)
    print('BuildMainGameScene.cs stage floor and shadow successfully patched!')
else:
    print('Failed to find targets:', target1 in data, target2 in data, target3 in data)
