using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    private static ItemSpawner _instance;
    public static ItemSpawner Instance
    {
        get
        {
            if (_instance == null) _instance = FindFirstObjectByType<ItemSpawner>();
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Prefab & Sprite Pools")]
    public GameObject itemPrefab;
    public Sprite[] foodSprites;
    public Sprite[] poisonSprites;
    public Sprite heartSprite;
    public Sprite magnetSprite;
    public Sprite shieldBarrierSprite;
    public Sprite timeSlowSprite;

    [Header("Spawn Placement")]
    public float minHorizontalSpacing = 1.2f;
    public float spawnYOffset = 0.85f;

    private Coroutine spawnCoroutine;
    private float lastSpawnX = -999f;
    private readonly List<GameObject> activeItems = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsPlaying)
        {
            StartSpawning();
        }
    }

    public void StartSpawning()
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawningAndClear()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            if (activeItems[i] != null)
            {
                Destroy(activeItems[i]);
            }
        }
        activeItems.Clear();
    }

    private IEnumerator SpawnRoutine()
    {
        // Initial brief grace period so player sees the slime before first drop
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                yield return null;
                continue;
            }

            int score = GameManager.Instance != null ? GameManager.Instance.currentScore : 0;
            GetTierParameters(score, out float spawnInterval, out float fallSpeed, out int poisonWeight);

            SpawnSingleItem(fallSpeed, poisonWeight);

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void GetTierParameters(int score, out float interval, out float speed, out int poisonWeight)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsFoodRushActive)
        {
            interval = 0.36f; // Rapid food cascade!
            speed = 5.2f;
            poisonWeight = 0; // ZERO hazards during Food Rush!
            return;
        }
        float baseInterval;
        float baseSpeed;
        int basePoison;

        if (score < 120)
        {
            baseInterval = 1.25f;
            baseSpeed = 3.5f;
            basePoison = 12;
        }
        else if (score < 260)
        {
            baseInterval = 1.00f;
            baseSpeed = 4.4f;
            basePoison = 18;
        }
        else if (score < 420)
        {
            baseInterval = 0.80f;
            baseSpeed = 5.3f;
            basePoison = 22;
        }
        else
        {
            baseInterval = 0.62f;
            baseSpeed = 6.3f;
            basePoison = 26;
        }

        float intervalMult = GameManager.Instance != null ? GameManager.Instance.SpawnIntervalMultiplier : 1.0f;
        float speedMult = GameManager.Instance != null ? GameManager.Instance.FallSpeedMultiplier : 1.0f;
        int poisonOffset = GameManager.Instance != null ? GameManager.Instance.PoisonWeightOffset : 0;

        interval = Mathf.Clamp(baseInterval * intervalMult, 0.35f, 2.2f);
        speed = Mathf.Clamp(baseSpeed * speedMult, 2.2f, 9.5f);
        poisonWeight = Mathf.Clamp(basePoison + poisonOffset, 5, 42);
    }

    public FallingItem SpawnSingleItem(float fallSpeed, int poisonWeight)
    {
        if (itemPrefab == null) return null;

        float spawnX = SelectSpawnX();
        float spawnY = ResponsiveCamera.TopBound + spawnYOffset;
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

        FallingItem.ItemType chosenType = ChooseItemType(poisonWeight);
        Sprite chosenSprite = ChooseSpriteForType(chosenType);

        GameObject obj = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
        activeItems.RemoveAll(item => item == null);
        activeItems.Add(obj);

        FallingItem fallingItem = obj.GetComponent<FallingItem>();
        if (fallingItem != null)
        {
            int pts = GameManager.Instance != null ? GameManager.Instance.BaseFoodPoints : 10;
            fallingItem.Initialize(chosenType, chosenSprite, fallSpeed, pts);
        }

        return fallingItem;
    }

    private float SelectSpawnX()
    {
        if (ResponsiveCamera.Instance != null)
        {
            ResponsiveCamera.Instance.RecalculateBounds();
        }

        // Keep spawned items well inside the visible horizontal bounds (accounting for wobbleAmplitude + sprite half-width)
        float safeMargin = 0.18f;
        float minX = ResponsiveCamera.LeftBound + safeMargin;
        float maxX = ResponsiveCamera.RightBound - safeMargin;
        if (maxX <= minX)
        {
            minX = ResponsiveCamera.LeftBound;
            maxX = ResponsiveCamera.RightBound;
        }
        if (maxX <= minX) return 0f;

        float span = maxX - minX;
        float effectiveSpacing = Mathf.Min(minHorizontalSpacing, span * 0.35f);

        float candidateX = Random.Range(minX, maxX);
        for (int attempt = 0; attempt < 5; attempt++)
        {
            candidateX = Random.Range(minX, maxX);
            if (Mathf.Abs(candidateX - lastSpawnX) >= effectiveSpacing)
                break;
        }

        lastSpawnX = candidateX;
        return candidateX;
    }

    private FallingItem.ItemType ChooseItemType(int poisonWeight)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsFoodRushActive)
        {
            return FallingItem.ItemType.Food; // 100% Food Only!
        }
        int bonusWeight = GameManager.Instance != null ? GameManager.Instance.PowerUpBonusWeight : 0;
        int magnetThreshold = Mathf.Clamp(3 + (bonusWeight > 0 ? 1 : 0), 2, 5);
        int shieldThreshold = magnetThreshold + Mathf.Clamp(3 + (bonusWeight > 0 ? 1 : 0), 2, 5);
        int timeSlowThreshold = shieldThreshold + Mathf.Clamp(3 + (bonusWeight > 0 ? 1 : 0), 2, 5);
        int heartThreshold = timeSlowThreshold + Mathf.Clamp(3 + bonusWeight, 2, 7);
        int poisonThreshold = heartThreshold + poisonWeight;

        int roll = Random.Range(0, 100);
        if (roll < magnetThreshold)
        {
            return FallingItem.ItemType.Magnet;
        }
        if (roll < shieldThreshold)
        {
            return FallingItem.ItemType.ShieldBarrier;
        }
        if (roll < timeSlowThreshold)
        {
            return FallingItem.ItemType.TimeSlow;
        }
        if (roll < heartThreshold)
        {
            return FallingItem.ItemType.LifeHeart;
        }
        if (roll < poisonThreshold)
        {
            return FallingItem.ItemType.Poison;
        }
        return FallingItem.ItemType.Food;
    }

    private Sprite ChooseSpriteForType(FallingItem.ItemType type)
    {
        switch (type)
        {
            case FallingItem.ItemType.Food:
                if (foodSprites != null && foodSprites.Length > 0)
                    return foodSprites[Random.Range(0, foodSprites.Length)];
                break;
            case FallingItem.ItemType.Poison:
                if (poisonSprites != null && poisonSprites.Length > 0)
                    return poisonSprites[Random.Range(0, poisonSprites.Length)];
                break;
            case FallingItem.ItemType.LifeHeart:
                if (heartSprite != null) return heartSprite;
                break;
            case FallingItem.ItemType.Magnet:
                if (magnetSprite != null) return magnetSprite;
                break;
            case FallingItem.ItemType.ShieldBarrier:
                if (shieldBarrierSprite != null) return shieldBarrierSprite;
                break;
            case FallingItem.ItemType.TimeSlow:
                if (timeSlowSprite != null) return timeSlowSprite;
                break;
        }
        return null;
    }
}
