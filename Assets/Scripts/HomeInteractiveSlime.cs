using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HomeInteractiveSlime : MonoBehaviour, IPointerClickHandler
{
    [Header("UI & Banner References")]
    public Image slimeImage;
    public RectTransform slimeRect;
    public RectTransform bannerPlateRect;
    public RectTransform playButtonRect;
    public GameObject reactionBubbleObj;
    public Text reactionBubbleText;

    [Header("Emotion Sprites")]
    public Sprite spriteIdle;
    public Sprite spriteHappy;
    public Sprite spriteSurprised;
    public Sprite spriteAngry;
    public Sprite spriteDizzy;
    public Sprite spritePeek;
    public Sprite spriteAnnoyed;

    private int tapStreakCount = 0;
    private float lastTapTime = -10f;
    private bool isHiding = false;
    private bool isPeeking = false;
    private bool isBeggingAtPlayButton = false;

    private float idleTimer = 0f;
    private int idleBoredCount = 0;

    private Coroutine activeAnimRoutine;
    private Coroutine activeEmotionResetRoutine;
    private Coroutine beggingBounceRoutine;
    private Coroutine activeBubbleRoutine;

    private Vector2 restingPosition;
    private Vector3 normalScale = Vector3.one;

    private void Awake()
    {
        if (slimeRect == null)
            slimeRect = GetComponent<RectTransform>();
        if (slimeImage == null)
            slimeImage = GetComponent<Image>();
        restingPosition = slimeRect.anchoredPosition;
    }

    private void OnEnable()
    {
        ResetToRestingState();
    }

    private void Update()
    {
        if (isHiding || isPeeking) return;

        // If player touches or clicks ANYWHERE on screen
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            if (isBeggingAtPlayButton)
            {
                // Return to resting place with happy face + dialog!
                SoundManager.Instance?.PlayBonusHeartSound();
                if (activeAnimRoutine != null) StopCoroutine(activeAnimRoutine);
                if (beggingBounceRoutine != null) StopCoroutine(beggingBounceRoutine);
                activeAnimRoutine = StartCoroutine(HappyReturnFromPlayButtonRoutine());
                return;
            }

            idleTimer = 0f;
        }

        // Idle timer accumulation
        if (!isBeggingAtPlayButton)
        {
            idleTimer += Time.unscaledDeltaTime;
            if (idleTimer >= 6.5f)
            {
                idleTimer = 0f;
                TriggerIdleBoredCycle();
            }
        }
    }

    public void SetRestingPosition(Vector2 pos)
    {
        restingPosition = pos;
        if (slimeRect != null && !isHiding && !isBeggingAtPlayButton)
            slimeRect.anchoredPosition = pos;
    }

    public void ResetToRestingState()
    {
        if (activeAnimRoutine != null) StopCoroutine(activeAnimRoutine);
        if (activeEmotionResetRoutine != null) StopCoroutine(activeEmotionResetRoutine);
        if (beggingBounceRoutine != null) StopCoroutine(beggingBounceRoutine);

        isHiding = false;
        isPeeking = false;
        isBeggingAtPlayButton = false;
        tapStreakCount = 0;
        idleTimer = 0f;
        idleBoredCount = 0;

        if (slimeRect != null)
        {
            slimeRect.anchoredPosition = restingPosition;
            slimeRect.localScale = normalScale;
            slimeRect.localRotation = Quaternion.identity;
        }

        transform.SetAsLastSibling();

        if (slimeImage != null && spriteIdle != null)
            slimeImage.sprite = spriteIdle;

        if (reactionBubbleObj != null)
            reactionBubbleObj.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlimeTapped();
    }

    public void OnSlimeTapped()
    {
        idleTimer = 0f;

        // 1. If player touches while Bloop is begging down at the PLAY button!
        if (isBeggingAtPlayButton)
        {
            SoundManager.Instance?.PlayBonusHeartSound();
            if (activeAnimRoutine != null) StopCoroutine(activeAnimRoutine);
            activeAnimRoutine = StartCoroutine(HappyReturnFromPlayButtonRoutine());
            return;
        }

        // 2. If player touches while Bloop is peeking from behind the title banner!
        if (isPeeking)
        {
            SoundManager.Instance?.PlayBonusHeartSound();
            ShowBubble("EEEEK! CAUGHT ME!", 1.4f);
            if (activeAnimRoutine != null) StopCoroutine(activeAnimRoutine);
            activeAnimRoutine = StartCoroutine(DuckBackDownRoutine());
            return;
        }

        // 3. If already deep in hiding, ignore taps until it peeks
        if (isHiding)
            return;

        float now = Time.time;
        if (now - lastTapTime > 3.2f)
            tapStreakCount = 0;

        tapStreakCount++;
        lastTapTime = now;
        idleBoredCount = 0;

        SoundManager.Instance?.PlayEatFoodSound();
        StartCoroutine(SquashBounceRoutine());

        // Check if annoyed threshold reached
        if (tapStreakCount >= 4)
        {
            if (activeAnimRoutine != null) StopCoroutine(activeAnimRoutine);
            activeAnimRoutine = StartCoroutine(AnnoyedHideRoutine());
        }
        else
        {
            TriggerRandomTapEmotion(tapStreakCount);
        }
    }

    private void TriggerIdleBoredCycle()
    {
        idleBoredCount++;
        if (idleBoredCount < 3)
        {
            // Show Bored / Sleepy Emotion
            if (slimeImage != null)
                slimeImage.sprite = (idleBoredCount == 1 && spriteAnnoyed != null) ? spriteAnnoyed : (spriteDizzy != null ? spriteDizzy : spriteIdle);

            string[] boredPhrases = {
                "...Anyone there? *yawn*",
                "I'm so boooored...",
                "Hello? Tap me!",
                "...Is my screen frozen?",
                "Feed me yummy food!",
                "Tap Play to start!"
            };
            ShowBubble(boredPhrases[Random.Range(0, boredPhrases.Length)], 2.5f);

            if (activeEmotionResetRoutine != null) StopCoroutine(activeEmotionResetRoutine);
            activeEmotionResetRoutine = StartCoroutine(RevertToIdleAfterDelay(3.0f));
        }
        else
        {
            // 3rd time idle: Impatient leap down to PLAY button!
            if (activeAnimRoutine != null) StopCoroutine(activeAnimRoutine);
            activeAnimRoutine = StartCoroutine(JumpToPlayButtonBeggingRoutine());
        }
    }

    private IEnumerator JumpToPlayButtonBeggingRoutine()
    {
        isBeggingAtPlayButton = true;
        if (slimeImage != null && spriteAngry != null)
            slimeImage.sprite = spriteAngry;

        ShowBubble("I WANNA PLAY!!", 1.8f);
        SoundManager.Instance?.PlayPoisonSound();

        transform.parent?.SetAsLastSibling();

        // Calculate jump target position relative to parent (bannerRoot) towards Play button (y ≈ -620f)
        Vector2 targetPos = new Vector2(230f, -620f);
        if (playButtonRect != null && slimeRect != null)
        {
            // Position Bloop perched right atop the Play button on the right
            Vector3 worldPlay = playButtonRect.position;
            Vector3 localPlay = slimeRect.parent.InverseTransformPoint(worldPlay);
            targetPos = new Vector2(localPlay.x + 230f, localPlay.y + 110f);
        }

        Vector2 startPos = slimeRect.anchoredPosition;
        float elapsed = 0f;
        float dur = 0.75f;

        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / dur;
            // Arced jump formula
            float arcY = 120f * 4f * t * (1f - t);
            Vector2 basePos = Vector2.Lerp(startPos, targetPos, t);
            slimeRect.anchoredPosition = new Vector2(basePos.x, basePos.y + arcY);

            // Stretch while leaping, squash on landing
            float stretch = 1f + Mathf.Sin(t * Mathf.PI) * 0.28f;
            slimeRect.localScale = new Vector3(1f / stretch, stretch, 1f);

            yield return null;
        }

        slimeRect.anchoredPosition = targetPos;
        slimeRect.localScale = new Vector3(1.30f, 0.70f, 1f);
        yield return new WaitForSecondsRealtime(0.08f);
        slimeRect.localScale = Vector3.one;

        // Start comical impatient bouncing right beside the Play button
        if (beggingBounceRoutine != null) StopCoroutine(beggingBounceRoutine);
        beggingBounceRoutine = StartCoroutine(BeggingBounceLoopRoutine());
    }

    private IEnumerator BeggingBounceLoopRoutine()
    {
        Vector2 basePos = slimeRect.anchoredPosition;
        float t = 0f;
        string[] begLines = {
            "TAP PLAY ALREADY!!",
            "PLEASE PLAY WITH ME!",
            "I AM READY TO EAT! TAP PLAY!",
            "LET'S DROP FOOD! PLEEEASE!"
        };
        int lineIdx = 0;
        float speechTimer = 0f;

        while (isBeggingAtPlayButton)
        {
            t += Time.unscaledDeltaTime * 6.5f;
            speechTimer += Time.unscaledDeltaTime;

            float hopY = Mathf.Abs(Mathf.Sin(t)) * 22f;
            float squishX = 1f + Mathf.Cos(t) * 0.14f;
            float squishY = 1f - Mathf.Cos(t) * 0.14f;

            slimeRect.anchoredPosition = basePos + new Vector2(0f, hopY);
            slimeRect.localScale = new Vector3(squishX, squishY, 1f);

            if (slimeImage != null)
            {
                slimeImage.sprite = (Mathf.Sin(t * 0.5f) > 0) ? (spriteAngry != null ? spriteAngry : spriteIdle) : (spriteSurprised != null ? spriteSurprised : spriteIdle);
            }

            if (speechTimer >= 3.2f)
            {
                speechTimer = 0f;
                lineIdx = (lineIdx + 1) % begLines.Length;
                ShowBubble(begLines[lineIdx], 2.2f);
            }

            yield return null;
        }
    }

    private IEnumerator HappyReturnFromPlayButtonRoutine()
    {
        isBeggingAtPlayButton = false;
        idleBoredCount = 0;
        idleTimer = 0f;

        if (beggingBounceRoutine != null)
        {
            StopCoroutine(beggingBounceRoutine);
            beggingBounceRoutine = null;
        }

        if (slimeImage != null && spriteHappy != null)
            slimeImage.sprite = spriteHappy;

        ShowBubble("YAAAY! LET'S PLAY!", 2.0f);

        Vector2 startPos = slimeRect.anchoredPosition;
        float elapsed = 0f;
        float dur = 0.70f;

        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / dur;
            float arcY = 160f * 4f * t * (1f - t);
            Vector2 basePos = Vector2.Lerp(startPos, restingPosition, t);
            slimeRect.anchoredPosition = new Vector2(basePos.x, basePos.y + arcY);

            // Joyful spin in the air
            slimeRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 2f) * 35f);
            yield return null;
        }

        slimeRect.anchoredPosition = restingPosition;
        slimeRect.localRotation = Quaternion.identity;
        transform.parent?.SetSiblingIndex(2);

        SoundManager.Instance?.PlayEatFoodSound();
        yield return SquashBounceRoutine();

        if (slimeImage != null && spriteIdle != null)
            slimeImage.sprite = spriteIdle;

        activeAnimRoutine = null;
    }

    private void TriggerRandomTapEmotion(int streak)
    {
        Sprite targetSprite = spriteIdle;
        string speech = "Boing!";

        if (streak == 1)
        {
            string[] s1 = new string[] { "Hehe!", "Hi!", "*Poke!*", "Boing~", ":D" };
            speech = s1[Random.Range(0, s1.Length)];
            targetSprite = Random.value > 0.5f ? spriteHappy : spriteSurprised;
        }
        else if (streak == 2)
        {
            string[] s2 = new string[] { "Tickles!", "Stop that~", "Whoa!", "Hehe!", "Squeak!" };
            speech = s2[Random.Range(0, s2.Length)];
            targetSprite = Random.value > 0.5f ? spriteHappy : spriteDizzy;
        }
        else if (streak == 3)
        {
            string[] s3 = new string[] { "Hey, watch it!", "Don't push it!", "Grrr!", "Hands off!", ">_<" };
            speech = s3[Random.Range(0, s3.Length)];
            targetSprite = Random.value > 0.5f ? spriteAngry : spriteAnnoyed;
        }

        if (slimeImage != null && targetSprite != null)
            slimeImage.sprite = targetSprite;

        ShowBubble(speech, 1.4f);

        if (activeEmotionResetRoutine != null)
            StopCoroutine(activeEmotionResetRoutine);
        activeEmotionResetRoutine = StartCoroutine(RevertToIdleAfterDelay(1.5f));
    }

    private IEnumerator RevertToIdleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isHiding && !isPeeking && !isBeggingAtPlayButton)
        {
            if (slimeImage != null && spriteIdle != null)
                slimeImage.sprite = spriteIdle;
        }
    }

    private IEnumerator SquashBounceRoutine()
    {
        if (slimeRect == null) yield break;

        float elapsed = 0f;
        float duration = 0.28f;
        Vector3 squish = new Vector3(1.22f, 0.80f, 1f);
        Vector3 stretch = new Vector3(0.86f, 1.18f, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            if (t < 0.35f)
            {
                slimeRect.localScale = Vector3.Lerp(Vector3.one, squish, t / 0.35f);
            }
            else if (t < 0.70f)
            {
                slimeRect.localScale = Vector3.Lerp(squish, stretch, (t - 0.35f) / 0.35f);
            }
            else
            {
                slimeRect.localScale = Vector3.Lerp(stretch, Vector3.one, (t - 0.70f) / 0.30f);
            }
            yield return null;
        }
        slimeRect.localScale = Vector3.one;
    }

    private IEnumerator AnnoyedHideRoutine()
    {
        isHiding = true;
        isPeeking = false;

        if (slimeImage != null && spriteAngry != null)
            slimeImage.sprite = spriteAngry;

        ShowBubble("THAT'S IT! HMPH!", 1.2f);
        SoundManager.Instance?.PlayPoisonSound();

        float quiverTime = 0f;
        while (quiverTime < 0.5f)
        {
            quiverTime += Time.unscaledDeltaTime;
            float wobbleX = Mathf.Sin(quiverTime * 45f) * 8f;
            slimeRect.anchoredPosition = restingPosition + new Vector2(wobbleX, 0f);
            yield return null;
        }
        slimeRect.anchoredPosition = restingPosition;

        transform.SetAsFirstSibling();

        if (reactionBubbleObj != null)
            reactionBubbleObj.SetActive(false);

        Vector2 hideTargetPos = new Vector2(0f, 20f);
        float slideTime = 0f;
        float slideDur = 0.45f;
        Vector2 startPos = slimeRect.anchoredPosition;

        if (slimeImage != null && spriteAnnoyed != null)
            slimeImage.sprite = spriteAnnoyed;

        while (slideTime < slideDur)
        {
            slideTime += Time.unscaledDeltaTime;
            float t = slideTime / slideDur;
            t = t * t * (3f - 2f * t);
            slimeRect.anchoredPosition = Vector2.Lerp(startPos, hideTargetPos, t);
            yield return null;
        }
        slimeRect.anchoredPosition = hideTargetPos;

        yield return new WaitForSeconds(2.0f);

        isPeeking = true;
        if (slimeImage != null && spritePeek != null)
            slimeImage.sprite = spritePeek;

        Vector2 peekPos = new Vector2(160f, 150f);
        float peekElapsed = 0f;
        float peekDur = 0.5f;

        while (peekElapsed < peekDur)
        {
            peekElapsed += Time.unscaledDeltaTime;
            float t = peekElapsed / peekDur;
            slimeRect.anchoredPosition = Vector2.Lerp(hideTargetPos, peekPos, t);
            yield return null;
        }
        slimeRect.anchoredPosition = peekPos;

        ShowBubble("...Still there?", 1.8f);

        float lookTimer = 0f;
        while (lookTimer < 1.8f)
        {
            lookTimer += Time.unscaledDeltaTime;
            float tilt = Mathf.Sin(lookTimer * 6f) * 6f;
            slimeRect.localRotation = Quaternion.Euler(0f, 0f, tilt);
            yield return null;
        }
        slimeRect.localRotation = Quaternion.identity;

        yield return CoastClearReturnRoutine(hideTargetPos);
    }

    private IEnumerator DuckBackDownRoutine()
    {
        isPeeking = false;
        if (slimeImage != null && spriteSurprised != null)
            slimeImage.sprite = spriteSurprised;

        Vector2 duckPos = new Vector2(0f, 20f);
        float duckTime = 0f;
        float duckDur = 0.22f;
        Vector2 cur = slimeRect.anchoredPosition;

        while (duckTime < duckDur)
        {
            duckTime += Time.unscaledDeltaTime;
            slimeRect.anchoredPosition = Vector2.Lerp(cur, duckPos, duckTime / duckDur);
            yield return null;
        }
        slimeRect.anchoredPosition = duckPos;

        yield return new WaitForSeconds(1.6f);

        isPeeking = true;
        if (slimeImage != null && spritePeek != null)
            slimeImage.sprite = spritePeek;

        Vector2 peekPos = new Vector2(160f, 150f);
        float pTime = 0f;
        while (pTime < 0.35f)
        {
            pTime += Time.unscaledDeltaTime;
            slimeRect.anchoredPosition = Vector2.Lerp(duckPos, peekPos, pTime / 0.35f);
            yield return null;
        }
        slimeRect.anchoredPosition = peekPos;

        ShowBubble("...Anyone?", 1.4f);
        yield return new WaitForSeconds(1.5f);

        yield return CoastClearReturnRoutine(duckPos);
    }

    private IEnumerator CoastClearReturnRoutine(Vector2 hideOrigin)
    {
        isPeeking = false;

        if (slimeImage != null && spriteHappy != null)
            slimeImage.sprite = spriteHappy;

        ShowBubble("Coast is clear! Whew!", 1.5f);

        Vector2 startHop = slimeRect.anchoredPosition;
        float hopElapsed = 0f;
        float hopDur = 0.65f;

        while (hopElapsed < hopDur)
        {
            hopElapsed += Time.unscaledDeltaTime;
            float t = hopElapsed / hopDur;
            float arcY = 70f * 4f * t * (1f - t);
            Vector2 basePos = Vector2.Lerp(startHop, restingPosition, t);
            slimeRect.anchoredPosition = new Vector2(basePos.x, basePos.y + arcY);

            if (t > 0.45f && transform.GetSiblingIndex() == 0)
            {
                transform.SetSiblingIndex(1); // Back in front of the game title!
            }

            yield return null;
        }

        slimeRect.anchoredPosition = restingPosition;
        transform.SetSiblingIndex(1); // In front of the game title!

        SoundManager.Instance?.PlayEatFoodSound();
        yield return SquashBounceRoutine();

        if (slimeImage != null && spriteIdle != null)
            slimeImage.sprite = spriteIdle;

        tapStreakCount = 0;
        isHiding = false;
        isPeeking = false;
        activeAnimRoutine = null;
    }

    private void ShowBubble(string text, float duration)
    {
        if (reactionBubbleObj == null || reactionBubbleText == null)
            return;

        reactionBubbleText.text = text;

        RectTransform bblRect = reactionBubbleObj.GetComponent<RectTransform>();
        RectTransform txtRect = reactionBubbleText.GetComponent<RectTransform>();

        reactionBubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
        reactionBubbleText.verticalOverflow = VerticalWrapMode.Truncate;

        float maxBubbleWidth = 720f;
        float minBubbleWidth = 300f;
        float padX = 64f;
        float padY = 44f;

        // 1. Measure preferred width on a single line
        TextGenerationSettings singleLineSettings = reactionBubbleText.GetGenerationSettings(Vector2.zero);
        float unconstrainedW = reactionBubbleText.cachedTextGeneratorForLayout.GetPreferredWidth(text, singleLineSettings) / reactionBubbleText.pixelsPerUnit;

        float targetW;
        float targetH;

        if (unconstrainedW + padX <= 540f)
        {
            // Fits cleanly on a single line
            targetW = Mathf.Clamp(unconstrainedW + padX, minBubbleWidth, 560f);
            TextGenerationSettings wrapSettings = reactionBubbleText.GetGenerationSettings(new Vector2(targetW - padX, 1000f));
            float h = reactionBubbleText.cachedTextGeneratorForLayout.GetPreferredHeight(text, wrapSettings) / reactionBubbleText.pixelsPerUnit;
            targetH = Mathf.Clamp(h + padY, 86f, 140f);
        }
        else
        {
            // Needs multiline wrap
            targetW = maxBubbleWidth;
            TextGenerationSettings wrapSettings = reactionBubbleText.GetGenerationSettings(new Vector2(targetW - padX, 1000f));
            float h = reactionBubbleText.cachedTextGeneratorForLayout.GetPreferredHeight(text, wrapSettings) / reactionBubbleText.pixelsPerUnit;
            targetH = Mathf.Clamp(h + padY, 110f, 320f);
        }

        if (bblRect != null)
        {
            bblRect.sizeDelta = new Vector2(targetW, targetH);
            if (txtRect != null)
            {
                txtRect.sizeDelta = new Vector2(targetW - padX, targetH - padY);
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
    }

    private IEnumerator PopBubbleScaleRoutine(float duration)
    {
        if (reactionBubbleObj == null) yield break;

        float popTime = 0f;
        while (popTime < 0.16f)
        {
            popTime += Time.unscaledDeltaTime;
            float s = Mathf.Lerp(0.4f, 1.15f, popTime / 0.16f);
            reactionBubbleObj.transform.localScale = Vector3.one * s;
            yield return null;
        }
        reactionBubbleObj.transform.localScale = Vector3.one;

        yield return new WaitForSeconds(duration);

        float fadeTime = 0f;
        while (fadeTime < 0.14f)
        {
            fadeTime += Time.unscaledDeltaTime;
            float s = Mathf.Lerp(1f, 0.2f, fadeTime / 0.14f);
            reactionBubbleObj.transform.localScale = Vector3.one * s;
            yield return null;
        }

        reactionBubbleObj.SetActive(false);
    }
}
