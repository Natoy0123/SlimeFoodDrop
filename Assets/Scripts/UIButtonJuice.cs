using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIButtonJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Squash & Bounce Settings")]
    public float pressedScale = 0.90f;
    public float hoverScale = 1.04f;
    public float bounceOvershoot = 1.12f;
    public float idleBreathingAmplitude = 0.014f;
    public float idleBreathingFrequency = 3.2f;

    private RectTransform rectTransform;
    private Image buttonImage;
    private Color baseColor = Color.white;
    private Vector3 baseScale = Vector3.one;
    private Vector3 currentScale = Vector3.one;
    private Vector3 scaleVelocity = Vector3.zero;
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
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    public void SetBaseColor(Color newBaseColor)
    {
        baseColor = newBaseColor;
        if (buttonImage == null) buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = isPressed ? Color.Lerp(baseColor, Color.white, 0.22f) : baseColor;
        }
    }

    private void OnEnable()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        isPressed = false;
        isHovered = false;
        currentScale = baseScale * 0.85f;
        rectTransform.localScale = currentScale;
    }

    private void Update()
    {
        Vector3 targetScale = baseScale;

        if (isPressed)
        {
            targetScale = new Vector3(baseScale.x * (pressedScale + 0.04f), baseScale.y * (pressedScale - 0.04f), 1f);
        }
        else if (isHovered)
        {
            targetScale = baseScale * hoverScale;
        }
        else if (idleBreathingAmplitude > 0.0001f)
        {
            float wave = Mathf.Sin(Time.unscaledTime * idleBreathingFrequency + phaseOffset) * idleBreathingAmplitude;
            targetScale = new Vector3(baseScale.x * (1f + wave), baseScale.y * (1f - wave * 0.7f), 1f);
        }

        currentScale = Vector3.SmoothDamp(currentScale, targetScale, ref scaleVelocity, 0.055f, Mathf.Infinity, Time.unscaledDeltaTime);
        rectTransform.localScale = currentScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        // Instant tactile squash on finger/mouse press
        currentScale = new Vector3(baseScale.x * 1.06f, baseScale.y * 0.86f, 1f);
        rectTransform.localScale = currentScale;

        if (buttonImage != null)
        {
            buttonImage.color = Color.Lerp(baseColor, Color.white, 0.22f);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressed) return;
        isPressed = false;

        // Spring overshoot pop on release
        currentScale = new Vector3(baseScale.x * bounceOvershoot, baseScale.y * bounceOvershoot, 1f);
        rectTransform.localScale = currentScale;

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
