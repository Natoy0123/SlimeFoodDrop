import os

path = r'C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\UIManager.cs'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Add fields
target1 = '''    [Header("Environment Transition Banner References")]
    public GameObject environmentBannerPanel;
    public CanvasGroup environmentBannerCanvasGroup;'''

repl1 = '''    [Header("Environment Transition Banner References")]
    public GameObject environmentBannerPanel;
    public CanvasGroup environmentBannerCanvasGroup;

    [Header("Food Rush Clock Widget (Top-Left HUD)")]
    public GameObject foodRushClockWidget;
    public RectTransform foodRushClockHand;
    public Text textFoodRushClockTimer;'''

# 2. Update ShowFoodRushBanner, UpdateFoodRushHUD, HideFoodRushBanner
target2 = '''    public void ShowFoodRushBanner(float duration)
    {
        if (textThemeToast != null)
        {
            textThemeToast.text = $"FOOD RUSH ACTIVE! ({duration:F0}s)";
            textThemeToast.color = new Color(1.0f, 0.90f, 0.30f, 1f);
            textThemeToast.gameObject.SetActive(true);
        }
    }

    public void UpdateFoodRushHUD(float remaining, float max)
    {
        if (textThemeToast != null && textThemeToast.gameObject.activeSelf)
        {
            textThemeToast.text = $"FOOD RUSH! {remaining:F1}s";
        }
        if (textFrenzyStreak != null)
        {
            textFrenzyStreak.text = $"RUSH: {remaining:F1}s";
            textFrenzyStreak.color = new Color(1.0f, 0.86f, 0.25f, 1f);
        }
    }

    public void HideFoodRushBanner()
    {
        if (textThemeToast != null)
        {
            textThemeToast.gameObject.SetActive(false);
        }
    }'''

repl2 = '''    public void ShowFoodRushBanner(float duration)
    {
        if (foodRushClockWidget != null)
        {
            foodRushClockWidget.SetActive(true);
            if (textFoodRushClockTimer != null)
            {
                textFoodRushClockTimer.text = $"{duration:F1}s";
            }
        }
    }

    public void UpdateFoodRushHUD(float remaining, float max)
    {
        if (foodRushClockWidget != null && foodRushClockWidget.activeSelf)
        {
            if (textFoodRushClockTimer != null)
            {
                textFoodRushClockTimer.text = $"{Mathf.Max(0f, remaining):F1}s";
            }
            if (foodRushClockHand != null)
            {
                // Smoothly spin clock needle clockwise
                float angle = -(Time.time * 360f) % 360f;
                foodRushClockHand.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
        if (textFrenzyStreak != null)
        {
            textFrenzyStreak.text = $"RUSH: {remaining:F1}s";
            textFrenzyStreak.color = new Color(1.0f, 0.86f, 0.25f, 1f);
        }
    }

    public void HideFoodRushBanner()
    {
        if (foodRushClockWidget != null)
        {
            foodRushClockWidget.SetActive(false);
        }
    }'''

# 3. Hide in SetGameplayHUDVisible
target3 = '''        if (!visible && environmentBannerPanel != null)
        {
            environmentBannerPanel.SetActive(false);
        }'''

repl3 = '''        if (!visible && environmentBannerPanel != null)
        {
            environmentBannerPanel.SetActive(false);
        }
        if (!visible && foodRushClockWidget != null)
        {
            foodRushClockWidget.SetActive(false);
        }'''

content_nl = content.replace('\r\n', '\n')
for i, (t, r) in enumerate([(target1, repl1), (target2, repl2), (target3, repl3)]):
    if t not in content_nl:
        print(f'FAILED TO FIND TARGET {i+1}:', t[:40])
    else:
        content_nl = content_nl.replace(t, r, 1)

content_out = content_nl.replace('\n', '\r\n')
with open(path, 'w', encoding='utf-8') as f:
    f.write(content_out)

print('UIManager.cs successfully patched!')
