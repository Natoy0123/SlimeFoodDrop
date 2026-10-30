path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs"
with open(path, "r", encoding="utf-8") as f:
    c = f.read()

# 1. Update CanvasScaler to match width/height equally (0.50f)
old_scaler = '''        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.15f;'''

new_scaler = '''        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.45f;'''

if old_scaler in c:
    c = c.replace(old_scaler, new_scaler, 1)
    print("CanvasScaler matchWidthOrHeight updated to 0.45f")
else:
    print("Warning: old_scaler not found")

# 2. Add roundedFrameSprite to Splash speech bubble
old_bubble = '''        Image bBg = cinematicBubbleObj.AddComponent<Image>();
        bBg.color = new Color(0.12f, 0.16f, 0.24f, 0.96f);
        Outline bOut = cinematicBubbleObj.AddComponent<Outline>();'''

new_bubble = '''        Image bBg = cinematicBubbleObj.AddComponent<Image>();
        bBg.color = new Color(0.12f, 0.16f, 0.24f, 0.96f);
        if (roundedFrameSprite != null)
        {
            bBg.sprite = roundedFrameSprite;
            bBg.type = Image.Type.Sliced;
        }
        Outline bOut = cinematicBubbleObj.AddComponent<Outline>();'''

if old_bubble in c:
    c = c.replace(old_bubble, new_bubble, 1)
    print("Splash Speech Bubble rounded frame applied")
else:
    print("Warning: old_bubble not found")

# 3. Add roundedFrameSprite to Splash loading bar track
old_track = '''        Image btImg = barTrack.AddComponent<Image>();
        btImg.color = new Color(0.18f, 0.12f, 0.26f, 1f);
        Outline btOutline = barTrack.AddComponent<Outline>();'''

new_track = '''        Image btImg = barTrack.AddComponent<Image>();
        btImg.color = new Color(0.18f, 0.12f, 0.26f, 1f);
        if (roundedFrameSprite != null)
        {
            btImg.sprite = roundedFrameSprite;
            btImg.type = Image.Type.Sliced;
        }
        Outline btOutline = barTrack.AddComponent<Outline>();'''

if old_track in c:
    c = c.replace(old_track, new_track, 1)
    print("Splash loading bar track rounded frame applied")
else:
    print("Warning: old_track not found")

# 4. Add roundedFrameSprite to Splash loading bar fill
old_fill = '''        Image bfImg = barFill.AddComponent<Image>();
        bfImg.color = new Color(0.35f, 0.92f, 0.48f, 1f);
        uiMgr.splashProgressFill = bfImg;'''

new_fill = '''        Image bfImg = barFill.AddComponent<Image>();
        bfImg.color = new Color(0.35f, 0.92f, 0.48f, 1f);
        if (roundedFrameSprite != null)
        {
            bfImg.sprite = roundedFrameSprite;
            bfImg.type = Image.Type.Sliced;
        }
        uiMgr.splashProgressFill = bfImg;'''

if old_fill in c:
    c = c.replace(old_fill, new_fill, 1)
    print("Splash loading bar fill rounded frame applied")
else:
    print("Warning: old_fill not found")

with open(path, "w", encoding="utf-8") as f:
    f.write(c)

print("BuildMainGameScene.cs updated successfully.")
