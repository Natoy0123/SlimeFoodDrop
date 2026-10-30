path = r'C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs'
with open(path, 'r', encoding='utf-8') as f:
    text = f.read()

# 1. Countdown subtitle
old_cd = '''        Text cdSubTxt = CreateUIText("Text_CountdownSubtitle", cdPanel.transform, font, "GET READY TO SLIDE...", 38, TextAnchor.MiddleCenter, Color.white);
        cdSubTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -110f);
        cdSubTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(760f, 70f);'''

new_cd = '''        Text cdSubTxt = CreateUIText("Text_CountdownSubtitle", cdPanel.transform, font, "GET READY TO SLIDE...", 38, TextAnchor.MiddleCenter, Color.white);
        cdSubTxt.resizeTextForBestFit = true;
        cdSubTxt.resizeTextMinSize = 28;
        cdSubTxt.resizeTextMaxSize = 38;
        cdSubTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -110f);
        cdSubTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(840f, 70f);'''

# 2. Vibration title font -> defaultBodyFont
old_vib = 'Text vibTitleLbl = CreateUIText("Label_VibrationTitle", audioBox.transform, font, "POISON HAPTICS", 25, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));'
new_vib = 'Text vibTitleLbl = CreateUIText("Label_VibrationTitle", audioBox.transform, defaultBodyFont, "POISON HAPTICS", 25, TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.78f, 1f));'

# 3. Guidebook title
old_gbt = '''        Text gbTitle = CreateUIText("Text_GuidebookTitle", gbCard.transform, font, "SLIME ALMANAC & GUIDE", 36, TextAnchor.MiddleCenter, new Color(0.52f, 0.98f, 0.68f, 1f));
        gbTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-24f, 600f);
        gbTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(720f, 54f);'''

new_gbt = '''        Text gbTitle = CreateUIText("Text_GuidebookTitle", gbCard.transform, font, "SLIME ALMANAC & GUIDE", 36, TextAnchor.MiddleCenter, new Color(0.52f, 0.98f, 0.68f, 1f));
        gbTitle.resizeTextForBestFit = true;
        gbTitle.resizeTextMinSize = 28;
        gbTitle.resizeTextMaxSize = 36;
        gbTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-24f, 600f);
        gbTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(760f, 54f);'''

# 4. ModeDesc size and string
old_mdesc = '''        Text subTxt = CreateUIText("Text_ModeDesc", btnObj.transform, defaultBodyFont, subtitle, 25, TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.84f, 0.95f));
        subTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        subTxt.verticalOverflow = VerticalWrapMode.Truncate;
        subTxt.raycastTarget = false;
        RectTransform sRect = subTxt.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0.5f, 0.5f);
        sRect.anchorMax = new Vector2(0.5f, 0.5f);
        sRect.pivot = new Vector2(0.5f, 0.5f);
        sRect.anchoredPosition = new Vector2(22f, -26f);
        sRect.sizeDelta = new Vector2(size.x - 120f, 48f);'''

new_mdesc = '''        Text subTxt = CreateUIText("Text_ModeDesc", btnObj.transform, defaultBodyFont, subtitle, 25, TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.84f, 0.95f));
        subTxt.resizeTextForBestFit = true;
        subTxt.resizeTextMinSize = 20;
        subTxt.resizeTextMaxSize = 25;
        subTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        subTxt.verticalOverflow = VerticalWrapMode.Truncate;
        subTxt.raycastTarget = false;
        RectTransform sRect = subTxt.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0.5f, 0.5f);
        sRect.anchorMax = new Vector2(0.5f, 0.5f);
        sRect.pivot = new Vector2(0.5f, 0.5f);
        sRect.anchoredPosition = new Vector2(22f, -26f);
        sRect.sizeDelta = new Vector2(size.x - 80f, 54f);'''

old_hard_str = '"Frenzy Speed  •  High Hazard Rate  •  +15 Pts / Catch"'
new_hard_str = '"Frenzy Speed  •  High Hazards  •  +15 Pts / Catch"'

for o, n in [(old_cd, new_cd), (old_vib, new_vib), (old_gbt, new_gbt), (old_mdesc, new_mdesc), (old_hard_str, new_hard_str)]:
    o_lf = o.replace('\r\n', '\n')
    n_lf = n.replace('\r\n', '\n')
    if o_lf in text:
        text = text.replace(o_lf, n_lf, 1)
        print('Applied replacement successfully')
    else:
        print('Warning: could not find:', o[:40])

with open(path, 'w', encoding='utf-8') as f:
    f.write(text)
print('SUCCESS!')
