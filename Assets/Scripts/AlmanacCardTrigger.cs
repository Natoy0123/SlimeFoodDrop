using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AlmanacCardTrigger : MonoBehaviour, IPointerClickHandler
{
    public Sprite artwork;
    public string categoryBadge;
    public Color badgeColor = Color.white;
    public string itemName;
    public string stats;
    [TextArea(3, 6)] public string loreDescription;
    [TextArea(2, 4)] public string strategyTips;

    private void Awake()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(TriggerModal);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TriggerModal();
    }

    public void TriggerModal()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowAlmanacItemDetail(artwork, categoryBadge, badgeColor, itemName, stats, loreDescription, strategyTips);
        }
    }
}
