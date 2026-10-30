using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class ResponsiveCamera : MonoBehaviour
{
    public static ResponsiveCamera Instance { get; private set; }

    public static float LeftBound { get; private set; } = -2.2f;
    public static float RightBound { get; private set; } = 2.2f;
    public static float TopBound { get; private set; } = 6.2f;
    public static float BottomBound { get; private set; } = -6.2f;

    [Header("Viewport Settings")]
    public float minOrthographicSize = 5.4f;
    public float minVisibleWorldWidth = 5.8f;
    public float horizontalPadding = 0.52f;
    public RectTransform safeAreaContainer;

    [Header("Scene References")]
    public SpriteRenderer[] backgroundLayers;
    public SpriteRenderer[] transitionBackgroundLayers;
    public float[] parallaxFactors = new float[] { 0.04f, 0.09f, 0.15f, 0.22f };
    public BoxCollider2D floorCollider;

    [Header("Floor Platform Settings")]
    public SpriteRenderer floorPlatformRenderer;
    public SpriteRenderer transitionFloorPlatformRenderer;
    public float platformHeightWorld = 0.85f;

    private Camera cam;
    private int lastScreenW = -1;
    private int lastScreenH = -1;
    private Rect lastSafeArea = Rect.zero;
    private float lastAspect = -1f;
    private float lastOrthoSize = -1f;
    private Vector3 baseCameraPos = new Vector3(0f, 0f, -10f);
    private Coroutine shakeRoutine;

    private readonly Dictionary<SpriteRenderer, SpriteRenderer[]> sideTileClones = new Dictionary<SpriteRenderer, SpriteRenderer[]>();

    private void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.rect = new Rect(0f, 0f, 1f, 1f);
        cam.ResetAspect();
        RecalculateBounds();
    }

    private void Update()
    {
        if (cam == null) cam = GetComponent<Camera>();

        bool screenChanged = Screen.width != lastScreenW || Screen.height != lastScreenH || Screen.safeArea != lastSafeArea;
        bool camChanged = Mathf.Abs(cam.aspect - lastAspect) > 0.001f || Mathf.Abs(cam.orthographicSize - lastOrthoSize) > 0.001f;

        if (screenChanged || camChanged)
        {
            if (cam.targetTexture == null)
            {
                cam.rect = new Rect(0f, 0f, 1f, 1f);
                cam.ResetAspect();
            }
            RecalculateBounds();
        }

        // Subtle horizontal parallax shift based on Slime X position
        if (Application.isPlaying && SlimeController.Instance != null)
        {
            float span = Mathf.Max(0.1f, RightBound - LeftBound);
            float slimeNormalizedX = Mathf.Clamp(SlimeController.Instance.transform.position.x / (span * 0.5f), -1f, 1f);

            UpdateParallaxGroup(backgroundLayers, slimeNormalizedX);
            UpdateParallaxGroup(transitionBackgroundLayers, slimeNormalizedX);
        }
    }

    private void UpdateParallaxGroup(SpriteRenderer[] group, float slimeNormalizedX)
    {
        if (group == null) return;
        for (int i = 0; i < group.Length; i++)
        {
            if (group[i] == null) continue;
            float factor = (parallaxFactors != null && i < parallaxFactors.Length) ? parallaxFactors[i] : 0.05f * (i + 1);
            Vector3 pos = group[i].transform.position;
            pos.x = Mathf.Lerp(pos.x, -slimeNormalizedX * factor, Time.deltaTime * 6f);
            // Retain anchored pos.y at bottom floor! Never overwrite pos.y = 0f!
            group[i].transform.position = pos;
        }
    }

    public void RecalculateBounds()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return;

        lastScreenW = Screen.width;
        lastScreenH = Screen.height;
        lastSafeArea = Screen.safeArea;

        // Compute true active aspect ratio directly from RenderTexture (if capturing) or physical Screen dimensions
        float activeAspect = cam.aspect;
        if (cam.targetTexture != null)
        {
            activeAspect = (float)cam.targetTexture.width / Mathf.Max(1, cam.targetTexture.height);
        }
        else if (Screen.width > 0 && Screen.height > 0)
        {
            activeAspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        }
        activeAspect = Mathf.Max(0.35f, activeAspect);
        lastAspect = activeAspect;

        // Guarantee at least minVisibleWorldWidth (5.8 world units) horizontally on narrow 20:9 / 21:9 phones
        // so objects, sprites, and backgrounds never look giant or zoomed-in!
        float requiredOrthoForWidth = (minVisibleWorldWidth * 0.5f) / activeAspect;
        cam.orthographicSize = Mathf.Max(minOrthographicSize, requiredOrthoForWidth);
        lastOrthoSize = cam.orthographicSize;

        float halfH = cam.orthographicSize;
        float halfW = halfH * activeAspect;

        // Respect device Screen.safeArea horizontal insets if running on a mobile screen with curved edges/cutouts
        float safeLeftRatio = 0f;
        float safeRightRatio = 1f;
        if (cam.targetTexture == null && Screen.width > 0 && Screen.safeArea.width > 0)
        {
            safeLeftRatio = Mathf.Clamp01(Screen.safeArea.xMin / Screen.width);
            safeRightRatio = Mathf.Clamp01(Screen.safeArea.xMax / Screen.width);
        }

        float visibleLeftEdge = Mathf.Lerp(-halfW, halfW, safeLeftRatio);
        float visibleRightEdge = Mathf.Lerp(-halfW, halfW, safeRightRatio);

        TopBound = halfH;
        BottomBound = -halfH;

        float effectivePadding = Mathf.Min(horizontalPadding, (visibleRightEdge - visibleLeftEdge) * 0.22f);
        LeftBound = visibleLeftEdge + effectivePadding;
        RightBound = visibleRightEdge - effectivePadding;

        if (floorCollider != null)
        {
            floorCollider.transform.position = new Vector3(0f, BottomBound + (platformHeightWorld * 0.5f), 0f);
            floorCollider.size = new Vector2((halfW * 2f) + 2.0f, platformHeightWorld);
        }

        PositionFloorPlatform(floorPlatformRenderer, halfW, halfH);
        PositionFloorPlatform(transitionFloorPlatformRenderer, halfW, halfH);

        ApplySafeAreaToUI();
        ScaleAndTileLayerGroup(backgroundLayers, halfW, halfH);
        ScaleAndTileLayerGroup(transitionBackgroundLayers, halfW, halfH);
    }

    private void ApplySafeAreaToUI()
    {
        if (safeAreaContainer == null) return;
        if (cam != null && cam.targetTexture != null)
        {
            safeAreaContainer.anchorMin = Vector2.zero;
            safeAreaContainer.anchorMax = Vector2.one;
            safeAreaContainer.offsetMin = Vector2.zero;
            safeAreaContainer.offsetMax = Vector2.zero;
            return;
        }

        Rect safe = Screen.safeArea;
        if (Screen.width > 0 && Screen.height > 0 && safe.width > 0 && safe.height > 0)
        {
            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            safeAreaContainer.anchorMin = anchorMin;
            safeAreaContainer.anchorMax = anchorMax;
            safeAreaContainer.offsetMin = Vector2.zero;
            safeAreaContainer.offsetMax = Vector2.zero;
        }
    }

        public void PositionFloorPlatform(SpriteRenderer sr, float halfW, float halfH)
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

    public void ScaleAndPositionRenderer(SpriteRenderer sr, float halfW, float halfH, float parallaxX = 0f)
    {
        if (sr == null || sr.sprite == null) return;
        Vector2 spriteSize = sr.sprite.bounds.size;
        if (spriteSize.x <= 0.001f || spriteSize.y <= 0.001f) return;

        float visibleW = halfW * 2f;
        float visibleH = halfH * 2f;

        // 14% width bleed for parallax shifting without exposing edges
        float requiredWidth = visibleW * 1.14f;
        float requiredHeight = visibleH * 1.02f;

        float scaleX = requiredWidth / spriteSize.x;
        float scaleY = requiredHeight / spriteSize.y;
        float uniformScale = Mathf.Max(scaleX, scaleY);

        sr.transform.localScale = new Vector3(uniformScale, uniformScale, 1f);

        // Crucial: Anchor bottom of sprite to BottomBound so the environment floor is ALWAYS touching the bottom!
        float scaledH = spriteSize.y * uniformScale;
        float anchoredY = BottomBound + (scaledH * 0.5f);

        Vector3 pos = sr.transform.position;
        pos.x = parallaxX;
        pos.y = anchoredY;
        sr.transform.position = pos;
    }

    public void ScaleAndTileLayerGroup(SpriteRenderer[] group, float halfW, float halfH)
    {
        if (group == null) return;
        foreach (SpriteRenderer sr in group)
        {
            if (sr == null || sr.sprite == null) continue;
            ScaleAndPositionRenderer(sr, halfW, halfH, sr.transform.position.x);
        }
    }

    private void SyncSideTiles(SpriteRenderer parentSr, float localSpriteWidth)
    {
        if (parentSr == null) return;

        if (!sideTileClones.TryGetValue(parentSr, out SpriteRenderer[] clones) || clones == null || clones[0] == null || clones[1] == null)
        {
            Transform leftT = parentSr.transform.Find("Tile_Left");
            Transform rightT = parentSr.transform.Find("Tile_Right");

            SpriteRenderer leftSr = leftT != null ? leftT.GetComponent<SpriteRenderer>() : CreateSideClone(parentSr, "Tile_Left");
            SpriteRenderer rightSr = rightT != null ? rightT.GetComponent<SpriteRenderer>() : CreateSideClone(parentSr, "Tile_Right");

            clones = new SpriteRenderer[] { leftSr, rightSr };
            sideTileClones[parentSr] = clones;
        }

        for (int i = 0; i < 2; i++)
        {
            SpriteRenderer clone = clones[i];
            if (clone == null) continue;
            clone.sprite = parentSr.sprite;
            clone.color = parentSr.color;
            clone.sortingOrder = parentSr.sortingOrder;
            clone.transform.localScale = Vector3.one;
            float dir = (i == 0) ? -1f : 1f;
            clone.transform.localPosition = new Vector3(dir * (localSpriteWidth - 0.02f), 0f, 0f);
        }
    }

    private SpriteRenderer CreateSideClone(SpriteRenderer parentSr, string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parentSr.transform, false);
        obj.transform.localScale = Vector3.one;
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = parentSr.sortingOrder;
        return sr;
    }

    public void TriggerShake(float duration = 0.18f, float magnitude = 0.18f)
    {
        if (!Application.isPlaying) return;
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float damper = 1f - Mathf.Clamp01(elapsed / duration);
            float offsetX = Random.Range(-1f, 1f) * magnitude * damper;
            float offsetY = Random.Range(-1f, 1f) * magnitude * damper;
            transform.position = baseCameraPos + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }
        transform.position = baseCameraPos;
        shakeRoutine = null;
    }
}
