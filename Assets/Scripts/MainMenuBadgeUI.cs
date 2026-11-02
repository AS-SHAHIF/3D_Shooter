using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Badges Menu in the Main Menu scene.
/// Allows viewing and claiming earned milestone badges (Wave 5, 10, 15).
/// Once claimed, the button shows "Claimed" and is disabled (not interactable).
/// </summary>
public class MainMenuBadgeUI : MonoBehaviour
{
    [System.Serializable]
    public class BadgeEntryUI
    {
        public string badgeId;
        public Image badgeIcon;
        public TMP_Text badgeTitleText;
        public TMP_Text badgeDescriptionText;
        public Button claimButton;
        public TMP_Text claimButtonText;
    }

    [Header("Badge UI Rows")]
    [SerializeField] private BadgeEntryUI survivalBadge;
    [SerializeField] private BadgeEntryUI eliteBadge;
    [SerializeField] private BadgeEntryUI legendBadge;

    [Header("Optional Badges Panel")]
    [SerializeField] private GameObject badgesPanel;

    private void OnEnable()
    {
        BadgeSystem.OnBadgeClaimed += HandleBadgeClaimed;
        BadgeSystem.OnBadgeUnlocked += HandleBadgeUnlocked;
        RefreshAllBadges();
    }

    private void OnDisable()
    {
        BadgeSystem.OnBadgeClaimed -= HandleBadgeClaimed;
        BadgeSystem.OnBadgeUnlocked -= HandleBadgeUnlocked;
    }

    private void Start()
    {
        // Wire up buttons
        if (survivalBadge != null && survivalBadge.claimButton != null)
        {
            survivalBadge.badgeId = BadgeSystem.BADGE_SURVIVAL;
            survivalBadge.claimButton.onClick.AddListener(() => OnClaimClicked(BadgeSystem.BADGE_SURVIVAL));
        }

        if (eliteBadge != null && eliteBadge.claimButton != null)
        {
            eliteBadge.badgeId = BadgeSystem.BADGE_ELITE;
            eliteBadge.claimButton.onClick.AddListener(() => OnClaimClicked(BadgeSystem.BADGE_ELITE));
        }

        if (legendBadge != null && legendBadge.claimButton != null)
        {
            legendBadge.badgeId = BadgeSystem.BADGE_LEGEND;
            legendBadge.claimButton.onClick.AddListener(() => OnClaimClicked(BadgeSystem.BADGE_LEGEND));
        }

        RefreshAllBadges();
    }

    public void RefreshAllBadges()
    {
        RefreshBadgeRow(survivalBadge, BadgeSystem.BADGE_SURVIVAL, "Survival", "Reach Wave 5");
        RefreshBadgeRow(eliteBadge, BadgeSystem.BADGE_ELITE, "Elite Survival", "Reach Wave 10");
        RefreshBadgeRow(legendBadge, BadgeSystem.BADGE_LEGEND, "Legendary Survivor", "Reach Wave 15");
    }

    private void RefreshBadgeRow(BadgeEntryUI entry, string badgeId, string defaultTitle, string defaultDesc)
    {
        if (entry == null) return;

        BadgeSystem.BadgeState state = BadgeSystem.GetBadgeState(badgeId);

        if (entry.badgeTitleText != null)
        {
            entry.badgeTitleText.text = defaultTitle;
        }

        if (entry.badgeDescriptionText != null)
        {
            entry.badgeDescriptionText.text = defaultDesc;
        }

        if (entry.claimButton != null)
        {
            switch (state)
            {
                case BadgeSystem.BadgeState.Locked:
                    entry.claimButton.interactable = false;
                    if (entry.claimButtonText != null) entry.claimButtonText.text = "Locked";
                    if (entry.badgeIcon != null) entry.badgeIcon.color = new Color(0.4f, 0.4f, 0.4f, 0.6f); // Dimmed
                    break;

                case BadgeSystem.BadgeState.Claimable:
                    entry.claimButton.interactable = true;
                    if (entry.claimButtonText != null) entry.claimButtonText.text = "Claim";
                    if (entry.badgeIcon != null) entry.badgeIcon.color = Color.white; // Full bright
                    break;

                case BadgeSystem.BadgeState.Claimed:
                    entry.claimButton.interactable = false;
                    if (entry.claimButtonText != null) entry.claimButtonText.text = "Claimed";
                    if (entry.badgeIcon != null) entry.badgeIcon.color = Color.white;
                    break;
            }
        }
    }

    private void OnClaimClicked(string badgeId)
    {
        if (BadgeSystem.TryClaimBadge(badgeId))
        {
            RefreshAllBadges();
        }
    }

    private void HandleBadgeClaimed(string id) => RefreshAllBadges();
    private void HandleBadgeUnlocked(string id) => RefreshAllBadges();

    public void OpenBadgesPanel()
    {
        if (badgesPanel != null) UIPopupAnimator.OpenPopup(badgesPanel);
        RefreshAllBadges();
    }

    public void CloseBadgesPanel()
    {
        if (badgesPanel != null) UIPopupAnimator.ClosePopup(badgesPanel);
    }
}
