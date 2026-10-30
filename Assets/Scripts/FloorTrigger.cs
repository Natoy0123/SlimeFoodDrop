using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FloorTrigger : MonoBehaviour
{
    private void Awake()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        FallingItem item = other.GetComponent<FallingItem>();
        if (item == null) return;

        if (!item.TryMarkTriggered()) return;

        if (GameManager.Instance != null && GameManager.Instance.IsPlaying)
        {
            if (item.itemType == FallingItem.ItemType.Food)
            {
                // Missed edible food does NOT deduct any life — only resets the combo streak!
                GameManager.Instance.OnFoodMissed();
            }
            // Poison and power-ups hitting the floor are safely destroyed with 0 penalty
        }

        Destroy(item.gameObject);
    }
}
