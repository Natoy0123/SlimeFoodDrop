import os, re

# ==============================================================================
# 1. Update ThemeManager.cs
# ==============================================================================
tm_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\ThemeManager.cs"
with open(tm_path, "r", encoding="utf-8") as f:
    tm_code = f.read()

# Add floorPlatformSprite to EnvironmentTheme
old_theme_struct = """        public Color[] layerTints = new Color[4]
        {
            Color.white,
            Color.white,
            Color.white,
            Color.white
        };
        public AudioClip bgmClip;
    }"""

new_theme_struct = """        public Color[] layerTints = new Color[4]
        {
            Color.white,
            Color.white,
            Color.white,
            Color.white
        };
        public AudioClip bgmClip;
        public Sprite floorPlatformSprite;
    }"""

if old_theme_struct in tm_code:
    tm_code = tm_code.replace(old_theme_struct, new_theme_struct, 1)
    print("ThemeManager.cs: Added floorPlatformSprite to EnvironmentTheme")

# Add floor platform renderers to ThemeManager
old_tm_renderers = """    [Header("Renderer Sets for Cross-Fade")]
    public SpriteRenderer[] primaryRenderers = new SpriteRenderer[4];
    public SpriteRenderer[] transitionRenderers = new SpriteRenderer[4];"""

new_tm_renderers = """    [Header("Renderer Sets for Cross-Fade")]
    public SpriteRenderer[] primaryRenderers = new SpriteRenderer[4];
    public SpriteRenderer[] transitionRenderers = new SpriteRenderer[4];

    [Header("Floor Platform Renderers")]
    public SpriteRenderer floorPlatformRenderer;
    public SpriteRenderer transitionFloorPlatformRenderer;"""

if old_tm_renderers in tm_code:
    tm_code = tm_code.replace(old_tm_renderers, new_tm_renderers, 1)
    print("ThemeManager.cs: Added floorPlatformRenderer fields")

# Update ApplyThemeImmediate for platform
old_apply_imm = """        if (ResponsiveCamera.Instance != null)
        {
            ResponsiveCamera.Instance.RecalculateBounds();
        }"""

new_apply_imm = """        if (floorPlatformRenderer != null)
        {
            floorPlatformRenderer.sprite = theme.floorPlatformSprite;
            floorPlatformRenderer.color = Color.white;
            floorPlatformRenderer.gameObject.SetActive(theme.floorPlatformSprite != null);
        }
        if (transitionFloorPlatformRenderer != null)
        {
            transitionFloorPlatformRenderer.gameObject.SetActive(false);
        }

        if (ResponsiveCamera.Instance != null)
        {
            ResponsiveCamera.Instance.RecalculateBounds();
        }"""

if old_apply_imm in tm_code:
    tm_code = tm_code.replace(old_apply_imm, new_apply_imm, 1)
    print("ThemeManager.cs: Updated ApplyThemeImmediate for platform")

# Update CrossFadeLayersRoutine for platform
old_crossfade_setup = """        for (int i = 0; i < 4; i++)
        {
            startColors[i] = (primaryRenderers != null && i < primaryRenderers.Length && primaryRenderers[i] != null)"""

new_crossfade_setup = """        if (transitionFloorPlatformRenderer != null && nextTheme.floorPlatformSprite != null)
        {
            transitionFloorPlatformRenderer.gameObject.SetActive(true);
            transitionFloorPlatformRenderer.sprite = nextTheme.floorPlatformSprite;
            transitionFloorPlatformRenderer.color = new Color(1f, 1f, 1f, 0f);
            if (ResponsiveCamera.Instance != null)
            {
                Camera c = ResponsiveCamera.Instance.GetComponent<Camera>();
                float halfH = c != null ? c.orthographicSize : 5.4f;
                float halfW = halfH * (c != null ? c.aspect : 0.56f);
                ResponsiveCamera.Instance.PositionFloorPlatform(transitionFloorPlatformRenderer, halfW, halfH);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            startColors[i] = (primaryRenderers != null && i < primaryRenderers.Length && primaryRenderers[i] != null)"""

if old_crossfade_setup in tm_code:
    tm_code = tm_code.replace(old_crossfade_setup, new_crossfade_setup, 1)
    print("ThemeManager.cs: Updated CrossFade setup for platform")

# Update CrossFade loop for platform
old_crossfade_loop = """                if (primaryRenderers != null && i < primaryRenderers.Length && primaryRenderers[i] != null)
                {
                    // Fade out old foreground layers slightly after the back layer begins covering
                    Color sc = startColors[i];
                    sc.a = startColors[i].a * (1f - t * 0.85f);
                    primaryRenderers[i].color = sc;
                }
            }

            yield return null;"""

