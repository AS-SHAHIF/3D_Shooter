using System.Collections;
using UnityEngine;

/// <summary>
/// Idle eye-catcher for UI logos/buttons: every N seconds the target does a quick
/// zoom pulse (scale up + back) while tilting left and right, then returns to normal.
/// Works on any RectTransform (Image, TMP text, panels...).
/// </summary>
public class UIPulseTilt : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private RectTransform target;   // auto-uses this object if empty

    [Header("Timing")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float interval = 5f;        // seconds between animations
    [SerializeField] private float duration = 1f;        // length of one animation
    [SerializeField] private float startDelay = 2f;      // first delay when the scene loads
    [SerializeField] private bool useUnscaledTime = true;// safe in menus (timeScale 0)

    [Header("Zoom Pulse")]
    [SerializeField] private float zoomAmount = 0.12f;   // 0.12 = pops up to 112% size
    [SerializeField] private AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Tilt Left / Right")]
    [SerializeField] private float tiltAngle = 10f;      // degrees each way
    [SerializeField] private float tiltWiggles = 1.5f;   // full wiggle cycles within duration

    [Header("Optional Sound")]
    [SerializeField] private AudioSource audioSource;    // optional
    [SerializeField] private AudioClip pulseClip;        // soft "shing"/pop when it animates

    private Vector3 baseScale;
    private Quaternion baseRotation;
    private Coroutine loopRoutine;

    private void Awake()
    {
        if (target == null) target = GetComponent<RectTransform>();
        if (target != null)
        {
            baseScale = target.localScale;
            baseRotation = target.localRotation;
        }
        else
        {
            Debug.LogWarning("UIPulseTilt: No RectTransform target found.", this);
        }
    }

    private void Start()
    {
        if (playOnStart) Play();
    }

    /// <summary>Start the repeating every-N-seconds loop. Can be called from a button too.</summary>
    public void Play()
    {
        if (target == null) return;
        if (loopRoutine != null) StopCoroutine(loopRoutine);
        loopRoutine = StartCoroutine(LoopRoutine());
    }

    /// <summary>Stop animating and snap back to the original pose.</summary>
    public void Stop()
    {
        if (loopRoutine != null)
        {
            StopCoroutine(loopRoutine);
            loopRoutine = null;
        }
        ResetPose();
    }

    private IEnumerator LoopRoutine()
    {
        // First wait after the scene loads, then the regular interval.
        float delay = startDelay;
        while (true)
        {
            yield return WaitRealtime(delay);
            yield return PulseOnce();
            delay = interval;
        }
    }

    private IEnumerator PulseOnce()
    {
        if (audioSource != null && pulseClip != null)
        {
            audioSource.PlayOneShot(pulseClip);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float n = Mathf.Clamp01(elapsed / duration);

            // Zoom pulse: 0 -> 1 -> 0 (pop out and settle back).
            float zoom = zoomCurve.Evaluate(Mathf.Sin(n * Mathf.PI));

            // Tilt: smooth left / right wiggle that starts and ends at 0.
            float tilt = Mathf.Sin(n * Mathf.PI * 2f * tiltWiggles) * tiltAngle;

            target.localScale = baseScale * (1f + zoomAmount * zoom);
            target.localRotation = baseRotation * Quaternion.Euler(0f, 0f, tilt);

            yield return null;
        }

        ResetPose();
    }

    private void ResetPose()
    {
        if (target == null) return;
        target.localScale = baseScale;
        target.localRotation = baseRotation;
    }

    private object WaitRealtime(float seconds)
    {
        if (seconds <= 0f) return null;
        return useUnscaledTime
            ? (object)new WaitForSecondsRealtime(seconds)
            : new WaitForSeconds(seconds);
    }

    private void OnDisable()
    {
        // Never leave the logo tilted/scaled if the scene switches mid-animation.
        ResetPose();
    }
}
