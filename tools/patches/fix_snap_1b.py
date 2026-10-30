path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs"
with open(path, "r", encoding="utf-8") as f:
    c = f.read()

target = """            ui.pixarSplashCinematic.speechBubbleGroup.SetActive(true);
            ui.pixarSplashCinematic.speechBubbleText.text = "PFFFT!!";"""

replacement = """            ui.pixarSplashCinematic.ShowSpeechBubble("PFFFT!!", new Vector2(0f, 85f));"""

if target in c:
    c = c.replace(target, replacement, 1)
    print("Replaced target for Screenshot 1B")
    with open(path, "w", encoding="utf-8") as f:
        f.write(c)
else:
    print("Warning: target not found")
