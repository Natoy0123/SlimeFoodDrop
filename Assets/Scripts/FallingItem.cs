using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class FallingItem : MonoBehaviour
{
    public enum ItemType
    {
        Food,
        Poison,
        LifeHeart,
        Magnet,
        ShieldBarrier,
        TimeSlow
    }

    [Header("Item Properties")]
    public ItemType itemType = ItemType.Food;
    public int pointValue = 10;
    public float fallSpeed = 4.0f;
    public float wobbleFrequency = 3.2f;
    public float wobbleAmplitude = 0.14f;
    public float magnetPullSpeed = 12.5f;
    public float magnetRadius = 5.4f;
    public float catchDistanceThreshold = 0.66f;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer powerUpHaloRenderer;
    private static Sprite cachedHaloSprite;

    private float baseX;
    private float wobbleSeed;
    private bool isBeingPulled = false;
    private bool hasTriggered = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.28f;

        EnsurePowerUpHalo();
    }

    private void EnsurePowerUpHalo()
    {
        Transform existing = transform.Find("PowerUp_Halo");
        if (existing != null)
        {
            powerUpHaloRenderer = existing.GetComponent<SpriteRenderer>();
            return;
        }

        GameObject haloObj = new GameObject("PowerUp_Halo");
        haloObj.transform.SetParent(transform, false);
        haloObj.transform.localPosition = Vector3.zero;
        powerUpHaloRenderer = haloObj.AddComponent<SpriteRenderer>();
        powerUpHaloRenderer.sortingOrder = (spriteRenderer != null ? spriteRenderer.sortingOrder : 4) - 1;
        powerUpHaloRenderer.sprite = GetOrCreateHaloSprite();
        haloObj.SetActive(false);
    }

    private static Sprite GetOrCreateHaloSprite()
    {
        if (cachedHaloSprite != null) return cachedHaloSprite;
        int size = 48;
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

                bool outerRing = dist >= 0.78f && dist <= 0.94f && ((int)(angle / 30f) % 2 == 0);
                bool innerGlow = dist < 0.78f;
                if (outerRing)
                    cols[y * size + x] = new Color(1f, 1f, 1f, 0.92f);
                else if (innerGlow)
                    cols[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01((1f - dist) * 0.48f));
                else
                    cols[y * size + x] = Color.clear;
            }
        }
        tex.SetPixels(cols);
        tex.Apply();
        cachedHaloSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 42f);
        return cachedHaloSprite;
    }

    public void Initialize(ItemType type, Sprite sprite, float speed, int points = 10)
    {
        itemType = type;
        fallSpeed = speed;
        pointValue = points;
        baseX = transform.position.x;
        wobbleSeed = Random.Range(0f, Mathf.PI * 2f);
        hasTriggered = false;
        isBeingPulled = false;

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }

        EnsurePowerUpHalo();
        UpdateHaloVisual(0.25f);
    }

    private void UpdateHaloVisual(float timeVal)
    {
        if (powerUpHaloRenderer == null) return;

        bool isPowerUp = itemType == ItemType.LifeHeart ||
                         itemType == ItemType.Magnet ||
                         itemType == ItemType.ShieldBarrier ||
                         itemType == ItemType.TimeSlow;

        bool showHalo = isPowerUp || isBeingPulled;
        if (powerUpHaloRenderer.gameObject.activeSelf != showHalo)
        {
            powerUpHaloRenderer.gameObject.SetActive(showHalo);
        }
        if (!showHalo) return;

        Color baseColor;
        switch (itemType)
        {
            case ItemType.LifeHeart:
                baseColor = new Color(1.0f, 0.45f, 0.65f, 0.78f);
                break;
            case ItemType.Magnet:
                baseColor = new Color(1.0f, 0.90f, 0.25f, 0.85f);
                break;
            case ItemType.ShieldBarrier:
                baseColor = new Color(0.28f, 0.94f, 1.0f, 0.85f);
                break;
            case ItemType.TimeSlow:
                baseColor = new Color(0.78f, 0.52f, 1.0f, 0.85f);
                break;
            default:
                // Food being pulled by Magnet!
                baseColor = new Color(1.0f, 0.92f, 0.35f, 0.82f);
                break;
        }

        float pulse = 1.05f + Mathf.Sin(timeVal * 7.5f + wobbleSeed) * 0.14f;
        powerUpHaloRenderer.transform.localScale = new Vector3(pulse, pulse, 1f);
        powerUpHaloRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, -timeVal * 95f);
        powerUpHaloRenderer.color = baseColor;
    }

    private void Update()
    {
        if (hasTriggered) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            return;

        // Instant proximity catch check so items pulled by Magnet or overlapping Slime NEVER get stuck on top of the character!
        if (SlimeController.Instance != null)
        {
            Vector3 slimePos = SlimeController.Instance.transform.position;
            float distToSlime = Vector2.Distance(transform.position, slimePos);
            if (distToSlime <= catchDistanceThreshold)
            {
                hasTriggered = true;
                HandleCaughtBySlime();
                return;
            }

            // Check if Fruit Magnet is active and this is an edible food item
            if (itemType == ItemType.Food && GameManager.Instance != null && GameManager.Instance.IsMagnetActive)
            {
                if (distToSlime <= magnetRadius)
                {
                    isBeingPulled = true;
                    UpdateHaloVisual(Time.time);
                    transform.position = Vector3.MoveTowards(transform.position, slimePos, magnetPullSpeed * Time.deltaTime);
                    if (Vector2.Distance(transform.position, slimePos) <= catchDistanceThreshold)
                    {
                        hasTriggered = true;
                        HandleCaughtBySlime();
                    }
                    return;
                }
            }
        }

        // Release magnet pull lock if magnet expired before reaching the slime so item resumes falling normally
        if (isBeingPulled && (GameManager.Instance == null || !GameManager.Instance.IsMagnetActive))
        {
            isBeingPulled = false;
            baseX = transform.position.x;
        }

        if (!isBeingPulled)
        {
            float speedMul = (GameManager.Instance != null) ? GameManager.Instance.GetTimeSlowMultiplier() : 1.0f;
            float nextY = transform.position.y - (fallSpeed * speedMul) * Time.deltaTime;
            float wobbleOffset = Mathf.Sin(Time.time * wobbleFrequency + wobbleSeed) * wobbleAmplitude;
            float nextX = Mathf.Clamp(baseX + wobbleOffset, ResponsiveCamera.LeftBound, ResponsiveCamera.RightBound);
            transform.position = new Vector3(nextX, nextY, 0f);
        }

        UpdateHaloVisual(Time.time);

        // Subtle rotation on falling items for visual life
        transform.Rotate(0f, 0f, 35f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTriggerCatchWithCollider(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryTriggerCatchWithCollider(other);
    }

    private void TryTriggerCatchWithCollider(Collider2D other)
    {
        if (hasTriggered) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

        if (other.CompareTag("Player") || other.GetComponent<SlimeController>() != null)
        {
            hasTriggered = true;
            HandleCaughtBySlime();
        }
    }

    public bool TryMarkTriggered()
    {
        if (hasTriggered) return false;
        hasTriggered = true;
        return true;
    }

    private void HandleCaughtBySlime()
    {
        if (GameManager.Instance != null)
        {
            switch (itemType)
            {
                case ItemType.Food:
                    GameManager.Instance.OnFoodCaught(pointValue, transform.position);
                    break;
                case ItemType.Poison:
                    GameManager.Instance.LoseLife("Poison");
                    break;
                case ItemType.LifeHeart:
                    GameManager.Instance.AddLife();
                    break;
                case ItemType.Magnet:
                    GameManager.Instance.ActivateMagnet(6.0f);
                    break;
                case ItemType.ShieldBarrier:
                    GameManager.Instance.ActivateShieldBarrier(12.0f);
                    break;
                case ItemType.TimeSlow:
                    GameManager.Instance.ActivateTimeSlow(6.0f);
                    break;
            }
        }
        Destroy(gameObject);
    }
}
