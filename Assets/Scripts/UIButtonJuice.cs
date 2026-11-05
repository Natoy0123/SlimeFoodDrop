using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIButtonJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Squash & Bounce Settings")]
    public float pressedScale = 0.94f;
    public float hoverScale = 1.04f;
    public float bounceOvershoot = 1.10f;
    public float idleBreathingAmplitude = 0.014f;
    public float idleBreathingFrequency = 3.2f;

    [Header("Physical Plinth Press Settings")]
    public float pressYOffset = -5f;

    private RectTransform rectTransform;
    private Image buttonImage;
    private Color baseColor = Color.white;
    private Vector3 baseScale = Vector3.one;
    private Vector3 currentScale = Vector3.one;
    private Vector3 scaleVelocity = Vector3.zero;
    private Vector2 baseAnchoredPosition = Vector2.zero;
    private Vector2 currentAnchoredPosition = Vector2.zero;
    private Vector2 posVelocity = Vector2.zero;
    private bool hasBasePosition = false;
    private bool isPressed = false;
    private bool isHovered = false;
    private float phaseOffset = 0f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        buttonImage = GetComponent<Image>();
        if (buttonImage != null) baseColor = buttonImage.color;
        baseScale = rectTransform.localScale;
        currentScale = baseScale;
        baseAnchoredPosition = rectTransform.anchoredPosition;
        currentAnchoredPosition = baseAnchoredPosition;
        hasBasePosition = true;
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    public void SetBaseColor(Color newBaseColor)
    {
        baseColor = newBaseColor;
        if (buttonImage == null) buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = isPressed ? Color.Lerp(baseColor, Color.white, 0.28f) : baseColor;
        }
    }

    private void OnEnable()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (!hasBasePosition && rectTransform != null)
        {
            baseAnchoredPosition = rectTransform.anchoredPosition;
            hasBasePosition = true;
        }
        isPressed = false;
        isHovered = false;
        currentScale = baseScale * 0.92f;
        rectTransform.localScale = currentScale;
        if (hasBasePosition)
        {
            currentAnchoredPosition = baseAnchoredPosition;
            rectTransform.anchoredPosition = baseAnchoredPosition;
        }
    }

    private void Update()
    {
        Vector3 targetScale = baseScale;
        Vector2 targetPos = hasBasePosition ? baseAnchoredPosition : Vector2.zero;

        if (isPressed)
        {
            targetScale = new Vector3(baseScale.x * (pressedScale + 0.02f), baseScale.y * (pressedScale - 0.02f), 1f);
            if (hasBasePosition) targetPos = baseAnchoredPosition + new Vector2(0f, pressYOffset);
        }
        else if (isHovered)
        {
            targetScale = baseScale * hoverScale;
            if (hasBasePosition) targetPos = baseAnchoredPosition + new Vector2(0f, 1f);
        }
        else if (idleBreathingAmplitude > 0.0001f)
        {
            float wave = Mathf.Sin(Time.unscaledTime * idleBreathingFrequency + phaseOffset) * idleBreathingAmplitude;
            targetScale = new Vector3(baseScale.x * (1f + wave), baseScale.y * (1f - wave * 0.7f), 1f);
        }

        currentScale = Vector3.SmoothDamp(currentScale, targetScale, ref scaleVelocity, 0.055f, Mathf.Infinity, Time.unscaledDeltaTime);
        rectTransform.localScale = currentScale;

        if (hasBasePosition)
        {
            currentAnchoredPosition = Vector2.SmoothDamp(currentAnchoredPosition, targetPos, ref posVelocity, 0.045f, Mathf.Infinity, Time.unscaledDeltaTime);
            rectTransform.anchoredPosition = currentAnchoredPosition;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        // Instant tactile squash and downward plinth depression on press
        currentScale = new Vector3(baseScale.x * 1.03f, baseScale.y * 0.93f, 1f);
        rectTransform.localScale = currentScale;

        if (hasBasePosition)
        {
            currentAnchoredPosition = baseAnchoredPosition + new Vector2(0f, pressYOffset);
            rectTransform.anchoredPosition = currentAnchoredPosition;
        }

        if (buttonImage != null)
        {
            buttonImage.color = Color.Lerp(baseColor, Color.white, 0.28f);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressed) return;
        isPressed = false;

        // Spring overshoot pop on release
        currentScale = new Vector3(baseScale.x * bounceOvershoot, baseScale.y * bounceOvershoot, 1f);
        rectTransform.localScale = currentScale;

        if (hasBasePosition)
        {
            currentAnchoredPosition = baseAnchoredPosition + new Vector2(0f, 2f);
            rectTransform.anchoredPosition = currentAnchoredPosition;
        }

        if (buttonImage != null)
        {
            buttonImage.color = baseColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPressed = false;
        isHovered = false;
        if (buttonImage != null)
        {
            buttonImage.color = baseColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClickSound();
        }
    }
}
