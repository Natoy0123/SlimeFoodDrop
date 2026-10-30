using System;
using System.Collections;
using UnityEngine;

public class ThemeManager : MonoBehaviour
{
    private static ThemeManager _instance;
    public static ThemeManager Instance
    {
        get
        {
            if (_instance == null) _instance = FindFirstObjectByType<ThemeManager>();
            return _instance;
        }
        private set { _instance = value; }
    }

    [Serializable]
    public class EnvironmentTheme
    {
        public string themeName = "Whispering Forest";
        public string environmentDescription = "Lush Woodland Glade  |  Peaceful Forest BGM";
        public int unlockScore = 0;
        public Sprite[] layers = new Sprite[4];
        public Color[] layerTints = new Color[4]
        {
            Color.white,
            Color.white,
            Color.white,
            Color.white
        };
        public AudioClip bgmClip;
        public Sprite floorPlatformSprite;
    }

    [Header("Progression Themes (Ordered by Score)")]
    public EnvironmentTheme[] themes;

    [Header("Renderer Sets for Cross-Fade")]
    public SpriteRenderer[] primaryRenderers = new SpriteRenderer[4];
    public SpriteRenderer[] transitionRenderers = new SpriteRenderer[4];

    [Header("Floor Platform Renderers")]
    public SpriteRenderer floorPlatformRenderer;
    public SpriteRenderer transitionFloorPlatformRenderer;

    [Header("Transition Timing")]
    public float crossFadeDuration = 1.6f;

    public int currentThemeIndex = 0;
    private Coroutine transitionRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void InitializeStartingTheme()
    {
        EnforceAuthoritativeUnlockScores();
        PrewarmAllThemeAssets();
        currentThemeIndex = 0;
        ApplyThemeImmediate(0);
    }

    private static readonly int[] AuthoritativeUnlockScores = { 0, 100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700 };

    public void EnforceAuthoritativeUnlockScores()
    {
        if (themes == null) return;
        for (int i = 0; i < themes.Length && i < AuthoritativeUnlockScores.Length; i++)
        {
            themes[i].unlockScore = AuthoritativeUnlockScores[i];
        }
    }

    private void PrewarmAllThemeAssets()
    {
        if (themes == null) return;
        for (int t = 0; t < themes.Length; t++)
        {
            EnvironmentTheme th = themes[t];
            if (th == null) continue;
            if (th.bgmClip != null && th.bgmClip.loadState == AudioDataLoadState.Unloaded)
            {
                th.bgmClip.LoadAudioData();
            }
            if (th.layers != null)
            {
                for (int i = 0; i < th.layers.Length; i++)
                {
                    if (th.layers[i] != null && th.layers[i].texture != null)
                    {
                        _ = th.layers[i].texture.width;
                    }
                }
            }
        }
    }

    public void EvaluateScoreForTheme(int currentScore)
    {
        if (themes == null || themes.Length == 0) return;

        int targetIndex = 0;
        for (int i = 0; i < themes.Length; i++)
        {
            if (currentScore >= themes[i].unlockScore)
            {
                targetIndex = i;
            }
        }

        // Clamp to final 10th environment so the game never ends here and stays on the final stage until game over
        targetIndex = Mathf.Clamp(targetIndex, 0, themes.Length - 1);

        if (targetIndex != currentThemeIndex)
        {
            TransitionToTheme(targetIndex);
        }
    }

    public void ApplyThemeImmediate(int index)
    {
        if (themes == null || index < 0 || index >= themes.Length) return;

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        currentThemeIndex = index;
        EnvironmentTheme theme = themes[index];

        for (int i = 0; i < 4; i++)
        {
            if (primaryRenderers != null && i < primaryRenderers.Length && primaryRenderers[i] != null)
            {
                Sprite s = (theme.layers != null && i < theme.layers.Length) ? theme.layers[i] : null;
                Color c = (theme.layerTints != null && i < theme.layerTints.Length) ? theme.layerTints[i] : Color.white;
                primaryRenderers[i].sprite = s;
                primaryRenderers[i].color = c;
            }

            if (transitionRenderers != null && i < transitionRenderers.Length && transitionRenderers[i] != null)
            {
                transitionRenderers[i].sprite = null;
                transitionRenderers[i].color = new Color(1f, 1f, 1f, 0f);
                transitionRenderers[i].gameObject.SetActive(false);
            }
        }

        if (floorPlatformRenderer != null)
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
        }

