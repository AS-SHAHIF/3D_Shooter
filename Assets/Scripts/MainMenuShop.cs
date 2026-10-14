using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the Main Menu Crystal Shop per the PDF design specification (Section 4).
/// Allows exchanging saved Total XP for Crystals (e.g., 100 XP -> 10 Crystals).
/// Also updates the UI for Total XP and Total Crystals.
/// </summary>
public class MainMenuShop : MonoBehaviour
{
    [Header("UI Text Displays")]
    [SerializeField] private TMP_Text totalXpUI;         // e.g. "Total XP: 1,250 XP"
    [SerializeField] private TMP_Text totalCrystalsUI;   // e.g. "Total Crystals: 55"

    [Header("Exchange Config")]
    [SerializeField] private int xpCost = 100;           // Exchange cost
    [SerializeField] private int crystalReward = 10;     // Reward
    [SerializeField] private Button buyButton;           // Purchase button

    [Header("Optional Shop Panel")]
    [SerializeField] private GameObject shopPanel;       // If shop is inside a popup modal

    private void OnEnable()
    {
        XPSystem.OnTotalXpChanged += HandleTotalXpChanged;
        CrystalSaveSystem.OnCrystalsChanged += HandleCrystalsChanged;

        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyCrystalsClicked);
        }

        RefreshUI();
    }

    private void OnDisable()
    {
        XPSystem.OnTotalXpChanged -= HandleTotalXpChanged;
        CrystalSaveSystem.OnCrystalsChanged -= HandleCrystalsChanged;

        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(OnBuyCrystalsClicked);
        }
    }

    private void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (totalXpUI != null)
        {
            totalXpUI.text = $"{XPSystem.TotalXP:N0} XP";
        }

        if (totalCrystalsUI != null)
        {
            totalCrystalsUI.text = $"{CrystalSaveSystem.Total}";
        }

        if (buyButton != null)
        {
            // Enable button only if the player can afford the exchange
            buyButton.interactable = (XPSystem.TotalXP >= xpCost);
        }
    }

    private void HandleTotalXpChanged(int newXp) => RefreshUI();
    private void HandleCrystalsChanged(int newCrystals) => RefreshUI();

    /// <summary>Called when player clicks the exchange/buy button.</summary>
    public void OnBuyCrystalsClicked()
    {
        if (XPSystem.TrySpendTotalXP(xpCost))
        {
            CrystalSaveSystem.Add(crystalReward);
            CrystalSaveSystem.Save();
            RefreshUI();
        }
    }

    public void OpenShop()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
        RefreshUI();
    }

    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }
}
