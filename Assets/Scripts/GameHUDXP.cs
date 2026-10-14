using TMPro;
using UnityEngine;

/// <summary>
/// Displays in-game Run XP on the HUD during gameplay.
/// </summary>
public class GameHUDXP : MonoBehaviour
{
    [SerializeField] private TMP_Text runXpUI;

    private void OnEnable()
    {
        XPSystem.OnRunXpChanged += UpdateRunXpText;
        UpdateRunXpText(XPSystem.RunXP);
    }

    private void OnDisable()
    {
        XPSystem.OnRunXpChanged -= UpdateRunXpText;
    }

    private void Start()
    {
        UpdateRunXpText(XPSystem.RunXP);
    }

    private void UpdateRunXpText(int xp)
    {
        if (runXpUI != null)
        {
            runXpUI.text = $"XP: {xp}";
        }
    }
}
