using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Typewriter ("typing in") reveal for a TMP title, played only once when the scene loads.
/// Attach to the title GameObject (or assign the label manually).
/// </summary>
public class TypewriterTitle : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private TMP_Text label;              // auto-uses this object's TMP_Text if empty

    [Header("Typing")]
    [SerializeField] private bool playOnStart = true;     // play automatically once on scene load
    [SerializeField] private float charactersPerSecond = 14f;
    [SerializeField] private float startDelay = 0.3f;     // small pause before typing begins
    [SerializeField] private bool useUnscaledTime = true; // safe even if timeScale is 0

    [Header("Optional Typing Sound")]
    [SerializeField] private AudioSource audioSource;     // optional - leave empty for silent typing
    [SerializeField] private AudioClip typingClip;        // short blip played per character
    [SerializeField] private float soundInterval = 0.05f; // minimum seconds between blips

    private string fullText = "";
    private Coroutine typingRoutine;
    private float lastBlipTime;

    private void Awake()
    {
        if (label == null) label = GetComponent<TMP_Text>();

        if (label != null)
        {
            fullText = label.text;
            // Start fully hidden; the coroutine reveals characters one by one.
            label.maxVisibleCharacters = 0;
        }
        else
        {
            Debug.LogWarning("TypewriterTitle: No TMP_Text found.", this);
        }
    }

    private void Start()
    {
        if (playOnStart) Play();
    }

    /// <summary>Start (or restart) the one-time typing animation. Can also be hooked to a UI button.</summary>
    public void Play()
    {
        if (label == null) return;

        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
        }
        typingRoutine = StartCoroutine(TypeRoutine());
    }

    private IEnumerator TypeRoutine()
    {
        label.text = fullText;
        label.maxVisibleCharacters = 0;

        if (startDelay > 0f)
        {
            yield return WaitRealtime(startDelay);
        }

        int totalCharacters = fullText.Length;

        for (int visible = 1; visible <= totalCharacters; visible++)
        {
            label.maxVisibleCharacters = visible;
            PlayBlip();

            float step = charactersPerSecond > 0f ? 1f / charactersPerSecond : 0f;
            yield return WaitRealtime(step);
        }

        typingRoutine = null;
    }

    private object WaitRealtime(float seconds)
    {
        if (seconds <= 0f) return null;
        return useUnscaledTime
            ? (object)new WaitForSecondsRealtime(seconds)
            : new WaitForSeconds(seconds);
    }

    private void PlayBlip()
    {
        if (audioSource == null || typingClip == null) return;

        float now = Time.unscaledTime;
        if (now - lastBlipTime < soundInterval) return;

        lastBlipTime = now;
        audioSource.PlayOneShot(typingClip);
    }
}
