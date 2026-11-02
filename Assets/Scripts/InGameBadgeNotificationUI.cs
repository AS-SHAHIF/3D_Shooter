using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays in-game popup toast notification when player completes a milestone wave
/// (e.g. Wave 5, 10, 15). Shows text + badge icon and automatically hides after displayDuration.
/// </summary>
public class InGameBadgeNotificationUI : MonoBehaviour
{
    public static InGameBadgeNotificationUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject notificationRoot;
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private Image badgeImage;

    [Header("Badge Sprites")]
    [SerializeField] private Sprite survivalBadgeIcon;
    [SerializeField] private Sprite eliteBadgeIcon;
    [SerializeField] private Sprite legendBadgeIcon;

    [Header("Timing")]
    [SerializeField] private float displayDuration = 3.5f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (notificationRoot != null)
        {
            notificationRoot.SetActive(false);
        }
    }

    private void OnEnable()
    {
        BadgeSystem.OnBadgeUnlocked += HandleBadgeUnlocked;
    }

    private void OnDisable()
    {
        BadgeSystem.OnBadgeUnlocked -= HandleBadgeUnlocked;
    }

    private void HandleBadgeUnlocked(string badgeId)
    {
        Sprite icon = null;
        if (badgeId == BadgeSystem.BADGE_SURVIVAL) icon = survivalBadgeIcon;
        else if (badgeId == BadgeSystem.BADGE_ELITE) icon = eliteBadgeIcon;
        else if (badgeId == BadgeSystem.BADGE_LEGEND) icon = legendBadgeIcon;

        string badgeName = BadgeSystem.GetBadgeTitle(badgeId);
        ShowBadgePopup($"You got {badgeName}!", icon);
    }

    public void ShowBadgePopup(string message, Sprite icon)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }
        currentRoutine = StartCoroutine(ShowPopupRoutine(message, icon));
    }

    private IEnumerator ShowPopupRoutine(string message, Sprite icon)
    {
        if (notificationRoot != null)
        {
            if (notificationText != null)
            {
                notificationText.text = message;
            }

            if (badgeImage != null)
            {
                if (icon != null)
                {
                    badgeImage.sprite = icon;
                    badgeImage.gameObject.SetActive(true);
                }
                else
                {
                    badgeImage.gameObject.SetActive(false);
                }
            }

            notificationRoot.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(displayDuration);

        if (notificationRoot != null)
        {
            notificationRoot.SetActive(false);
        }
        currentRoutine = null;
    }
}
