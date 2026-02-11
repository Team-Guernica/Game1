using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TempInventoryItemDrag : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas rootCanvas;
    private RectTransform rect;
    private Image img;

    private TempInventorySlot fromSlot;
    private Transform originalParent;
    private Vector2 originalAnchoredPos;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        img = GetComponent<Image>();

        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null)
        {
            Debug.LogError("[TempInventoryItemDrag] Canvas를 찾지 못했음");
        }

        fromSlot = GetComponentInParent<TempInventorySlot>();
        if (fromSlot == null)
        {
            Debug.LogError("[TempInventoryItemDrag] 부모에 TempInventorySlot이 필요함");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (fromSlot == null || !fromSlot.HasItem()) return;

        originalParent = transform.parent;
        originalAnchoredPos = rect.anchoredPosition;

        // 드래그 중에는 드롭 타겟을 막지 않게
        img.raycastTarget = false;

        // 최상단으로 올려서 다른 UI 위로 보이게
        transform.SetParent(rootCanvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (fromSlot == null || !fromSlot.HasItem()) return;

        // 화면 좌표 따라가기
        rect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (fromSlot == null || !fromSlot.HasItem())
        {
            // 원복
            img.raycastTarget = true;
            return;
        }

        // 드롭된 대상 아래에서 슬롯 찾기
        TempInventorySlot toSlot = null;

        if (eventData.pointerEnter != null)
        {
            // 1) 바로 슬롯이 잡히는 경우
            toSlot = eventData.pointerEnter.GetComponentInParent<TempInventorySlot>();
        }

        // 슬롯 위에 드롭했으면 스왑
        if (toSlot != null && toSlot != fromSlot)
        {
            fromSlot.SwapWith(toSlot);
        }

        // 원래 위치로 복귀(스왑 여부와 관계없이 이미지 오브젝트는 항상 제자리)
        transform.SetParent(originalParent, true);
        rect.anchoredPosition = originalAnchoredPos;

        // 레이캐스트 다시 켬
        img.raycastTarget = true;

        // 슬롯 UI 갱신(혹시 모를 상태 보정)
        fromSlot.RefreshUI();
        if (toSlot != null) toSlot.RefreshUI();
    }
}