        if (SoundManager.Instance != null && theme.bgmClip != null)
        {
            SoundManager.Instance.CrossfadeBGM(theme.bgmClip, 0.8f);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateThemeLabel(theme.themeName, false);
        }
    }

    public void TransitionToTheme(int newIndex)
    {
        if (themes == null || newIndex < 0 || newIndex >= themes.Length) return;
        if (newIndex == currentThemeIndex && transitionRoutine == null) return;

        currentThemeIndex = newIndex;
        EnvironmentTheme nextTheme = themes[newIndex];

        if (SoundManager.Instance != null && nextTheme.bgmClip != null)
        {
            SoundManager.Instance.CrossfadeBGM(nextTheme.bgmClip, crossFadeDuration);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateThemeLabel(nextTheme.themeName, true);
            UIManager.Instance.ShowEnvironmentTransitionBanner(
                newIndex + 1,
                nextTheme.themeName,
                GetThemeDescription(newIndex, nextTheme));
        }

        if (!Application.isPlaying || !gameObject.activeInHierarchy)
        {
            ApplyThemeImmediate(newIndex);
            return;
        }

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }
        transitionRoutine = StartCoroutine(CrossFadeLayersRoutine(nextTheme));
    }

    public void AnnounceCurrentThemeBanner()
    {
        if (themes == null || currentThemeIndex < 0 || currentThemeIndex >= themes.Length) return;
        EnvironmentTheme theme = themes[currentThemeIndex];
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowEnvironmentTransitionBanner(
                currentThemeIndex + 1,
                theme.themeName,
                GetThemeDescription(currentThemeIndex, theme));
        }
    }

    private string GetThemeDescription(int idx, EnvironmentTheme theme)
    {
        if (!string.IsNullOrEmpty(theme.environmentDescription))
            return theme.environmentDescription;

        switch (idx)
        {
            case 0: return "Lush Woodland Canopy  |  Peaceful Forest BGM";
            case 1: return "Breezy Rolling Meadow  |  Sunny Plains BGM";
            case 2: return "Golden Alpine Crags  |  Windswept Mountain BGM";
            case 3: return "Emerald Palm Coast  |  Lush Tropical Lagoon BGM";
            case 4: return "Sunbaked Canyon Dunes  |  Desert Mirage BGM";
            case 5: return "Coral Abyss Lagoon  |  Ocean Wave BGM";
            case 6: return "Steamworks Machinery  |  Chiptune Factory BGM";
            case 7: return "Crystal Luminescent Cavern  |  Deep Echo BGM";
            case 8: return "Floating Sky Sanctuary  |  Ancient Cloud Ruins BGM";
            default: return "Ancient Frosty Moonlit Fortress  |  Nocturnal Citadel BGM";
        }
    }

    private IEnumerator CrossFadeLayersRoutine(EnvironmentTheme nextTheme)
    {
        Color[] startColors = new Color[4];
        Color[] targetColors = new Color[4];

        if (transitionFloorPlatformRenderer != null && nextTheme.floorPlatformSprite != null)
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
            startColors[i] = (primaryRenderers != null && i < primaryRenderers.Length && primaryRenderers[i] != null)
                ? primaryRenderers[i].color
                : Color.white;

            targetColors[i] = (nextTheme.layerTints != null && i < nextTheme.layerTints.Length)
                ? nextTheme.layerTints[i]
                : Color.white;

            if (transitionRenderers != null && i < transitionRenderers.Length && transitionRenderers[i] != null)
            {
                Sprite nextSprite = (nextTheme.layers != null && i < nextTheme.layers.Length) ? nextTheme.layers[i] : null;
                transitionRenderers[i].gameObject.SetActive(nextSprite != null);
                transitionRenderers[i].sprite = nextSprite;
                if (nextSprite != null && ResponsiveCamera.Instance != null)
                {
                    Camera c = ResponsiveCamera.Instance.GetComponent<Camera>();
                    float halfH = c != null ? c.orthographicSize : 5.4f;
                    float halfW = halfH * (c != null ? c.aspect : 0.56f);
                    ResponsiveCamera.Instance.ScaleAndPositionRenderer(transitionRenderers[i], halfW, halfH, 0f);
                }
                Color tc = targetColors[i];
                tc.a = 0f;
                transitionRenderers[i].color = tc;
            }
        }

        float elapsed = 0f;
        float dur = Mathf.Max(0.1f, crossFadeDuration);

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / dur));

            for (int i = 0; i < 4; i++)
            {
                if (transitionRenderers != null && i < transitionRenderers.Length && transitionRenderers[i] != null)
                {
                    Color tc = targetColors[i];
                    tc.a = targetColors[i].a * t;
                    transitionRenderers[i].color = tc;
                }

                if (primaryRenderers != null && i < primaryRenderers.Length && primaryRenderers[i] != null)
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

            yield return null;
        }

        // Commit new theme to primary renderers and hide transition renderers
        for (int i = 0; i < 4; i++)
        {
            if (primaryRenderers != null && i < primaryRenderers.Length && primaryRenderers[i] != null)
            {
                Sprite nextSprite = (nextTheme.layers != null && i < nextTheme.layers.Length) ? nextTheme.layers[i] : null;
                primaryRenderers[i].sprite = nextSprite;
                primaryRenderers[i].color = targetColors[i];
                if (nextSprite != null && ResponsiveCamera.Instance != null)
                {
                    Camera c = ResponsiveCamera.Instance.GetComponent<Camera>();
                    float halfH = c != null ? c.orthographicSize : 5.4f;
                    float halfW = halfH * (c != null ? c.aspect : 0.56f);
                    ResponsiveCamera.Instance.ScaleAndPositionRenderer(primaryRenderers[i], halfW, halfH, primaryRenderers[i].transform.position.x);
                }
            }

            if (transitionRenderers != null && i < transitionRenderers.Length && transitionRenderers[i] != null)
            {
                transitionRenderers[i].sprite = null;
                transitionRenderers[i].color = new Color(1f, 1f, 1f, 0f);
                transitionRenderers[i].gameObject.SetActive(false);
            }
        }

        if (floorPlatformRenderer != null)
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
    }
}
