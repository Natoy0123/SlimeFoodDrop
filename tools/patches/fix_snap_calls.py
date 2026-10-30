path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs"
with open(path, "r", encoding="utf-8") as f:
    c = f.read()

target_1 = """                ui.pixarSplashCinematic.speechBubbleGroup.SetActive(true);
                ui.pixarSplashCinematic.speechBubbleText.text = "...Did you see what I just ate?!";
                ui.pixarSplashCinematic.speechBubbleRect.anchoredPosition = new Vector2(0f, 75f);"""

replacement_1 = """                ui.pixarSplashCinematic.ShowSpeechBubble("...Did you see what I just ate?!", new Vector2(0f, 75f));"""

target_2 = """                ui.pixarSplashCinematic.speechBubbleGroup.SetActive(true);
                ui.pixarSplashCinematic.speechBubbleText.text = "PFFFT!!";"""

replacement_2 = """                ui.pixarSplashCinematic.ShowSpeechBubble("PFFFT!!", new Vector2(0f, 85f));"""

if target_1 in c:
    c = c.replace(target_1, replacement_1, 1)
    print("Replaced target_1")
else:
    print("Warning: target_1 not found")

if target_2 in c:
    c = c.replace(target_2, replacement_2, 1)
    print("Replaced target_2")
else:
    print("Warning: target_2 not found")

with open(path, "w", encoding="utf-8") as f:
    f.write(c)
