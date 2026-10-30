using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using UnityEngine.EventSystems;



[RequireComponent(typeof(SpriteRenderer))]

[RequireComponent(typeof(CircleCollider2D))]

public class SlimeController : MonoBehaviour

{

    private static SlimeController _instance;

    public static SlimeController Instance

    {

        get

        {

            if (_instance == null) _instance = FindFirstObjectByType<SlimeController>();

            return _instance;

        }

        private set { _instance = value; }

    }



    public enum AnimState

    {

        Idle,

        Move,

        Eat,

        Hurt,

        Dead

    }



    [Header("Movement Settings")]

    public float dragFollowSpeed = 18f;

    public float tapGlideSpeed = 14f;

    public float keyboardMoveSpeed = 9f;

    public float bottomOffset = 1.25f;



    [Header("Jiggle / Squash & Stretch")]

    public Vector3 baseScale = Vector3.one;

    public float frameRate = 14f;



    [Header("Sprite Animation Frames")]

    public SpriteRenderer spriteRenderer;

    public Sprite[] idleFrames;

    public Sprite[] moveFrames;

    public Sprite[] eatFrames;

    public Sprite[] hurtFrames;

    public Sprite[] deadFrames;



    [Header("Mouth Animation (Pixar Eat and Chomp)")]

    public SpriteRenderer mouthRenderer;

    public Sprite mouthOpenSprite;

    public Sprite mouthChewSprite;

    public Sprite mouthSmileSprite;

    private Coroutine mouthRoutine;

    private readonly Vector3 mouthBaseOffset = new Vector3(-0.01f, -0.075f, 0f);



    [Header("Slime Trail")]

    public Sprite slimeTrailSprite;

    private float trailSpawnTimer = 0f;

    private const float TrailSpawnInterval = 0.075f;

    private static Sprite cachedSlimeTrailSprite;



    private AnimState currentAnimState = AnimState.Idle;

    private int currentFrameIndex = 0;

    private float frameTimer = 0f;

    private float specialStateTimer = 0f;



    private float targetX = 0f;

    private bool hasGlideTarget = false;

    private float currentVelocityX = 0f;

    private Vector3 currentDeformScale = Vector3.one;

    private Vector3 scaleVelocity = Vector3.zero;

    private Camera mainCam;



    // Power-Up Visual Effect Renderers on the Slime

    private SpriteRenderer shieldBubbleRenderer;

    private SpriteRenderer shieldRuneOrbitRenderer;

    private SpriteRenderer magnetWaveOuterRenderer;

    private SpriteRenderer magnetWaveInnerRenderer;

    private SpriteRenderer timeSlowClockRenderer;

    private SpriteRenderer healBurstRenderer;



    private float healBurstTimer = 0f;

    private float shieldFlashTimer = 0f;



    private static Sprite cachedShieldBubbleSprite;

    private static Sprite cachedShieldRuneSprite;

    private static Sprite cachedMagnetWaveSprite;

    private static Sprite cachedTimeSlowClockSprite;

    private static Sprite cachedDefaultMouthSprite;

    private static Sprite GetOrCreateDefaultMouthSprite()

    {

        if (cachedDefaultMouthSprite != null) return cachedDefaultMouthSprite;

        int size = 64;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        tex.filterMode = FilterMode.Point;

        Color[] cols = new Color[size * size];

        for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

        for (int y = 14; y <= 50; y++)

        {

            for (int x = 8; x <= 56; x++)

            {

                float dx = (x - 32f) / 22f;

                float dy = (y - 32f) / 16f;

                if (dx * dx + dy * dy <= 1f)

                {

                    cols[y * size + x] = (y < 26) ? new Color(0.92f, 0.42f, 0.55f, 1f) : new Color(0.12f, 0.08f, 0.14f, 1f);

                }

            }

        }

        tex.SetPixels(cols);

        tex.Apply();

        cachedDefaultMouthSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);

