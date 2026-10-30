import os

# ==============================================================================
# 1. Update SoundManager.cs
# ==============================================================================
sm_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\SoundManager.cs"
with open(sm_path, "r", encoding="utf-8") as f:
    sm_code = f.read()

# Add powerup pickup fields
old_sm_fields = """    [Header("Tailored SFX Clips")]
    public AudioClip buttonClickClip;
    public AudioClip slimeEatClip;
    public AudioClip slimePukeClip;
    public AudioClip foodScamperClip;"""

new_sm_fields = """    [Header("Tailored SFX Clips")]
    public AudioClip buttonClickClip;
    public AudioClip slimeEatClip;
    public AudioClip slimePukeClip;
    public AudioClip foodScamperClip;

    [Header("Power-Up Specific Pickup Clips")]
    public AudioClip shieldPickupClip;
    public AudioClip magnetPickupClip;
    public AudioClip timeSlowPickupClip;
    public AudioClip heartPickupClip;"""

if old_sm_fields in sm_code:
    sm_code = sm_code.replace(old_sm_fields, new_sm_fields, 1)
    print("SoundManager.cs: Added pickup audio fields")

# Add powerup pickup play methods
old_sm_methods = """    public void PlayBonusHeartSound() => PlayChimeSound();
    public void PlayPoisonSound() => PlayHurtSound();
    public void PlayJumpSound() => PlayChimeSound();"""

new_sm_methods = """    public void PlayShieldPickupSound()
    {
        AudioClip c = shieldPickupClip != null ? shieldPickupClip : powerupClip;
        if (sfxSource != null && c != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(c, 0.95f * userSfxVolume);
        }
    }

    public void PlayMagnetPickupSound()
    {
        AudioClip c = magnetPickupClip != null ? magnetPickupClip : powerupClip;
        if (sfxSource != null && c != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(c, 0.95f * userSfxVolume);
        }
    }

    public void PlayTimeSlowPickupSound()
    {
        AudioClip c = timeSlowPickupClip != null ? timeSlowPickupClip : powerupClip;
        if (sfxSource != null && c != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(c, 0.95f * userSfxVolume);
        }
    }

    public void PlayHeartPickupSound()
    {
        AudioClip c = heartPickupClip != null ? heartPickupClip : chimeClip;
        if (sfxSource != null && c != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(c, 0.95f * userSfxVolume);
        }
    }

    public void PlayBonusHeartSound() => PlayHeartPickupSound();
    public void PlayPoisonSound() => PlayHurtSound();
    public void PlayJumpSound() => PlayChimeSound();"""

if old_sm_methods in sm_code:
    sm_code = sm_code.replace(old_sm_methods, new_sm_methods, 1)
    print("SoundManager.cs: Added powerup pickup play methods")

# Increase active loop volume to 0.85f for shield and magnet
sm_code = sm_code.replace("sSrc.volume = 0.50f * userSfxVolume;", "sSrc.volume = 0.85f * userSfxVolume;")
sm_code = sm_code.replace("mSrc.volume = 0.50f * userSfxVolume;", "mSrc.volume = 0.85f * userSfxVolume;")
print("SoundManager.cs: Boosted active loop volumes to 0.85f")

with open(sm_path, "w", encoding="utf-8") as f:
    f.write(sm_code)

# ==============================================================================
# 2. Update GameManager.cs
# ==============================================================================
gm_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\GameManager.cs"
with open(gm_path, "r", encoding="utf-8") as f:
    gm_code = f.read()

# Update ActivateMagnet
old_act_mag = """        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPowerupSound();
            SoundManager.Instance.UpdatePowerUpAudio(isShieldActive, isMagnetActive, isTimeSlowActive);
        }"""

new_act_mag = """        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMagnetPickupSound();
            SoundManager.Instance.UpdatePowerUpAudio(isShieldActive, isMagnetActive, isTimeSlowActive);
        }"""

