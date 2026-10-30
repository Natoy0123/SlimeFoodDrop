runner_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\GameManager.cs"
with open(runner_path, 'r', encoding='utf-8') as f:
    text = f.read()

# 1. Add TriggerFoodRush & EndFoodRush right above ActivateMagnet
target_mag = "    public void ActivateMagnet(float duration = 6.0f)"
new_methods = """    public void TriggerFoodRush()
    {
        isFoodRushActive = true;
        foodRushMaxDuration = FoodRushDurationForCurrentDifficulty;
        foodRushTimeRemaining = foodRushMaxDuration;
        frenzyStreak = 0;
        frenzyStreakTimer = 0f;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayChimeSound();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFoodRushBanner(foodRushMaxDuration);
            UIManager.Instance.UpdateFrenzyStreakHUD(0, FrenzyTargetForCurrentDifficulty);
        }

        if (SlimeController.Instance != null)
        {
            SlimeController.Instance.TriggerCelebration();
        }
    }

    public void EndFoodRush()
    {
        isFoodRushActive = false;
        foodRushTimeRemaining = 0f;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideFoodRushBanner();
            UIManager.Instance.UpdateFrenzyStreakHUD(0, FrenzyTargetForCurrentDifficulty);
        }
    }

    public void ActivateMagnet(float duration = 6.0f)"""

assert target_mag in text, "target_mag not found in GameManager.cs"
text = text.replace(target_mag, new_methods, 1)

# 2. Reset streak on poison damage
target_hurt = """        comboCount = 0;
        currentLives = Mathf.Clamp(currentLives - 1, 0, maxLives);"""

new_hurt = """        comboCount = 0;
        frenzyStreak = 0;
        frenzyStreakTimer = 0f;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateFrenzyStreakHUD(0, FrenzyTargetForCurrentDifficulty);
            UIManager.Instance.SpawnStreakBrokenIndicator();
        }
        currentLives = Mathf.Clamp(currentLives - 1, 0, maxLives);"""

assert target_hurt in text, "target_hurt not found in GameManager.cs"
text = text.replace(target_hurt, new_hurt, 1)

with open(runner_path, 'w', encoding='utf-8') as f:
    f.write(text)

print("GameManager.cs updated successfully with FoodRush and streak reset!")
