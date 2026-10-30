import os

psc_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\PixarSplashCinematic.cs"
with open(psc_path, "r", encoding="utf-8") as f:
    c = f.read()

old_show = """    public void ShowSpeechBubble(string text, Vector2 localPos)
    {
        if (speechBubbleGroup == null) return;
        speechBubbleGroup.SetActive(true);
        if (speechBubbleText != null)
        {
            speechBubbleText.text = text;
            speechBubbleText.horizontalOverflow = HorizontalWrapMode.Overflow;
            speechBubbleText.verticalOverflow = VerticalWrapMode.Overflow;
            float unconstrainedW = speechBubbleText.preferredWidth;

            float targetW;
            float targetH;

            if (unconstrainedW + 64f <= 480f)
            {
                targetW = Mathf.Clamp(unconstrainedW + 64f, 220f, 520f);
                speechBubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
                if (speechBubbleRect != null) speechBubbleRect.sizeDelta = new Vector2(targetW, 100f);
                RectTransform tRect = speechBubbleText.GetComponent<RectTransform>();
                if (tRect != null) tRect.sizeDelta = new Vector2(targetW - 36f, 48f);
                float prefH = speechBubbleText.preferredHeight;
                targetH = Mathf.Clamp(prefH + 34f, 72f, 100f);
            }
            else
            {
                targetW = 560f;
                speechBubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
                if (speechBubbleRect != null) speechBubbleRect.sizeDelta = new Vector2(targetW, 100f);
                RectTransform tRect = speechBubbleText.GetComponent<RectTransform>();
                if (tRect != null) tRect.sizeDelta = new Vector2(targetW - 40f, 400f);
                float prefH = speechBubbleText.preferredHeight;
                targetH = Mathf.Clamp(prefH + 42f, 96f, 240f);
            }

            if (speechBubbleRect != null)
            {
                speechBubbleRect.sizeDelta = new Vector2(targetW, targetH);
                RectTransform tRect = speechBubbleText.GetComponent<RectTransform>();
                if (tRect != null)
                {
                    tRect.sizeDelta = new Vector2(targetW - 32f, targetH - 20f);
                }
            }
        }
        if (speechBubbleRect != null) speechBubbleRect.anchoredPosition = localPos;
    }"""

new_show = """    public void ShowSpeechBubble(string text, Vector2 localPos)
    {
        if (speechBubbleGroup == null) return;
        speechBubbleGroup.SetActive(true);
        if (speechBubbleText != null)
        {
            speechBubbleText.text = text;
            speechBubbleText.horizontalOverflow = HorizontalWrapMode.Overflow;
            speechBubbleText.verticalOverflow = VerticalWrapMode.Overflow;
            float unconstrainedW = speechBubbleText.preferredWidth;

            float targetW;
            float targetH;

            if (unconstrainedW + 64f <= 780f)
            {
                targetW = Mathf.Clamp(unconstrainedW + 64f, 220f, 800f);
                speechBubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
                if (speechBubbleRect != null) speechBubbleRect.sizeDelta = new Vector2(targetW, 100f);
                RectTransform tRect = speechBubbleText.GetComponent<RectTransform>();
                if (tRect != null) tRect.sizeDelta = new Vector2(targetW - 36f, 54f);
                float prefH = speechBubbleText.preferredHeight;
                targetH = Mathf.Clamp(prefH + 34f, 72f, 100f);
            }
            else
            {
                targetW = 760f;
                speechBubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
                if (speechBubbleRect != null) speechBubbleRect.sizeDelta = new Vector2(targetW, 100f);
                RectTransform tRect = speechBubbleText.GetComponent<RectTransform>();
                if (tRect != null) tRect.sizeDelta = new Vector2(targetW - 40f, 400f);
                float prefH = speechBubbleText.preferredHeight;
                targetH = Mathf.Clamp(prefH + 42f, 96f, 240f);
            }

            if (speechBubbleRect != null)
            {
                speechBubbleRect.sizeDelta = new Vector2(targetW, targetH);
                RectTransform tRect = speechBubbleText.GetComponent<RectTransform>();
                if (tRect != null)
                {
                    tRect.sizeDelta = new Vector2(targetW - 32f, targetH - 20f);
                }
            }
            Canvas.ForceUpdateCanvases();
        }
        if (speechBubbleRect != null) speechBubbleRect.anchoredPosition = localPos;
    }"""

if old_show in c:
    c = c.replace(old_show, new_show, 1)
    print("Replaced ShowSpeechBubble with 780f single-line clamp and ForceUpdateCanvases")
    with open(psc_path, "w", encoding="utf-8") as f:
        f.write(c)
else:
    print("Warning: old_show not found")
