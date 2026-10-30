import os

# 1. Update PixarSplashCinematic.cs to make ShowSpeechBubble public
psc_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\PixarSplashCinematic.cs"
with open(psc_path, "r", encoding="utf-8") as f:
    psc_code = f.read()

psc_code = psc_code.replace("private void ShowSpeechBubble(string text, Vector2 localPos)", "public void ShowSpeechBubble(string text, Vector2 localPos)")
with open(psc_path, "w", encoding="utf-8") as f:
    f.write(psc_code)
print("PixarSplashCinematic.cs: Made ShowSpeechBubble public")

# 2. Update BuildMainGameScene.cs
b_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs"
with open(b_path, "r", encoding="utf-8") as f:
    b_code = f.read()

# Fix Screenshot 1A to call ShowSpeechBubble
old_snap_1a = """                if (ui.pixarSplashCinematic != null)
                {
                    ui.pixarSplashCinematic.bloopRect.anchoredPosition = new Vector2(0f, -80f);
                    ui.pixarSplashCinematic.bloopImage.sprite = ui.pixarSplashCinematic.slimeTurnLookUser;
                    ui.pixarSplashCinematic.foodImage.gameObject.SetActive(false);
                    ui.pixarSplashCinematic.speechBubbleGroup.SetActive(true);
                    ui.pixarSplashCinematic.speechBubbleText.text = "...Did you see what I just ate?!";
                    ui.pixarSplashCinematic.speechBubbleRect.anchoredPosition = new Vector2(0f, 75f);
                }"""

new_snap_1a = """                if (ui.pixarSplashCinematic != null)
                {
                    ui.pixarSplashCinematic.bloopRect.anchoredPosition = new Vector2(0f, -80f);
                    ui.pixarSplashCinematic.bloopImage.sprite = ui.pixarSplashCinematic.slimeTurnLookUser;
                    ui.pixarSplashCinematic.foodImage.gameObject.SetActive(false);
                    ui.pixarSplashCinematic.ShowSpeechBubble("...Did you see what I just ate?!", new Vector2(0f, 75f));
                }"""

if old_snap_1a in b_code:
    b_code = b_code.replace(old_snap_1a, new_snap_1a, 1)
    print("BuildMainGameScene.cs: Updated Screenshot 1A to call ShowSpeechBubble")

# Fix Screenshot 1B to call ShowSpeechBubble
old_snap_1b = """                ui.pixarSplashCinematic.bloopImage.sprite = ui.pixarSplashCinematic.slimeSpitLaunch;
                ui.pixarSplashCinematic.bloopRect.localScale = new Vector3(0.70f, 1.48f, 1f);
                ui.pixarSplashCinematic.speechBubbleGroup.SetActive(true);
                ui.pixarSplashCinematic.speechBubbleText.text = "PFFFT!!";
                ui.pixarSplashCinematic.projectileImage.gameObject.SetActive(true);
                ui.pixarSplashCinematic.projectileRect.anchoredPosition = new Vector2(240f, 60f);"""

new_snap_1b = """                ui.pixarSplashCinematic.bloopImage.sprite = ui.pixarSplashCinematic.slimeSpitLaunch;
                ui.pixarSplashCinematic.bloopRect.localScale = new Vector3(0.70f, 1.48f, 1f);
                ui.pixarSplashCinematic.ShowSpeechBubble("PFFFT!!", new Vector2(0f, 85f));
                ui.pixarSplashCinematic.projectileImage.gameObject.SetActive(true);
                ui.pixarSplashCinematic.projectileRect.anchoredPosition = new Vector2(240f, 60f);"""

if old_snap_1b in b_code:
    b_code = b_code.replace(old_snap_1b, new_snap_1b, 1)
    print("BuildMainGameScene.cs: Updated Screenshot 1B to call ShowSpeechBubble")

