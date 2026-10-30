b_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs"
with open(b_path, "r", encoding="utf-8") as f:
    c = f.read()

target = """        gm.ActivateMagnet(6.0f);
        gm.ActivateShieldBarrier(12.0f);
        gm.ActivateTimeSlow(6.0f);
        Debug.Assert(gm.IsMagnetActive && gm.IsShieldActive && gm.IsTimeSlowActive, "[SP-10 FAIL] All 3 timed power-ups should be active.");"""

replacement = """        gm.ActivateMagnet(6.0f);
        gm.ActivateShieldBarrier(12.0f);
        gm.ActivateTimeSlow(6.0f);
        Debug.Assert(gm.IsMagnetActive && gm.IsShieldActive && gm.IsTimeSlowActive, "[SP-10 FAIL] All 3 timed power-ups should be active.");
        Debug.Assert(SoundManager.Instance.magnetActiveSource != null && SoundManager.Instance.magnetActiveSource.isPlaying,
            "[SP-10 FAIL] Magnet audio source must be playing when Magnet is active!");
        Debug.Assert(SoundManager.Instance.shieldActiveSource != null && SoundManager.Instance.shieldActiveSource.isPlaying,
            "[SP-10 FAIL] Shield audio source must be playing when Shield is active!");
        Debug.Assert(SoundManager.Instance.timeSlowActiveSource != null && SoundManager.Instance.timeSlowActiveSource.isPlaying,
            "[SP-10 FAIL] Time-Slow audio source must be playing when Time-Slow is active!");"""

if target in c:
    c = c.replace(target, replacement, 1)
    print("Replaced powerup assertions successfully")
    with open(b_path, "w", encoding="utf-8") as f:
        f.write(c)
else:
    print("Warning: target not found")
