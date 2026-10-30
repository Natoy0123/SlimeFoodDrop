bmgs_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs"
with open(bmgs_path, "r", encoding="utf-8") as f:
    text = f.read()

# 1. Update Audio and Haptics labels in Settings Modal
old_audio_labels = """        // Row 1: BGM Volume (28px horizontal gaps between [-] [100%] [+])
        Text bgmLbl = CreateUIText("Label_BgmVolume", audioBox.transform, font, "MUSIC (BGM)", 24, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));
        bgmLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(-175f, 98f);
        bgmLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(340f, 52f);

        Button btnBgmMinus = CreateUIButton("Button_BgmMinus", audioBox.transform, font, "-", new Vector2(75f, 98f), new Vector2(76f, 66f), new Color(0.68f, 0.26f, 0.26f, 1f), 40);
        Text bgmValTxt = CreateUIText("Text_BgmValue", audioBox.transform, font, "100%", 22, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.38f, 1f));
        bgmValTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(195f, 98f);
        bgmValTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(130f, 52f);
        Button btnBgmPlus = CreateUIButton("Button_BgmPlus", audioBox.transform, font, "+", new Vector2(315f, 98f), new Vector2(76f, 66f), new Color(0.20f, 0.66f, 0.34f, 1f), 40);

        uiMgr.btnBgmMinus = btnBgmMinus;
        uiMgr.btnBgmPlus = btnBgmPlus;
        uiMgr.textBgmVolumeValue = bgmValTxt;

        // Row 2: SFX Volume
        Text sfxLbl = CreateUIText("Label_SfxVolume", audioBox.transform, font, "SOUND (SFX)", 24, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));
        sfxLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(-175f, 0f);
        sfxLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(340f, 52f);

        Button btnSfxMinus = CreateUIButton("Button_SfxMinus", audioBox.transform, font, "-", new Vector2(75f, 0f), new Vector2(76f, 66f), new Color(0.68f, 0.26f, 0.26f, 1f), 40);
        Text sfxValTxt = CreateUIText("Text_SfxValue", audioBox.transform, font, "100%", 22, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.38f, 1f));
        sfxValTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(195f, 0f);
        sfxValTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(130f, 52f);
        Button btnSfxPlus = CreateUIButton("Button_SfxPlus", audioBox.transform, font, "+", new Vector2(315f, 0f), new Vector2(76f, 66f), new Color(0.20f, 0.66f, 0.34f, 1f), 40);

        uiMgr.btnSfxMinus = btnSfxMinus;
        uiMgr.btnSfxPlus = btnSfxPlus;
        uiMgr.textSfxVolumeValue = sfxValTxt;

        // Row 3: Poison Haptics / Vibration Toggle Icon Button
        Text vibTitleLbl = CreateUIText("Label_VibrationTitle", audioBox.transform, font, "POISON HAPTICS", 22, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));"""

new_audio_labels = """        // Row 1: BGM Volume (28px horizontal gaps between [-] [100%] [+])
        Text bgmLbl = CreateUIText("Label_BgmVolume", audioBox.transform, defaultBodyFont, "MUSIC (BGM)", 26, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));
        bgmLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(-175f, 98f);
        bgmLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(340f, 52f);

        Button btnBgmMinus = CreateUIButton("Button_BgmMinus", audioBox.transform, font, "-", new Vector2(75f, 98f), new Vector2(76f, 66f), new Color(0.68f, 0.26f, 0.26f, 1f), 40);
        Text bgmValTxt = CreateUIText("Text_BgmValue", audioBox.transform, defaultBodyFont, "100%", 24, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.38f, 1f));
        bgmValTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(195f, 98f);
        bgmValTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(130f, 52f);
        Button btnBgmPlus = CreateUIButton("Button_BgmPlus", audioBox.transform, font, "+", new Vector2(315f, 98f), new Vector2(76f, 66f), new Color(0.20f, 0.66f, 0.34f, 1f), 40);

        uiMgr.btnBgmMinus = btnBgmMinus;
        uiMgr.btnBgmPlus = btnBgmPlus;
        uiMgr.textBgmVolumeValue = bgmValTxt;

        // Row 2: SFX Volume
        Text sfxLbl = CreateUIText("Label_SfxVolume", audioBox.transform, defaultBodyFont, "SOUND (SFX)", 26, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));
        sfxLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(-175f, 0f);
        sfxLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(340f, 52f);

        Button btnSfxMinus = CreateUIButton("Button_SfxMinus", audioBox.transform, font, "-", new Vector2(75f, 0f), new Vector2(76f, 66f), new Color(0.68f, 0.26f, 0.26f, 1f), 40);
        Text sfxValTxt = CreateUIText("Text_SfxValue", audioBox.transform, defaultBodyFont, "100%", 24, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.38f, 1f));
        sfxValTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(195f, 0f);
        sfxValTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(130f, 52f);
        Button btnSfxPlus = CreateUIButton("Button_SfxPlus", audioBox.transform, font, "+", new Vector2(315f, 0f), new Vector2(76f, 66f), new Color(0.20f, 0.66f, 0.34f, 1f), 40);

        uiMgr.btnSfxMinus = btnSfxMinus;
        uiMgr.btnSfxPlus = btnSfxPlus;
        uiMgr.textSfxVolumeValue = sfxValTxt;

        // Row 3: Poison Haptics / Vibration Toggle Icon Button
        Text vibTitleLbl = CreateUIText("Label_VibrationTitle", audioBox.transform, defaultBodyFont, "POISON HAPTICS", 26, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));"""