# Initialize cinematicBubbleObj with wrap mode and larger size
old_bubble_init = """        // Speech Bubble
        GameObject cinematicBubbleObj = CreateUIElement("Cinematic_SpeechBubble", stageObj.transform);
        RectTransform bRect = cinematicBubbleObj.GetComponent<RectTransform>();
        bRect.anchoredPosition = new Vector2(0f, 80f);
        bRect.sizeDelta = new Vector2(440f, 62f);
        Image bBg = cinematicBubbleObj.AddComponent<Image>();
        bBg.color = new Color(0.12f, 0.16f, 0.24f, 0.96f);
        if (roundedFrameSprite != null)
        {
            bBg.sprite = roundedFrameSprite;
            bBg.type = Image.Type.Sliced;
        }
        Outline bOut = cinematicBubbleObj.AddComponent<Outline>();
        bOut.effectColor = new Color(1f, 0.88f, 0.44f, 1f);
        bOut.effectDistance = new Vector2(2f, -2f);

        Text bubbleTxt = CreateUIText("Text_Bubble", cinematicBubbleObj.transform, font, "...Still there?", 24, TextAnchor.MiddleCenter, Color.white);
        RectTransform btRectInternal = bubbleTxt.GetComponent<RectTransform>();
        btRectInternal.anchorMin = Vector2.zero;
        btRectInternal.anchorMax = Vector2.one;
        btRectInternal.offsetMin = new Vector2(10f, 6f);
        btRectInternal.offsetMax = new Vector2(-10f, -6f);"""

new_bubble_init = """        // Speech Bubble
        GameObject cinematicBubbleObj = CreateUIElement("Cinematic_SpeechBubble", stageObj.transform);
        RectTransform bRect = cinematicBubbleObj.GetComponent<RectTransform>();
        bRect.anchoredPosition = new Vector2(0f, 80f);
        bRect.sizeDelta = new Vector2(560f, 96f);
        Image bBg = cinematicBubbleObj.AddComponent<Image>();
        bBg.color = new Color(0.12f, 0.16f, 0.24f, 0.96f);
        if (roundedFrameSprite != null)
        {
            bBg.sprite = roundedFrameSprite;
            bBg.type = Image.Type.Sliced;
        }
        Outline bOut = cinematicBubbleObj.AddComponent<Outline>();
        bOut.effectColor = new Color(1f, 0.88f, 0.44f, 1f);
        bOut.effectDistance = new Vector2(2f, -2f);

        Text bubbleTxt = CreateUIText("Text_Bubble", cinematicBubbleObj.transform, font, "...Still there?", 24, TextAnchor.MiddleCenter, Color.white);
        bubbleTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        bubbleTxt.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform btRectInternal = bubbleTxt.GetComponent<RectTransform>();
        btRectInternal.anchorMin = Vector2.zero;
        btRectInternal.anchorMax = Vector2.one;
        btRectInternal.offsetMin = new Vector2(16f, 10f);
        btRectInternal.offsetMax = new Vector2(-16f, -10f);"""

if old_bubble_init in b_code:
    b_code = b_code.replace(old_bubble_init, new_bubble_init, 1)
    print("BuildMainGameScene.cs: Updated cinematicBubbleObj init with Wrap mode and 560x96 size")

# Adjust cornerBadge and tutorial title positions
b_code = b_code.replace("cbRect.anchoredPosition = new Vector2(96f, 24f);", "cbRect.anchoredPosition = new Vector2(96f, 38f);")
b_code = b_code.replace("tsRect.anchoredPosition = new Vector2(192f, -14f);", "tsRect.anchoredPosition = new Vector2(210f, -16f);")
b_code = b_code.replace("ttRect.anchoredPosition = new Vector2(192f, -44f);", "ttRect.anchoredPosition = new Vector2(210f, -48f);")

with open(b_path, "w", encoding="utf-8") as f:
    f.write(b_code)

print("BuildMainGameScene.cs: Tutorial badge and splash bubble adjustments complete.")
