import os

b_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs"
with open(b_path, "r", encoding="utf-8") as f:
    c = f.read()

# Add in-game confirmation over settings screenshot and assertion
old_conf_test = """        // Screenshot 3C: Restart Confirmation Modal Guard
        ui.PromptRestartConfirmation();
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_Confirmation_Modal.png"));
        ui.HideConfirmation();"""

new_conf_test = """        // Screenshot 3C: Restart Confirmation Modal Guard (From In-Game Settings)
        ui.ShowSettingsModal(true); // Open In-Game Pause Settings first
        ui.PromptRestartConfirmation(); // Open Restart Confirmation on top of Settings
        Debug.Assert(ui.confirmationModal.transform.GetSiblingIndex() > ui.settingsPanel.transform.GetSiblingIndex(),
            "[FAIL] Confirmation modal must have higher sibling index (render in front) than settings modal!");
        RenderViewportScreenshot(cam, respCam, 1080, 1920, Path.Combine(logDir, "Verify_Phone_9x16_Confirmation_Modal.png"));
        ui.HideConfirmation();
        ui.HideSettingsModal();"""

if old_conf_test in c:
    c = c.replace(old_conf_test, new_conf_test, 1)
    print("BuildMainGameScene.cs: Updated Screenshot 3C to test confirmation on top of settings")

# Add power-up audio assertions to verification
old_powerup_asserts = """        // Verify Power-Ups: Magnet, Shield, Time-Slow & Heart
        gm.ActivateShieldBarrier(12.0f);
        Debug.Assert(gm.IsShieldActive, "[SP-10 FAIL] Shield Barrier must be active after ActivateShieldBarrier()!");"""

new_powerup_asserts = """        // Verify Power-Ups: Magnet, Shield, Time-Slow & Heart with full audio validation
        gm.ActivateMagnet(6.0f);
        Debug.Assert(gm.IsMagnetActive, "[SP-10 FAIL] Magnet must be active after ActivateMagnet()!");
        Debug.Assert(SoundManager.Instance.magnetActiveSource != null && SoundManager.Instance.magnetActiveSource.isPlaying,
            "[SP-10 FAIL] Magnet audio source must be playing when Magnet is active!");
        Debug.Assert(SoundManager.Instance.magnetActiveSource.volume > 0.8f,
            "[SP-10 FAIL] Magnet active audio volume must be audible (> 0.8f)!");

        gm.ActivateShieldBarrier(12.0f);
        Debug.Assert(gm.IsShieldActive, "[SP-10 FAIL] Shield Barrier must be active after ActivateShieldBarrier()!");
        Debug.Assert(SoundManager.Instance.shieldActiveSource != null && SoundManager.Instance.shieldActiveSource.isPlaying,
            "[SP-10 FAIL] Shield audio source must be playing when Shield is active!");
        Debug.Assert(SoundManager.Instance.shieldActiveSource.volume > 0.8f,
            "[SP-10 FAIL] Shield active audio volume must be audible (> 0.8f)!");

        gm.ActivateTimeSlow(6.0f);
        Debug.Assert(gm.IsTimeSlowActive, "[SP-10 FAIL] Time-Slow must be active after ActivateTimeSlow()!");
        Debug.Assert(SoundManager.Instance.timeSlowActiveSource != null && SoundManager.Instance.timeSlowActiveSource.isPlaying,
            "[SP-10 FAIL] Time-Slow audio source must be playing when Time-Slow is active!");
        Debug.Assert(SoundManager.Instance.timeSlowActiveSource.volume > 0.8f,
            "[SP-10 FAIL] Time-Slow active audio volume must be audible (> 0.8f)!");"""

if old_powerup_asserts in c:
    c = c.replace(old_powerup_asserts, new_powerup_asserts, 1)
    print("BuildMainGameScene.cs: Added powerup audio source and volume assertions")

with open(b_path, "w", encoding="utf-8") as f:
    f.write(c)

print("BuildMainGameScene.cs: Updated verification suite successfully.")
