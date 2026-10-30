using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cinematic Pixar-style splash intro animation with dynamic contact shadow physics:
/// Shadows continuously track Bloop's and Food's exact squash & stretch scale,
/// height above the floor, and stage blackout lighting.
/// </summary>
public class PixarSplashCinematic : MonoBehaviour
{
    [Header("UI Element References")]
    public RectTransform stageContainer;
    public Image spotlightImage;
    public Image bloopImage;
    public RectTransform bloopRect;
    public Image bloopShadow;
    public Image foodImage;
    public RectTransform foodRect;
    public Image foodShadow;
    public Image projectileImage;
    public RectTransform projectileRect;
    public GameObject speechBubbleGroup;
    public Text speechBubbleText;
    public RectTransform speechBubbleRect;
    public Button skipButton;

    [Header("Underneath Loading Bar References")]
    public CanvasGroup loadingBarGroup;
    public RectTransform loadingBarFillRect;
    public Text loadingPercentText;
    public Text loadingStatusText;

    [Header("Splash Cutscene Sprites")]
    public Sprite foodRun1;
    public Sprite foodRun2;
    public Sprite foodPoisonReveal;
    public Sprite slimeChaseExcited;
    public Sprite slimeChompBite;
    public Sprite slimeRealizePoison;
    public Sprite slimeTurnLookUser;
    public Sprite slimeGagPuff1;
    public Sprite slimeGagPuff2;
    public Sprite slimeSpitLaunch;
    public Sprite slimeReliefSmile;
    public Sprite spitProjectileSprite;
    public Sprite spotlightSprite;
    public Sprite contactShadowSprite;
    public Sprite slimeTrailSprite;

    private Coroutine cinematicRoutine;
    private Action currentOnComplete;
    private bool isSkipped = false;

