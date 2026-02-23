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

    [Header("Slot Type")]
    [SerializeField] private bool isDraggable = true; // 드래그 가능 여부

    public ItemId ItemId => itemId;
    public int Amount => amount;
    public Sprite Icon => icon;
    public bool IsDraggable => isDraggable;

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

        // 수량 텍스트는 Raycast를 막지 않도록 설정
        if (amountText != null)
        {
            amountText.raycastTarget = false;
        }

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

    /// <summary>
    /// 레시피 슬롯을 드래그 불가능하게 설정
    /// </summary>
    public void SetDraggable(bool draggable)
    {
        isDraggable = draggable;
    }
}