if old_act_mag in gm_code:
    gm_code = gm_code.replace(old_act_mag, new_act_mag, 1)
    print("GameManager.cs: Updated ActivateMagnet sound")

# Update ActivateShieldBarrier
old_act_shield = """        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPowerupSound();
            SoundManager.Instance.UpdatePowerUpAudio(isShieldActive, isMagnetActive, isTimeSlowActive);
        }"""

new_act_shield = """        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayShieldPickupSound();
            SoundManager.Instance.UpdatePowerUpAudio(isShieldActive, isMagnetActive, isTimeSlowActive);
        }"""

if old_act_shield in gm_code:
    gm_code = gm_code.replace(old_act_shield, new_act_shield, 1)
    print("GameManager.cs: Updated ActivateShieldBarrier sound")

# Update ActivateTimeSlow
old_act_slow = """        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPowerupSound();
            SoundManager.Instance.UpdatePowerUpAudio(isShieldActive, isMagnetActive, isTimeSlowActive);
        }"""

new_act_slow = """        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayTimeSlowPickupSound();
            SoundManager.Instance.UpdatePowerUpAudio(isShieldActive, isMagnetActive, isTimeSlowActive);
        }"""

if old_act_slow in gm_code:
    gm_code = gm_code.replace(old_act_slow, new_act_slow, 1)
    print("GameManager.cs: Updated ActivateTimeSlow sound")

# Update AddLife
old_add_life = """        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBonusHeartSound();
        }"""

new_add_life = """        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHeartPickupSound();
        }"""

if old_add_life in gm_code:
    gm_code = gm_code.replace(old_add_life, new_add_life, 1)
    print("GameManager.cs: Updated AddLife sound")

with open(gm_path, "w", encoding="utf-8") as f:
    f.write(gm_code)

# ==============================================================================
# 3. Update UIManager.cs for Confirmation Modal z-order
# ==============================================================================
ui_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\UIManager.cs"
with open(ui_path, "r", encoding="utf-8") as f:
    ui_code = f.read()

old_show_conf = """        confirmationModal.SetActive(true);
        if (Application.isPlaying)
        {
            StartCoroutine(PopInTransform(confirmationModal.transform));
        }"""

new_show_conf = """        // Always bring confirmation modal to the front of all active dialogs (including settings!)
        confirmationModal.transform.SetAsLastSibling();
        confirmationModal.SetActive(true);
        if (Application.isPlaying)
        {
            StartCoroutine(PopInTransform(confirmationModal.transform));
        }"""

if old_show_conf in ui_code:
    ui_code = ui_code.replace(old_show_conf, new_show_conf, 1)
    print("UIManager.cs: Set confirmationModal SetAsLastSibling on open")

with open(ui_path, "w", encoding="utf-8") as f:
    f.write(ui_code)

# ==============================================================================
# 4. Update BuildMainGameScene.cs for audio wiring, text sizes, wrapping
# ==============================================================================
b_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs"
with open(b_path, "r", encoding="utf-8") as f:
    b_code = f.read()

# Add pickup clips loading
old_audio_loads = """        sm.shieldActiveClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_powerup_shield_active.wav");
        sm.magnetActiveClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_powerup_magnet_active.wav");
        sm.timeSlowActiveClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_powerup_timeslow_active.wav");"""

new_audio_loads = """        sm.shieldActiveClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_powerup_shield_active.wav");
        sm.magnetActiveClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_powerup_magnet_active.wav");
        sm.timeSlowActiveClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_powerup_timeslow_active.wav");
        sm.shieldPickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_pickup_shield.wav");
        sm.magnetPickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_pickup_magnet.wav");
        sm.timeSlowPickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_pickup_timeslow.wav");
        sm.heartPickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_pickup_heart.wav");"""

if old_audio_loads in b_code:
    b_code = b_code.replace(old_audio_loads, new_audio_loads, 1)
    print("BuildMainGameScene.cs: Loaded pickup audio clips into SoundManager")

