path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs"
with open(path, "r", encoding="utf-8") as f:
    text = f.read()

target = """        // Magnet Active Timer (Legacy mini text below High Score)
        Text magTxt = CreateUIText("Text_MagnetTimer", topBar.transform, defaultBodyFont, "MAGNET: 6.0s", 16, TextAnchor.MiddleRight, new Color(0.45f, 0.95f, 1f, 1f));
        RectTransform magRect = magTxt.GetComponent<RectTransform>();
        magRect.anchorMin = new Vector2(1f, 0.5f);
        magRect.anchorMax = new Vector2(1f, 0.5f);
        magRect.pivot = new Vector2(1f, 0.5f);
        magRect.anchoredPosition = new Vector2(-110f, -36f);
        magRect.sizeDelta = new Vector2(200f, 32f);
        magTxt.gameObject.SetActive(false);
        uiMgr.textMagnetTimer = magTxt;"""

replacement = """        // Magnet Active Timer (Legacy placeholder preserved disabled for compatibility)
        Text magTxt = CreateUIText("Text_MagnetTimer", topBar.transform, defaultBodyFont, "", 16, TextAnchor.MiddleRight, Color.clear);
        RectTransform magRect = magTxt.GetComponent<RectTransform>();
        magRect.anchorMin = new Vector2(1f, 0.5f);
        magRect.anchorMax = new Vector2(1f, 0.5f);
        magRect.pivot = new Vector2(1f, 0.5f);
        magRect.anchoredPosition = new Vector2(-110f, -36f);
        magRect.sizeDelta = new Vector2(200f, 32f);
        magTxt.gameObject.SetActive(false);
        uiMgr.textMagnetTimer = magTxt;"""

if target in text:
    text = text.replace(target, replacement, 1)
    with open(path, "w", encoding="utf-8") as f:
        f.write(text)
    print("[SUCCESS] BuildMainGameScene.cs legacy magTxt hidden!")
else:
    print("[NOTICE] Target already updated or modified.")
