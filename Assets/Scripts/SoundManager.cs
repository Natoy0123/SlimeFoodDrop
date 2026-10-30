using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null) _instance = FindFirstObjectByType<SoundManager>();
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource bgmCrossfadeSource;
    public AudioSource sfxSource;

    [Header("BGM Clips (Matched to Background Themes)")]
    public AudioClip forestAmbienceClip;
    public AudioClip[] themeBgmClips;
    public float maxBgmVolume = 0.48f;

    [Header("User Volume Multipliers (0.0 - 1.0)")]
    public float userBgmVolume = 0.8f;
    public float userSfxVolume = 1.0f;

    [Header("SFX Clips")]
    public AudioClip[] munchClips;
    public AudioClip hurtClip;
    public AudioClip chimeClip;
    public AudioClip missClip;
    public AudioClip powerupClip;

    [Header("Tailored SFX Clips")]
    public AudioClip buttonClickClip;
    public AudioClip slimeEatClip;
    public AudioClip slimePukeClip;
    public AudioClip foodScamperClip;

    [Header("Power-Up Specific Pickup Clips")]
    public AudioClip shieldPickupClip;
    public AudioClip magnetPickupClip;
    public AudioClip timeSlowPickupClip;
    public AudioClip heartPickupClip;

    [Header("Power-Up Active Continuous Audio")]
    public AudioSource powerupActiveSource;
    public AudioSource shieldActiveSource;
    public AudioSource magnetActiveSource;
    public AudioSource timeSlowActiveSource;
    public AudioClip shieldActiveClip;
    public AudioClip magnetActiveClip;
    public AudioClip timeSlowActiveClip;

    private const string BgmVolKey = "SlimeFoodDrop_BgmVol";
    private const string SfxVolKey = "SlimeFoodDrop_SfxVol";

    private Coroutine bgmFadeRoutine;

    public float EffectiveBgmCap => maxBgmVolume * Mathf.Clamp01(userBgmVolume);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        userBgmVolume = PlayerPrefs.GetFloat(BgmVolKey, 0.8f);
        userSfxVolume = PlayerPrefs.GetFloat(SfxVolKey, 1.0f);
    }

    private void Start()
    {
        PlayBGM();
    }

    public void SetBgmVolume(float value01)
    {
        userBgmVolume = Mathf.Clamp01(value01);
        PlayerPrefs.SetFloat(BgmVolKey, userBgmVolume);
        PlayerPrefs.Save();

        if (bgmSource != null && bgmFadeRoutine == null)
        {
            bgmSource.volume = EffectiveBgmCap;
        }
    }

    public void SetSfxVolume(float value01)
    {
        userSfxVolume = Mathf.Clamp01(value01);
        PlayerPrefs.SetFloat(SfxVolKey, userSfxVolume);
        PlayerPrefs.Save();
    }

    public void PlayBGM()
    {
        AudioClip initialClip = (themeBgmClips != null && themeBgmClips.Length > 0 && themeBgmClips[0] != null)
            ? themeBgmClips[0]
            : forestAmbienceClip;

        if (bgmSource != null && initialClip != null)
        {
            bgmSource.clip = initialClip;
            bgmSource.loop = true;
            bgmSource.volume = EffectiveBgmCap;
            if (!bgmSource.isPlaying)
            {
                bgmSource.Play();
            }
        }
    }

    public void CrossfadeBGM(AudioClip newClip, float duration = 1.4f)
    {
        if (newClip == null || bgmSource == null) return;

        if (bgmSource.clip == newClip && bgmSource.isPlaying)
        {
            bgmSource.volume = EffectiveBgmCap;
            return;
        }

        if (!Application.isPlaying || !gameObject.activeInHierarchy || bgmCrossfadeSource == null || duration <= 0.05f)
        {
            bgmSource.clip = newClip;
            bgmSource.loop = true;
            bgmSource.volume = EffectiveBgmCap;
            if (Application.isPlaying) bgmSource.Play();
            return;
        }

        if (bgmFadeRoutine != null)
        {
            StopCoroutine(bgmFadeRoutine);
        }
        bgmFadeRoutine = StartCoroutine(CrossfadeRoutine(newClip, duration));
    }

    private IEnumerator CrossfadeRoutine(AudioClip nextClip, float duration)
    {
        bgmCrossfadeSource.clip = nextClip;
        bgmCrossfadeSource.loop = true;
        bgmCrossfadeSource.volume = 0f;
        bgmCrossfadeSource.Play();

        float startVol = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float targetCap = EffectiveBgmCap;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t);
            bgmCrossfadeSource.volume = Mathf.Lerp(0f, targetCap, t);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.clip = nextClip;
        bgmSource.loop = true;
        bgmSource.volume = EffectiveBgmCap;
        bgmSource.timeSamples = bgmCrossfadeSource.timeSamples;
        bgmSource.Play();

        bgmCrossfadeSource.Stop();
        bgmCrossfadeSource.volume = 0f;
        bgmFadeRoutine = null;
    }

    public void PlayMunchSound()
    {
        if (sfxSource == null || munchClips == null || munchClips.Length == 0) return;
        AudioClip clip = munchClips[Random.Range(0, munchClips.Length)];
        if (clip != null)
        {
            sfxSource.pitch = Random.Range(0.92f, 1.08f);
            sfxSource.PlayOneShot(clip, 0.85f * userSfxVolume);
        }
    }

    public void PlayHurtSound()
    {
        if (sfxSource == null || hurtClip == null) return;
        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.PlayOneShot(hurtClip, 0.95f * userSfxVolume);
    }

    public void PlayChimeSound()
    {
        if (sfxSource == null || chimeClip == null) return;
        sfxSource.pitch = 1.0f;
        sfxSource.PlayOneShot(chimeClip, 0.9f * userSfxVolume);
    }

    public void PlayMissSound()
    {
        if (sfxSource == null || missClip == null) return;
        sfxSource.pitch = 1.0f;
        sfxSource.PlayOneShot(missClip, 0.85f * userSfxVolume);
    }

    public void PlayPowerupSound()
    {
        if (sfxSource == null || powerupClip == null) return;
        sfxSource.pitch = 1.05f;
        sfxSource.PlayOneShot(powerupClip, 0.9f * userSfxVolume);
    }

    public void PlayEatFoodSound()
    {
        if (sfxSource == null) return;
        AudioClip clip = slimeEatClip != null ? slimeEatClip : ((munchClips != null && munchClips.Length > 0) ? munchClips[0] : null);
        if (clip != null)
        {
            sfxSource.pitch = Random.Range(0.96f, 1.06f);
            sfxSource.PlayOneShot(clip, 0.90f * userSfxVolume);
        }
        else
        {
            PlayMunchSound();
        }
    }

    public void PlayShieldPickupSound()
    {
        AudioClip c = shieldPickupClip != null ? shieldPickupClip : powerupClip;
        if (sfxSource != null && c != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(c, 0.95f * userSfxVolume);
        }
    }

    public void PlayMagnetPickupSound()
    {
        AudioClip c = magnetPickupClip != null ? magnetPickupClip : powerupClip;
        if (sfxSource != null && c != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(c, 0.95f * userSfxVolume);
        }
    }

    public void PlayTimeSlowPickupSound()
    {
        AudioClip c = timeSlowPickupClip != null ? timeSlowPickupClip : powerupClip;
        if (sfxSource != null && c != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(c, 0.95f * userSfxVolume);
        }
    }

    public void PlayHeartPickupSound()
    {
        AudioClip c = heartPickupClip != null ? heartPickupClip : chimeClip;
        if (sfxSource != null && c != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(c, 0.95f * userSfxVolume);
        }
    }

    public void PlayBonusHeartSound() => PlayHeartPickupSound();
    public void PlayPoisonSound() => PlayHurtSound();
    public void PlayJumpSound() => PlayChimeSound();

    public void PlayPukeSound()
    {
        if (sfxSource == null || slimePukeClip == null) return;
        sfxSource.pitch = 1.0f;
        sfxSource.PlayOneShot(slimePukeClip, 0.95f * userSfxVolume);
    }

    public void PlayFoodScamperSound()
    {
        if (sfxSource == null || foodScamperClip == null) return;
        sfxSource.pitch = 1.0f;
        sfxSource.PlayOneShot(foodScamperClip, 0.85f * userSfxVolume);
    }

    public void UpdatePowerUpAudio(bool shield, bool magnet, bool timeSlow)
    {
        // 1. Barrier Shield Shimmer
        AudioSource sSrc = shieldActiveSource != null ? shieldActiveSource : powerupActiveSource;
        if (sSrc != null && shieldActiveClip != null)
        {
            if (shield)
            {
                if (!sSrc.isPlaying || sSrc.clip != shieldActiveClip)
                {
                    sSrc.clip = shieldActiveClip;
                    sSrc.loop = true;
                    sSrc.volume = 0.38f * userSfxVolume;
                    sSrc.Play();
                }
            }
            else if (sSrc.isPlaying && sSrc.clip == shieldActiveClip)
            {
                sSrc.Stop();
            }
        }

        // 2. Star Magnet Whir
        AudioSource mSrc = magnetActiveSource != null ? magnetActiveSource : powerupActiveSource;
        if (mSrc != null && magnetActiveClip != null)
        {
            if (magnet)
            {
                if (!mSrc.isPlaying || mSrc.clip != magnetActiveClip)
                {
                    mSrc.clip = magnetActiveClip;
                    mSrc.loop = true;
                    mSrc.volume = 0.40f * userSfxVolume;
                    mSrc.Play();
                }
            }
            else if (mSrc.isPlaying && mSrc.clip == magnetActiveClip)
            {
                mSrc.Stop();
            }
        }

        // 3. Chrono Time-Slow Clock Ticking Loop
        AudioSource tSrc = timeSlowActiveSource != null ? timeSlowActiveSource : powerupActiveSource;
        if (tSrc != null && timeSlowActiveClip != null)
        {
            if (timeSlow)
            {
                if (!tSrc.isPlaying || tSrc.clip != timeSlowActiveClip)
                {
                    tSrc.clip = timeSlowActiveClip;
                    tSrc.loop = true;
                    tSrc.volume = 0.42f * userSfxVolume;
                    tSrc.Play();
                }
            }
            else if (tSrc.isPlaying && tSrc.clip == timeSlowActiveClip)
            {
                tSrc.Stop();
            }
        }
    }

    public void PlayButtonClickSound()
    {
        if (sfxSource == null) return;
        AudioClip clip = buttonClickClip != null ? buttonClickClip : chimeClip;
        if (clip != null)
        {
            sfxSource.pitch = Random.Range(0.96f, 1.04f);
            sfxSource.PlayOneShot(clip, 0.85f * userSfxVolume);
        }
    }

    public void PlayCountdownTick(bool isFinalGo)
    {
        if (sfxSource == null) return;
        AudioClip clip = isFinalGo ? chimeClip : powerupClip;
        if (clip != null)
        {
            sfxSource.pitch = isFinalGo ? 1.25f : 0.95f;
            sfxSource.PlayOneShot(clip, 0.80f * userSfxVolume);
        }
    }
}
