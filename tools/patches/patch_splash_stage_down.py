import os

path = r'C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs'
with open(path, 'r', encoding='utf-8') as f:
    text = f.read()

target = '''        GameObject stageObj = CreateUIElement("Cinematic_StageContainer", splashObj.transform);
        RectTransform stageRect = stageObj.GetComponent<RectTransform>();
        stageRect.anchorMin = new Vector2(0.5f, 0.5f);
        stageRect.anchorMax = new Vector2(0.5f, 0.5f);
        stageRect.pivot = new Vector2(0.5f, 0.5f);
        stageRect.anchoredPosition = new Vector2(0f, 15f);
        stageRect.sizeDelta = new Vector2(920f, 600f);
        cinematic.stageContainer = stageRect;

        // 1. Theater Stage Wooden Floor Platform (Solid ground for Bloop and Food to stand and run on!)
        Image stageFloorImg = CreateUISpriteIcon("Stage_Floor", stageObj.transform, stageFloorSprite, new Vector2(0f, -205f), new Vector2(920f, 180f));
        stageFloorImg.raycastTarget = false;

        // 2. Volumetric Theater Spotlight Cone (Shining down onto the wooden stage floor)
        Image spotlightImg = CreateUISpriteIcon("Cinematic_Spotlight", stageObj.transform, spotlightSprite, new Vector2(0f, 65f), new Vector2(860f, 620f));'''

repl = '''        GameObject stageObj = CreateUIElement("Cinematic_StageContainer", splashObj.transform);
        RectTransform stageRect = stageObj.GetComponent<RectTransform>();
        stageRect.anchorMin = new Vector2(0.5f, 0.5f);
        stageRect.anchorMax = new Vector2(0.5f, 0.5f);
        stageRect.pivot = new Vector2(0.5f, 0.5f);
        stageRect.anchoredPosition = new Vector2(0f, -240f);
        stageRect.sizeDelta = new Vector2(920f, 600f);
        cinematic.stageContainer = stageRect;

        // 1. Theater Stage Wooden Floor Platform (Solid ground for Bloop and Food to stand and run on!)
        Image stageFloorImg = CreateUISpriteIcon("Stage_Floor", stageObj.transform, stageFloorSprite, new Vector2(0f, -205f), new Vector2(920f, 180f));
        stageFloorImg.raycastTarget = false;

        // 2. Volumetric Theater Spotlight Cone (Shining down onto the wooden stage floor)
        Image spotlightImg = CreateUISpriteIcon("Cinematic_Spotlight", stageObj.transform, spotlightSprite, new Vector2(0f, 120f), new Vector2(880f, 780f));'''

text_nl = text.replace('\r\n', '\n')
target_nl = target.replace('\r\n', '\n')
repl_nl = repl.replace('\r\n', '\n')

if target_nl in text_nl:
    text_nl = text_nl.replace(target_nl, repl_nl, 1)
    with open(path, 'wb') as f:
        f.write(text_nl.replace('\n', '\r\n').encode('utf-8'))
    print('Successfully moved splash stage down to 70% level in BuildMainGameScene.cs!')
else:
    print('Failed to find target in BuildMainGameScene.cs')