new_crossfade_loop = """                if (primaryRenderers != null && i < primaryRenderers.Length && primaryRenderers[i] != null)
                {
                    // Fade out old foreground layers slightly after the back layer begins covering
                    Color sc = startColors[i];
                    sc.a = startColors[i].a * (1f - t * 0.85f);
                    primaryRenderers[i].color = sc;
                }
            }

            if (transitionFloorPlatformRenderer != null && nextTheme.floorPlatformSprite != null)
            {
                transitionFloorPlatformRenderer.color = new Color(1f, 1f, 1f, t);
            }
            if (floorPlatformRenderer != null)
            {
                floorPlatformRenderer.color = new Color(1f, 1f, 1f, 1f - (t * 0.85f));
            }

            yield return null;"""

if old_crossfade_loop in tm_code:
    tm_code = tm_code.replace(old_crossfade_loop, new_crossfade_loop, 1)
    print("ThemeManager.cs: Updated CrossFade loop for platform")

# Update CrossFade finish for platform
old_crossfade_finish = """        transitionRoutine = null;
    }"""

new_crossfade_finish = """        if (floorPlatformRenderer != null)
        {
            floorPlatformRenderer.sprite = nextTheme.floorPlatformSprite;
            floorPlatformRenderer.color = Color.white;
            floorPlatformRenderer.gameObject.SetActive(nextTheme.floorPlatformSprite != null);
        }
        if (transitionFloorPlatformRenderer != null)
        {
            transitionFloorPlatformRenderer.gameObject.SetActive(false);
        }

        transitionRoutine = null;
    }"""

if old_crossfade_finish in tm_code:
    tm_code = tm_code.replace(old_crossfade_finish, new_crossfade_finish, 1)
    print("ThemeManager.cs: Updated CrossFade finish for platform")

with open(tm_path, "w", encoding="utf-8") as f:
    f.write(tm_code)

# ==============================================================================
# 2. Update ResponsiveCamera.cs
# ==============================================================================
rc_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\ResponsiveCamera.cs"
with open(rc_path, "r", encoding="utf-8") as f:
    rc_code = f.read()

# Add platform renderers & height fields
old_rc_fields = """    [Header("Scene References")]
    public SpriteRenderer[] backgroundLayers;
    public SpriteRenderer[] transitionBackgroundLayers;
    public float[] parallaxFactors = new float[] { 0.04f, 0.09f, 0.15f, 0.22f };
    public BoxCollider2D floorCollider;"""

new_rc_fields = """    [Header("Scene References")]
    public SpriteRenderer[] backgroundLayers;
    public SpriteRenderer[] transitionBackgroundLayers;
    public float[] parallaxFactors = new float[] { 0.04f, 0.09f, 0.15f, 0.22f };
    public BoxCollider2D floorCollider;

    [Header("Floor Platform Settings")]
    public SpriteRenderer floorPlatformRenderer;
    public SpriteRenderer transitionFloorPlatformRenderer;
    public float platformHeightWorld = 0.85f;"""

if old_rc_fields in rc_code:
    rc_code = rc_code.replace(old_rc_fields, new_rc_fields, 1)
    print("ResponsiveCamera.cs: Added floorPlatformRenderer fields")

# Update floorCollider positioning and platform positioning in RecalculateBounds
old_rc_bounds = """        if (floorCollider != null)
        {
            floorCollider.transform.position = new Vector3(0f, BottomBound - 0.65f, 0f);
            floorCollider.size = new Vector2((halfW * 2f) + 2.0f, 1.2f);
        }

        ApplySafeAreaToUI();
        ScaleAndTileLayerGroup(backgroundLayers, halfW, halfH);
        ScaleAndTileLayerGroup(transitionBackgroundLayers, halfW, halfH);"""

new_rc_bounds = """        if (floorCollider != null)
        {
            floorCollider.transform.position = new Vector3(0f, BottomBound + (platformHeightWorld * 0.5f), 0f);
            floorCollider.size = new Vector2((halfW * 2f) + 2.0f, platformHeightWorld);
        }

        PositionFloorPlatform(floorPlatformRenderer, halfW, halfH);
        PositionFloorPlatform(transitionFloorPlatformRenderer, halfW, halfH);

        ApplySafeAreaToUI();
        ScaleAndTileLayerGroup(backgroundLayers, halfW, halfH);
        ScaleAndTileLayerGroup(transitionBackgroundLayers, halfW, halfH);"""