# Fix diffTip overflowing text
old_diff_tip = """        Text diffTip = CreateUIText("Text_DiffTip", diffCard.transform, font, "STAGE THEMES UNLOCK AT 120, 260 & 420 PTS WITH DYNAMIC BGM!", 18, TextAnchor.MiddleCenter, new Color(0.95f, 0.88f, 0.40f, 1f));
        diffTip.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -220f);
        diffTip.GetComponent<RectTransform>().sizeDelta = new Vector2(720f, 60f);"""

new_diff_tip = """        Text diffTip = CreateUIText("Text_DiffTip", diffCard.transform, font, "STAGE THEMES UNLOCK AT 120, 260 & 420 PTS\\nWITH DYNAMIC BGM & UNIQUE HAZARDS!", 20, TextAnchor.MiddleCenter, new Color(0.95f, 0.88f, 0.40f, 1f));
        diffTip.horizontalOverflow = HorizontalWrapMode.Wrap;
        diffTip.lineSpacing = 1.15f;
        RectTransform dtRect = diffTip.GetComponent<RectTransform>();
        dtRect.anchoredPosition = new Vector2(0f, -226f);
        dtRect.sizeDelta = new Vector2(780f, 88f);"""

if old_diff_tip in b_code:
    b_code = b_code.replace(old_diff_tip, new_diff_tip, 1)
    print("BuildMainGameScene.cs: Fixed diffTip overflowing text and wrapped on two lines")

# Fix CreateDifficultyCardButton text sizing & wrapping
old_card_btn = """        Text titleTxt = CreateUIText("Text_ModeTitle", btnObj.transform, font, title, 22, TextAnchor.MiddleCenter, Color.white);
        RectTransform tRect = titleTxt.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0.5f, 0.5f);
        tRect.anchorMax = new Vector2(0.5f, 0.5f);
        tRect.pivot = new Vector2(0.5f, 0.5f);
        tRect.anchoredPosition = new Vector2(22f, 22f);
        tRect.sizeDelta = new Vector2(size.x - 120f, 52f);

        Text subTxt = CreateUIText("Text_ModeDesc", btnObj.transform, defaultBodyFont, subtitle, 16, TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.84f, 0.95f));
        RectTransform sRect = subTxt.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0.5f, 0.5f);
        sRect.anchorMax = new Vector2(0.5f, 0.5f);
        sRect.pivot = new Vector2(0.5f, 0.5f);
        sRect.anchoredPosition = new Vector2(22f, -24f);
        sRect.sizeDelta = new Vector2(size.x - 120f, 40f);"""

new_card_btn = """        Text titleTxt = CreateUIText("Text_ModeTitle", btnObj.transform, font, title, 28, TextAnchor.MiddleCenter, Color.white);
        RectTransform tRect = titleTxt.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0.5f, 0.5f);
        tRect.anchorMax = new Vector2(0.5f, 0.5f);
        tRect.pivot = new Vector2(0.5f, 0.5f);
        tRect.anchoredPosition = new Vector2(24f, 24f);
        tRect.sizeDelta = new Vector2(size.x - 130f, 56f);

        Text subTxt = CreateUIText("Text_ModeDesc", btnObj.transform, defaultBodyFont, subtitle, 20, TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.84f, 0.95f));
        subTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        RectTransform sRect = subTxt.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0.5f, 0.5f);
        sRect.anchorMax = new Vector2(0.5f, 0.5f);
        sRect.pivot = new Vector2(0.5f, 0.5f);
        sRect.anchoredPosition = new Vector2(24f, -24f);
        sRect.sizeDelta = new Vector2(size.x - 130f, 48f);"""

if old_card_btn in b_code:
    b_code = b_code.replace(old_card_btn, new_card_btn, 1)
    print("BuildMainGameScene.cs: Updated CreateDifficultyCardButton font sizes to 28/20")

