using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PickupRowUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI amountText;
    public Button button;

    public void Setup(Pickup pickup, Action onClick)
    {
        if (iconImage)
        {
            iconImage.sprite = pickup.icon;
            iconImage.enabled = pickup.icon != null;
        }
        if (nameText) nameText.text = pickup.displayName;
        if (amountText) amountText.text = $"x{pickup.amount}";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick());
    }

}
