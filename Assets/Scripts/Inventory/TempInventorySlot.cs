using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TempInventorySlot : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text amountText;

    [Header("State")]
    [SerializeField] private ItemId itemId;
    [SerializeField] private int amount;
    [SerializeField] private Sprite icon;

    public ItemId ItemId => itemId;
    public int Amount => amount;
    public Sprite Icon => icon;

    private void Awake()
    {
        if (itemImage == null)
        {
            var t = transform.Find("ItemImage");
            if (t != null) itemImage = t.GetComponent<Image>();
            if (itemImage == null) itemImage = GetComponentInChildren<Image>(true);
        }

        if (amountText == null)
            amountText = GetComponentInChildren<TMP_Text>(true);

        RefreshUI();
    }

    public bool HasItem()
    {
        return amount > 0 && icon != null;
    }

    public void Set(ItemId id, int amt, Sprite ico)
    {
        itemId = id;
        amount = Mathf.Max(0, amt);
        icon = ico;
        RefreshUI();
    }

    public void Clear()
    {
        amount = 0;
        icon = null;
        RefreshUI();
    }

    public void RefreshUI()
    {
        bool has = HasItem();

        if (itemImage != null)
        {
            itemImage.enabled = has;
            itemImage.sprite = has ? icon : null;
        }

        if (amountText != null)
        {
            amountText.text = has ? amount.ToString() : "";
        }
    }

    public void SwapWith(TempInventorySlot other)
    {
        if (other == null || other == this) return;

        (itemId, other.itemId) = (other.itemId, itemId);
        (amount, other.amount) = (other.amount, amount);
        (icon, other.icon) = (other.icon, icon);

        RefreshUI();
        other.RefreshUI();
    }
}