if old_rc_bounds in rc_code:
    rc_code = rc_code.replace(old_rc_bounds, new_rc_bounds, 1)
    print("ResponsiveCamera.cs: Updated RecalculateBounds for floor platform")

# Add PositionFloorPlatform method
pos_method = """    public void PositionFloorPlatform(SpriteRenderer sr, float halfW, float halfH)
    {
        if (sr == null) return;
        float visibleW = halfW * 2f;
        float requiredW = visibleW * 1.15f;
        Vector2 spriteSize = sr.sprite != null ? sr.sprite.bounds.size : new Vector2(18.28f, 2.28f);
        float scaleX = requiredW / Mathf.Max(0.01f, spriteSize.x);
        float scaleY = platformHeightWorld / Mathf.Max(0.01f, spriteSize.y);
        sr.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        float anchoredY = BottomBound + (platformHeightWorld * 0.5f);
        sr.transform.position = new Vector3(0f, anchoredY, 0f);
    }
"""

if "public void PositionFloorPlatform" not in rc_code:
    idx = rc_code.find("public void ScaleAndPositionRenderer")
    rc_code = rc_code[:idx] + pos_method + "\n    " + rc_code[idx:]
    print("ResponsiveCamera.cs: Added PositionFloorPlatform method")

with open(rc_path, "w", encoding="utf-8") as f:
    f.write(rc_code)

# ==============================================================================
# 3. Update SlimeController.cs bottomOffset
# ==============================================================================
sc_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\SlimeController.cs"
with open(sc_path, "r", encoding="utf-8") as f:
    sc_code = f.read()

sc_code = sc_code.replace("public float bottomOffset = 0.58f;", "public float bottomOffset = 1.25f;")
print("SlimeController.cs: Updated bottomOffset to 1.25f")

with open(sc_path, "w", encoding="utf-8") as f:
    f.write(sc_code)

# ==============================================================================
# 4. Update GameManager.cs
# ==============================================================================
gm_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\GameManager.cs"
with open(gm_path, "r", encoding="utf-8") as f:
    gm_code = f.read()

# Make sure GameStartCountdownAndTutorialRoutine passes shouldShowFullTutorial correctly!
old_gm_tut = """        bool coreTutorialSeen = PlayerPrefs.GetInt(TutorialSeenPrefKey, 0) == 1;
        string diffKey = GetDiffTutorialPrefKey(currentDifficulty);
        bool diffTutorialSeen = PlayerPrefs.GetInt(diffKey, 0) == 1;
        bool shouldShowTutorial = forceShowTutorialNextStart || !coreTutorialSeen || !diffTutorialSeen;
        forceShowTutorialNextStart = false;

        if (shouldShowTutorial && UIManager.Instance != null)
        {
            currentState = GameState.Tutorial;
            if (UIManager.Instance.countdownPanel != null)
            {
                UIManager.Instance.countdownPanel.SetActive(false);
            }
            if (UIManager.Instance.environmentBannerPanel != null)
            {
                UIManager.Instance.environmentBannerPanel.SetActive(false);
            }

            yield return StartCoroutine(UIManager.Instance.RunInGameTutorialSequence(currentDifficulty, !coreTutorialSeen));"""

new_gm_tut = """        bool coreTutorialSeen = PlayerPrefs.GetInt(TutorialSeenPrefKey, 0) == 1;
        string diffKey = GetDiffTutorialPrefKey(currentDifficulty);
        bool diffTutorialSeen = PlayerPrefs.GetInt(diffKey, 0) == 1;
        bool shouldShowTutorial = forceShowTutorialNextStart || !coreTutorialSeen || !diffTutorialSeen;
        bool shouldShowFullTutorial = forceShowTutorialNextStart || !coreTutorialSeen;
        forceShowTutorialNextStart = false;

        if (shouldShowTutorial && UIManager.Instance != null)
        {
            currentState = GameState.Tutorial;
            if (UIManager.Instance.countdownPanel != null)
            {
                UIManager.Instance.countdownPanel.SetActive(false);
            }
            if (UIManager.Instance.environmentBannerPanel != null)
            {
                UIManager.Instance.environmentBannerPanel.SetActive(false);
            }

            yield return StartCoroutine(UIManager.Instance.RunInGameTutorialSequence(currentDifficulty, shouldShowFullTutorial));"""

