using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickupPopupUI : MonoBehaviour
{
    public static PickupPopupUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform content;
    [SerializeField] private PickupRowUI rowPrefab;

    private readonly Dictionary<Pickup, PickupRowUI> rowMap = new Dictionary<Pickup, PickupRowUI>();
    private readonly List<Pickup> toRemove = new List<Pickup>();

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Refresh(IReadOnlyList<Pickup> items, PickupDetector detector)
    {
        var current = new HashSet<Pickup>(items);

        // Remove rows for items that are gone
        toRemove.Clear();
        foreach (var kv in rowMap)
        {
            if (kv.Key == null || !current.Contains(kv.Key))
            {
                toRemove.Add(kv.Key);
            }
        }
        foreach (var key in toRemove)
        {
            if (rowMap[key] != null) Destroy(rowMap[key].gameObject);
            rowMap.Remove(key);
        }

        // Add rows only for new items
        foreach (Pickup item in items)
        {
            if (item == null || rowMap.ContainsKey(item)) continue;

            PickupRowUI row = Instantiate(rowPrefab, content);
            Pickup captured = item;
            row.Setup(captured, () => detector.Collect(captured));
            rowMap[item] = row;
        }

        panel.SetActive(items.Count > 0);
    }
}