    private void Awake()
    {
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipClicked);
        }
    }

    public void StartCinematic(Action onComplete)
    {
        currentOnComplete = onComplete;
        isSkipped = false;

        if (cinematicRoutine != null) StopCoroutine(cinematicRoutine);
        cinematicRoutine = StartCoroutine(CinematicSequenceRoutine());
    }

    public void OnSkipClicked()
    {
        if (isSkipped) return;
        isSkipped = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClickSound();
        }

        if (cinematicRoutine != null)
        {
            StopCoroutine(cinematicRoutine);
            cinematicRoutine = null;
        }

        StartCoroutine(FastFinishRoutine());
    }

    private IEnumerator FastFinishRoutine()
    {
        if (loadingBarGroup != null) loadingBarGroup.alpha = 1f;
        UpdateLoadingBar(1f, "100%", "READY TO DROP!");
        yield return new WaitForSecondsRealtime(0.15f);

        CanvasGroup splashCg = GetComponent<CanvasGroup>();
        if (splashCg != null)
        {
            float fadeDur = 0.35f;
            float elapsed = 0f;
            while (elapsed < fadeDur)
            {
                elapsed += Time.unscaledDeltaTime;
                splashCg.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDur);
                yield return null;
            }
            splashCg.alpha = 0f;
        }

        currentOnComplete?.Invoke();
    }

    private void UpdateLoadingBar(float progress, string pct, string status)
    {
        float clamped = Mathf.Clamp01(progress);
        if (loadingBarFillRect != null)
        {
            loadingBarFillRect.anchorMin = Vector2.zero;
            loadingBarFillRect.anchorMax = new Vector2(Mathf.Max(0.01f, clamped), 1f);
            loadingBarFillRect.offsetMin = new Vector2(6f, 6f);
            loadingBarFillRect.offsetMax = new Vector2(-6f, -6f);
        }
        if (loadingPercentText != null) loadingPercentText.text = pct;
        if (loadingStatusText != null) loadingStatusText.text = status;
    }

    /// <summary>
    /// Updates Bloop's contact shadow dynamically. The shadow anchors to the wooden floor
    /// surface (y = -180f) and morphs its horizontal/vertical scale in direct response to
    /// Bloop's squash, stretch, and vertical leap height.
    /// </summary>
    public void UpdateBloopShadow(float x, float y, Vector3 localScale)
    {
        if (bloopShadow == null) return;
        float floorY = -180f;
        float heightAboveFloor = Mathf.Max(0f, y - (-80f));
        float heightRatio = Mathf.Clamp01(heightAboveFloor / 140f);

        bloopShadow.rectTransform.anchoredPosition = new Vector2(x, floorY);

        // When squishing wide, shadow widens horizontally. When stretching tall, shadow contracts.
        float shadowScaleX = localScale.x * (1f - heightRatio * 0.32f);
        float shadowScaleY = (localScale.x > 0.05f ? (1f / Mathf.Sqrt(localScale.x)) : 1f) * Mathf.Lerp(1f, 0.45f, heightRatio);
        bloopShadow.rectTransform.localScale = new Vector3(shadowScaleX, shadowScaleY, 1f);

        // Grounded contact shadow is rich and opaque (0.88), softening gracefully as Bloop leaps higher
        float shadowAlpha = Mathf.Lerp(0.88f, 0.30f, heightRatio);
        bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, shadowAlpha);
    }

    /// <summary>
    /// Updates Food's contact shadow dynamically. Mirrors food squash/shrink and vertical hopping.
    /// </summary>
    public void UpdateFoodShadow(float x, float y, Vector3 localScale)
    {
        if (foodShadow == null) return;
        float floorY = -180f;
        float heightAboveFloor = Mathf.Max(0f, y - (-95f));
        float heightRatio = Mathf.Clamp01(heightAboveFloor / 30f);

        foodShadow.rectTransform.anchoredPosition = new Vector2(x, floorY);

        float shadowScaleX = localScale.x * (1f - heightRatio * 0.28f);
        float shadowScaleY = localScale.y * (1f - heightRatio * 0.28f);
        foodShadow.rectTransform.localScale = new Vector3(shadowScaleX, shadowScaleY, 1f);

        float shadowAlpha = Mathf.Lerp(0.85f, 0.30f, heightRatio);
        foodShadow.color = new Color(0.08f, 0.05f, 0.12f, shadowAlpha);
    }

    private IEnumerator CinematicSequenceRoutine()
    {
        // 0. Initialize initial hidden/starting states
        if (loadingBarGroup != null) loadingBarGroup.alpha = 0f;
        UpdateLoadingBar(0.01f, "0%", "INITIALIZING STAGE ENGINE...");

        if (speechBubbleGroup != null) speechBubbleGroup.SetActive(false);
        if (projectileImage != null) projectileImage.gameObject.SetActive(false);

        if (spotlightImage != null)
        {
            spotlightImage.color = new Color(1f, 1f, 1f, 0f);
            spotlightImage.gameObject.SetActive(true);
        }

        bloopImage.gameObject.SetActive(true);
        bloopImage.sprite = slimeChaseExcited;
        bloopRect.anchoredPosition = new Vector2(-550f, -80f);
        bloopRect.localScale = Vector3.one;
        bloopImage.color = Color.white;
        UpdateBloopShadow(-550f, -80f, Vector3.one);

        foodImage.gameObject.SetActive(true);
        foodImage.sprite = foodRun1;
        foodRect.anchoredPosition = new Vector2(-460f, -95f);
        foodRect.localScale = Vector3.one;
        UpdateFoodShadow(-460f, -95f, Vector3.one);

        if (bloopShadow != null) bloopShadow.gameObject.SetActive(true);
        if (foodShadow != null) foodShadow.gameObject.SetActive(true);

        // --- ACT 1: Warm Spotlight lights up, Scampering Food scurries in ---
        float t = 0f;
        while (t < 0.6f)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / 0.6f);
            if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, a * 0.90f);
            yield return null;
        }

        // Food scurries from offscreen-left (-480f) into the spotlight beam at (40f, -95f)
        float foodRunTime = 1.25f;
        t = 0f;
        int stepToggle = 0;
        float stepTimer = 0f;
        if (SoundManager.Instance != null) SoundManager.Instance.PlayFoodScamperSound();
        while (t < foodRunTime)
        {
            t += Time.unscaledDeltaTime;
            stepTimer += Time.unscaledDeltaTime;
            float u = t / foodRunTime;
            float curX = Mathf.Lerp(-480f, 40f, u);
            float hopY = -95f + Mathf.Abs(Mathf.Sin(u * Mathf.PI * 8f)) * 26f;
            foodRect.anchoredPosition = new Vector2(curX, hopY);

            UpdateFoodShadow(curX, hopY, foodRect.localScale);

            if (stepTimer > 0.11f)
            {
                stepTimer = 0f;
                stepToggle = 1 - stepToggle;
                foodImage.sprite = (stepToggle == 0) ? foodRun1 : foodRun2;
            }

            yield return null;
        }

        foodRect.anchoredPosition = new Vector2(40f, -95f);
        UpdateFoodShadow(40f, -95f, foodRect.localScale);

        // Food stops in the spotlight beam, looks left and right
        yield return new WaitForSecondsRealtime(0.28f);

        // --- ACT 2: Bloop leaps in pursuit with Pixar squash & stretch ---
        bloopImage.sprite = slimeChaseExcited;

        // Leap 1: -480f -> -290f
        float lt = 0f;
        float leapDur = 0.42f;
        float trailTimer = 0f;
        if (SoundManager.Instance != null) SoundManager.Instance.PlayJumpSound();
        while (lt < leapDur)
        {
            lt += Time.unscaledDeltaTime;
            trailTimer += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(lt / leapDur);
            float curX = Mathf.Lerp(-480f, -290f, p);
            float curY = -80f + Mathf.Sin(p * Mathf.PI) * 95f;
            bloopRect.anchoredPosition = new Vector2(curX, curY);

            if (trailTimer >= 0.08f)
            {
                trailTimer = 0f;
                SpawnSplashTrailDroplet(curX);
            }

            float stretch = 1f + Mathf.Sin(p * Mathf.PI) * 0.32f;
            float squash = 1f - Mathf.Sin(p * Mathf.PI) * 0.22f;
            bloopRect.localScale = new Vector3(squash, stretch, 1f);

            UpdateBloopShadow(curX, curY, bloopRect.localScale);

            yield return null;
        }
        bloopRect.localScale = new Vector3(1.30f, 0.72f, 1f);
        UpdateBloopShadow(-290f, -80f, bloopRect.localScale);
        SpawnSplashTrailDroplet(-290f);
        yield return new WaitForSecondsRealtime(0.06f);
        bloopRect.localScale = Vector3.one;
        UpdateBloopShadow(-290f, -80f, bloopRect.localScale);

        // Leap 2: -290f -> -120f (closing in!)
        lt = 0f;
        leapDur = 0.40f;
        trailTimer = 0f;
        if (SoundManager.Instance != null) SoundManager.Instance.PlayJumpSound();
        while (lt < leapDur)
        {
            lt += Time.unscaledDeltaTime;
            trailTimer += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(lt / leapDur);
            float curX = Mathf.Lerp(-290f, -120f, p);
            float curY = -80f + Mathf.Sin(p * Mathf.PI) * 95f;
            bloopRect.anchoredPosition = new Vector2(curX, curY);

            if (trailTimer >= 0.08f)
            {
                trailTimer = 0f;
                SpawnSplashTrailDroplet(curX);
            }

            float stretch = 1f + Mathf.Sin(p * Mathf.PI) * 0.32f;
            float squash = 1f - Mathf.Sin(p * Mathf.PI) * 0.22f;
            bloopRect.localScale = new Vector3(squash, stretch, 1f);

            UpdateBloopShadow(curX, curY, bloopRect.localScale);

            yield return null;
        }
        bloopRect.localScale = new Vector3(1.35f, 0.70f, 1f);
        UpdateBloopShadow(-120f, -80f, bloopRect.localScale);
        SpawnSplashTrailDroplet(-120f);

        // Food panics, shivering and revealing its toxic glint right in front of Bloop!
        foodImage.sprite = foodPoisonReveal != null ? foodPoisonReveal : foodRun1;
        float fearTime = 0f;
        while (fearTime < 0.25f)
        {
            fearTime += Time.unscaledDeltaTime;
            float shiver = Mathf.Sin(fearTime * 50f) * 4f;
            foodRect.anchoredPosition = new Vector2(40f + shiver, -95f);
            UpdateFoodShadow(40f + shiver, -95f, foodRect.localScale);
            yield return null;
        }

        // --- ACT 3: The Grand Pounce — Bloop launches directly onto the food & chomps down! ---
        lt = 0f;
        leapDur = 0.44f;
        trailTimer = 0f;
        if (SoundManager.Instance != null) SoundManager.Instance.PlayJumpSound();
        bloopImage.sprite = slimeChompBite;

        while (lt < leapDur)
        {
            lt += Time.unscaledDeltaTime;
            trailTimer += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(lt / leapDur);
            float curX = Mathf.Lerp(-120f, 40f, p);
            float curY = -80f + Mathf.Sin(p * Mathf.PI) * 110f;
            bloopRect.anchoredPosition = new Vector2(curX, curY);

            if (trailTimer >= 0.08f)
            {
                trailTimer = 0f;
                SpawnSplashTrailDroplet(curX);
            }

            float stretch = 1f + Mathf.Sin(p * Mathf.PI) * 0.35f;
            bloopRect.localScale = new Vector3(1f / stretch, stretch, 1f);

            UpdateBloopShadow(curX, curY, bloopRect.localScale);

            yield return null;
        }

        // Direct catch! Bloop lands squarely at (40f, -80f) squashing down over the food!
        bloopRect.anchoredPosition = new Vector2(40f, -80f);
        bloopRect.localScale = new Vector3(1.45f, 0.65f, 1f);
        UpdateBloopShadow(40f, -80f, bloopRect.localScale);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEatFoodSound();
        }

        // Food is sucked into Bloop's mouth shrinking to 0
        float chompTime = 0.16f;
        float ct = 0f;
        Vector2 foodCatchPos = foodRect.anchoredPosition;
        while (ct < chompTime)
        {
            ct += Time.unscaledDeltaTime;
            float cp = ct / chompTime;
            foodRect.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, cp);
            foodRect.anchoredPosition = Vector2.Lerp(foodCatchPos, new Vector2(40f, -70f), cp);
            UpdateFoodShadow(foodRect.anchoredPosition.x, foodRect.anchoredPosition.y, foodRect.localScale);
            yield return null;
        }
        foodImage.gameObject.SetActive(false);
        if (foodShadow != null) foodShadow.gameObject.SetActive(false);

        // Big comical gulp wobble
        yield return new WaitForSecondsRealtime(0.08f);
        bloopRect.localScale = new Vector3(0.82f, 1.35f, 1f); // Stretched tall gulping
        UpdateBloopShadow(40f, -80f, bloopRect.localScale);
        yield return new WaitForSecondsRealtime(0.12f);
        bloopRect.localScale = new Vector3(1.22f, 0.85f, 1f);
        UpdateBloopShadow(40f, -80f, bloopRect.localScale);
        yield return new WaitForSecondsRealtime(0.10f);
        bloopRect.localScale = Vector3.one;
        UpdateBloopShadow(40f, -80f, bloopRect.localScale);

        // Bloop slides smoothly to center stage (0f, -80f)
        float slideT = 0f;
        float slideDur = 0.30f;
        while (slideT < slideDur)
        {
            slideT += Time.unscaledDeltaTime;
            float sp = slideT / slideDur;
            float cx = Mathf.Lerp(40f, 0f, sp);
            bloopRect.anchoredPosition = new Vector2(cx, -80f);
            UpdateBloopShadow(cx, -80f, bloopRect.localScale);
            yield return null;
        }
        bloopRect.anchoredPosition = new Vector2(0f, -80f);
        UpdateBloopShadow(0f, -80f, bloopRect.localScale);

        yield return new WaitForSecondsRealtime(0.24f);

        // --- ACT 4: Toxic Gurgle Realization ---
        bloopImage.sprite = slimeRealizePoison;
        if (SoundManager.Instance != null) SoundManager.Instance.PlayPoisonSound();

        ShowSpeechBubble("...Wait a second?!", new Vector2(0f, 75f));

        // Sickly green-purple belly rumble flicker
        float rumbleTime = 1.15f;
        t = 0f;
        Vector2 basePos = bloopRect.anchoredPosition;
        while (t < rumbleTime)
        {
            t += Time.unscaledDeltaTime;
            float jitterX = UnityEngine.Random.Range(-5f, 5f);
            float jitterY = UnityEngine.Random.Range(-4f, 4f);
            bloopRect.anchoredPosition = basePos + new Vector2(jitterX, jitterY);

            float g = 0.75f + Mathf.Sin(t * 26f) * 0.25f;
            bloopImage.color = new Color(0.6f, g, 0.75f, 1f);
            UpdateBloopShadow(bloopRect.anchoredPosition.x, -80f, bloopRect.localScale);
            yield return null;
        }
        bloopRect.anchoredPosition = basePos;
        bloopImage.color = Color.white;
        UpdateBloopShadow(0f, -80f, bloopRect.localScale);
        HideSpeechBubble();

        yield return new WaitForSecondsRealtime(0.20f);

        // --- ACT 5: 4th-Wall Turn — Bloop looks right through the phone glass at the player ---
        bloopImage.sprite = slimeTurnLookUser;
        bloopRect.localScale = Vector3.one;
        UpdateBloopShadow(0f, -80f, bloopRect.localScale);

        ShowSpeechBubble("...Did you see what I just ate?!", new Vector2(0f, 75f));

        yield return new WaitForSecondsRealtime(1.35f);
        bloopRect.localScale = new Vector3(1.08f, 0.92f, 1f);
        UpdateBloopShadow(0f, -80f, bloopRect.localScale);
        yield return new WaitForSecondsRealtime(0.18f);
        bloopRect.localScale = Vector3.one;
        UpdateBloopShadow(0f, -80f, bloopRect.localScale);
        yield return new WaitForSecondsRealtime(0.60f);

        HideSpeechBubble();

        // --- ACT 6: Cheeks puff out, hysterically gagging, then SPIT! ---
        bloopImage.sprite = slimeGagPuff1;
        ShowSpeechBubble("Hrrk... GLP!", new Vector2(0f, 85f));

        float gagTime = 1.55f;
        t = 0f;
        while (t < gagTime)
        {
            t += Time.unscaledDeltaTime;
            float puffRatio = t / gagTime;
            bloopImage.sprite = (t > 0.70f) ? slimeGagPuff2 : slimeGagPuff1;

            if (t > 0.70f)
                ShowSpeechBubble("MMMPH!!", new Vector2(0f, 95f));

            float puffScale = 1f + puffRatio * 0.60f;
            bloopRect.localScale = new Vector3(puffScale, puffScale * 0.90f, 1f);

            float jitter = Mathf.Sin(t * 36f) * 7f;
            bloopRect.anchoredPosition = basePos + new Vector2(jitter, 0f);

            // Shadow expands dynamically as Bloop's cheeks puff out huge!
            UpdateBloopShadow(bloopRect.anchoredPosition.x, -80f, bloopRect.localScale);

            yield return null;
        }

        HideSpeechBubble();

        // Extreme squash down before the projectile launch
        bloopRect.localScale = new Vector3(1.60f, 0.44f, 1f);
        UpdateBloopShadow(0f, -80f, bloopRect.localScale);
        yield return new WaitForSecondsRealtime(0.12f);

        // LAUNCH SPIT!
        bloopImage.sprite = slimeSpitLaunch;
        bloopRect.localScale = new Vector3(0.65f, 1.55f, 1f);
        UpdateBloopShadow(0f, -80f, bloopRect.localScale);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPukeSound();
        }

        ShowSpeechBubble("PFFFT!!", new Vector2(0f, 85f));

        // Projectile shoots out spinning toward the top-right ceiling light
        if (projectileImage != null)
        {
            projectileImage.gameObject.SetActive(true);
            projectileImage.sprite = spitProjectileSprite;
            projectileRect.anchoredPosition = new Vector2(20f, -40f);

            float pDur = 0.70f;
            float pt = 0f;
            while (pt < pDur)
            {
                pt += Time.unscaledDeltaTime;
                float u = pt / pDur;
                float px = Mathf.Lerp(20f, 520f, u);
                float py = -40f + Mathf.Sin(u * Mathf.PI) * 250f;
                projectileRect.anchoredPosition = new Vector2(px, py);
                projectileRect.localRotation = Quaternion.Euler(0f, 0f, u * 900f);
                yield return null;
            }
            projectileImage.gameObject.SetActive(false);
        }

        // --- ACT 6B: Spotlight loses power and flickers off in a dramatic theater short-circuit! ---
        if (SoundManager.Instance != null) SoundManager.Instance.PlayPoisonSound();

        // 1. Violent electrical malfunction short-circuit flicker
        float[] shortCircuitFlicker = new float[] { 0.12f, 0.95f, 0.05f, 0.80f, 0.0f, 0.65f, 0.08f, 0.40f, 0.0f };
        for (int fl = 0; fl < shortCircuitFlicker.Length; fl++)
        {
            float op = shortCircuitFlicker[fl];
            if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, op * 0.90f);
            if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, op * 0.88f);
            yield return new WaitForSecondsRealtime(0.06f);
        }

        // 2. Complete power failure — Total Blackout!
        if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, 0f);
        if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, 0f);

        // Bloop comical double-take in the pitch dark
        ShowSpeechBubble("...Wait, who turned off the lights?!", new Vector2(0f, 75f));

        // Dramatic suspense blackout duration (1.4 seconds of dark)
        yield return new WaitForSecondsRealtime(1.40f);

        HideSpeechBubble();

        // 3. Emergency backup generator / ignition starter sparks
        if (SoundManager.Instance != null) SoundManager.Instance.PlayCountdownTick(false);
        // Spark 1
        if (spotlightImage != null) spotlightImage.color = new Color(1f, 0.95f, 0.85f, 0.35f);
        if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, 0.35f);
        yield return new WaitForSecondsRealtime(0.08f);

        // Quick drop to dark
        if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, 0f);
        if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, 0f);
        yield return new WaitForSecondsRealtime(0.18f);

        // Spark 2 (stronger)
        if (SoundManager.Instance != null) SoundManager.Instance.PlayCountdownTick(false);
        if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, 0.70f);
        if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, 0.70f);
        yield return new WaitForSecondsRealtime(0.07f);

        // 4. Emergency power stabilizes — warm light hums to full stage illumination
        if (SoundManager.Instance != null) SoundManager.Instance.PlayCountdownTick(true);
        float restoreDur = 0.55f;
        float rElapsed = 0f;
        while (rElapsed < restoreDur)
        {
            rElapsed += Time.unscaledDeltaTime;
            float rProgress = Mathf.Clamp01(rElapsed / restoreDur);
            float rLight = Mathf.Lerp(0.35f, 0.90f, rProgress);
            float rShadow = Mathf.Lerp(0.35f, 0.88f, rProgress);
            if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, rLight);
            if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, rShadow);
            yield return null;
        }
        if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, 0.90f);
        if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, 0.88f);

        // --- ACT 7: Bloop collapses in sweet relief, Bottom Loading Bar reveals & completes ---
        bloopImage.sprite = slimeReliefSmile;
        bloopRect.localScale = new Vector3(1.22f, 0.82f, 1f);
        bloopRect.anchoredPosition = new Vector2(0f, -80f);
        UpdateBloopShadow(0f, -80f, bloopRect.localScale);

        if (SoundManager.Instance != null) SoundManager.Instance.PlayChimeSound();
        ShowSpeechBubble("Phew! That was close!", new Vector2(0f, 75f));

        // Reveal and fill loading bar underneath smoothly over 2.65 seconds
        float loadTime = 2.65f;
        t = 0f;
        string[] steps = {
            "MOUNTING 10 BIOME STAGE ATLASES...",
            "INITIALIZING WHISPERING FOREST...",
            "LOADING TROPICAL, DESERT & OCEAN BGM...",
            "ALL 10 STAGES READY TO DROP!"
        };

        while (t < loadTime)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / loadTime);

            if (loadingBarGroup != null) loadingBarGroup.alpha = Mathf.Min(1f, u * 2.0f);

            // Gentle breathing animation while in relief
            float breath = 1f + Mathf.Sin(t * 3.5f) * 0.05f;
            bloopRect.localScale = new Vector3(1.22f * breath, 0.82f / breath, 1f);
            UpdateBloopShadow(0f, -80f, bloopRect.localScale);

            int stepIdx = Mathf.Min(steps.Length - 1, Mathf.FloorToInt(u * steps.Length));
            int pct = Mathf.RoundToInt(u * 100f);
            UpdateLoadingBar(u, $"{pct}%", steps[stepIdx]);

            yield return null;
        }

        UpdateLoadingBar(1f, "100%", "ALL 10 STAGES READY TO DROP!");
        yield return new WaitForSecondsRealtime(0.45f);

        // Smooth fade out before transitioning to Home Screen
        CanvasGroup splashCg = GetComponent<CanvasGroup>();
        if (splashCg != null)
        {
            float fadeDur = 0.45f;
            float elapsed = 0f;
            while (elapsed < fadeDur)
            {
                elapsed += Time.unscaledDeltaTime;
                splashCg.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDur);
                yield return null;
            }
            splashCg.alpha = 0f;
        }

        HideSpeechBubble();
        currentOnComplete?.Invoke();
    }

    public void ShowSpeechBubble(string text, Vector2 localPos)
    {
        if (speechBubbleGroup == null) return;
        speechBubbleGroup.SetActive(true);
        if (speechBubbleText != null)
        {
            speechBubbleText.text = text;
            speechBubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
            speechBubbleText.verticalOverflow = VerticalWrapMode.Truncate;

            float maxW = 780f;
            float padX = 48f;
            float padY = 32f;

            TextGenerationSettings singleSettings = speechBubbleText.GetGenerationSettings(Vector2.zero);
            float unconstrainedW = speechBubbleText.cachedTextGeneratorForLayout.GetPreferredWidth(text, singleSettings) / speechBubbleText.pixelsPerUnit;

            float targetW;
            float targetH;

            if (unconstrainedW + padX <= 680f)
            {
                targetW = Mathf.Clamp(unconstrainedW + padX, 240f, 700f);
                TextGenerationSettings wrapSettings = speechBubbleText.GetGenerationSettings(new Vector2(targetW - padX, 1000f));
                float h = speechBubbleText.cachedTextGeneratorForLayout.GetPreferredHeight(text, wrapSettings) / speechBubbleText.pixelsPerUnit;
                targetH = Mathf.Clamp(h + padY, 76f, 130f);
            }
            else
            {
                targetW = maxW;
                TextGenerationSettings wrapSettings = speechBubbleText.GetGenerationSettings(new Vector2(targetW - padX, 1000f));
                float h = speechBubbleText.cachedTextGeneratorForLayout.GetPreferredHeight(text, wrapSettings) / speechBubbleText.pixelsPerUnit;
                targetH = Mathf.Clamp(h + padY, 96f, 260f);
            }

            if (speechBubbleRect != null)
            {
                speechBubbleRect.sizeDelta = new Vector2(targetW, targetH);
                RectTransform tRect = speechBubbleText.GetComponent<RectTransform>();
                if (tRect != null)
                {
                    tRect.sizeDelta = new Vector2(targetW - padX, targetH - padY);
                }
            }
            Canvas.ForceUpdateCanvases();
        }
        if (speechBubbleRect != null) speechBubbleRect.anchoredPosition = localPos;
    }

    private void HideSpeechBubble()
    {
        if (speechBubbleGroup == null) return;
        speechBubbleGroup.SetActive(false);
    }

    private void SpawnSplashTrailDroplet(float x, float y = -180f)
    {
        if (slimeTrailSprite == null || stageContainer == null) return;
        GameObject dropObj = new GameObject("Splash_SlimeTrail");
        dropObj.transform.SetParent(stageContainer, false);
        dropObj.transform.SetSiblingIndex(2); // Behind Bloop
        RectTransform rt = dropObj.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(x + UnityEngine.Random.Range(-18f, 18f), y + UnityEngine.Random.Range(-8f, 8f));
        rt.sizeDelta = new Vector2(36f, 36f);
        Image img = dropObj.AddComponent<Image>();
        img.sprite = slimeTrailSprite;
        img.raycastTarget = false;
        img.color = new Color(1f, 1f, 1f, 0.85f);
        StartCoroutine(FadeSplashTrailRoutine(dropObj, rt, img));
    }

    private IEnumerator FadeSplashTrailRoutine(GameObject dropObj, RectTransform rt, Image img)
    {
        if (dropObj == null || rt == null || img == null) yield break;
        float dur = 0.75f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.one * UnityEngine.Random.Range(0.8f, 1.25f);
        Vector3 endScale = startScale * 0.15f;
        rt.localScale = startScale;

        while (elapsed < dur)
        {
            if (dropObj == null || rt == null || img == null) yield break;
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / dur;
            rt.localScale = Vector3.Lerp(startScale, endScale, t);
            img.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.85f, 0f, t));
            yield return null;
        }

        if (dropObj != null)
        {
            Destroy(dropObj);
        }
    }
}