        return cachedDefaultMouthSprite;

    }



    private static Sprite cachedHealBurstSprite;



    private void Awake()

    {

        Instance = this;

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        CircleCollider2D col = GetComponent<CircleCollider2D>();

        if (col != null)

        {

            col.radius = 0.38f;

            col.offset = new Vector2(0f, -0.04f);

        }

        mainCam = Camera.main;

        currentDeformScale = baseScale;

        EnsurePowerUpAuraVisuals();

    }



    private void Start()

    {

        float startY = ResponsiveCamera.BottomBound + bottomOffset;

        transform.position = new Vector3(0f, startY, 0f);

        targetX = 0f;

    }



    private void Update()

    {

        if (mainCam == null) mainCam = Camera.main;



        bool isPlaying = GameManager.Instance == null || GameManager.Instance.IsPlaying;

        if (isPlaying)

        {

            HandleInputAndMovement();

        }



        UpdateAnimationState(isPlaying);

        ApplyProceduralJiggle(isPlaying);

        UpdatePowerUpAuraVisuals();



        if (isPlaying && Mathf.Abs(currentVelocityX) > 0.6f)

        {

            trailSpawnTimer += Time.deltaTime;

            if (trailSpawnTimer >= TrailSpawnInterval)

            {

                trailSpawnTimer = 0f;

                SpawnSlimeTrailDroplet();

            }

        }

    }



    private void SpawnSlimeTrailDroplet()

    {

        Sprite s = slimeTrailSprite != null ? slimeTrailSprite : cachedSlimeTrailSprite;

        if (s == null) return;



        GameObject droplet = new GameObject("SlimeTrail_Drop");

        droplet.transform.position = new Vector3(

            transform.position.x + UnityEngine.Random.Range(-0.14f, 0.14f),

            transform.position.y - 0.28f + UnityEngine.Random.Range(-0.03f, 0.03f),

            0f

        );

        SpriteRenderer sr = droplet.AddComponent<SpriteRenderer>();

        sr.sprite = s;

        sr.sortingOrder = 2; // Behind slime

        sr.color = new Color(1f, 1f, 1f, 0.85f);

        droplet.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.65f, 1.05f);



        StartCoroutine(FadeTrailDropletRoutine(droplet, sr));

    }



    private IEnumerator FadeTrailDropletRoutine(GameObject droplet, SpriteRenderer sr)

    {

        if (droplet == null || sr == null) yield break;

        float dur = 0.65f;

        float elapsed = 0f;

        Vector3 startScale = droplet.transform.localScale;

        Vector3 endScale = startScale * 0.20f;

        Color c = sr.color;



        while (elapsed < dur)

        {

            if (droplet == null || sr == null) yield break;

            elapsed += Time.deltaTime;

            float t = elapsed / dur;

            droplet.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            sr.color = new Color(c.r, c.g, c.b, Mathf.Lerp(0.85f, 0f, t));

            yield return null;

        }



        if (droplet != null)

        {

            Destroy(droplet);

        }

    }



    public void EnsurePowerUpAuraVisuals()

    {

        int baseOrder = (spriteRenderer != null) ? spriteRenderer.sortingOrder : 5;



        // 1. Barrier Shield Glowing Dome + Rotating Rune Ring

        shieldBubbleRenderer = GetOrCreateChildRenderer("ShieldBubble_Aura", new Vector3(0f, 0.02f, 0f), baseOrder + 2, GetOrCreateShieldBubbleSprite());

        shieldRuneOrbitRenderer = GetOrCreateChildRenderer("ShieldRune_Orbit", new Vector3(0f, 0.02f, 0f), baseOrder + 3, GetOrCreateShieldRuneSprite());



        // 2. Star Magnet Contracting Magnetic Field Waves (Outer & Inner)

        magnetWaveOuterRenderer = GetOrCreateChildRenderer("MagnetField_OuterWave", new Vector3(0f, 0.04f, 0f), baseOrder - 1, GetOrCreateMagnetWaveSprite());

        magnetWaveInnerRenderer = GetOrCreateChildRenderer("MagnetField_InnerWave", new Vector3(0f, 0.04f, 0f), baseOrder + 1, GetOrCreateMagnetWaveSprite());



        // 3. Chrono Time-Slow Rotating Clock-Dial Aura

        timeSlowClockRenderer = GetOrCreateChildRenderer("TimeSlow_ClockAura", new Vector3(0f, 0.04f, 0f), baseOrder - 1, GetOrCreateTimeSlowClockSprite());



        // 4. Golden Heart Heal Sparkle Burst

        healBurstRenderer = GetOrCreateChildRenderer("HealSparkle_Burst", new Vector3(0f, 0.12f, 0f), baseOrder + 4, GetOrCreateHealBurstSprite());



        // 5. Attached Animated Mouth on Slime Face

        mouthRenderer = GetOrCreateChildRenderer("Slime_Mouth", mouthBaseOffset, baseOrder + 2, mouthOpenSprite != null ? mouthOpenSprite : GetOrCreateDefaultMouthSprite());

        if (mouthRenderer != null)

        {

            mouthRenderer.transform.localScale = Vector3.one * 0.45f;

            mouthRenderer.gameObject.SetActive(false);

        }

    }



    private SpriteRenderer GetOrCreateChildRenderer(string childName, Vector3 localPos, int sortingOrder, Sprite sp)

    {

        Transform existing = transform.Find(childName);

        SpriteRenderer sr;

        if (existing != null)

        {

            sr = existing.GetComponent<SpriteRenderer>();

            if (sr == null) sr = existing.gameObject.AddComponent<SpriteRenderer>();

        }

        else

        {

            GameObject obj = new GameObject(childName);

            obj.transform.SetParent(transform, false);

            obj.transform.localPosition = localPos;

            sr = obj.AddComponent<SpriteRenderer>();

            obj.SetActive(false);

        }

        sr.sortingOrder = sortingOrder;

        sr.sprite = sp;

        return sr;

    }



    public void UpdatePowerUpAuraVisuals()

    {

        if (shieldBubbleRenderer == null || magnetWaveOuterRenderer == null || timeSlowClockRenderer == null)

        {

            EnsurePowerUpAuraVisuals();

        }



        float t = Application.isPlaying ? Time.time : 0.35f;

        float dt = Application.isPlaying ? Time.deltaTime : 0.016f;



        bool shieldOn = (GameManager.Instance != null && GameManager.Instance.IsShieldActive) || shieldFlashTimer > 0f;

        bool magnetOn = GameManager.Instance != null && GameManager.Instance.IsMagnetActive;

        bool timeSlowOn = GameManager.Instance != null && GameManager.Instance.IsTimeSlowActive;



        // --- 1. BARRIER SHIELD GLOWING BUBBLE & ORBITING RUNE CREST ---

        if (shieldBubbleRenderer != null)

        {

            if (shieldBubbleRenderer.gameObject.activeSelf != shieldOn)

                shieldBubbleRenderer.gameObject.SetActive(shieldOn);



            if (shieldOn)

            {

                if (shieldFlashTimer > 0f)

                {

                    shieldFlashTimer -= dt;

                    float flashU = 1f - Mathf.Clamp01(shieldFlashTimer / 0.35f);

                    float expand = Mathf.Lerp(1.05f, 1.95f, flashU);

                    shieldBubbleRenderer.transform.localScale = new Vector3(expand, expand, 1f);

                    shieldBubbleRenderer.color = new Color(0.65f, 1.0f, 1.0f, 1f - flashU);

                }

                else

                {

                    float pulse = 1.05f + Mathf.Sin(t * 6.2f) * 0.06f;

                    shieldBubbleRenderer.transform.localScale = new Vector3(pulse, pulse, 1f);

                    float alpha = 0.78f + Mathf.Sin(t * 5.0f) * 0.16f;

                    shieldBubbleRenderer.color = new Color(0.32f, 0.95f, 1.0f, alpha);

                }

            }

        }



        if (shieldRuneOrbitRenderer != null)

        {

            bool showRunes = GameManager.Instance != null && GameManager.Instance.IsShieldActive;

            if (shieldRuneOrbitRenderer.gameObject.activeSelf != showRunes)

                shieldRuneOrbitRenderer.gameObject.SetActive(showRunes);



            if (showRunes)

            {

                shieldRuneOrbitRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, t * 75f);

                shieldRuneOrbitRenderer.transform.localScale = Vector3.one * (1.08f + Mathf.Cos(t * 4.5f) * 0.04f);

                shieldRuneOrbitRenderer.color = new Color(0.75f, 0.98f, 1.0f, 0.88f);

            }

        }



        // --- 2. STAR MAGNET PULSING / CONTRACTING MAGNETIC FIELD ---

        if (magnetWaveOuterRenderer != null && magnetWaveInnerRenderer != null)

        {

            if (magnetWaveOuterRenderer.gameObject.activeSelf != magnetOn)

                magnetWaveOuterRenderer.gameObject.SetActive(magnetOn);

            if (magnetWaveInnerRenderer.gameObject.activeSelf != magnetOn)

                magnetWaveInnerRenderer.gameObject.SetActive(magnetOn);



            if (magnetOn)

            {

                // Two staggered inward-contracting magnetic attraction waves!

                float wave1 = Mathf.Repeat(t * 1.35f, 1f);

                float scale1 = Mathf.Lerp(1.85f, 0.65f, wave1);

                float alpha1 = Mathf.Sin(wave1 * Mathf.PI) * 0.85f;

                magnetWaveOuterRenderer.transform.localScale = new Vector3(scale1, scale1, 1f);

                magnetWaveOuterRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, t * 55f);

                magnetWaveOuterRenderer.color = new Color(1.0f, 0.88f, 0.25f, Mathf.Max(0.35f, alpha1));



                float wave2 = Mathf.Repeat(t * 1.35f + 0.5f, 1f);

                float scale2 = Mathf.Lerp(1.85f, 0.65f, wave2);

                float alpha2 = Mathf.Sin(wave2 * Mathf.PI) * 0.85f;

                magnetWaveInnerRenderer.transform.localScale = new Vector3(scale2, scale2, 1f);

                magnetWaveInnerRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, -t * 70f);

                magnetWaveInnerRenderer.color = new Color(1.0f, 0.45f, 0.72f, Mathf.Max(0.35f, alpha2));

            }

        }



        // --- 3. CHRONO TIME-SLOW ROTATING CLOCK-DIAL AURA ---

        if (timeSlowClockRenderer != null)

        {

            if (timeSlowClockRenderer.gameObject.activeSelf != timeSlowOn)

                timeSlowClockRenderer.gameObject.SetActive(timeSlowOn);



            if (timeSlowOn)

            {

                float clockPulse = 1.32f + Mathf.Sin(t * 3.2f) * 0.05f;

                timeSlowClockRenderer.transform.localScale = new Vector3(clockPulse, clockPulse, 1f);

                timeSlowClockRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, -t * 38f);

                timeSlowClockRenderer.color = new Color(0.78f, 0.58f, 1.0f, 0.82f);

            }

        }



        // --- 4. GOLDEN HEART HEAL SPARKLE BURST ---

        if (healBurstRenderer != null)

        {

            bool showHeal = healBurstTimer > 0f;

            if (healBurstRenderer.gameObject.activeSelf != showHeal)

                healBurstRenderer.gameObject.SetActive(showHeal);



            if (showHeal)

            {

                healBurstTimer -= dt;

                float u = 1f - Mathf.Clamp01(healBurstTimer / 0.65f);

                float s = Mathf.Lerp(0.75f, 1.65f, u);

                healBurstRenderer.transform.localScale = new Vector3(s, s, 1f);

                healBurstRenderer.transform.localPosition = new Vector3(0f, 0.1f + u * 0.55f, 0f);

                healBurstRenderer.color = new Color(1.0f, 0.90f, 0.35f, 1f - u);

            }

        }

    }



    public void TriggerCelebration()

    {

        TriggerHealBurstEffect();

    }



    public void TriggerHealBurstEffect()

    {

        healBurstTimer = 0.65f;

        TriggerEatEffect();

        UpdatePowerUpAuraVisuals();

    }



    public void TriggerShieldBlockEffect()

    {

        shieldFlashTimer = 0.35f;

        TriggerEatEffect();

        UpdatePowerUpAuraVisuals();

    }



    private static Sprite GetOrCreateShieldBubbleSprite()

    {

        if (cachedShieldBubbleSprite != null) return cachedShieldBubbleSprite;

        int size = 64;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        tex.filterMode = FilterMode.Point;

        Color[] cols = new Color[size * size];

        float center = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)

        {

            for (int x = 0; x < size; x++)

            {

                float dx = x - center;

                float dy = y - center;

                float dist = Mathf.Sqrt(dx * dx + dy * dy) / center;

                if (dist > 1.0f)

                {

                    cols[y * size + x] = Color.clear;

                }

                else if (dist >= 0.85f)

                {

                    // Crisp glowing outer energy rim

                    cols[y * size + x] = new Color(0.55f, 0.98f, 1.0f, 0.95f);

                }

                else if (dist >= 0.72f)

                {

                    cols[y * size + x] = new Color(0.22f, 0.85f, 1.0f, 0.52f);

                }

                else

                {

                    // Specular top-left highlight arc inside the bubble

                    bool isHighlight = (dx < -4f && dy > 4f && dist > 0.45f && dist < 0.66f);

                    cols[y * size + x] = isHighlight

                        ? new Color(0.90f, 1.0f, 1.0f, 0.85f)

                        : new Color(0.18f, 0.75f, 1.0f, 0.18f);

                }

            }

        }

        tex.SetPixels(cols);

        tex.Apply();

        cachedShieldBubbleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 38f);

        return cachedShieldBubbleSprite;

    }



    private static Sprite GetOrCreateShieldRuneSprite()

    {

        if (cachedShieldRuneSprite != null) return cachedShieldRuneSprite;

        int size = 64;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        tex.filterMode = FilterMode.Point;

        Color[] cols = new Color[size * size];

        float center = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)

        {

            for (int x = 0; x < size; x++)

            {

                float dx = x - center;

                float dy = y - center;

                float dist = Mathf.Sqrt(dx * dx + dy * dy) / center;

                float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

                if (angle < 0f) angle += 360f;



                // 4 orbiting diamond shield nodes at 0, 90, 180, 270 degrees

                bool nearRing = dist >= 0.88f && dist <= 0.98f;

                bool isNode = nearRing && (Mathf.Abs(angle % 90f - 45f) < 12f);

                cols[y * size + x] = isNode ? new Color(0.85f, 1.0f, 1.0f, 0.95f) : Color.clear;

            }

        }

        tex.SetPixels(cols);

        tex.Apply();

        cachedShieldRuneSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 36f);

        return cachedShieldRuneSprite;

    }



    private static Sprite GetOrCreateMagnetWaveSprite()

    {

        if (cachedMagnetWaveSprite != null) return cachedMagnetWaveSprite;

        int size = 64;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        tex.filterMode = FilterMode.Point;

        Color[] cols = new Color[size * size];

        float center = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)

        {

            for (int x = 0; x < size; x++)

            {

                float dx = x - center;

                float dy = y - center;

                float dist = Mathf.Sqrt(dx * dx + dy * dy) / center;

                float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

                if (angle < 0f) angle += 360f;



                // Dashed magnetic flux ring with 8 field arcs

                bool onOuterRing = dist >= 0.84f && dist <= 0.95f && ((int)(angle / 22.5f) % 2 == 0);

                bool onInnerRing = dist >= 0.52f && dist <= 0.60f && ((int)((angle + 11f) / 22.5f) % 2 == 0);

                if (onOuterRing)

                    cols[y * size + x] = new Color(1.0f, 0.92f, 0.35f, 0.95f);

                else if (onInnerRing)

                    cols[y * size + x] = new Color(0.45f, 0.95f, 1.0f, 0.85f);

                else

                    cols[y * size + x] = Color.clear;

            }

        }

        tex.SetPixels(cols);

        tex.Apply();

        cachedMagnetWaveSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 36f);

        return cachedMagnetWaveSprite;

    }



    private static Sprite GetOrCreateTimeSlowClockSprite()

    {

        if (cachedTimeSlowClockSprite != null) return cachedTimeSlowClockSprite;

        int size = 64;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        tex.filterMode = FilterMode.Point;

        Color[] cols = new Color[size * size];

        float center = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)

        {

            for (int x = 0; x < size; x++)

            {

                float dx = x - center;

                float dy = y - center;

                float dist = Mathf.Sqrt(dx * dx + dy * dy) / center;

                float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

                if (angle < 0f) angle += 360f;



                bool outerRim = dist >= 0.88f && dist <= 0.96f;

                bool clockTick = (dist >= 0.72f && dist <= 0.88f) && (Mathf.Abs(angle % 30f) < 4.5f);

                bool clockHand = (dist <= 0.68f) && ((Mathf.Abs(dx) <= 1.2f && dy > 0f) || (Mathf.Abs(dy) <= 1.2f && dx > 0f && dist <= 0.48f));



                if (outerRim || clockTick)

                    cols[y * size + x] = new Color(0.82f, 0.68f, 1.0f, 0.92f);

                else if (clockHand)

                    cols[y * size + x] = new Color(0.55f, 0.95f, 1.0f, 0.90f);

                else if (dist < 0.88f)

                    cols[y * size + x] = new Color(0.45f, 0.25f, 0.80f, 0.14f);

                else

                    cols[y * size + x] = Color.clear;

            }

        }

        tex.SetPixels(cols);

        tex.Apply();

        cachedTimeSlowClockSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 36f);

        return cachedTimeSlowClockSprite;

    }



    private static Sprite GetOrCreateHealBurstSprite()

    {

        if (cachedHealBurstSprite != null) return cachedHealBurstSprite;

        int size = 64;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        tex.filterMode = FilterMode.Point;

        Color[] cols = new Color[size * size];

        // Draw 4 pixel-art '+' heal crosses around a ring

        Vector2[] centers =

        {

            new Vector2(16, 32),

            new Vector2(48, 32),

            new Vector2(24, 48),

            new Vector2(40, 48)

        };

        for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

        foreach (Vector2 c in centers)

        {

            int cx = Mathf.RoundToInt(c.x);

            int cy = Mathf.RoundToInt(c.y);

            for (int d = -4; d <= 4; d++)

            {

                for (int w = -1; w <= 1; w++)

                {

                    cols[(cy + w) * size + (cx + d)] = new Color(1.0f, 0.92f, 0.35f, 1f);

                    cols[(cy + d) * size + (cx + w)] = new Color(1.0f, 0.92f, 0.35f, 1f);

                }

            }

        }

        tex.SetPixels(cols);

        tex.Apply();

        cachedHealBurstSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 36f);

        return cachedHealBurstSprite;

    }



    private void HandleInputAndMovement()

    {

        float fixedY = ResponsiveCamera.BottomBound + bottomOffset;

        float prevX = transform.position.x;



        bool pointerDown = false;

        bool pointerHeld = false;

        Vector2 screenPos = Vector2.zero;

        float keyboardAxis = 0f;



        ReadUnifiedInput(out pointerDown, out pointerHeld, out screenPos, out keyboardAxis);



        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();



        if (!overUI && (pointerDown || pointerHeld) && mainCam != null)

        {

            Vector3 worldPoint = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCam.transform.position.z)));

            targetX = Mathf.Clamp(worldPoint.x, ResponsiveCamera.LeftBound, ResponsiveCamera.RightBound);

            hasGlideTarget = true;

        }

        else if (Mathf.Abs(keyboardAxis) > 0.01f)

        {

            hasGlideTarget = false;

            targetX = Mathf.Clamp(transform.position.x + keyboardAxis * keyboardMoveSpeed * Time.deltaTime, ResponsiveCamera.LeftBound, ResponsiveCamera.RightBound);

            transform.position = new Vector3(targetX, fixedY, 0f);

        }



        if (hasGlideTarget)

        {

            float speed = pointerHeld ? dragFollowSpeed : tapGlideSpeed;

            float t = 1f - Mathf.Exp(-speed * Time.deltaTime);
            float newX = Mathf.Lerp(transform.position.x, targetX, t);

            if (!pointerHeld && Mathf.Abs(newX - targetX) < 0.03f)

            {

                newX = targetX;

                hasGlideTarget = false;

            }

            newX = Mathf.Clamp(newX, ResponsiveCamera.LeftBound, ResponsiveCamera.RightBound);

            transform.position = new Vector3(newX, fixedY, 0f);

        }

        else

        {

            float clampedX = Mathf.Clamp(transform.position.x, ResponsiveCamera.LeftBound, ResponsiveCamera.RightBound);

            transform.position = new Vector3(clampedX, fixedY, 0f);

        }



        currentVelocityX = (transform.position.x - prevX) / Mathf.Max(0.0001f, Time.deltaTime);



        if (spriteRenderer != null && Mathf.Abs(currentVelocityX) > 0.4f)

        {

            spriteRenderer.flipX = currentVelocityX > 0f;

        }



        if (mouthRenderer != null && mouthRenderer.gameObject.activeSelf)

        {

            mouthRenderer.flipX = spriteRenderer != null ? spriteRenderer.flipX : false;

            float mx = (mouthRenderer.flipX) ? -mouthBaseOffset.x : mouthBaseOffset.x;

            mouthRenderer.transform.localPosition = new Vector3(mx, mouthBaseOffset.y, 0f);

        }

    }



    private void ReadUnifiedInput(out bool pointerDown, out bool pointerHeld, out Vector2 screenPos, out float keyboardAxis)

    {

        pointerDown = false;

        pointerHeld = false;

        screenPos = Vector2.zero;

        keyboardAxis = 0f;



        if (Input.touchCount > 0)

        {

            Touch t = Input.GetTouch(0);

            pointerDown = t.phase == TouchPhase.Began;

            pointerHeld = t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Began;

            screenPos = t.position;

        }

        else if (Input.GetMouseButton(0))

        {

            pointerDown = Input.GetMouseButtonDown(0);

            pointerHeld = true;

            screenPos = Input.mousePosition;

        }



        keyboardAxis = Input.GetAxisRaw("Horizontal");

    }



    private void UpdateAnimationState(bool isPlaying)

    {

        if (!isPlaying && GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.GameOver)

        {

            currentAnimState = AnimState.Dead;

        }

        else if (specialStateTimer > 0f)

        {

            specialStateTimer -= Time.deltaTime;

            if (specialStateTimer <= 0f)

            {

                if (spriteRenderer != null) spriteRenderer.color = Color.white;

                currentAnimState = Mathf.Abs(currentVelocityX) > 0.5f ? AnimState.Move : AnimState.Idle;

            }

        }

        else

        {

            currentAnimState = Mathf.Abs(currentVelocityX) > 0.5f ? AnimState.Move : AnimState.Idle;

        }



        Sprite[] activeFrames = GetActiveFrameArray();

        if (activeFrames == null || activeFrames.Length == 0 || spriteRenderer == null) return;



        frameTimer += Time.deltaTime;

        float interval = 1f / Mathf.Max(1f, frameRate);

        if (frameTimer >= interval)

        {

            frameTimer -= interval;

            if (currentAnimState == AnimState.Dead)

            {

                currentFrameIndex = Mathf.Min(currentFrameIndex + 1, activeFrames.Length - 1);

            }

            else

            {

                currentFrameIndex = (currentFrameIndex + 1) % activeFrames.Length;

            }

            spriteRenderer.sprite = activeFrames[currentFrameIndex];

        }

    }



    private Sprite[] GetActiveFrameArray()

    {

        switch (currentAnimState)

        {

            case AnimState.Move:

                return (moveFrames != null && moveFrames.Length > 0) ? moveFrames : idleFrames;

            case AnimState.Eat:

                return (eatFrames != null && eatFrames.Length > 0) ? eatFrames : idleFrames;

            case AnimState.Hurt:

                return (hurtFrames != null && hurtFrames.Length > 0) ? hurtFrames : idleFrames;

            case AnimState.Dead:

                return (deadFrames != null && deadFrames.Length > 0) ? deadFrames : idleFrames;

            default:

                return idleFrames;

        }

    }



    private void ApplyProceduralJiggle(bool isPlaying)

    {

        Vector3 desiredScale = baseScale;



        if (isPlaying)

        {

            float speedFactor = Mathf.Clamp01(Mathf.Abs(currentVelocityX) / 12f);

            if (speedFactor > 0.05f)

            {

                desiredScale.x = baseScale.x * (1f + speedFactor * 0.16f);

                desiredScale.y = baseScale.y * (1f - speedFactor * 0.12f);

            }

            else

            {

                float pulse = Mathf.Sin(Time.time * 4.5f) * 0.045f;

                desiredScale.x = baseScale.x * (1f - pulse);

                desiredScale.y = baseScale.y * (1f + pulse);

            }



            float targetTilt = Mathf.Clamp(-currentVelocityX * 1.2f, -10f, 10f);

            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetTilt);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f - Mathf.Exp(-14f * Time.deltaTime));

        }

        else

        {

            transform.rotation = Quaternion.identity;

        }



        currentDeformScale = Vector3.SmoothDamp(currentDeformScale, desiredScale, ref scaleVelocity, 0.07f);

        transform.localScale = currentDeformScale;

    }



    public void TriggerEatEffect()

    {

        currentAnimState = AnimState.Eat;

        currentFrameIndex = 0;

        specialStateTimer = 0.28f;

        if (spriteRenderer != null) spriteRenderer.color = Color.white;



        currentDeformScale = new Vector3(baseScale.x * 1.28f, baseScale.y * 0.74f, 1f);

        transform.localScale = currentDeformScale;



        if (mouthRenderer != null)
        {
            mouthRenderer.gameObject.SetActive(true);
            if (mouthOpenSprite != null) mouthRenderer.sprite = mouthOpenSprite;
        }

        if (mouthRoutine != null) StopCoroutine(mouthRoutine);

        mouthRoutine = StartCoroutine(AnimateEatMouthRoutine());

    }



    private IEnumerator AnimateEatMouthRoutine()

    {

        if (mouthRenderer == null) yield break;

        mouthRenderer.gameObject.SetActive(true);



        if (mouthOpenSprite != null) mouthRenderer.sprite = mouthOpenSprite;

        yield return new WaitForSeconds(0.12f);



        if (mouthChewSprite != null) mouthRenderer.sprite = mouthChewSprite;

        yield return new WaitForSeconds(0.10f);



        if (mouthSmileSprite != null) mouthRenderer.sprite = mouthSmileSprite;

        yield return new WaitForSeconds(0.12f);



        mouthRenderer.gameObject.SetActive(false);

        mouthRoutine = null;

    }



    public void TriggerHurtEffect()

    {

        if (mouthRoutine != null) { StopCoroutine(mouthRoutine); mouthRoutine = null; }

        if (mouthRenderer != null) mouthRenderer.gameObject.SetActive(false);

        currentAnimState = AnimState.Hurt;

        currentFrameIndex = 0;

        specialStateTimer = 0.42f;

        if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.45f, 0.55f, 1f);



        currentDeformScale = new Vector3(baseScale.x * 0.76f, baseScale.y * 1.26f, 1f);

        transform.localScale = currentDeformScale;

    }



    public void TriggerDeadEffect()

    {

        if (mouthRoutine != null) { StopCoroutine(mouthRoutine); mouthRoutine = null; }

        if (mouthRenderer != null) mouthRenderer.gameObject.SetActive(false);

        currentAnimState = AnimState.Dead;

        currentFrameIndex = 0;

        specialStateTimer = 999f;

        if (spriteRenderer != null) spriteRenderer.color = Color.white;

    }



    public void ResetToAliveState()

    {

        currentAnimState = AnimState.Idle;

        currentFrameIndex = 0;

        frameTimer = 0f;

        specialStateTimer = 0f;

        healBurstTimer = 0f;

        shieldFlashTimer = 0f;

        if (mouthRoutine != null) { StopCoroutine(mouthRoutine); mouthRoutine = null; }

        if (mouthRenderer != null) mouthRenderer.gameObject.SetActive(false);

        hasGlideTarget = false;

        targetX = 0f;

        currentVelocityX = 0f;

        currentDeformScale = baseScale;

        scaleVelocity = Vector3.zero;

        transform.localScale = baseScale;

        transform.rotation = Quaternion.identity;



        float startY = ResponsiveCamera.BottomBound + bottomOffset;

        transform.position = new Vector3(0f, startY, 0f);



        if (spriteRenderer != null)

        {

            spriteRenderer.color = Color.white;

            spriteRenderer.flipX = false;

            if (idleFrames != null && idleFrames.Length > 0)

            {

                spriteRenderer.sprite = idleFrames[0];

            }

        }

        UpdatePowerUpAuraVisuals();

    }

}