if old_gm_tut in gm_code:
    gm_code = gm_code.replace(old_gm_tut, new_gm_tut, 1)
    print("GameManager.cs: Updated tutorial trigger to show full 5 steps on difficulty select or first play")

with open(gm_path, "w", encoding="utf-8") as f:
    f.write(gm_code)

# ==============================================================================
# 5. Update UIManager.cs
# ==============================================================================
ui_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\UIManager.cs"
with open(ui_path, "r", encoding="utf-8") as f:
    ui_code = f.read()

# Add activeTutorialRoutine field
if "private Coroutine activeTutorialRoutine;" not in ui_code:
    ui_code = ui_code.replace("private bool tutorialSkipRequested = false;", "private bool tutorialSkipRequested = false;\n    private Coroutine activeTutorialRoutine;")
    print("UIManager.cs: Added activeTutorialRoutine field")

# Update CancelTutorialAndCountdownOverlays to cancel activeTutorialRoutine
old_ui_cancel = """    public void CancelTutorialAndCountdownOverlays()
    {
        tutorialSkipRequested = true;
        tutorialTapReceived = true;
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }"""

new_ui_cancel = """    public void CancelTutorialAndCountdownOverlays()
    {
        tutorialSkipRequested = true;
        tutorialTapReceived = true;
        if (activeTutorialRoutine != null)
        {
            StopCoroutine(activeTutorialRoutine);
            activeTutorialRoutine = null;
        }
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }"""

if old_ui_cancel in ui_code:
    ui_code = ui_code.replace(old_ui_cancel, new_ui_cancel, 1)
    print("UIManager.cs: Updated CancelTutorialAndCountdownOverlays to cleanly stop activeTutorialRoutine")

# Update RunInGameTutorialSequence to manage activeTutorialRoutine and prevent duplicates
old_ui_run = """    public IEnumerator RunInGameTutorialSequence(GameManager.DifficultyLevel difficulty, bool includeFullTutorial)
    {
        if (tutorialPanel == null)
        {
            yield break;
        }"""

new_ui_run = """    public IEnumerator RunInGameTutorialSequence(GameManager.DifficultyLevel difficulty, bool includeFullTutorial)
    {
        if (tutorialPanel == null)
        {
            yield break;
        }

        if (activeTutorialRoutine != null)
        {
            StopCoroutine(activeTutorialRoutine);
            activeTutorialRoutine = null;
        }"""

if old_ui_run in ui_code:
    ui_code = ui_code.replace(old_ui_run, new_ui_run, 1)
    print("UIManager.cs: Prevented duplicate tutorial coroutines in RunInGameTutorialSequence")

with open(ui_path, "w", encoding="utf-8") as f:
    f.write(ui_code)

# ==============================================================================
# 6. Update HomeInteractiveSlime.cs
# ==============================================================================
his_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\HomeInteractiveSlime.cs"
with open(his_path, "r", encoding="utf-8") as f:
    his_code = f.read()

# Replace boredPhrases with clean conversational text without kaomoji/emojis
old_bored_regex = r"string\[\] boredPhrases = \{[^}]+\};"
new_bored = """string[] boredPhrases = {
                "...Anyone there? *yawn*",
                "I'm so boooored...",
                "Hello? Tap me!",
                "...Is my screen frozen?",
                "Feed me yummy food!",
                "Tap Play to start!"
            };"""
his_code = re.sub(old_bored_regex, new_bored, his_code, count=1)
print("HomeInteractiveSlime.cs: Cleaned boredPhrases text")