assert old_audio_labels in text, "old_audio_labels not found in BuildMainGameScene.cs"
text = text.replace(old_audio_labels, new_audio_labels, 1)
print("[OK] Settings audio & vibration labels updated to defaultBodyFont 26pt!")

# 2. Update Session Actions header and 4 tile buttons
old_actions = """        Text sessionHeaderLbl = CreateUIText("Label_SessionActions", inGameGroup.transform, font, "IN-GAME ACTIONS", 22, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.42f, 0.95f));
        sessionHeaderLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 88f);
        sessionHeaderLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(500f, 32f);

        // 4 Icon Tile Buttons with 36px horizontal gap between each 164x144 button!
        Button btnSetRestart = CreateUIIconTileButton(
            "Button_SettingsRestart",
            inGameGroup.transform,
            font,
            iconRestartSprite,
            "RESTART",
            new Vector2(-294f, -18f),
            new Vector2(164f, 144f),
            new Vector2(64f, 64f),
            new Color(0.68f, 0.44f, 0.16f, 1f),
            20
        );
        uiMgr.btnSettingsRestart = btnSetRestart;

        Button btnSetGuide = CreateUIIconTileButton(
            "Button_SettingsGuidebook",
            inGameGroup.transform,
            font,
            guidebookIconSprite,
            "ALMANAC",
            new Vector2(-98f, -18f),
            new Vector2(164f, 144f),
            new Vector2(64f, 64f),
            new Color(0.16f, 0.28f, 0.38f, 1f),
            20
        );
        uiMgr.btnSettingsGuidebook = btnSetGuide;

        Button btnReplayTut = CreateUIIconTileButton(
            "Button_ReplayTutorial",
            inGameGroup.transform,
            font,
            iconTutorialSprite,
            "TUTORIAL",
            new Vector2(98f, -18f),
            new Vector2(164f, 144f),
            new Vector2(64f, 64f),
            new Color(0.32f, 0.26f, 0.44f, 1f),
            20
        );
        uiMgr.btnSettingsReplayTutorial = btnReplayTut;

        Button btnSetHome = CreateUIIconTileButton(
            "Button_SettingsHome",
            inGameGroup.transform,
            font,
            iconHomeSprite,
            "HOME",
            new Vector2(294f, -18f),
            new Vector2(164f, 144f),
            new Vector2(64f, 64f),
            new Color(0.58f, 0.20f, 0.22f, 1f),
            20
        );"""

new_actions = """        Text sessionHeaderLbl = CreateUIText("Label_SessionActions", inGameGroup.transform, defaultBodyFont, "IN-GAME ACTIONS", 26, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.42f, 0.95f));
        sessionHeaderLbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 88f);
        sessionHeaderLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(500f, 34f);

        // 4 Icon Tile Buttons with 36px horizontal gap between each 164x144 button!
        Button btnSetRestart = CreateUIIconTileButton(
            "Button_SettingsRestart",
            inGameGroup.transform,
            defaultBodyFont,
            iconRestartSprite,
            "RESTART",
            new Vector2(-294f, -18f),
            new Vector2(164f, 144f),
            new Vector2(64f, 64f),
            new Color(0.68f, 0.44f, 0.16f, 1f),
            24
        );
        uiMgr.btnSettingsRestart = btnSetRestart;

        Button btnSetGuide = CreateUIIconTileButton(
            "Button_SettingsGuidebook",
            inGameGroup.transform,
            defaultBodyFont,
            guidebookIconSprite,
            "ALMANAC",
            new Vector2(-98f, -18f),
            new Vector2(164f, 144f),
            new Vector2(64f, 64f),
            new Color(0.16f, 0.28f, 0.38f, 1f),
            24
        );
        uiMgr.btnSettingsGuidebook = btnSetGuide;

        Button btnReplayTut = CreateUIIconTileButton(
            "Button_ReplayTutorial",
            inGameGroup.transform,
            defaultBodyFont,
            iconTutorialSprite,
            "TUTORIAL",
            new Vector2(98f, -18f),
            new Vector2(164f, 144f),
            new Vector2(64f, 64f),
            new Color(0.32f, 0.26f, 0.44f, 1f),
            24
        );
        uiMgr.btnSettingsReplayTutorial = btnReplayTut;

        Button btnSetHome = CreateUIIconTileButton(
            "Button_SettingsHome",
            inGameGroup.transform,
            defaultBodyFont,
            iconHomeSprite,
            "HOME",
            new Vector2(294f, -18f),
            new Vector2(164f, 144f),
            new Vector2(64f, 64f),
            new Color(0.58f, 0.20f, 0.22f, 1f),
            24
        );"""