# Fix Confirmation Modal text sizing
old_conf_txt = """        Text titleTxt = CreateUIText("Text_ConfirmTitle", strip.transform, arcadeFont, "CONFIRMATION", 24, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.38f, 1f));"""
new_conf_txt = """        Text titleTxt = CreateUIText("Text_ConfirmTitle", strip.transform, arcadeFont, "CONFIRMATION", 28, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.38f, 1f));"""
b_code = b_code.replace(old_conf_txt, new_conf_txt)

old_conf_msg = """        Text msgTxt = CreateUIText("Text_ConfirmMessage", cpCard.transform, bodyFont, "Are you sure you want to proceed?", 20, TextAnchor.MiddleCenter, new Color(0.96f, 0.98f, 1f, 1f));
        msgTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        msgTxt.lineSpacing = 1.15f;
        RectTransform mRect = msgTxt.GetComponent<RectTransform>();
        mRect.anchorMin = new Vector2(0.5f, 0.5f);
        mRect.anchorMax = new Vector2(0.5f, 0.5f);
        mRect.pivot = new Vector2(0.5f, 0.5f);
        mRect.anchoredPosition = new Vector2(0f, 24f);
        mRect.sizeDelta = new Vector2(680f, 140f);"""

new_conf_msg = """        Text msgTxt = CreateUIText("Text_ConfirmMessage", cpCard.transform, bodyFont, "Are you sure you want to proceed?", 26, TextAnchor.MiddleCenter, new Color(0.96f, 0.98f, 1f, 1f));
        msgTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        msgTxt.lineSpacing = 1.18f;
        RectTransform mRect = msgTxt.GetComponent<RectTransform>();
        mRect.anchorMin = new Vector2(0.5f, 0.5f);
        mRect.anchorMax = new Vector2(0.5f, 0.5f);
        mRect.pivot = new Vector2(0.5f, 0.5f);
        mRect.anchoredPosition = new Vector2(0f, 24f);
        mRect.sizeDelta = new Vector2(700f, 150f);"""

if old_conf_msg in b_code:
    b_code = b_code.replace(old_conf_msg, new_conf_msg, 1)
    print("BuildMainGameScene.cs: Updated Confirmation message font size to 26")

# Confirmation buttons font size
b_code = b_code.replace(
    'Button btnCancel = CreateUIButton("Button_ConfirmCancel", cpCard.transform, arcadeFont, "CANCEL", new Vector2(-160f, -140f), new Vector2(280f, 88f), new Color(0.24f, 0.26f, 0.36f, 1f), 20);',
    'Button btnCancel = CreateUIButton("Button_ConfirmCancel", cpCard.transform, arcadeFont, "CANCEL", new Vector2(-160f, -140f), new Vector2(280f, 88f), new Color(0.24f, 0.26f, 0.36f, 1f), 24);'
)
b_code = b_code.replace(
    'Button btnYes = CreateUIButton("Button_ConfirmYes", cpCard.transform, arcadeFont, "YES, PROCEED", new Vector2(160f, -140f), new Vector2(280f, 88f), new Color(0.68f, 0.24f, 0.22f, 1f), 20);',
    'Button btnYes = CreateUIButton("Button_ConfirmYes", cpCard.transform, arcadeFont, "YES, PROCEED", new Vector2(160f, -140f), new Vector2(280f, 88f), new Color(0.68f, 0.24f, 0.22f, 1f), 24);'
)

# Bump Almanac item detail modal font sizes
b_code = b_code.replace('16, TextAnchor.UpperLeft', '22, TextAnchor.UpperLeft')
b_code = b_code.replace('15, TextAnchor.UpperLeft', '22, TextAnchor.UpperLeft')
b_code = b_code.replace('btnClose, 20)', 'btnClose, 26)')

with open(b_path, "w", encoding="utf-8") as f:
    f.write(b_code)

print("All audio and UI changes written successfully.")
