using UnityEngine;
using UnityEngine.UI;
public class ZombieHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private bool hideUntilDamaged = false;

    private Camera cam;

    private void Start()
    {
        if (hideUntilDamaged) gameObject.SetActive(false);
    }

    public void SetHealth(int current, int max)
    {
        if (fillImage == null || max <= 0) return;
        if (hideUntilDamaged && current < max && current > 0)
        {
            gameObject.SetActive(true);
        }
        fillImage.fillAmount = Mathf.Clamp01((float)current / max);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    public void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam != null) transform.rotation = cam.transform.rotation;
    }
}
