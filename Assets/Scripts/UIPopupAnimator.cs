using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Provides juicy, stylish opening and closing animations for popups and UI panels.
/// Supports smooth scale bounce (overshoot), alpha fading, and unscaled time
/// (works even while the game is paused or during death screens).
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UIPopupAnimator : MonoBehaviour
{
    public enum AnimationStyle
    {
        ScaleAndFade,       // Bouncy scale + smooth alpha fade (Recommended)
        SlideUpAndFade,     // Slides in from slightly below + fade
        ScaleOnly,          // Pure elastic scale
        FadeOnly            // Smooth fade
    }

    [Header("Animation Style")]
    [SerializeField] private AnimationStyle style = AnimationStyle.ScaleAndFade;
    [SerializeField] private float openDuration = 0.28f;
    [SerializeField] private float closeDuration = 0.18f;

    [Header("Scale Properties")]
    [Range(0.1f, 1f)]
    [SerializeField] private float startScale = 0.7f;
    [SerializeField] private float overshoot = 1.4f;

    [Header("Slide Properties (if SlideUpAndFade)")]
    [SerializeField] private float slideOffset = 80f;

    [Header("Audio")]
    [SerializeField] private bool playSound = true;

    [Header("Auto Trigger")]
    [Tooltip("If true, automatically plays the opening animation whenever SetActive(true) is called on this panel.")]
    [SerializeField] private bool animateOnEnable = true;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalAnchoredPosition;
    private Vector3 originalScale;
    private Coroutine activeRoutine;
    private bool isClosing;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalAnchoredPosition = rectTransform.anchoredPosition;
            originalScale = rectTransform.localScale == Vector3.zero ? Vector3.one : rectTransform.localScale;
        }
        else
        {
            originalScale = transform.localScale == Vector3.zero ? Vector3.one : transform.localScale;
        }
    }

    private void OnEnable()
    {
        if (animateOnEnable && !isClosing)
        {
            Open();
        }
    }

    /// <summary>
    /// Open the popup with a stylish animation.
    /// </summary>
    public void Open(Action onComplete = null)
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        gameObject.SetActive(true);
        isClosing = false;

        if (playSound)
        {
            SoundManager.Instance?.PlayPopupOpen();
        }

        activeRoutine = StartCoroutine(AnimateOpenRoutine(onComplete));
    }

    /// <summary>
    /// Close the popup with a stylish shrink/fade animation and deactivate it on complete.
    /// </summary>
    public void Close(Action onComplete = null)
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
            return;
        }

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        isClosing = true;

        if (playSound)
        {
            SoundManager.Instance?.PlayPopupClose();
        }

        activeRoutine = StartCoroutine(AnimateCloseRoutine(onComplete));
    }

    private IEnumerator AnimateOpenRoutine(Action onComplete)
    {
        EnsureReferences();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        Vector3 targetScale = originalScale == Vector3.zero ? Vector3.one : originalScale;
        Vector3 initialScale = targetScale * startScale;

        if (rectTransform != null)
        {
            rectTransform.localScale = initialScale;
            if (style == AnimationStyle.SlideUpAndFade)
            {
                rectTransform.anchoredPosition = originalAnchoredPosition - new Vector2(0f, slideOffset);
            }
        }
        else
        {
            transform.localScale = initialScale;
        }

        float elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);

            // Alpha fade (faster than scale for punchy feel)
            canvasGroup.alpha = Mathf.Clamp01(t * 1.5f);

            // Scale Ease Out Back
            float easeScale = EaseOutBack(t, overshoot);
            Vector3 currentScale = Vector3.LerpUnclamped(initialScale, targetScale, easeScale);

            if (rectTransform != null)
            {
                if (style == AnimationStyle.ScaleAndFade || style == AnimationStyle.ScaleOnly)
                {
                    rectTransform.localScale = currentScale;
                }
                else if (style == AnimationStyle.SlideUpAndFade)
                {
                    float easeSlide = EaseOutCubic(t);
                    rectTransform.anchoredPosition = Vector2.Lerp(originalAnchoredPosition - new Vector2(0f, slideOffset), originalAnchoredPosition, easeSlide);
                    rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, easeSlide);
                }
            }
            else
            {
                transform.localScale = currentScale;
            }

            yield return null;
        }

        // Finalize state
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        if (rectTransform != null)
        {
            rectTransform.localScale = targetScale;
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }
        else
        {
            transform.localScale = targetScale;
        }

        activeRoutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator AnimateCloseRoutine(Action onComplete)
    {
        EnsureReferences();

        canvasGroup.interactable = false; // Block clicks while closing
        canvasGroup.blocksRaycasts = false;

        Vector3 baseScale = originalScale == Vector3.zero ? Vector3.one : originalScale;
        Vector3 minScale = baseScale * (startScale * 0.9f);

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < closeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / closeDuration);

            // Smooth alpha fade
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

            // Smooth shrink ease
            float easeShrink = EaseInQuad(t);
            Vector3 currentScale = Vector3.Lerp(baseScale, minScale, easeShrink);

            if (rectTransform != null)
            {
                rectTransform.localScale = currentScale;
                if (style == AnimationStyle.SlideUpAndFade)
                {
                    rectTransform.anchoredPosition = Vector2.Lerp(originalAnchoredPosition, originalAnchoredPosition - new Vector2(0f, slideOffset * 0.5f), easeShrink);
                }
            }
            else
            {
                transform.localScale = currentScale;
            }

            yield return null;
        }

        canvasGroup.alpha = 0f;
        if (rectTransform != null)
        {
            rectTransform.localScale = baseScale;
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }

        isClosing = false;
        gameObject.SetActive(false);

        activeRoutine = null;
        onComplete?.Invoke();
    }

    private void EnsureReferences()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (originalScale == Vector3.zero)
        {
            originalScale = rectTransform != null ? (rectTransform.localScale == Vector3.zero ? Vector3.one : rectTransform.localScale) : Vector3.one;
        }
    }

    #region Easing Curves

    private static float EaseOutBack(float t, float s)
    {
        t -= 1f;
        return t * t * ((s + 1f) * t + s) + 1f;
    }

    private static float EaseOutCubic(float t)
    {
        t -= 1f;
        return t * t * t + 1f;
    }

    private static float EaseInQuad(float t)
    {
        return t * t;
    }

    #endregion

    #region Static Helper API

    /// <summary>
    /// Open any UI panel GameObject with a stylish animation.
    /// If UIPopupAnimator is not already on the panel, it is added automatically.
    /// </summary>
    public static void OpenPopup(GameObject panel, Action onComplete = null)
    {
        if (panel == null) return;
        var animator = panel.GetComponent<UIPopupAnimator>() ?? panel.AddComponent<UIPopupAnimator>();
        animator.Open(onComplete);
    }

    /// <summary>
    /// Close any UI panel GameObject with a stylish animation.
    /// If UIPopupAnimator is not already on the panel, it is added automatically.
    /// </summary>
    public static void ClosePopup(GameObject panel, Action onComplete = null)
    {
        if (panel == null) return;
        var animator = panel.GetComponent<UIPopupAnimator>() ?? panel.AddComponent<UIPopupAnimator>();
        animator.Close(onComplete);
    }

    #endregion
}
