path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\UIManager.cs"
with open(path, "r", encoding="utf-8") as f:
    text = f.read()

target = """    public void UpdateMagnetTimer(float remainingSeconds)
    {
        if (textMagnetTimer == null) return;

        if (remainingSeconds > 0.05f)
        {
            textMagnetTimer.gameObject.SetActive(true);
            textMagnetTimer.text = $"MAGNET: {remainingSeconds:0.0}s";
        }
        else
        {
            textMagnetTimer.gameObject.SetActive(false);
        }
    }"""

replacement = """    public void UpdateMagnetTimer(float remainingSeconds)
    {
        // Legacy HUD text deactivated in favor of dedicated Power-Up Duration Bars below TopBar
        if (textMagnetTimer != null && textMagnetTimer.gameObject.activeSelf)
        {
            textMagnetTimer.gameObject.SetActive(false);
        }
    }"""

assert target in text, "target not found in UIManager.cs"
text = text.replace(target, replacement, 1)
with open(path, "w", encoding="utf-8") as f:
    f.write(text)
print("[SUCCESS] UIManager.cs legacy UpdateMagnetTimer disabled!")