# Robust ShowBubble implementation
old_show_bubble = """    private void ShowBubble(string text, float duration)
    {
        if (reactionBubbleObj == null || reactionBubbleText == null)
            return;

        reactionBubbleText.text = text;

        RectTransform bblRect = reactionBubbleObj.GetComponent<RectTransform>();
        RectTransform txtRect = reactionBubbleText.GetComponent<RectTransform>();

        // Dynamically adapt bubble dimensions to the text content
        reactionBubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
        reactionBubbleText.verticalOverflow = VerticalWrapMode.Overflow;

        float prefW = reactionBubbleText.preferredWidth;
        float targetW;
        float targetH;

        if (prefW <= 380f)
        {
            // Fits cleanly on a single line
            targetW = Mathf.Clamp(prefW + 52f, 180f, 460f);
            if (txtRect != null) txtRect.sizeDelta = new Vector2(targetW - 32f, 44f);
            float prefH = reactionBubbleText.preferredHeight;
            targetH = Mathf.Clamp(prefH + 34f, 66f, 110f);
        }
        else
        {
            // Multiline wrap
            targetW = 440f;
            if (txtRect != null) txtRect.sizeDelta = new Vector2(targetW - 36f, 260f);
            float prefH = reactionBubbleText.preferredHeight;
            targetH = Mathf.Clamp(prefH + 36f, 84f, 220f);
        }

        if (bblRect != null)
        {
            bblRect.sizeDelta = new Vector2(targetW, targetH);
            if (txtRect != null)
            {
                txtRect.sizeDelta = new Vector2(targetW - 32f, targetH - 24f);
            }

            if (slimeRect != null)
            {
                float halfH = targetH * 0.5f;
                float bubbleY = slimeRect.anchoredPosition.y + 115f + halfH;
                float bubbleX = slimeRect.anchoredPosition.x;

                // Clamp X within the home screen bounds so bubble never overflows the phone edge
                float maxScreenX = 460f - (targetW * 0.5f);
                float minScreenX = -460f + (targetW * 0.5f);
                bubbleX = Mathf.Clamp(bubbleX, minScreenX, maxScreenX);

                bblRect.anchoredPosition = new Vector2(bubbleX, bubbleY);
            }
        }

        reactionBubbleObj.SetActive(true);
        reactionBubbleObj.transform.localScale = Vector3.one * 0.4f;

        if (activeBubbleRoutine != null) StopCoroutine(activeBubbleRoutine);
        activeBubbleRoutine = StartCoroutine(PopBubbleScaleRoutine(duration));
    }"""

new_show_bubble = """    private void ShowBubble(string text, float duration)
    {
        if (reactionBubbleObj == null || reactionBubbleText == null)
            return;

        reactionBubbleText.text = text;

        RectTransform bblRect = reactionBubbleObj.GetComponent<RectTransform>();
        RectTransform txtRect = reactionBubbleText.GetComponent<RectTransform>();

        // 1. Measure true unconstrained single-line width
        reactionBubbleText.horizontalOverflow = HorizontalWrapMode.Overflow;
        reactionBubbleText.verticalOverflow = VerticalWrapMode.Overflow;
        float unconstrainedW = reactionBubbleText.preferredWidth;

        float targetW;
        float targetH;

        if (unconstrainedW + 64f <= 480f)
        {
            // Fits cleanly on a single line!
            targetW = Mathf.Clamp(unconstrainedW + 64f, 200f, 500f);
            reactionBubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
            if (txtRect != null) txtRect.sizeDelta = new Vector2(targetW - 32f, 48f);
            float prefH = reactionBubbleText.preferredHeight;
            targetH = Mathf.Clamp(prefH + 34f, 72f, 100f);
        }
        else
        {
            // Needs multiline wrap
            targetW = 540f;
            reactionBubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
            float textWrapWidth = targetW - 44f;
            if (txtRect != null) txtRect.sizeDelta = new Vector2(textWrapWidth, 400f);
            float prefH = reactionBubbleText.preferredHeight;
            targetH = Mathf.Clamp(prefH + 42f, 96f, 240f);
        }

        if (bblRect != null)
        {
            bblRect.sizeDelta = new Vector2(targetW, targetH);
            if (txtRect != null)
            {
                txtRect.sizeDelta = new Vector2(targetW - 36f, targetH - 24f);
            }

            if (slimeRect != null)
            {
                float halfH = targetH * 0.5f;
                float bubbleY = slimeRect.anchoredPosition.y + 115f + halfH;
                float bubbleX = slimeRect.anchoredPosition.x;

                // Clamp X within the home screen bounds so bubble never overflows the phone edge
                float maxScreenX = 490f - (targetW * 0.5f);
                float minScreenX = -490f + (targetW * 0.5f);
                bubbleX = Mathf.Clamp(bubbleX, minScreenX, maxScreenX);

                bblRect.anchoredPosition = new Vector2(bubbleX, bubbleY);
            }
        }

        reactionBubbleObj.SetActive(true);
        reactionBubbleObj.transform.localScale = Vector3.one * 0.4f;

        if (activeBubbleRoutine != null) StopCoroutine(activeBubbleRoutine);
        activeBubbleRoutine = StartCoroutine(PopBubbleScaleRoutine(duration));
    }"""

if old_show_bubble in his_code:
    his_code = his_code.replace(old_show_bubble, new_show_bubble, 1)
    print("HomeInteractiveSlime.cs: Updated ShowBubble with dynamic sizing & edge clamping")
