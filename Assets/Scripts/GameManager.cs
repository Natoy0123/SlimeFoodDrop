using System.Collections;

using UnityEngine;

using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviour

{

    private static GameManager _instance;

    public static GameManager Instance

    {

        get

        {

            if (_instance == null) _instance = FindFirstObjectByType<GameManager>();

            return _instance;

        }

        private set { _instance = value; }

    }



    public enum GameState

    {

        SplashScreen,

        HomeScreen,

        SelectingDifficulty,

        Countdown,

        Tutorial,

        Playing,

        Paused,

        GameOver

    }



    public enum DifficultyLevel

    {

        Easy = 0,

        Medium = 1,

        Hard = 2

    }



    [Header("State & Difficulty")]

    public GameState currentState = GameState.SplashScreen;

    public DifficultyLevel currentDifficulty = DifficultyLevel.Medium;

    public bool showSplashOnStart = true;

    public bool enableCountdownAndTutorial = true;



    private static bool hasShownSplashThisSession = false;



    [Header("Health")]

    public int maxLives = 3;

    public int currentLives = 3;



    [Header("Scoring")]

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

    public float FoodRushDurationForCurrentDifficulty => currentDifficulty == DifficultyLevel.Easy ? 15.0f : 10.0f;



    [Header("Power-Ups & Settings")]

    public bool isMagnetActive = false;

    public float magnetTimeRemaining = 0f;

    public float magnetMaxDuration = 6.0f;



    public bool isShieldActive = false;

    public float shieldTimeRemaining = 0f;

    public float shieldMaxDuration = 12.0f;



    public bool isTimeSlowActive = false;

    public float timeSlowTimeRemaining = 0f;

    public float timeSlowMaxDuration = 6.0f;



    public bool vibrationEnabled = true;



    private const string HighScoreKey = "SlimeFoodDrop_HighScore";

    private const string DifficultyPrefKey = "SlimeFoodDrop_Difficulty";

    private const string VibratePrefKey = "SlimeFoodDrop_Vibrate";

    private const string OverallTutorialCompletedPrefKey = "SlimeFoodDrop_OverallTutorialCompleted_v6";

    private bool showDifficultyContextOnStart = false;



    private Coroutine gameStartSequenceRoutine;

    private GameState stateBeforePause = GameState.Playing;



    public bool IsPlaying => currentState == GameState.Playing;

    public bool IsGameplaySessionPaused =>

        stateBeforePause == GameState.Playing ||

        stateBeforePause == GameState.Countdown ||

        stateBeforePause == GameState.Tutorial;

    public bool IsMagnetActive => isMagnetActive && magnetTimeRemaining > 0f;

    public bool IsShieldActive => isShieldActive && shieldTimeRemaining > 0f;

    public bool IsTimeSlowActive => isTimeSlowActive && timeSlowTimeRemaining > 0f;



    public float GetTimeSlowMultiplier()

    {

        return IsTimeSlowActive ? 0.48f : 1.0f;

    }



    // Difficulty Parameter Multipliers

    public float FallSpeedMultiplier

    {

        get

        {

            switch (currentDifficulty)

            {

                case DifficultyLevel.Easy: return 0.78f;

                case DifficultyLevel.Hard: return 1.28f;

                default: return 1.00f;

            }

        }

    }



    public float SpawnIntervalMultiplier

    {

        get

        {

            switch (currentDifficulty)

            {

                case DifficultyLevel.Easy: return 1.25f;

                case DifficultyLevel.Hard: return 0.76f;

                default: return 1.00f;

            }

        }

    }



    public int PoisonWeightOffset

    {

        get

        {

            switch (currentDifficulty)

            {

                case DifficultyLevel.Easy: return -5;

                case DifficultyLevel.Hard: return +9;

                default: return 0;

            }

        }

    }



    public int PowerUpBonusWeight

    {

        get

        {

            switch (currentDifficulty)

            {

                case DifficultyLevel.Easy: return 3;

                case DifficultyLevel.Hard: return -1;

                default: return 0;

            }

        }

    }



    public int BaseFoodPoints

    {

        get

        {

            switch (currentDifficulty)

            {

                case DifficultyLevel.Easy: return 10;

                case DifficultyLevel.Hard: return 15;

                default: return 10;

            }

        }

    }



    private void Awake()

    {

        if (Instance != null && Instance != this)

        {

            Destroy(gameObject);

            return;

        }

        Instance = this;

        Application.targetFrameRate = 120;

        QualitySettings.vSyncCount = 0;

        Time.fixedDeltaTime = 1f / 60f;

        Time.timeScale = 1f;

        currentLives = maxLives;

        currentScore = 0;

        comboCount = 0;

        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        currentDifficulty = (DifficultyLevel)Mathf.Clamp(PlayerPrefs.GetInt(DifficultyPrefKey, 1), 0, 2);

        vibrationEnabled = PlayerPrefs.GetInt(VibratePrefKey, 1) == 1;

    }



    private void Start()

    {

        if (ThemeManager.Instance != null)

        {

            ThemeManager.Instance.InitializeStartingTheme();

        }



        if (SlimeController.Instance != null)

        {

            SlimeController.Instance.gameObject.SetActive(true);

            SlimeController.Instance.ResetToAliveState();

        }



        if (UIManager.Instance != null)

        {

            UIManager.Instance.UpdateHearts(currentLives);

            UIManager.Instance.UpdateScore(currentScore, false);

            UIManager.Instance.UpdateHighScore(highScore);

            UIManager.Instance.UpdateDifficultyBadge(currentDifficulty);

            SyncPowerUpHUD();

            UIManager.Instance.HideGameOver();

            UIManager.Instance.HideSettingsModal();

            UIManager.Instance.HideGuidebookModal();



            if (showSplashOnStart && !hasShownSplashThisSession)

            {

                hasShownSplashThisSession = true;

                currentState = GameState.SplashScreen;

                UIManager.Instance.SetGameplayHUDVisible(false);

                UIManager.Instance.PlaySplashSequence(OnSplashSequenceComplete);

            }

            else

            {

                OpenHomeScreen();

            }

        }

    }



    private void OnSplashSequenceComplete()

    {

        OpenHomeScreen();

    }



    private void ResetAllPowerUps()

    {

        isMagnetActive = false;

        magnetTimeRemaining = 0f;

        isShieldActive = false;

        shieldTimeRemaining = 0f;

        isTimeSlowActive = false;

        timeSlowTimeRemaining = 0f;

        SyncPowerUpHUD();

        if (SoundManager.Instance != null)

        {

            SoundManager.Instance.UpdatePowerUpAudio(false, false, false);

        }

    }



    private void SyncPowerUpHUD()

    {

        if (UIManager.Instance != null)

        {

            UIManager.Instance.UpdateMagnetTimer(magnetTimeRemaining);

            UIManager.Instance.UpdatePowerUpDurationBars(

                magnetTimeRemaining, magnetMaxDuration,

                shieldTimeRemaining, shieldMaxDuration,

                timeSlowTimeRemaining, timeSlowMaxDuration);

        }

    }



    public void OpenHomeScreen()

    {

        if (gameStartSequenceRoutine != null)

        {

            StopCoroutine(gameStartSequenceRoutine);

            gameStartSequenceRoutine = null;

        }



        Time.timeScale = 1f;

        if (ItemSpawner.Instance != null)

        {

            ItemSpawner.Instance.StopSpawningAndClear();

        }



        currentState = GameState.HomeScreen;

        ResetAllPowerUps();



        if (ThemeManager.Instance != null)

        {

            ThemeManager.Instance.InitializeStartingTheme();

        }



        if (SlimeController.Instance != null)

        {

            SlimeController.Instance.ResetToAliveState();

            SlimeController.Instance.gameObject.SetActive(false);

        }



        if (UIManager.Instance != null)

        {

            UIManager.Instance.CancelTutorialAndCountdownOverlays();

            UIManager.Instance.HideGameOver();

            UIManager.Instance.HideSettingsModal();

            UIManager.Instance.HideGuidebookModal();

            UIManager.Instance.HideDifficultyScreen();

            UIManager.Instance.SetLowLifeWarning(false);

            UIManager.Instance.SetGameplayHUDVisible(false);

            UIManager.Instance.ShowHomeScreen(highScore);

        }

    }



    public void OpenDifficultySelection()

    {

        if (gameStartSequenceRoutine != null)

        {

            StopCoroutine(gameStartSequenceRoutine);

            gameStartSequenceRoutine = null;

        }



        Time.timeScale = 1f;

        if (ItemSpawner.Instance != null)

        {

            ItemSpawner.Instance.StopSpawningAndClear();

        }



        currentState = GameState.SelectingDifficulty;

        if (SlimeController.Instance != null)

        {

            SlimeController.Instance.ResetToAliveState();

            SlimeController.Instance.gameObject.SetActive(false);

        }



        if (UIManager.Instance != null)

        {

            UIManager.Instance.CancelTutorialAndCountdownOverlays();

            UIManager.Instance.HideGameOver();

            UIManager.Instance.HideSettingsModal();

            UIManager.Instance.HideGuidebookModal();

            UIManager.Instance.SetLowLifeWarning(false);

            UIManager.Instance.SetGameplayHUDVisible(false);

            UIManager.Instance.ShowDifficultyScreen(currentDifficulty);

        }

    }



    private bool forceShowTutorialNextStart = false;



    public void SelectEasyAndStart()
    {
        showDifficultyContextOnStart = true;
        StartGameWithDifficulty(DifficultyLevel.Easy);
    }

    public void SelectMediumAndStart()
    {
        showDifficultyContextOnStart = true;
        StartGameWithDifficulty(DifficultyLevel.Medium);
    }

    public void SelectHardAndStart()
    {
        showDifficultyContextOnStart = true;
        StartGameWithDifficulty(DifficultyLevel.Hard);
    }



    public void StartGameWithDifficulty(DifficultyLevel difficulty, bool immediateForTest = false)

    {

        if (gameStartSequenceRoutine != null)

        {

            StopCoroutine(gameStartSequenceRoutine);

            gameStartSequenceRoutine = null;

        }



        Time.timeScale = 1f;

        currentDifficulty = difficulty;

        PlayerPrefs.SetInt(DifficultyPrefKey, (int)difficulty);

        PlayerPrefs.Save();



        currentLives = maxLives;

        currentScore = 0;

        comboCount = 0;

        ResetAllPowerUps();



        if (ItemSpawner.Instance != null)

        {

            ItemSpawner.Instance.StopSpawningAndClear();

        }



        if (ThemeManager.Instance != null)

        {

            ThemeManager.Instance.InitializeStartingTheme();

        }



        if (SlimeController.Instance != null)

        {

            SlimeController.Instance.gameObject.SetActive(true);

            SlimeController.Instance.ResetToAliveState();

        }



        if (UIManager.Instance != null)

        {

            UIManager.Instance.CancelTutorialAndCountdownOverlays();

            UIManager.Instance.HideSplashAndHomeScreens();

            UIManager.Instance.HideDifficultyScreen();

            UIManager.Instance.HideGameOver();

            UIManager.Instance.HideSettingsModal();

            UIManager.Instance.HideGuidebookModal();

            UIManager.Instance.SetGameplayHUDVisible(true);

            UIManager.Instance.UpdateHearts(currentLives);

            UIManager.Instance.UpdateScore(currentScore, false);

            UIManager.Instance.UpdateHighScore(highScore);

            UIManager.Instance.UpdateDifficultyBadge(currentDifficulty);

            UIManager.Instance.SetLowLifeWarning(false);

            SyncPowerUpHUD();

        }



        if (SoundManager.Instance != null)

        {

            SoundManager.Instance.PlayHeartPickupSound();

        }



        if (!Application.isPlaying || !enableCountdownAndTutorial || immediateForTest)

        {

            currentState = GameState.Playing;

            if (ThemeManager.Instance != null)

            {

                ThemeManager.Instance.AnnounceCurrentThemeBanner();

            }

            if (ItemSpawner.Instance != null && Application.isPlaying)

            {

                ItemSpawner.Instance.StartSpawning();

            }

            return;

        }



        gameStartSequenceRoutine = StartCoroutine(GameStartCountdownAndTutorialRoutine());

    }



    private string GetDiffTutorialPrefKey(DifficultyLevel diff)

    {

        return $"SlimeFoodDrop_DiffIntroSeen_v5_{(int)diff}";

    }



    private IEnumerator GameStartCountdownAndTutorialRoutine()

    {

        // Two-tier tutorial system:

        // Tier 1: Overall game tutorial (core mechanics) triggers ONLY on first play overall.

        // Once played, regardless of difficulty, it will not be triggered again automatically.

        bool overallTutorialSeen = PlayerPrefs.GetInt(OverallTutorialCompletedPrefKey, 0) == 1;



        // Tier 2: Difficulty tutorial context triggers when choosing a difficulty from the Difficulty menu.

        bool shouldShowTutorial = forceShowTutorialNextStart || !overallTutorialSeen || showDifficultyContextOnStart;

        bool shouldShowFullTutorial = forceShowTutorialNextStart || !overallTutorialSeen;



        forceShowTutorialNextStart = false;

        showDifficultyContextOnStart = false;



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



            yield return StartCoroutine(UIManager.Instance.RunInGameTutorialSequence(currentDifficulty, shouldShowFullTutorial));



            // Mark overall tutorial as completed so it never triggers automatically again

            PlayerPrefs.SetInt(OverallTutorialCompletedPrefKey, 1);

            PlayerPrefs.Save();

        }



        // Step 2: Always pause gameplay for 3 seconds and count down (3... 2... 1... GO!) AFTER tutorial finishes

        currentState = GameState.Countdown;

        if (UIManager.Instance != null)

        {

            if (UIManager.Instance.tutorialPanel != null)

            {

                UIManager.Instance.tutorialPanel.SetActive(false);

            }

            yield return StartCoroutine(UIManager.Instance.RunThreeSecondCountdown());

        }



        currentState = GameState.Playing;

        if (ThemeManager.Instance != null)

        {

            ThemeManager.Instance.AnnounceCurrentThemeBanner();

        }

        if (ItemSpawner.Instance != null)

        {

            ItemSpawner.Instance.StartSpawning();

        }

        gameStartSequenceRoutine = null;

    }



    public void OpenSettings()

    {

        if (currentState == GameState.SplashScreen) return;



        if (currentState != GameState.Paused)

        {

            stateBeforePause = currentState;

        }

        currentState = GameState.Paused;

        Time.timeScale = 0f;



        if (UIManager.Instance != null)

        {

            UIManager.Instance.HideGuidebookModal();

            UIManager.Instance.ShowSettingsModal();

        }

    }



    public void CloseSettingsAndResume()

    {

        Time.timeScale = 1f;

        if (UIManager.Instance != null)

        {

            UIManager.Instance.HideSettingsModal();

        }

        currentState = (stateBeforePause == GameState.Paused) ? GameState.Playing : stateBeforePause;

    }



    public void OpenGuidebook()

    {

        if (currentState == GameState.SplashScreen) return;



        if (currentState != GameState.Paused)

        {

            stateBeforePause = currentState;

        }

        currentState = GameState.Paused;

        Time.timeScale = 0f;



        if (UIManager.Instance != null)

        {

            UIManager.Instance.HideSettingsModal();

            UIManager.Instance.ShowGuidebookModal(0);

        }

    }



    public void CloseGuidebook()

    {

        if (UIManager.Instance != null)

        {

            UIManager.Instance.HideGuidebookModal();

        }

        Time.timeScale = 1f;

        currentState = (stateBeforePause == GameState.Paused) ? GameState.Playing : stateBeforePause;

    }



    public void ReplayTutorialFromSettings()
    {
        forceShowTutorialNextStart = true;
        Time.timeScale = 1f;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideSettingsModal();
            UIManager.Instance.HideGuidebookModal();
        }
        StartGameWithDifficulty(currentDifficulty);

    }



    public void SetVibrationEnabled(bool enabled)

    {

        vibrationEnabled = enabled;

        PlayerPrefs.SetInt(VibratePrefKey, enabled ? 1 : 0);

        PlayerPrefs.Save();



        if (enabled)

        {

            TriggerDeviceVibration();

        }

    }



    private void TriggerDeviceVibration()

    {

        if (!vibrationEnabled) return;

#if UNITY_ANDROID || UNITY_IOS

        if (Application.isMobilePlatform)

        {

            Handheld.Vibrate();

        }

#endif

    }



    private void Update()

    {

        if (!IsPlaying) return;



        bool anyChanged = false;



        if (isMagnetActive)

        {

            magnetTimeRemaining -= Time.deltaTime;

            if (magnetTimeRemaining <= 0f)

            {

                magnetTimeRemaining = 0f;

                isMagnetActive = false;

            }

            anyChanged = true;

        }



        if (isShieldActive)

        {

            shieldTimeRemaining -= Time.deltaTime;

            if (shieldTimeRemaining <= 0f)

            {

                shieldTimeRemaining = 0f;

                isShieldActive = false;

            }

            anyChanged = true;

        }



        if (isTimeSlowActive)

        {

            timeSlowTimeRemaining -= Time.deltaTime;

            if (timeSlowTimeRemaining <= 0f)

            {

                timeSlowTimeRemaining = 0f;

                isTimeSlowActive = false;

            }

            anyChanged = true;

        }



        // Active Food Rush countdown timer

        if (isFoodRushActive)

        {

            foodRushTimeRemaining -= Time.deltaTime;

            if (foodRushTimeRemaining <= 0f)

            {

                EndFoodRush();

            }

            else if (UIManager.Instance != null)

            {

                UIManager.Instance.UpdateFoodRushHUD(foodRushTimeRemaining, foodRushMaxDuration);

            }

        }

        else if (frenzyStreak > 0)

        {

            // 4.0-second streak sustain window

            frenzyStreakTimer -= Time.deltaTime;

            if (frenzyStreakTimer <= 0f)

            {

                frenzyStreak = 0;

                frenzyStreakTimer = 0f;

                if (UIManager.Instance != null)

                {

                    UIManager.Instance.UpdateFrenzyStreakHUD(0, FrenzyTargetForCurrentDifficulty);

                }

            }

        }



        if (anyChanged)

        {

            SyncPowerUpHUD();

            if (SoundManager.Instance != null)

            {

                SoundManager.Instance.UpdatePowerUpAudio(isShieldActive, isMagnetActive, isTimeSlowActive);

            }

        }

    }



    public void OnFoodCaught(int basePoints)

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

    }



    public void AddScore(int points)

    {

        if (!IsPlaying) return;



        currentScore += points;

        if (currentScore > highScore)

        {

            highScore = currentScore;

            if (UIManager.Instance != null)

            {

                UIManager.Instance.UpdateHighScore(highScore);

            }

        }



        if (UIManager.Instance != null)

        {

            UIManager.Instance.UpdateScore(currentScore, true);

        }



        if (ThemeManager.Instance != null)

        {

            ThemeManager.Instance.EvaluateScoreForTheme(currentScore);

        }

    }



    public void OnFoodMissed()

    {

        if (!IsPlaying) return;

        // Missing edible food does NOT reduce any life — it only resets the combo streak!

        comboCount = 0;

    }



    public void LoseLife(string cause)

    {

        if (!IsPlaying) return;



        // Only poisonous hazards reduce player lives!

        if (cause != "Poison")

        {

            comboCount = 0;

            return;

        }



        // If Magic Barrier Shield is active, absorb the Poison hit completely!

        if (IsShieldActive)

        {

            isShieldActive = false;

            shieldTimeRemaining = 0f;

            SyncPowerUpHUD();



            if (SlimeController.Instance != null)

            {

                SlimeController.Instance.TriggerShieldBlockEffect();

            }

            if (SoundManager.Instance != null)

            {

                SoundManager.Instance.PlayChimeSound();

            }

            if (UIManager.Instance != null)

            {

                UIManager.Instance.ShowPowerUpStatusToast("BARRIER BLOCKED POISON! (0 DMG)", new Color(0.35f, 0.96f, 1.0f, 1f));

            }

            return;

        }



        comboCount = 0;

        frenzyStreak = 0;

        frenzyStreakTimer = 0f;

        if (UIManager.Instance != null)

        {

            UIManager.Instance.UpdateFrenzyStreakHUD(0, FrenzyTargetForCurrentDifficulty);

            UIManager.Instance.SpawnStreakBrokenIndicator();

        }

        currentLives = Mathf.Clamp(currentLives - 1, 0, maxLives);



        if (ResponsiveCamera.Instance != null)

        {

            ResponsiveCamera.Instance.TriggerShake(0.22f, 0.2f);

        }



        TriggerDeviceVibration();

        if (SlimeController.Instance != null)

        {

            SlimeController.Instance.TriggerHurtEffect();

        }

        if (SoundManager.Instance != null)

        {

            SoundManager.Instance.PlayHurtSound();

        }



        if (UIManager.Instance != null)

        {

            UIManager.Instance.UpdateHearts(currentLives);

            UIManager.Instance.SetLowLifeWarning(currentLives == 1);

        }



        if (currentLives <= 0)

        {

            TriggerGameOver();

        }

    }



    public void AddLife()

    {

        if (!IsPlaying) return;



        if (currentLives < maxLives)

        {

            currentLives++;

            if (UIManager.Instance != null)

            {

                UIManager.Instance.UpdateHearts(currentLives);

                UIManager.Instance.SetLowLifeWarning(currentLives == 1);

                UIManager.Instance.ShowPowerUpStatusToast("+1 GOLDEN HEART RESTORED!", new Color(1.0f, 0.85f, 0.30f, 1f));

            }

        }

        else

        {

            // Full health fallback: award +50 bonus points

            AddScore(50);

            if (UIManager.Instance != null)

            {

                UIManager.Instance.ShowPowerUpStatusToast("MAX HEARTS! +50 BONUS PTS!", new Color(1.0f, 0.85f, 0.30f, 1f));

            }

        }



        if (SlimeController.Instance != null)

        {

            SlimeController.Instance.TriggerHealBurstEffect();

        }

        if (SoundManager.Instance != null)

        {

            SoundManager.Instance.PlayChimeSound();

        }

    }



    public void TriggerFoodRush()

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



    public void ActivateMagnet(float duration = 6.0f)

    {

        if (!IsPlaying) return;



        isMagnetActive = true;

        magnetMaxDuration = Mathf.Max(1f, duration);

        magnetTimeRemaining = Mathf.Max(magnetTimeRemaining, duration);



        if (SlimeController.Instance != null)

        {

            SlimeController.Instance.TriggerEatEffect();

            SlimeController.Instance.UpdatePowerUpAuraVisuals();

        }

        if (SoundManager.Instance != null)

        {

            SoundManager.Instance.PlayMagnetPickupSound();

            SoundManager.Instance.UpdatePowerUpAudio(isShieldActive, isMagnetActive, isTimeSlowActive);

        }

        SyncPowerUpHUD();

        if (UIManager.Instance != null)

        {

            UIManager.Instance.ShowPowerUpStatusToast("STAR MAGNET ACTIVE! (6s)", new Color(1.0f, 0.88f, 0.32f, 1f));

        }

    }



    public void ActivateShieldBarrier(float duration = 12.0f)

    {

        if (!IsPlaying) return;



        isShieldActive = true;

        shieldMaxDuration = Mathf.Max(1f, duration);

        shieldTimeRemaining = Mathf.Max(shieldTimeRemaining, duration);



        if (SlimeController.Instance != null)

        {

            SlimeController.Instance.TriggerEatEffect();

            SlimeController.Instance.UpdatePowerUpAuraVisuals();

        }

        if (SoundManager.Instance != null)

        {

            SoundManager.Instance.PlayShieldPickupSound();

            SoundManager.Instance.UpdatePowerUpAudio(isShieldActive, isMagnetActive, isTimeSlowActive);

        }

        SyncPowerUpHUD();

        if (UIManager.Instance != null)

        {

            UIManager.Instance.ShowPowerUpStatusToast("BARRIER SHIELD ACTIVE! (BLOCKS 1 HIT)", new Color(0.35f, 0.96f, 1.0f, 1f));

        }

    }



    public void ActivateTimeSlow(float duration = 6.0f)

    {

        if (!IsPlaying) return;



        isTimeSlowActive = true;

        timeSlowMaxDuration = Mathf.Max(1f, duration);

        timeSlowTimeRemaining = Mathf.Max(timeSlowTimeRemaining, duration);



        if (SlimeController.Instance != null)

        {

            SlimeController.Instance.TriggerEatEffect();

            SlimeController.Instance.UpdatePowerUpAuraVisuals();

        }

        if (SoundManager.Instance != null)

        {

            SoundManager.Instance.PlayTimeSlowPickupSound();

            SoundManager.Instance.UpdatePowerUpAudio(isShieldActive, isMagnetActive, isTimeSlowActive);

        }

        SyncPowerUpHUD();

        if (UIManager.Instance != null)

        {

            UIManager.Instance.ShowPowerUpStatusToast("CHRONO TIME-SLOW! (50% DROP SPEED)", new Color(0.78f, 0.58f, 1.0f, 1f));

        }

    }



    public void TriggerGameOver()

    {

        if (currentState == GameState.GameOver) return;



        currentState = GameState.GameOver;

        ResetAllPowerUps();



        if (ItemSpawner.Instance != null)

        {

            ItemSpawner.Instance.StopSpawningAndClear();

        }



        if (SlimeController.Instance != null)

        {

            SlimeController.Instance.TriggerDeadEffect();

        }



        if (SoundManager.Instance != null)

        {

            SoundManager.Instance.PlayMissSound();

        }



        if (UIManager.Instance != null)

        {

            UIManager.Instance.SetLowLifeWarning(false);

            UIManager.Instance.ShowGameOver(currentScore, highScore);

        }



        // Persist high score on GameOver rather than on active scoring frames

        PlayerPrefs.SetInt(HighScoreKey, highScore);

        PlayerPrefs.Save();

    }



    private void OnApplicationPause(bool pauseStatus)

    {

        if (pauseStatus)

        {

            PlayerPrefs.SetInt(HighScoreKey, highScore);

            PlayerPrefs.Save();

        }

    }



    public void RestartGame()

    {

        Time.timeScale = 1f;

        StartGameWithDifficulty(currentDifficulty);

    }



    public void QuitGame()

    {

        Time.timeScale = 1f;

#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;

#else

        Application.Quit();

#endif

    }

}

