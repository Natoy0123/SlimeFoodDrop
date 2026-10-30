import os

# 1. Update SlimeController.cs to add TriggerCelebration
slime_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\SlimeController.cs"
with open(slime_path, 'r', encoding='utf-8') as f:
    slime_text = f.read()

if "TriggerCelebration" not in slime_text:
    target_heal = "    public void TriggerHealBurstEffect()"
    new_heal = """    public void TriggerCelebration()
    {
        TriggerHealBurstEffect();
    }

    public void TriggerHealBurstEffect()"""
    assert target_heal in slime_text
    slime_text = slime_text.replace(target_heal, new_heal, 1)
    with open(slime_path, 'w', encoding='utf-8') as f:
        f.write(slime_text)
    print("Added TriggerCelebration to SlimeController.cs")

# 2. Update GameManager.cs with the missing fields
gm_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\GameManager.cs"
with open(gm_path, 'r', encoding='utf-8') as f:
    gm_text = f.read()

target_hdr = """    [Header("Scoring")]
    public int currentScore = 0;
    public int comboCount = 0;
    public int highScore = 0;"""

new_hdr = """    [Header("Scoring")]
    public int currentScore = 0;
    public int comboCount = 0;
    public int highScore = 0;

    [Header("Frenzy & Food Rush State")]
    public int frenzyStreak = 0;
    public float frenzyStreakTimer = 0f;
    public const float MaxFrenzyStreakWindow = 4.0f;
    public bool isFoodRushActive = false;
    public float foodRushTimeRemaining = 0f;
    public float foodRushMaxDuration = 10f;
    public bool IsFoodRushActive => isFoodRushActive && foodRushTimeRemaining > 0f;

    public int FrenzyTargetForCurrentDifficulty => currentDifficulty == DifficultyLevel.Easy ? 10 : (currentDifficulty == DifficultyLevel.Medium ? 12 : 15);
    public float FoodRushDurationForCurrentDifficulty => currentDifficulty == DifficultyLevel.Easy ? 15.0f : 10.0f;"""

if "public int frenzyStreak = 0;" not in gm_text:
    assert target_hdr in gm_text, "target_hdr not found in GameManager.cs"
    gm_text = gm_text.replace(target_hdr, new_hdr, 1)
    with open(gm_path, 'w', encoding='utf-8') as f:
        f.write(gm_text)
    print("Added frenzy fields to GameManager.cs")

# 3. Update UIManager.cs replacing hudPanel with topBarHud
ui_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\UIManager.cs"
with open(ui_path, 'r', encoding='utf-8') as f:
    ui_text = f.read()

ui_text = ui_text.replace("hudPanel", "topBarHud")
with open(ui_path, 'w', encoding='utf-8') as f:
    f.write(ui_text)
print("Fixed hudPanel references in UIManager.cs")