assert old_actions in text, "old_actions not found in BuildMainGameScene.cs"
text = text.replace(old_actions, new_actions, 1)
print("[OK] Settings in-game actions updated to defaultBodyFont 24-26pt!")

# 3. Update Confirmation Modal typography
old_confirm = """        Text titleTxt = CreateUIText("Text_ConfirmTitle", strip.transform, arcadeFont, "CONFIRMATION", 28, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.38f, 1f));
        RectTransform ttRect = titleTxt.GetComponent<RectTransform>();
        ttRect.anchorMin = Vector2.zero;
        ttRect.anchorMax = Vector2.one;
        ttRect.offsetMin = Vector2.zero;
        ttRect.offsetMax = Vector2.zero;

        Text msgTxt = CreateUIText("Text_ConfirmMessage", cpCard.transform, bodyFont, "Are you sure you want to proceed?", 26, TextAnchor.MiddleCenter, new Color(0.96f, 0.98f, 1f, 1f));
        msgTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        msgTxt.verticalOverflow = VerticalWrapMode.Truncate;
        msgTxt.lineSpacing = 1.15f;
        RectTransform mtRect = msgTxt.GetComponent<RectTransform>();
        mtRect.anchoredPosition = new Vector2(0f, 15f);
        mtRect.sizeDelta = new Vector2(720f, 130f);

        Button btnCancel = CreateUIButton("Button_ConfirmCancel", cpCard.transform, arcadeFont, "CANCEL", new Vector2(-160f, -140f), new Vector2(280f, 88f), new Color(0.28f, 0.26f, 0.36f, 1f), 24);
        Button btnYes = CreateUIButton("Button_ConfirmYes", cpCard.transform, arcadeFont, "YES, PROCEED", new Vector2(160f, -140f), new Vector2(280f, 88f), new Color(0.68f, 0.24f, 0.26f, 1f), 24);"""

new_confirm = """        Text titleTxt = CreateUIText("Text_ConfirmTitle", strip.transform, defaultBodyFont, "CONFIRMATION", 32, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.38f, 1f));
        RectTransform ttRect = titleTxt.GetComponent<RectTransform>();
        ttRect.anchorMin = Vector2.zero;
        ttRect.anchorMax = Vector2.one;
        ttRect.offsetMin = Vector2.zero;
        ttRect.offsetMax = Vector2.zero;

        Text msgTxt = CreateUIText("Text_ConfirmMessage", cpCard.transform, defaultBodyFont, "Are you sure you want to proceed?", 28, TextAnchor.MiddleCenter, new Color(0.96f, 0.98f, 1f, 1f));
        msgTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        msgTxt.verticalOverflow = VerticalWrapMode.Truncate;
        msgTxt.lineSpacing = 1.15f;
        RectTransform mtRect = msgTxt.GetComponent<RectTransform>();
        mtRect.anchoredPosition = new Vector2(0f, 15f);
        mtRect.sizeDelta = new Vector2(740f, 130f);

        Button btnCancel = CreateUIButton("Button_ConfirmCancel", cpCard.transform, defaultBodyFont, "CANCEL", new Vector2(-160f, -140f), new Vector2(280f, 88f), new Color(0.28f, 0.26f, 0.36f, 1f), 26);
        Button btnYes = CreateUIButton("Button_ConfirmYes", cpCard.transform, defaultBodyFont, "YES, PROCEED", new Vector2(160f, -140f), new Vector2(280f, 88f), new Color(0.68f, 0.24f, 0.26f, 1f), 26);"""

assert old_confirm in text, "old_confirm not found in BuildMainGameScene.cs"
text = text.replace(old_confirm, new_confirm, 1)
print("[OK] Confirmation modal updated to defaultBodyFont 26-32pt!")

with open(bmgs_path, "w", encoding="utf-8") as f:
    f.write(text)

print("[SUCCESS] Settings and Confirmation modals updated successfully!")
