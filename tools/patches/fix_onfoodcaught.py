gm_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\GameManager.cs"
with open(gm_path, 'r', encoding='utf-8') as f:
    text = f.read()

target = """    public void OnFoodCaught(int basePoints)
    {
        if (!IsPlaying) return;

        comboCount++;
        int effectiveBase = Mathf.Max(basePoints, BaseFoodPoints);
        int bonus = (comboCount / 5) * 5;
        int totalPoints = effectiveBase + bonus;
        AddScore(totalPoints);

        if (SlimeController.Instance != null)
        {
            SlimeController.Instance.TriggerEatEffect();
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMunchSound();
        }
    }"""

replacement = """    public void OnFoodCaught(int basePoints)
    {
        Vector3 pos = SlimeController.Instance != null ? SlimeController.Instance.transform.position : Vector3.zero;
        OnFoodCaught(basePoints, pos);
    }

    public void OnFoodCaught(int basePoints, Vector3 foodWorldPos)
    {
        if (!IsPlaying) return;

        comboCount++;
        int effectiveBase = Mathf.Max(basePoints, BaseFoodPoints);
        if (IsFoodRushActive) effectiveBase += 10; // Extra +10 bonus points during Food Rush!
        int bonus = (comboCount / 5) * 5;
        int totalPoints = effectiveBase + bonus;
        AddScore(totalPoints);

        // Frenzy Streak logic (excluding poison / powerups)
        if (!IsFoodRushActive)
        {
            frenzyStreak++;
            frenzyStreakTimer = MaxFrenzyStreakWindow; // Reset 4-second streak window
            int target = FrenzyTargetForCurrentDifficulty;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.SpawnFloatingStreakIndicator(frenzyStreak, foodWorldPos);
                UIManager.Instance.UpdateFrenzyStreakHUD(frenzyStreak, target);
            }

            if (frenzyStreak >= target)
            {
                TriggerFoodRush();
            }
        }
        else
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SpawnFloatingRushBonus(foodWorldPos, totalPoints);
            }
        }

        if (SlimeController.Instance != null)
        {
            SlimeController.Instance.TriggerEatEffect();
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMunchSound();
        }
    }"""

assert target in text, "target not found in GameManager.cs"
text = text.replace(target, replacement, 1)

with open(gm_path, 'w', encoding='utf-8') as f:
    f.write(text)

print("GameManager.cs updated with OnFoodCaught 2-param overload!")

# Update FallingItem.cs
fi_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\FallingItem.cs"
with open(fi_path, 'r', encoding='utf-8') as f:
    fi_text = f.read()

target_fi = "GameManager.Instance.OnFoodCaught(pointValue);"
new_fi = "GameManager.Instance.OnFoodCaught(pointValue, transform.position);"
assert target_fi in fi_text, "target_fi not found in FallingItem.cs"
fi_text = fi_text.replace(target_fi, new_fi, 1)

with open(fi_path, 'w', encoding='utf-8') as f:
    f.write(fi_text)

print("FallingItem.cs updated to pass transform.position into OnFoodCaught!")
