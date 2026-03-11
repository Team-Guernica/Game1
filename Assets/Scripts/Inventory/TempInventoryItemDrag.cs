using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

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
    private bool isDragging = false;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        img = GetComponent<Image>();

        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null)
        {
            Debug.LogError("[TempInventoryItemDrag] Canvas를 찾을 수 없음");
        }

        fromSlot = GetComponentInParent<TempInventorySlot>();
        if (fromSlot == null)
        {
            Debug.LogError("[TempInventoryItemDrag] 부모 TempInventorySlot이 필요함");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[Drag] OnBeginDrag 시작");
        
        if (fromSlot == null || !fromSlot.HasItem())
        {
            Debug.LogWarning("[Drag] 슬롯이 없거나 아이템이 없음");
            return;
        }

        // 드래그 불가능한 슬롯 체크
        if (!fromSlot.IsDraggable)
        {
            Debug.LogWarning("[TempInventoryItemDrag] 이 슬롯은 드래그할 수 없습니다.");
            return;
        }

        isDragging = true;
        originalParent = transform.parent;
        originalAnchoredPos = rect.anchoredPosition;

        img.raycastTarget = false;

        transform.SetParent(rootCanvas.transform, true);
        
        Debug.Log($"[Drag] 드래그 시작: {fromSlot.gameObject.name}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        rect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[Drag] OnEndDrag 시작, isDragging={isDragging}");
        
        if (!isDragging)
        {
            return;
        }

        TempInventorySlot toSlot = null;

        // Raycast를 사용하여 정확한 슬롯 찾기
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = eventData.position
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        Debug.Log($"[Drag] Raycast 결과 수: {results.Count}");

        foreach (var result in results)
        {
            Debug.Log($"[Drag] Hit: {result.gameObject.name}");
            TempInventorySlot slot = result.gameObject.GetComponentInParent<TempInventorySlot>();
            if (slot != null && slot != fromSlot)
            {
                toSlot = slot;
                Debug.Log($"[Drag] 타겟 슬롯 찾음: {toSlot.gameObject.name}");
                break;
            }
        }

        if (toSlot != null)
        {
            Debug.Log($"[Drag] 슬롯 스왑 시도: {fromSlot.gameObject.name} → {toSlot.gameObject.name}");
            if (TradeUIManager.Instance != null)
            {
                TradeUIManager.Instance.HandleSlotSwap(fromSlot, toSlot);
            }
            else
            {
                Debug.Log($"[Drag] TradeUIManager 없음, 직접 스왑");
                fromSlot.SwapWith(toSlot);
            }
        }
        else
        {
            Debug.LogWarning("[Drag] 타겟 슬롯을 찾지 못했습니다.");
        }

        transform.SetParent(originalParent, true);
        rect.anchoredPosition = originalAnchoredPos;

        img.raycastTarget = true;
        isDragging = false;

        fromSlot.RefreshUI();
        if (toSlot != null) toSlot.RefreshUI();
    }
}