else:
    print("Warning: old_show_bubble not found in HomeInteractiveSlime.cs")

with open(his_path, "w", encoding="utf-8") as f:
    f.write(his_code)

# ==============================================================================
# 7. Update PixarSplashCinematic.cs
# ==============================================================================
psc_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\PixarSplashCinematic.cs"
with open(psc_path, "r", encoding="utf-8") as f:
    psc_code = f.read()

old_psc_bubble = """    private void ShowSpeechBubble(string text, Vector2 localPos)
    {
        if (speechBubbleGroup == null) return;
        speechBubbleGroup.SetActive(true);
        if (speechBubbleText != null)
        {
            speechBubbleText.text = text;
            speechBubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
            speechBubbleText.verticalOverflow = VerticalWrapMode.Overflow;

            float prefW = speechBubbleText.preferredWidth;
            float targetW;
            float targetH;

            if (prefW <= 480f)
            {
                targetW = Mathf.Clamp(prefW + 54f, 220f, 540f);
                float prefH = speechBubbleText.preferredHeight;
                targetH = Mathf.Clamp(prefH + 34f, 62f, 96f);
            }
            else
            {
                targetW = 540f;
                RectTransform tRect = speechBubbleText.GetComponent<RectTransform>();
                if (tRect != null) tRect.sizeDelta = new Vector2(500f, 200f);
                float prefH = speechBubbleText.preferredHeight;
                targetH = Mathf.Clamp(prefH + 36f, 84f, 180f);
            }

            if (speechBubbleRect != null)
            {
                speechBubbleRect.sizeDelta = new Vector2(targetW, targetH);
                RectTransform tRect = speechBubbleText.GetComponent<RectTransform>();
                if (tRect != null)
                {
                    tRect.sizeDelta = new Vector2(targetW - 32f, targetH - 20f);
                }
            }
        }
        if (speechBubbleRect != null) speechBubbleRect.anchoredPosition = localPos;
    }"""

new_psc_bubble = """    private void ShowSpeechBubble(string text, Vector2 localPos)
    {
        if (speechBubbleGroup == null) return;
        speechBubbleGroup.SetActive(true);
        if (speechBubbleText != null)
        {
            speechBubbleText.text = text;
            speechBubbleText.horizontalOverflow = HorizontalWrapMode.Overflow;
            speechBubbleText.verticalOverflow = VerticalWrapMode.Overflow;
            float unconstrainedW = speechBubbleText.preferredWidth;

            float targetW;
            float targetH;

            if (unconstrainedW + 64f <= 480f)
            {
                targetW = Mathf.Clamp(unconstrainedW + 64f, 220f, 520f);
                speechBubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
                if (speechBubbleRect != null) speechBubbleRect.sizeDelta = new Vector2(targetW, 100f);
                RectTransform tRect = speechBubbleText.GetComponent<RectTransform>();
                if (tRect != null) tRect.sizeDelta = new Vector2(targetW - 36f, 48f);
                float prefH = speechBubbleText.preferredHeight;
                targetH = Mathf.Clamp(prefH + 34f, 72f, 100f);
            }
            else
            {
                targetW = 560f;
                speechBubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
                if (speechBubbleRect != null) speechBubbleRect.sizeDelta = new Vector2(targetW, 100f);
                RectTransform tRect = speechBubbleText.GetComponent<RectTransform>();
                if (tRect != null) tRect.sizeDelta = new Vector2(targetW - 40f, 400f);
                float prefH = speechBubbleText.preferredHeight;
                targetH = Mathf.Clamp(prefH + 42f, 96f, 240f);
            }

            if (speechBubbleRect != null)
            {
                speechBubbleRect.sizeDelta = new Vector2(targetW, targetH);
                RectTransform tRect = speechBubbleText.GetComponent<RectTransform>();
                if (tRect != null)
                {
                    tRect.sizeDelta = new Vector2(targetW - 32f, targetH - 20f);
                }
            }
        }
        if (speechBubbleRect != null) speechBubbleRect.anchoredPosition = localPos;
    }"""

if old_psc_bubble in psc_code:
    psc_code = psc_code.replace(old_psc_bubble, new_psc_bubble, 1)
    print("PixarSplashCinematic.cs: Updated ShowSpeechBubble dynamic multiline calculation")
else:
    print("Warning: old_psc_bubble not found in PixarSplashCinematic.cs")

with open(psc_path, "w", encoding="utf-8") as f:
    f.write(psc_code)

print("All C# logic files successfully updated.")
