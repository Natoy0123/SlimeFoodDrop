using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null) _instance = FindFirstObjectByType<UIManager>();
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Splash Screen References")]
    public GameObject splashPanel;
    public CanvasGroup splashCanvasGroup;
    public RectTransform splashLogoGroup;
    public RectTransform splashProgressBarFillRect;
    public Image splashProgressFill;
    public Text splashPercentText;
    public Text splashStatusText;
    public PixarSplashCinematic pixarSplashCinematic;

    [Header("Home Screen (Title + Play, Guidebook, Settings & Exit) References")]
    public GameObject homePanel;
    public Text textHomeBestScore;
    public Button btnHomePlay;
    public Button btnHomeGuidebook;
    public Button btnHomeSettings;
    public Button btnHomeQuit;
    public HomeInteractiveSlime homeInteractiveSlime;

    [Header("Difficulty Selection Screen References")]
    public GameObject difficultyPanel;
    public Button btnEasy;
    public Button btnMedium;
    public Button btnHard;
    public Button btnDifficultyBack;

    [Header("Gameplay HUD References")]
    public GameObject topBarHud;
    public Image[] heartIcons;
    public Sprite heartFull;
    public Sprite heartEmpty;
    public Text textScore;
    public Text textHighScore;
    public Text textDifficultyBadge;
    public Text textThemeBadge;
    public Text textMagnetTimer;
    public Text textThemeToast;
    public Button btnHudSettings;
    public Text textFrenzyStreak;
    public Font arcadeFontRef;
    public Font retroBodyFontRef;

    [Header("Power-Up Duration HUD Bars")]
    public GameObject powerUpBarsContainer;
    public GameObject magnetBarRow;
    public RectTransform magnetBarFillRect;
    public Text textMagnetBarLabel;
    public GameObject shieldBarRow;
    public RectTransform shieldBarFillRect;
    public Text textShieldBarLabel;
    public GameObject timeSlowBarRow;
    public RectTransform timeSlowBarFillRect;
    public Text textTimeSlowBarLabel;

    [Header("Environment Transition Banner References")]
    public GameObject environmentBannerPanel;
    public CanvasGroup environmentBannerCanvasGroup;
    public Text textEnvStageBadge;
    public Text textEnvTitle;
    public Text textEnvSubtitle;

    [Header("1-Life Danger Indicator References")]
    public GameObject lowLifeWarningRoot;
    public Image[] lowLifeBorderEdges;
    public Text textLowLifeBanner;

    [Header("3-Second Countdown Overlay References")]
    public GameObject countdownPanel;
    public Text textCountdownNumber;
    public Text textCountdownSubtitle;

    [Header("Bottom-Screen Slime Tutor Dialogue References")]
    public GameObject tutorialPanel;
    public Image imgTutorialSlimePortrait;
    public Text textTutorialSpeakerName;
    public Text textTutorialStepBadge;
    public Text textTutorialTitle;
    public Text textTutorialBody;
    public Text textTutorialTapPrompt;
    public Image imgTutorialIcon1;
    public Image imgTutorialIcon2;
    public Image imgTutorialIcon3;
    public Image imgTutorialIcon4;
    public Button btnTutorialTapAnywhere;
    public Button btnTutorialSkip;
    public Sprite tutorialFoodSprite;
    public Sprite tutorialPoisonSprite;
    public Sprite tutorialHeartSprite;
    public Sprite tutorialMagnetSprite;
    public Sprite tutorialShieldSprite;
    public Sprite tutorialTimeSlowSprite;

    [Header("Slime Guidebook (Compendium / Almanac) Modal References")]
    public GameObject guidebookPanel;
    public Button[] btnGuidebookTabs;
    public GameObject[] guidebookTabPages;
    public Button btnGuidebookClose;

    [Header("Settings Modal References")]
    public GameObject settingsPanel;
    public RectTransform settingsCardRect;
    public Text textSettingsTitle;
    public RectTransform settingsCloseBtnRect;
    public RectTransform settingsAudioBoxRect;
    public RectTransform settingsResumeBtnRect;
    public Text textSettingsResumeLabel;
    public GameObject inGameSettingsActionsGroup;
    public Text textBgmVolumeValue;
    public Button btnBgmMinus;
    public Button btnBgmPlus;
    public Text textSfxVolumeValue;
    public Button btnSfxMinus;
    public Button btnSfxPlus;
    public Button btnToggleVibrate;
    public Text textVibrateLabel;
    public Button btnSettingsGuidebook;
    public Button btnSettingsResume;
    public Button btnSettingsClose;
    public Button btnSettingsRestart;
    public Button btnSettingsHome;
    public Button btnSettingsReplayTutorial;
    public Button btnSettingsQuit;

    [Header("Game Over Modal")]
    public GameObject gameOverPanel;
    public Text textFinalScore;
    public Text textBestScore;
    public Button btnRestart;
    public Button btnChangeDifficulty;
    public Button btnGameOverHome;
    public Button btnQuit;

    [Header("Confirmation Modal (Exit & Restart Guard)")]
    public GameObject confirmationModal;
    public Text textConfirmTitle;
    public Text textConfirmMessage;
    public Button btnConfirmYes;
    public Button btnConfirmCancel;
    private Action onConfirmAction;

    [Header("Almanac Item Detail Modal")]
    public GameObject almanacDetailModal;
    public Image imgDetailArtwork;
    public Text textDetailCategoryBadge;
    public Text textDetailTitle;
    public Text textDetailStats;
    public Text textDetailDescription;
    public Text textDetailTips;
    public Button btnDetailClose;

    private Coroutine scorePulseRoutine;
    private Coroutine toastRoutine;
    private Coroutine envBannerRoutine;
    private Coroutine splashRoutine;
    private Coroutine lowLifePulseRoutine;
    private int lastDisplayedLives = 3;
    private bool tutorialTapReceived = false;
    private bool tutorialSkipRequested = false;
    private Coroutine activeTutorialRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Home Screen Buttons
        if (btnHomePlay != null)
        {
            btnHomePlay.onClick.RemoveAllListeners();
            btnHomePlay.onClick.AddListener(() => GameManager.Instance?.OpenDifficultySelection());
        }
        if (btnHomeGuidebook != null)
        {
            btnHomeGuidebook.onClick.RemoveAllListeners();
            btnHomeGuidebook.onClick.AddListener(() => GameManager.Instance?.OpenGuidebook());
        }
        if (btnHomeSettings != null)
        {
            btnHomeSettings.onClick.RemoveAllListeners();
            btnHomeSettings.onClick.AddListener(() => GameManager.Instance?.OpenSettings());
        }
        if (btnHomeQuit != null)
        {
            btnHomeQuit.onClick.RemoveAllListeners();
            btnHomeQuit.onClick.AddListener(PromptQuitConfirmation);
        }

        // Difficulty Screen Buttons
        if (btnEasy != null)
        {
            btnEasy.onClick.RemoveAllListeners();
            btnEasy.onClick.AddListener(() => GameManager.Instance?.SelectEasyAndStart());
        }
        if (btnMedium != null)
        {
            btnMedium.onClick.RemoveAllListeners();
            btnMedium.onClick.AddListener(() => GameManager.Instance?.SelectMediumAndStart());
        }
        if (btnHard != null)
        {
            btnHard.onClick.RemoveAllListeners();
            btnHard.onClick.AddListener(() => GameManager.Instance?.SelectHardAndStart());
        }
        if (btnDifficultyBack != null)
        {
            btnDifficultyBack.onClick.RemoveAllListeners();
            btnDifficultyBack.onClick.AddListener(() => GameManager.Instance?.OpenHomeScreen());
        }

        // HUD Settings Button
        if (btnHudSettings != null)
        {
            btnHudSettings.onClick.RemoveAllListeners();
            btnHudSettings.onClick.AddListener(() => GameManager.Instance?.OpenSettings());
        }

        // Tutorial Tap Anywhere & Skip Buttons
        if (btnTutorialTapAnywhere != null)
        {
            btnTutorialTapAnywhere.onClick.RemoveAllListeners();
            btnTutorialTapAnywhere.onClick.AddListener(OnTutorialScreenTapped);
        }
        if (btnTutorialSkip != null)
        {
            btnTutorialSkip.onClick.RemoveAllListeners();
            btnTutorialSkip.onClick.AddListener(OnTutorialSkipClicked);
        }

        // Guidebook Buttons
        if (btnGuidebookClose != null)
        {
            btnGuidebookClose.onClick.RemoveAllListeners();
            btnGuidebookClose.onClick.AddListener(() => GameManager.Instance?.CloseGuidebook());
        }
        if (btnGuidebookTabs != null)
        {
            for (int i = 0; i < btnGuidebookTabs.Length; i++)
            {
                int tabIdx = i;
                if (btnGuidebookTabs[i] != null)
                {
                    btnGuidebookTabs[i].onClick.RemoveAllListeners();
                    btnGuidebookTabs[i].onClick.AddListener(() => SelectGuidebookTab(tabIdx));
                }
            }
        }

        // Settings Modal Buttons
        if (btnBgmMinus != null)
        {
            btnBgmMinus.onClick.RemoveAllListeners();
            btnBgmMinus.onClick.AddListener(() => AdjustBgmVolume(-0.15f));
        }
        if (btnBgmPlus != null)
        {
            btnBgmPlus.onClick.RemoveAllListeners();
            btnBgmPlus.onClick.AddListener(() => AdjustBgmVolume(+0.15f));
        }
        if (btnSfxMinus != null)
        {
            btnSfxMinus.onClick.RemoveAllListeners();
            btnSfxMinus.onClick.AddListener(() => AdjustSfxVolume(-0.15f));
        }
        if (btnSfxPlus != null)
        {
            btnSfxPlus.onClick.RemoveAllListeners();
            btnSfxPlus.onClick.AddListener(() => AdjustSfxVolume(+0.15f));
        }
        if (btnToggleVibrate != null)
        {
            btnToggleVibrate.onClick.RemoveAllListeners();
            btnToggleVibrate.onClick.AddListener(ToggleVibrationSetting);
        }
        if (btnSettingsGuidebook != null)
        {
            btnSettingsGuidebook.onClick.RemoveAllListeners();
            btnSettingsGuidebook.onClick.AddListener(() => GameManager.Instance?.OpenGuidebook());
        }
        if (btnSettingsResume != null)
        {
            btnSettingsResume.onClick.RemoveAllListeners();
            btnSettingsResume.onClick.AddListener(() => GameManager.Instance?.CloseSettingsAndResume());
        }
        if (btnSettingsClose != null)
        {
            btnSettingsClose.onClick.RemoveAllListeners();
            btnSettingsClose.onClick.AddListener(() => GameManager.Instance?.CloseSettingsAndResume());
        }
        if (btnSettingsRestart != null)
        {
            btnSettingsRestart.onClick.RemoveAllListeners();
            btnSettingsRestart.onClick.AddListener(PromptRestartConfirmation);
        }
        if (btnSettingsHome != null)
        {
            btnSettingsHome.onClick.RemoveAllListeners();
            btnSettingsHome.onClick.AddListener(PromptHomeConfirmation);
        }
        if (btnSettingsReplayTutorial != null)
        {
            btnSettingsReplayTutorial.onClick.RemoveAllListeners();
            btnSettingsReplayTutorial.onClick.AddListener(() => GameManager.Instance?.ReplayTutorialFromSettings());
        }
        if (btnSettingsQuit != null)
        {
            btnSettingsQuit.onClick.RemoveAllListeners();
            btnSettingsQuit.onClick.AddListener(PromptQuitConfirmation);
        }

        // Game Over Modal Buttons
        if (btnRestart != null)
        {
            btnRestart.onClick.RemoveAllListeners();
            btnRestart.onClick.AddListener(PromptRestartConfirmation);
        }
        if (btnChangeDifficulty != null)
        {
            btnChangeDifficulty.onClick.RemoveAllListeners();
            btnChangeDifficulty.onClick.AddListener(() => GameManager.Instance?.OpenDifficultySelection());
        }
        if (btnGameOverHome != null)
        {
            btnGameOverHome.onClick.RemoveAllListeners();
            btnGameOverHome.onClick.AddListener(() => GameManager.Instance?.OpenHomeScreen());
        }
        if (btnQuit != null)
        {
            btnQuit.onClick.RemoveAllListeners();
            btnQuit.onClick.AddListener(PromptQuitConfirmation);
        }

        // Confirmation Modal Buttons
        if (btnConfirmCancel != null)
        {
            btnConfirmCancel.onClick.RemoveAllListeners();
            btnConfirmCancel.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClickSound();
                HideConfirmation();
            });
        }
        if (btnConfirmYes != null)
        {
            btnConfirmYes.onClick.RemoveAllListeners();
            btnConfirmYes.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClickSound();
                Action act = onConfirmAction;
                HideConfirmation();
                act?.Invoke();
            });
        }

        // Almanac Item Detail Close Button
        if (btnDetailClose != null)
        {
            btnDetailClose.onClick.RemoveAllListeners();
            btnDetailClose.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClickSound();
                HideAlmanacItemDetail();
            });
        }

        // Wire all Almanac Card Triggers so tapping any card ALWAYS opens its details dialog at runtime
        AlmanacCardTrigger[] almanacTriggers = FindObjectsByType<AlmanacCardTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < almanacTriggers.Length; i++)
        {
            AlmanacCardTrigger trig = almanacTriggers[i];
            Button b = trig.GetComponent<Button>();
            if (b != null)
            {
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(trig.TriggerModal);
            }
        }
    }

    private IEnumerator FadeInCanvasGroup(CanvasGroup cg, float duration)
    {
        if (cg == null) yield break;
        cg.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    public void UpdateFrenzyStreakHUD(int streak, int target)
    {
        if (textFrenzyStreak == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsFoodRushActive)
        {
            textFrenzyStreak.text = $"RUSH: {GameManager.Instance.foodRushTimeRemaining:F1}s";
            textFrenzyStreak.color = new Color(1.0f, 0.86f, 0.25f, 1f);
            textFrenzyStreak.gameObject.SetActive(true);
            return;
        }

        if (streak <= 0)
        {
            textFrenzyStreak.text = $"FRENZY: x0/{target}";
            textFrenzyStreak.color = new Color(0.70f, 0.85f, 0.95f, 0.80f);
        }
        else
        {
            textFrenzyStreak.text = $"FRENZY: x{streak}/{target}";
            textFrenzyStreak.color = GetStreakColor(streak);
        }
        textFrenzyStreak.gameObject.SetActive(true);
    }

    public Color GetStreakColor(int streak)
    {
        if (streak >= 15) return new Color(1.0f, 0.20f, 0.65f, 1f); // Blazing Magenta
        if (streak >= 10) return new Color(1.0f, 0.50f, 0.15f, 1f); // Fiery Neon Orange
        if (streak >= 7)  return new Color(1.0f, 0.86f, 0.22f, 1f); // Radiant Amber Gold
        if (streak >= 4)  return new Color(0.28f, 0.94f, 1.0f, 1f);  // Electric Cyan
        return new Color(0.48f, 1.0f, 0.56f, 1f);                  // Fresh Mint Green
    }

    public void SpawnFloatingStreakIndicator(int streakCount, Vector3 worldPos)
    {
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null && topBarHud != null) rootCanvas = topBarHud.GetComponentInParent<Canvas>();
        if (rootCanvas == null) return;

        Camera uiCam = (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;
        if (uiCam == null) uiCam = Camera.main;

        GameObject floatObj = new GameObject($"FloatStreak_x{streakCount}", typeof(RectTransform));
        floatObj.transform.SetParent(rootCanvas.transform, false);
        floatObj.transform.SetAsLastSibling();
        RectTransform rt = floatObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCam, worldPos);
        RectTransform canvasRt = rootCanvas.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPoint, uiCam, out Vector2 localPoint))
        {
            rt.anchoredPosition = localPoint + new Vector2(0f, 75f);
        }
        else
        {
            rt.anchoredPosition = new Vector2(0f, -280f);
        }
        rt.sizeDelta = new Vector2(380f, 90f);

        Text txt = floatObj.AddComponent<Text>();
        Font chosenFont = retroBodyFontRef != null ? retroBodyFontRef : (arcadeFontRef != null ? arcadeFontRef : (textScore != null ? textScore.font : null));
        if (chosenFont == null) chosenFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.font = chosenFont;
        txt.text = (streakCount >= 15) ? $"x{streakCount} FRENZY!" : $"x{streakCount}";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;

        Color streakColor = GetStreakColor(streakCount);
        txt.color = streakColor;
        txt.fontSize = (streakCount >= 15 ? 48 : (streakCount >= 10 ? 42 : (streakCount >= 7 ? 38 : (streakCount >= 4 ? 34 : 30))));

        Outline ol = floatObj.AddComponent<Outline>();
        ol.effectColor = new Color(0.04f, 0.02f, 0.08f, 0.98f);
        ol.effectDistance = new Vector2(3f, -3f);

        CanvasGroup cg = floatObj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        if (Application.isPlaying)
        {
            StartCoroutine(AnimateFloatingIndicator(rt, cg, floatObj));
        }
    }

    public void SpawnFloatingRushBonus(Vector3 worldPos, int points)
    {
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null && topBarHud != null) rootCanvas = topBarHud.GetComponentInParent<Canvas>();
        if (rootCanvas == null) return;

        Camera uiCam = (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;
        if (uiCam == null) uiCam = Camera.main;

        GameObject floatObj = new GameObject("Float_RushBonus", typeof(RectTransform));
        floatObj.transform.SetParent(rootCanvas.transform, false);
        floatObj.transform.SetAsLastSibling();
        RectTransform rt = floatObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCam, worldPos);
        RectTransform canvasRt = rootCanvas.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPoint, uiCam, out Vector2 localPoint))
        {
            rt.anchoredPosition = localPoint + new Vector2(0f, 75f);
        }
        else
        {
            rt.anchoredPosition = new Vector2(0f, -280f);
        }
        rt.sizeDelta = new Vector2(380f, 90f);

        Text txt = floatObj.AddComponent<Text>();
        Font chosenFont = retroBodyFontRef != null ? retroBodyFontRef : (arcadeFontRef != null ? arcadeFontRef : (textScore != null ? textScore.font : null));
        if (chosenFont == null) chosenFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.font = chosenFont;
        txt.text = $"+{points} RUSH!";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(1.0f, 0.88f, 0.28f, 1f);
        txt.fontSize = 40;

        Outline ol = floatObj.AddComponent<Outline>();
        ol.effectColor = new Color(0.12f, 0.08f, 0.02f, 0.95f);
        ol.effectDistance = new Vector2(3f, -3f);

        CanvasGroup cg = floatObj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        if (Application.isPlaying)
        {
            StartCoroutine(AnimateFloatingIndicator(rt, cg, floatObj));
        }
    }

    public void SpawnStreakBrokenIndicator()
    {
        if (SlimeController.Instance == null) return;
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null && topBarHud != null) rootCanvas = topBarHud.GetComponentInParent<Canvas>();
        if (rootCanvas == null) return;

        Camera uiCam = (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? rootCanvas.worldCamera : null;
        if (uiCam == null) uiCam = Camera.main;

        GameObject floatObj = new GameObject("Float_StreakLost", typeof(RectTransform));
        floatObj.transform.SetParent(rootCanvas.transform, false);
        floatObj.transform.SetAsLastSibling();
        RectTransform rt = floatObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCam, SlimeController.Instance.transform.position);
        RectTransform canvasRt = rootCanvas.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPoint, uiCam, out Vector2 localPoint))
        {
            rt.anchoredPosition = localPoint + new Vector2(0f, 95f);
        }
        else
        {
            rt.anchoredPosition = new Vector2(0f, -320f);
        }
        rt.sizeDelta = new Vector2(380f, 70f);

        Text txt = floatObj.AddComponent<Text>();
        Font chosenFont = retroBodyFontRef != null ? retroBodyFontRef : (arcadeFontRef != null ? arcadeFontRef : (textScore != null ? textScore.font : null));
        if (chosenFont == null) chosenFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.font = chosenFont;
        txt.text = "STREAK LOST!";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(0.95f, 0.38f, 0.38f, 0.95f);
        txt.fontSize = 28;

        Outline ol = floatObj.AddComponent<Outline>();
        ol.effectColor = new Color(0.10f, 0.04f, 0.04f, 0.95f);
        ol.effectDistance = new Vector2(3f, -3f);

        CanvasGroup cg = floatObj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        if (Application.isPlaying)
        {
            StartCoroutine(AnimateFloatingIndicator(rt, cg, floatObj));
        }
    }

    private IEnumerator AnimateFloatingIndicator(RectTransform rt, CanvasGroup cg, GameObject targetObj)
    {
        float dur = 0.70f;
        float elapsed = 0f;
        Vector2 startPos = rt.anchoredPosition;
        Vector2 targetPos = startPos + new Vector2(0f, 100f);

        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(elapsed / dur);

            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, u));
            float scale = u < 0.25f ? Mathf.Lerp(1.35f, 1.0f, u / 0.25f) : 1.0f;
            rt.localScale = new Vector3(scale, scale, 1f);

            if (u > 0.50f)
            {
                cg.alpha = Mathf.Lerp(1f, 0f, (u - 0.50f) / 0.50f);
            }
            else
            {
                cg.alpha = 1f;
            }

            yield return null;
        }

        if (targetObj != null) Destroy(targetObj);
    }

    public void ShowFoodRushBanner(float duration)
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
    }

    public void SetGameplayHUDVisible(bool visible)
    {
        if (topBarHud != null)
        {
            topBarHud.SetActive(visible);
        }
        if (powerUpBarsContainer != null && !visible)
        {
            powerUpBarsContainer.SetActive(false);
        }
        if (!visible && environmentBannerPanel != null)
        {
            environmentBannerPanel.SetActive(false);
        }
    }

    public void PlaySplashSequence(Action onComplete)
    {
        if (splashPanel == null)
        {
            onComplete?.Invoke();
            return;
        }

        if (homePanel != null) homePanel.SetActive(false);
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (guidebookPanel != null) guidebookPanel.SetActive(false);
        if (countdownPanel != null) countdownPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        splashPanel.SetActive(true);
        if (splashCanvasGroup != null) splashCanvasGroup.alpha = 1f;

        if (pixarSplashCinematic != null)
        {
            pixarSplashCinematic.StartCinematic(onComplete);
            return;
        }

        SetSplashProgressVisual(0.01f, "0%", "INITIALIZING ENGINE...");

        if (!Application.isPlaying)
        {
            onComplete?.Invoke();
            return;
        }

        if (splashRoutine != null) StopCoroutine(splashRoutine);
        splashRoutine = StartCoroutine(SplashRoutine(onComplete));
    }

    public void SetSplashProgress(float normalizedProgress)
    {
        int pct = Mathf.RoundToInt(Mathf.Clamp01(normalizedProgress) * 100f);
        SetSplashProgressVisual(normalizedProgress, $"{pct}%", "LOADING 4 PARALLAX BIOME THEMES...");
    }

    public void SetSplashProgressVisual(float normalizedProgress, string percentLabel, string statusLabel)
    {
        float clamped = Mathf.Clamp01(normalizedProgress);

        if (splashProgressBarFillRect != null)
        {
            splashProgressBarFillRect.anchorMin = new Vector2(0f, 0f);
            splashProgressBarFillRect.anchorMax = new Vector2(Mathf.Max(0.01f, clamped), 1f);
            splashProgressBarFillRect.offsetMin = Vector2.zero;
            splashProgressBarFillRect.offsetMax = Vector2.zero;
        }
        else if (splashProgressFill != null)
        {
            RectTransform rt = splashProgressFill.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(Mathf.Max(0.01f, clamped), 1f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }

        if (splashPercentText != null) splashPercentText.text = percentLabel;
        if (splashStatusText != null) splashStatusText.text = statusLabel;
    }

    private IEnumerator SplashRoutine(Action onComplete)
    {
        float[] stageTargets = { 0.18f, 0.44f, 0.76f, 0.94f, 1.00f };
        float[] stageDurations = { 0.35f, 0.48f, 0.55f, 0.38f, 0.24f };
        float[] stagePauses = { 0.08f, 0.12f, 0.10f, 0.14f, 0.18f };
        string[] stageLabels =
        {
            "INITIALIZING PIXEL SHADERS...",
            "LOADING 22 EDIBLE FOODS & 4 POWER-UPS...",
            "PREPARING 4 PARALLAX BIOME STAGES...",
            "SYNTHESIZING BGM & SOUNDTRACKS...",
            "READY TO DROP!"
        };

        float currentProgress = 0.01f;
        float totalTimer = 0f;

        for (int s = 0; s < stageTargets.Length; s++)
        {
            float startVal = currentProgress;
            float targetVal = stageTargets[s];
            float dur = stageDurations[s];
            float t = 0f;

            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                totalTimer += Time.unscaledDeltaTime;
                float u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / dur));
                currentProgress = Mathf.Lerp(startVal, targetVal, u);

                int pct = Mathf.RoundToInt(currentProgress * 100f);
                SetSplashProgressVisual(currentProgress, $"{pct}%", stageLabels[s]);

                if (splashLogoGroup != null)
                {
                    float bounce = 1f + Mathf.Sin(totalTimer * 5.8f) * 0.03f;
                    splashLogoGroup.localScale = new Vector3(bounce, 2f - bounce, 1f);
                }

                yield return null;
            }

            currentProgress = targetVal;
            int finalStagePct = Mathf.RoundToInt(currentProgress * 100f);
            SetSplashProgressVisual(currentProgress, $"{finalStagePct}%", stageLabels[s]);

            float pauseTimer = 0f;
            while (pauseTimer < stagePauses[s])
            {
                pauseTimer += Time.unscaledDeltaTime;
                totalTimer += Time.unscaledDeltaTime;
                if (splashLogoGroup != null)
                {
                    float bounce = 1f + Mathf.Sin(totalTimer * 5.8f) * 0.03f;
                    splashLogoGroup.localScale = new Vector3(bounce, 2f - bounce, 1f);
                }
                yield return null;
            }
        }

        float fadeDur = 0.32f;
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeDur)
        {
            fadeElapsed += Time.unscaledDeltaTime;
            if (splashCanvasGroup != null)
            {
                splashCanvasGroup.alpha = 1f - Mathf.Clamp01(fadeElapsed / fadeDur);
            }
            yield return null;
        }

        if (splashPanel != null) splashPanel.SetActive(false);
        splashRoutine = null;
        onComplete?.Invoke();
    }

    public void ShowHomeScreen(int bestScore = 0)
    {
        if (splashPanel != null) splashPanel.SetActive(false);
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (guidebookPanel != null) guidebookPanel.SetActive(false);
        if (countdownPanel != null) countdownPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        if (textHomeBestScore != null)
        {
            textHomeBestScore.text = $"BEST SCORE: {bestScore}";
        }

        if (homePanel != null)
        {
            homePanel.SetActive(true);
            CanvasGroup homeCg = homePanel.GetComponent<CanvasGroup>();
            if (homeCg == null) homeCg = homePanel.AddComponent<CanvasGroup>();
            if (Application.isPlaying)
            {
                StartCoroutine(FadeInCanvasGroup(homeCg, 0.40f));
            }
            else
            {
                homeCg.alpha = 1f;
            }
            homeInteractiveSlime?.ResetToRestingState();
            if (Application.isPlaying)
            {
                StartCoroutine(PopInTransform(homePanel.transform));
            }
        }
    }

    public void ShowDifficultyScreen(GameManager.DifficultyLevel selected)
    {
        if (splashPanel != null) splashPanel.SetActive(false);
        if (homePanel != null) homePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (guidebookPanel != null) guidebookPanel.SetActive(false);

        if (difficultyPanel != null)
        {
            difficultyPanel.SetActive(true);
            if (Application.isPlaying)
            {
                StartCoroutine(PopInTransform(difficultyPanel.transform));
            }
        }
        UpdateDifficultyBadge(selected);
    }

    public void HideDifficultyScreen()
    {
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
    }

    public void HideSplashAndHomeScreens()
    {
        if (splashRoutine != null)
        {
            StopCoroutine(splashRoutine);
            splashRoutine = null;
        }
        if (splashPanel != null) splashPanel.SetActive(false);
        if (homePanel != null) homePanel.SetActive(false);
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
    }

    public void CancelTutorialAndCountdownOverlays()
    {
        tutorialSkipRequested = true;
        tutorialTapReceived = true;
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (countdownPanel != null) countdownPanel.SetActive(false);
    }

    // =========================================================================
    // 3-SECOND COUNTDOWN SEQUENCE (3 -> 2 -> 1 -> GO!)
    // =========================================================================
    public IEnumerator RunThreeSecondCountdown()
    {
        if (countdownPanel == null)
        {
            yield break;
        }

        // Guarantee Tutorial is hidden while the 3-second countdown runs!
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        countdownPanel.SetActive(true);
        string[] steps = { "3", "2", "1", "GO!" };
        string[] subtitles =
        {
            "GET READY TO CATCH!",
            "WATCH OUT FOR POISON!",
            "EYES ON THE SKY...",
            "CATCH THE FOOD!"
        };
        Color[] stepColors =
        {
            new Color(1f, 0.90f, 0.35f, 1f),
            new Color(1f, 0.72f, 0.25f, 1f),
            new Color(0.45f, 0.95f, 0.55f, 1f),
            new Color(0.35f, 0.95f, 1.0f, 1f)
        };

        for (int i = 0; i < steps.Length; i++)
        {
            if (textCountdownNumber != null)
            {
                textCountdownNumber.text = steps[i];
                textCountdownNumber.color = stepColors[i];
            }
            if (textCountdownSubtitle != null)
            {
                textCountdownSubtitle.text = subtitles[i];
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayCountdownTick(i == steps.Length - 1);
            }

            float duration = (i < 3) ? 0.92f : 0.45f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (textCountdownNumber != null)
                {
                    float scale = (t < 0.25f)
                        ? Mathf.Lerp(1.45f, 1.0f, t / 0.25f)
                        : Mathf.Lerp(1.0f, 0.88f, (t - 0.25f) / 0.75f);
                    textCountdownNumber.transform.localScale = new Vector3(scale, scale, 1f);
                }
                yield return null;
            }
        }

        if (textCountdownNumber != null)
        {
            textCountdownNumber.transform.localScale = Vector3.one;
        }
        countdownPanel.SetActive(false);
    }

    // =========================================================================
    // BOTTOM-SCREEN SLIME TUTOR DIALOGUE (5 LESSONS BY BLOOP THE SLIME)
    // =========================================================================
    public void OnTutorialScreenTapped()
    {
        tutorialTapReceived = true;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClickSound();
        }
    }

    public void OnTutorialSkipClicked()
    {
        tutorialSkipRequested = true;
        tutorialTapReceived = true;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClickSound();
        }
    }

    public IEnumerator RunInGameTutorialSequence()
    {
        GameManager.DifficultyLevel diff = GameManager.Instance != null
            ? GameManager.Instance.currentDifficulty
            : GameManager.DifficultyLevel.Medium;
        yield return StartCoroutine(RunInGameTutorialSequence(diff, true));
    }

    public void GetDifficultyTutorialContent(
        GameManager.DifficultyLevel difficulty,
        out string badge, out string title, out string body, out Sprite[] icons)
    {
        switch (difficulty)
        {
            case GameManager.DifficultyLevel.Easy:
                badge = "DIFFICULTY  •  EASY MODE";
                title = "EASY MODE: RELAXED DROP!";
                body = "\"Items fall 22% SLOWER with 25% more breathing room! Poison spawns less often, Power-Ups drop more frequently, and each food grants +10 Pts!\"";
                icons = new Sprite[] { tutorialFoodSprite, tutorialHeartSprite, tutorialShieldSprite, tutorialTimeSlowSprite };
                break;

            case GameManager.DifficultyLevel.Hard:
                badge = "DIFFICULTY  •  HARD MODE";
                title = "HARD MODE: FRENZY SPEED!";
                body = "\"Items plunge 28% FASTER and spawn 24% more rapidly! Poison hazards appear +9% more often—use Shields & Time-Slow wisely! (+15 Pts / food)\"";
                icons = new Sprite[] { tutorialPoisonSprite, tutorialShieldSprite, tutorialTimeSlowSprite, tutorialFoodSprite };
                break;

            default:
                badge = "DIFFICULTY  •  MEDIUM MODE";
                title = "MEDIUM MODE: CLASSIC ARCADE!";
                body = "\"Standard 100% arcade drop speed with balanced Poison rates (12%–26% across stages) and classic Power-Ups. Each food gives +10 Pts (+5 Combo Bonus)!\"";
                icons = new Sprite[] { tutorialFoodSprite, tutorialMagnetSprite, tutorialShieldSprite, tutorialPoisonSprite };
                break;
        }
    }

    public IEnumerator RunInGameTutorialSequence(GameManager.DifficultyLevel difficulty, bool includeFullTutorial = true)
    {
        if (tutorialPanel == null)
        {
            yield break;
        }

        // Guarantee Countdown is hidden while the Tutorial dialogue is open!
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }

        float prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        tutorialPanel.SetActive(true);
        tutorialSkipRequested = false;
        tutorialTapReceived = false;

        // Drain any lingering touch input from tapping the Difficulty button so the tutorial NEVER instantly skips!
        float drainTimer = 0f;
        while (drainTimer < 0.22f || Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            drainTimer += Time.unscaledDeltaTime;
            tutorialTapReceived = false;
            yield return null;
        }
        tutorialTapReceived = false;

        GetDifficultyTutorialContent(difficulty, out string diffBadge, out string diffTitle, out string diffBody, out Sprite[] diffIcons);

        string[] badges;
        string[] titles;
        string[] bodies;
        Sprite[][] stepIcons;

        if (includeFullTutorial)
        {
            badges = new string[]
            {
                diffBadge + "  (1/5)",
                "SLIME TUTOR  •  STEP 2/5",
                "SLIME TUTOR  •  STEP 3/5",
                "SLIME TUTOR  •  STEP 4/5",
                "SLIME TUTOR  •  STEP 5/5"
            };

            titles = new string[]
            {
                diffTitle,
                "CATCH FOODS FOR COMBOS (0 DMG ON MISS)!",
                "ONLY POISON REDUCES YOUR HEARTS!",
                "4 MAGIC POWER-UPS & DURATION BARS!",
                "STAGE THEMES, 1-LIFE ALERT & ALMANAC!"
            };

            bodies = new string[]
            {
                diffBody,
                "\"Drag or tap anywhere to help me glide! Catch falling FOODS to score points & build combos. Missing food costs 0 Hearts—it only resets your combo!\"",
                "\"Dodge Toxic Mushrooms, Potions, Goblets & Spikes! ONLY catching Poison costs -1 Heart & vibrates your phone. Let poison hit the floor safely!\"",
                "\"Catch 4 Power-Ups: HEART (+1 Life), MAGNET (6s pull), BARRIER SHIELD (12s / blocks 1 Poison), and TIME-SLOW (6s 50% slow-mo)! Watch their top timer bars!\"",
                "\"At 100, 250, 450, 700... 2700 pts we unlock new Biome Stages & Music! At 1 Heart the screen pulses RED. Open the ALMANAC anytime to inspect all items & stages!\""
            };

            stepIcons = new Sprite[][]
            {
                diffIcons,
                new Sprite[] { tutorialFoodSprite, tutorialHeartSprite, tutorialFoodSprite, tutorialMagnetSprite },
                new Sprite[] { tutorialPoisonSprite, tutorialPoisonSprite, tutorialShieldSprite, tutorialPoisonSprite },
                new Sprite[] { tutorialHeartSprite, tutorialMagnetSprite, tutorialShieldSprite, tutorialTimeSlowSprite },
                new Sprite[] { tutorialFoodSprite, tutorialPoisonSprite, tutorialShieldSprite, tutorialTimeSlowSprite }
            };
        }
        else
        {
            // Player already knows core rules -> Show the 1-step Difficulty Showcase for the selected difficulty!
            badges = new string[] { diffBadge };
            titles = new string[] { diffTitle };
            bodies = new string[] { diffBody };
            stepIcons = new Sprite[][] { diffIcons };
        }

        // Open the dialog container ONCE at the start of the tutorial (never reopen between steps!)
        StartCoroutine(PopInTransform(tutorialPanel.transform));

        const float charsPerSecond = 62f;

        for (int step = 0; step < titles.Length; step++)
        {
            if (tutorialSkipRequested) break;

            if (textTutorialSpeakerName != null) textTutorialSpeakerName.text = "BLOOP THE TUTOR";
            if (textTutorialStepBadge != null) textTutorialStepBadge.text = badges[step];
            if (textTutorialTitle != null) textTutorialTitle.text = titles[step];

            SetTutorialShowcaseIcons(stepIcons[step]);

            // Subtle bounce on the corner Slime portrait when swapping content in-place
            if (imgTutorialSlimePortrait != null)
            {
                StartCoroutine(PulseTransform(imgTutorialSlimePortrait.transform, 1.18f, 0.18f));
            }

            string fullBody = bodies[step] ?? "";
            int totalChars = fullBody.Length;
            int shownChars = 0;
            float charTimer = 0f;
            bool isTyping = totalChars > 0;

            if (textTutorialBody != null)
            {
                textTutorialBody.text = "";
            }
            if (textTutorialTapPrompt != null)
            {
                textTutorialTapPrompt.text = "[ TAP TO SKIP TYPING ]";
            }

            tutorialTapReceived = false;
            float stepTimer = 0f;
            float minInputDelay = 0.25f;

            while (!tutorialSkipRequested)
            {
                float dt = Time.unscaledDeltaTime;
                stepTimer += dt;

                // Gentle idle talk animation on the corner Slime portrait
                if (imgTutorialSlimePortrait != null)
                {
                    float speed = isTyping ? 14f : 6.5f;
                    float amp = isTyping ? 0.06f : 0.03f;
                    float bounce = Mathf.Sin(stepTimer * speed) * amp;
                    imgTutorialSlimePortrait.transform.localScale = new Vector3(1f - bounce, 1f + bounce, 1f);
                }

                // Check for screen tap
                bool screenTapped = tutorialTapReceived;
                if (!screenTapped && stepTimer >= minInputDelay)
                {
                    if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                    {
                        screenTapped = true;
                        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClickSound();
                    }
                }
                tutorialTapReceived = false;

                if (isTyping)
                {
                    if (screenTapped)
                    {
                        // First tap while typing: immediately reveal the full text of the current step!
                        shownChars = totalChars;
                        isTyping = false;
                        if (textTutorialBody != null) textTutorialBody.text = fullBody;
                        if (textTutorialTapPrompt != null)
                        {
                            textTutorialTapPrompt.text = (step < titles.Length - 1)
                                ? "[ TAP TO CONTINUE > ]"
                                : "[ TAP TO START! > ]";
                        }
                        minInputDelay = stepTimer + 0.28f;
                    }
                    else
                    {
                        charTimer += dt * charsPerSecond;
                        int nextChars = Mathf.Clamp(Mathf.FloorToInt(charTimer), 0, totalChars);
                        if (nextChars != shownChars)
                        {
                            shownChars = nextChars;
                            if (textTutorialBody != null)
                            {
                                textTutorialBody.text = fullBody.Substring(0, shownChars);
                            }
                        }
                        if (shownChars >= totalChars)
                        {
                            isTyping = false;
                            if (textTutorialBody != null) textTutorialBody.text = fullBody;
                            if (textTutorialTapPrompt != null)
                            {
                                textTutorialTapPrompt.text = (step < titles.Length - 1)
                                    ? "[ TAP TO CONTINUE > ]"
                                    : "[ TAP TO START! > ]";
                            }
                        }
                    }
                }
                else if (screenTapped)
                {
                    // Text is already fully shown -> advance to next step in-place (or finish tutorial)!
                    break;
                }

                yield return null;
            }
        }

        if (imgTutorialSlimePortrait != null)
        {
            imgTutorialSlimePortrait.transform.localScale = Vector3.one;
        }
        tutorialPanel.SetActive(false);
        Time.timeScale = prevTimeScale;
    }

    private void SetTutorialShowcaseIcons(Sprite[] icons)
    {
        Image[] slots = { imgTutorialIcon1, imgTutorialIcon2, imgTutorialIcon3, imgTutorialIcon4 };
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            Sprite sp = (icons != null && i < icons.Length && icons[i] != null) ? icons[i] : tutorialFoodSprite;
            if (sp != null)
            {
                slots[i].sprite = sp;
                slots[i].gameObject.SetActive(true);
            }
        }
    }

    // =========================================================================
    // SLIME GUIDEBOOK (COMPENDIUM / ALMANAC MODAL)
    // =========================================================================
    public void ShowGuidebookModal(int defaultTab = 0)
    {
        if (guidebookPanel == null) return;
        guidebookPanel.SetActive(true);
        SelectGuidebookTab(defaultTab);
        if (Application.isPlaying)
        {
            StartCoroutine(PopInTransform(guidebookPanel.transform));
        }
    }

    public void HideGuidebookModal()
    {
        if (guidebookPanel != null)
        {
            guidebookPanel.SetActive(false);
        }
    }

    public void SelectGuidebookTab(int tabIndex)
    {
        if (SoundManager.Instance != null && Application.isPlaying)
        {
            SoundManager.Instance.PlayButtonClickSound();
        }

        if (guidebookTabPages != null)
        {
            for (int i = 0; i < guidebookTabPages.Length; i++)
            {
                if (guidebookTabPages[i] != null)
                {
                    guidebookTabPages[i].SetActive(i == tabIndex);
                }
            }
        }

        if (btnGuidebookTabs != null)
        {
            Color activeBg = new Color(0.20f, 0.68f, 0.92f, 1f);
            Color inactiveBg = new Color(0.15f, 0.19f, 0.28f, 0.96f);
            Color activeOutline = new Color(1.0f, 0.88f, 0.36f, 1f);
            Color inactiveOutline = new Color(0.06f, 0.05f, 0.09f, 0.95f);

            for (int i = 0; i < btnGuidebookTabs.Length; i++)
            {
                Button tabBtn = btnGuidebookTabs[i];
                if (tabBtn == null) continue;
                bool isSelected = (i == tabIndex);
                Color targetBg = isSelected ? activeBg : inactiveBg;

                Image img = tabBtn.GetComponent<Image>();
                if (img != null)
                {
                    img.color = targetBg;
                }

                // Update UIButtonJuice baseColor so OnPointerUp / OnPointerExit never wipes out the active tab highlight!
                UIButtonJuice juice = tabBtn.GetComponent<UIButtonJuice>();
                if (juice != null)
                {
                    juice.SetBaseColor(targetBg);
                }

                Outline ol = tabBtn.GetComponent<Outline>();
                if (ol != null)
                {
                    ol.effectColor = isSelected ? activeOutline : inactiveOutline;
                }

                Transform lblTr = tabBtn.transform.Find("Text_Label");
                if (lblTr != null)
                {
                    Text lbl = lblTr.GetComponent<Text>();
                    if (lbl != null)
                    {
                        lbl.color = isSelected
                            ? new Color(1f, 0.98f, 0.82f, 1f)
                            : new Color(0.76f, 0.82f, 0.90f, 0.92f);
                    }
                }
            }
        }
    }

    // =========================================================================
    // SETTINGS MODAL (AUDIO, VIBRATION, GUIDEBOOK, TUTORIAL, RESTART, QUIT)
    // =========================================================================
    public void ShowSettingsModal(bool forceInGame = false)
    {
        if (settingsPanel == null) return;
        RefreshSettingsUI(forceInGame);
        settingsPanel.SetActive(true);
        if (Application.isPlaying)
        {
            StartCoroutine(PopInTransform(settingsPanel.transform));
        }
    }

    public void HideSettingsModal()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void RefreshSettingsUI(bool forceInGame = false)
    {
        float bgm = SoundManager.Instance != null ? SoundManager.Instance.userBgmVolume : 1f;
        float sfx = SoundManager.Instance != null ? SoundManager.Instance.userSfxVolume : 1f;
        bool vib = GameManager.Instance == null || GameManager.Instance.vibrationEnabled;
        bool isInGamePaused = forceInGame || (GameManager.Instance != null && GameManager.Instance.IsGameplaySessionPaused);

        // Context-aware visibility: Home -> Settings ONLY shows Audio & Vibration + Close/Back!
        // In-Game Pause -> Settings ALSO shows In-Game Session Actions (Restart, Almanac, Replay Tutorial, Home).
        if (inGameSettingsActionsGroup != null)
        {
            inGameSettingsActionsGroup.SetActive(isInGamePaused);
        }

        if (textSettingsTitle != null)
        {
            textSettingsTitle.text = isInGamePaused ? "GAME PAUSED" : "SETTINGS";
            RectTransform titleRt = textSettingsTitle.GetComponent<RectTransform>();
            if (titleRt != null)
            {
                titleRt.anchoredPosition = isInGamePaused ? new Vector2(-24f, 430f) : new Vector2(-24f, 255f);
            }
        }

        if (settingsCloseBtnRect != null)
        {
            settingsCloseBtnRect.anchoredPosition = isInGamePaused ? new Vector2(360f, 430f) : new Vector2(360f, 255f);
        }

        if (settingsCardRect != null)
        {
            settingsCardRect.sizeDelta = isInGamePaused ? new Vector2(880f, 1000f) : new Vector2(880f, 650f);
        }

        if (settingsAudioBoxRect != null)
        {
            settingsAudioBoxRect.anchoredPosition = isInGamePaused ? new Vector2(0f, 185f) : new Vector2(0f, 10f);
        }

        if (settingsResumeBtnRect != null)
        {
            settingsResumeBtnRect.anchoredPosition = isInGamePaused ? new Vector2(0f, -68f) : new Vector2(0f, -235f);
        }

        if (textSettingsResumeLabel != null)
        {
            textSettingsResumeLabel.text = isInGamePaused ? "RESUME GAME" : "DONE";
        }

        if (textBgmVolumeValue != null)
        {
            textBgmVolumeValue.text = $"{Mathf.RoundToInt(bgm * 100f)}%";
        }
        if (textSfxVolumeValue != null)
        {
            textSfxVolumeValue.text = $"{Mathf.RoundToInt(sfx * 100f)}%";
        }
        if (textVibrateLabel != null)
        {
            textVibrateLabel.text = vib ? "ON" : "OFF";
            textVibrateLabel.color = vib
                ? new Color(0.92f, 1f, 0.94f, 1f)
                : new Color(0.95f, 0.80f, 0.80f, 1f);
        }
        if (btnToggleVibrate != null)
        {
            Color vibBtnColor = vib
                ? new Color(0.20f, 0.60f, 0.34f, 1f)
                : new Color(0.42f, 0.24f, 0.26f, 1f);
            Image vImg = btnToggleVibrate.GetComponent<Image>();
            if (vImg != null) vImg.color = vibBtnColor;
            UIButtonJuice vJuice = btnToggleVibrate.GetComponent<UIButtonJuice>();
            if (vJuice != null) vJuice.SetBaseColor(vibBtnColor);
        }
    }

    private void AdjustBgmVolume(float delta)
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.SetBgmVolume(SoundManager.Instance.userBgmVolume + delta);
        SoundManager.Instance.PlayButtonClickSound();
        RefreshSettingsUI();
    }

    private void AdjustSfxVolume(float delta)
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.SetSfxVolume(SoundManager.Instance.userSfxVolume + delta);
        SoundManager.Instance.PlayButtonClickSound();
        RefreshSettingsUI();
    }

    private void ToggleVibrationSetting()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.SetVibrationEnabled(!GameManager.Instance.vibrationEnabled);
        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClickSound();
        RefreshSettingsUI();
    }

    // =========================================================================
    // 1-LIFE CRITICAL WARNING INDICATOR (PULSING RED VIGNETTE + BANNER)
    // =========================================================================
    public void SetLowLifeWarning(bool active)
    {
        if (lowLifeWarningRoot == null) return;

        if (!active)
        {
            if (lowLifePulseRoutine != null)
            {
                StopCoroutine(lowLifePulseRoutine);
                lowLifePulseRoutine = null;
            }
            lowLifeWarningRoot.SetActive(false);
            return;
        }

        lowLifeWarningRoot.SetActive(true);
        if (Application.isPlaying)
        {
            if (lowLifePulseRoutine != null) StopCoroutine(lowLifePulseRoutine);
            lowLifePulseRoutine = StartCoroutine(LowLifePulseRoutine());
        }
        else
        {
            ApplyLowLifeVisualAlpha(0.75f);
        }
    }

    private IEnumerator LowLifePulseRoutine()
    {
        float t = 0f;
        while (true)
        {
            t += Time.deltaTime * 5.2f;
            float wave = (Mathf.Sin(t) + 1f) * 0.5f;
            float edgeAlpha = Mathf.Lerp(0.18f, 0.58f, wave);
            ApplyLowLifeVisualAlpha(edgeAlpha);

            if (heartIcons != null && heartIcons.Length > 0 && heartIcons[0] != null)
            {
                float heartScale = 1f + wave * 0.24f;
                heartIcons[0].transform.localScale = new Vector3(heartScale, heartScale, 1f);
            }

            yield return null;
        }
    }

    private void ApplyLowLifeVisualAlpha(float edgeAlpha)
    {
        if (lowLifeBorderEdges != null)
        {
            for (int i = 0; i < lowLifeBorderEdges.Length; i++)
            {
                if (lowLifeBorderEdges[i] != null)
                {
                    Color c = lowLifeBorderEdges[i].color;
                    c.a = edgeAlpha;
                    lowLifeBorderEdges[i].color = c;
                }
            }
        }

        if (textLowLifeBanner != null)
        {
            Color bc = textLowLifeBanner.color;
            bc.a = Mathf.Clamp01(edgeAlpha + 0.35f);
            textLowLifeBanner.color = bc;
        }
    }

    // =========================================================================
    // HUD & POWER-UP DURATION BARS
    // =========================================================================
    public void UpdateDifficultyBadge(GameManager.DifficultyLevel difficulty)
    {
        if (textDifficultyBadge == null) return;

        switch (difficulty)
        {
            case GameManager.DifficultyLevel.Easy:
                textDifficultyBadge.text = "MODE: EASY";
                textDifficultyBadge.color = new Color(0.45f, 0.98f, 0.52f, 1f);
                break;
            case GameManager.DifficultyLevel.Hard:
                textDifficultyBadge.text = "MODE: HARD";
                textDifficultyBadge.color = new Color(1.0f, 0.42f, 0.42f, 1f);
                break;
            default:
                textDifficultyBadge.text = "MODE: MEDIUM";
                textDifficultyBadge.color = new Color(1.0f, 0.86f, 0.35f, 1f);
                break;
        }
    }

    public void UpdateThemeLabel(string themeName, bool showTransitionBanner)
    {
        if (textThemeBadge != null && !string.IsNullOrEmpty(themeName))
        {
            textThemeBadge.text = $"STAGE: {themeName.ToUpperInvariant()}";
        }
    }

    public void ShowEnvironmentTransitionBanner(int stageNumber, string envName, string envSubtitle)
    {
        if (environmentBannerPanel == null) return;

        if (textEnvStageBadge != null)
        {
            textEnvStageBadge.text = $"ENVIRONMENT STAGE {stageNumber}";
        }
        if (textEnvTitle != null)
        {
            textEnvTitle.text = envName.ToUpperInvariant();
        }
        if (textEnvSubtitle != null)
        {
            textEnvSubtitle.text = envSubtitle;
            textEnvSubtitle.horizontalOverflow = HorizontalWrapMode.Wrap;
            textEnvSubtitle.verticalOverflow = VerticalWrapMode.Truncate;
        }

        environmentBannerPanel.SetActive(true);
        if (environmentBannerCanvasGroup != null)
        {
            environmentBannerCanvasGroup.alpha = 1f;
        }

        if (Application.isPlaying)
        {
            if (envBannerRoutine != null) StopCoroutine(envBannerRoutine);
            envBannerRoutine = StartCoroutine(AnimateEnvironmentBannerRoutine());
        }
    }

    private IEnumerator AnimateEnvironmentBannerRoutine()
    {
        RectTransform rt = environmentBannerPanel.GetComponent<RectTransform>();
        float inDur = 0.28f;
        float t = 0f;
        while (t < inDur)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / inDur);
            if (environmentBannerCanvasGroup != null) environmentBannerCanvasGroup.alpha = u;
            if (rt != null)
            {
                float s = Mathf.Lerp(0.86f, 1.0f, Mathf.SmoothStep(0f, 1f, u));
                rt.localScale = new Vector3(s, s, 1f);
            }
            yield return null;
        }
        if (rt != null) rt.localScale = Vector3.one;
        if (environmentBannerCanvasGroup != null) environmentBannerCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(2.3f);

        float outDur = 0.45f;
        t = 0f;
        while (t < outDur)
        {
            t += Time.deltaTime;
            if (environmentBannerCanvasGroup != null)
            {
                environmentBannerCanvasGroup.alpha = 1f - Mathf.Clamp01(t / outDur);
            }
            yield return null;
        }

        if (environmentBannerPanel != null) environmentBannerPanel.SetActive(false);
        envBannerRoutine = null;
    }

    public void ShowPowerUpStatusToast(string message, Color color)
    {
        if (textThemeToast == null) return;
        textThemeToast.text = message;
        textThemeToast.color = color;
        textThemeToast.gameObject.SetActive(true);
        if (Application.isPlaying)
        {
            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(AnimateThemeToast());
        }
    }

    private IEnumerator AnimateThemeToast()
    {
        textThemeToast.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
        float elapsed = 0f;
        while (elapsed < 0.22f)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.Lerp(0.75f, 1.12f, elapsed / 0.22f);
            textThemeToast.transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        textThemeToast.transform.localScale = Vector3.one;
        yield return new WaitForSeconds(1.5f);
        textThemeToast.gameObject.SetActive(false);
    }

    public void UpdateHearts(int currentLives)
    {
        if (heartIcons == null) return;

        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] == null) continue;
            bool isFull = i < currentLives;
            if (heartFull != null && heartEmpty != null)
            {
                heartIcons[i].sprite = isFull ? heartFull : heartEmpty;
            }
            heartIcons[i].color = isFull ? Color.white : new Color(1f, 1f, 1f, 0.45f);
            heartIcons[i].transform.localScale = Vector3.one;

            if (Application.isPlaying && currentLives != lastDisplayedLives && i == Mathf.Clamp(currentLives - 1, 0, heartIcons.Length - 1))
            {
                StartCoroutine(PulseTransform(heartIcons[i].transform, 1.28f, 0.18f));
            }
        }
        lastDisplayedLives = currentLives;
    }

    public void UpdateScore(int score, bool animatePulse = true)
    {
        if (textScore != null)
        {
            textScore.text = $"SCORE: {score}";
            if (animatePulse && Application.isPlaying && gameObject.activeInHierarchy)
            {
                if (scorePulseRoutine != null) StopCoroutine(scorePulseRoutine);
                scorePulseRoutine = StartCoroutine(PulseTransform(textScore.transform, 1.22f, 0.14f));
            }
        }
    }

    public void UpdateHighScore(int bestScore)
    {
        if (textHighScore != null)
        {
            textHighScore.text = $"BEST: {bestScore}";
        }
    }

    public void UpdateMagnetTimer(float remainingSeconds)
    {
        // Legacy HUD text deactivated in favor of dedicated Power-Up Duration Bars below TopBar
        if (textMagnetTimer != null && textMagnetTimer.gameObject.activeSelf)
        {
            textMagnetTimer.gameObject.SetActive(false);
        }
    }

    public void UpdatePowerUpDurationBars(
        float magnetRem, float magnetMax,
        float shieldRem, float shieldMax,
        float timeSlowRem, float timeSlowMax)
    {
        bool showMagnet = magnetRem > 0.05f;
        bool showShield = shieldRem > 0.05f;
        bool showTimeSlow = timeSlowRem > 0.05f;
        bool anyActive = showMagnet || showShield || showTimeSlow;

        if (powerUpBarsContainer != null)
        {
            powerUpBarsContainer.SetActive(anyActive);
        }

        UpdateSinglePowerUpBar(
            magnetBarRow, magnetBarFillRect, textMagnetBarLabel,
            showMagnet, magnetRem, magnetMax, $"MAGNET  {magnetRem:0.0}s");

        UpdateSinglePowerUpBar(
            shieldBarRow, shieldBarFillRect, textShieldBarLabel,
            showShield, shieldRem, shieldMax, $"BARRIER SHIELD  {shieldRem:0.0}s (1 HIT)");

        UpdateSinglePowerUpBar(
            timeSlowBarRow, timeSlowBarFillRect, textTimeSlowBarLabel,
            showTimeSlow, timeSlowRem, timeSlowMax, $"TIME-SLOW  {timeSlowRem:0.0}s (50%)");
    }

    private void UpdateSinglePowerUpBar(
        GameObject rowObj, RectTransform fillRect, Text labelText,
        bool visible, float remaining, float maxDuration, string label)
    {
        if (rowObj != null && rowObj.activeSelf != visible)
        {
            rowObj.SetActive(visible);
        }
        if (!visible) return;

        float ratio = Mathf.Clamp01(remaining / Mathf.Max(0.1f, maxDuration));
        if (fillRect != null)
        {
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(Mathf.Max(0.02f, ratio), 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }
        if (labelText != null)
        {
            labelText.text = label;
        }
    }

    public void ShowGameOver(int finalScore, int bestScore)
    {
        if (textFinalScore != null)
        {
            textFinalScore.text = $"FINAL SCORE: {finalScore}";
        }
        if (textBestScore != null)
        {
            textBestScore.text = $"HIGH SCORE: {bestScore}";
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.transform.SetAsLastSibling();
            gameOverPanel.SetActive(true);
            if (Application.isPlaying)
            {
                StartCoroutine(PopInTransform(gameOverPanel.transform));
            }
        }
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private IEnumerator PulseTransform(Transform target, float peakScale, float duration)
    {
        if (target == null) yield break;
        float half = Mathf.Max(0.02f, duration * 0.5f);
        float t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float s = Mathf.Lerp(1f, peakScale, t / half);
            target.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float s = Mathf.Lerp(peakScale, 1f, t / half);
            target.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    private IEnumerator PopInTransform(Transform target)
    {
        if (target == null) yield break;
        float dur = 0.22f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / dur);
            float scale = u < 0.7f
                ? Mathf.Lerp(0.84f, 1.04f, u / 0.7f)
                : Mathf.Lerp(1.04f, 1.0f, (u - 0.7f) / 0.3f);
            target.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    public void PromptQuitConfirmation()
    {
        ShowConfirmation(
            "QUIT GAME?",
            "Are you sure you want to exit Slime Food Drop?",
            "EXIT",
            new Color(0.68f, 0.22f, 0.24f, 1f),
            () => GameManager.Instance?.QuitGame()
        );
    }

    public void PromptHomeConfirmation()
    {
        ShowConfirmation(
            "EXIT TO TITLE?",
            "Are you sure you want to return to the title screen? Your current run and score will be lost.",
            "EXIT RUN",
            new Color(0.68f, 0.22f, 0.24f, 1f),
            () =>
            {
                HideSettingsModal();
                HideGameOver();
                GameManager.Instance?.OpenHomeScreen();
            }
        );
    }

    public void PromptRestartConfirmation()
    {
        ShowConfirmation(
            "RESTART RUN?",
            "Are you sure you want to restart? Your current score and combo streak will be reset.",
            "RESTART",
            new Color(0.18f, 0.64f, 0.34f, 1f),
            () =>
            {
                HideSettingsModal();
                HideGameOver();
                GameManager.Instance?.RestartGame();
            }
        );
    }

    public void ShowConfirmation(string title, string message, string confirmLabel, Color confirmColor, Action onConfirm)
    {
        if (confirmationModal == null)
        {
            onConfirm?.Invoke();
            return;
        }

        if (SoundManager.Instance != null && Application.isPlaying)
        {
            SoundManager.Instance.PlayButtonClickSound();
        }

        onConfirmAction = onConfirm;
        if (textConfirmTitle != null) textConfirmTitle.text = title;
        if (textConfirmMessage != null) textConfirmMessage.text = message;

        if (btnConfirmYes != null)
        {
            Transform lbl = btnConfirmYes.transform.Find("Text_Label");
            if (lbl != null && lbl.GetComponent<Text>() != null)
            {
                lbl.GetComponent<Text>().text = confirmLabel;
            }
            Image img = btnConfirmYes.GetComponent<Image>();
            if (img != null) img.color = confirmColor;
            UIButtonJuice juice = btnConfirmYes.GetComponent<UIButtonJuice>();
            if (juice != null) juice.SetBaseColor(confirmColor);
        }

        // Always bring confirmation modal to the absolute top of the visual layer stack!
        confirmationModal.transform.SetAsLastSibling();
        Canvas modalCanvas = confirmationModal.GetComponent<Canvas>();
        if (modalCanvas == null)
        {
            modalCanvas = confirmationModal.AddComponent<Canvas>();
            confirmationModal.AddComponent<GraphicRaycaster>();
        }
        modalCanvas.overrideSorting = true;
        modalCanvas.sortingOrder = 500;

        confirmationModal.SetActive(true);
        if (Application.isPlaying)
        {
            StartCoroutine(PopInTransform(confirmationModal.transform));
        }
    }

    public void HideConfirmation()
    {
        if (confirmationModal != null)
        {
            confirmationModal.SetActive(false);
        }
        onConfirmAction = null;
    }

    public void ShowAlmanacItemDetail(Sprite artwork, string categoryBadge, Color badgeColor, string title, string stats, string description, string tips)
    {
        if (almanacDetailModal == null) return;

        if (SoundManager.Instance != null && Application.isPlaying)
        {
            SoundManager.Instance.PlayButtonClickSound();
        }

        if (imgDetailArtwork != null)
        {
            imgDetailArtwork.sprite = artwork;
            imgDetailArtwork.gameObject.SetActive(artwork != null);
        }
        if (textDetailCategoryBadge != null)
        {
            textDetailCategoryBadge.text = categoryBadge;
            textDetailCategoryBadge.color = badgeColor;
        }
        if (textDetailTitle != null) textDetailTitle.text = title;
        if (textDetailStats != null) textDetailStats.text = stats;
        if (textDetailDescription != null) textDetailDescription.text = description;
        if (textDetailTips != null) textDetailTips.text = tips;

        almanacDetailModal.transform.SetAsLastSibling();
        almanacDetailModal.SetActive(true);
        if (Application.isPlaying)
        {
            StartCoroutine(PopInTransform(almanacDetailModal.transform));
        }
    }

    public void HideAlmanacItemDetail()
    {
        if (almanacDetailModal != null)
        {
            almanacDetailModal.SetActive(false);
        }
    }

    private void OnRestartClicked()
    {
        PromptRestartConfirmation();
    }

    private void OnQuitClicked()
    {
        PromptQuitConfirmation();
    }
}
