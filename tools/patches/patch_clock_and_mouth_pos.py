import os

# 1. Update SlimeController.cs mouthBaseOffset to center between eyes
path_slime = r'C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\SlimeController.cs'
with open(path_slime, 'rb') as f:
    b_slime = f.read()

target_slime = b'private readonly Vector3 mouthBaseOffset = new Vector3(0.04f, -0.07f, 0f);'
repl_slime = b'private readonly Vector3 mouthBaseOffset = new Vector3(-0.01f, -0.075f, 0f);'

if target_slime in b_slime:
    b_slime = b_slime.replace(target_slime, repl_slime, 1)
    with open(path_slime, 'wb') as f:
        f.write(b_slime)
    print('Updated mouthBaseOffset in SlimeController.cs')
else:
    print('Target mouthBaseOffset not found in SlimeController.cs')

# 2. Update BuildMainGameScene.cs FoodRush_Clock_Widget layout
path_scene = r'C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs'
with open(path_scene, 'rb') as f:
    b_scene = f.read()

target_scene = b'''        // Circular Clock Face Dial (Left side of widget)\r
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
        frtRect.sizeDelta = new Vector2(114f, 24f);'''

repl_scene = b'''        // Circular Clock Face Dial (Left side of widget: center is 0, so -58f sits cleanly on left)\r
        Image clockFaceImg = CreateUISpriteIcon("Clock_Face", foodRushClock.transform, clockFaceSprite, new Vector2(-58f, 0f), new Vector2(46f, 46f));\r
        clockFaceImg.raycastTarget = false;\r
\r
        // Rotating Clock Hand Needle\r
        GameObject clockHandObj = CreateUIElement("Clock_Hand", clockFaceImg.transform);\r
        RectTransform handRect = clockHandObj.GetComponent<RectTransform>();\r
        handRect.anchorMin = new Vector2(0.5f, 0.5f);\r
        handRect.anchorMax = new Vector2(0.5f, 0.5f);\r
        handRect.pivot = new Vector2(0.5f, 0.5f);\r
        handRect.anchoredPosition = Vector2.zero;\r
        handRect.sizeDelta = new Vector2(46f, 46f);\r
        Image handImg = clockHandObj.AddComponent<Image>();\r
        handImg.sprite = clockHandSprite;\r
        handImg.preserveAspect = true;\r
        handImg.raycastTarget = false;\r
\r
        // Header: "FOOD RUSH" (Right side of widget)\r
        Text frBadge = CreateUIText("Text_RushBadge", foodRushClock.transform, defaultBodyFont, "FOOD RUSH", 14, TextAnchor.MiddleLeft, new Color(1.0f, 0.85f, 0.32f, 1f));\r
        RectTransform frbRect = frBadge.GetComponent<RectTransform>();\r
        frbRect.anchorMin = new Vector2(0.5f, 0.5f);\r
        frbRect.anchorMax = new Vector2(0.5f, 0.5f);\r
        frbRect.pivot = new Vector2(0.5f, 0.5f);\r
        frbRect.anchoredPosition = new Vector2(26f, 11f);\r
        frbRect.sizeDelta = new Vector2(108f, 20f);\r
\r
        // Digital timer countdown: "15.0s" (Right side below header)\r
        Text frTimer = CreateUIText("Text_RushTimer", foodRushClock.transform, defaultBodyFont, "15.0s", 24, TextAnchor.MiddleLeft, new Color(1f, 0.98f, 0.85f, 1f));\r
        RectTransform frtRect = frTimer.GetComponent<RectTransform>();\r
        frtRect.anchorMin = new Vector2(0.5f, 0.5f);\r
        frtRect.anchorMax = new Vector2(0.5f, 0.5f);\r
        frtRect.pivot = new Vector2(0.5f, 0.5f);\r
        frtRect.anchoredPosition = new Vector2(26f, -12f);\r
        frtRect.sizeDelta = new Vector2(108f, 24f);'''

if target_scene in b_scene:
    b_scene = b_scene.replace(target_scene, repl_scene, 1)
    with open(path_scene, 'wb') as f:
        f.write(b_scene)
    print('Updated FoodRush_Clock_Widget in BuildMainGameScene.cs')
else:
    print('Target not found in BuildMainGameScene.cs')
