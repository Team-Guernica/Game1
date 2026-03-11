using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TradeUIManager : BaseBehaviour
{
    public static TradeUIManager Instance { get; private set;}

    [Header("UI Roots")]
    [SerializeField] private GameObject tradePanel;

    public TradeRecipe currentNpcRecipe = new TradeRecipe();

    [Header("Offer Slot (1)")]
    [SerializeField] private ItemId offerItemId = ItemId.WhiteDebris;
    [SerializeField] private int offerAmount = 0;

    [Header("UI Binding")]
    [SerializeField] private TempInventorySlot[] inventorySlots; // 인벤 UI 슬롯들
    [SerializeField] private TempInventorySlot[] requireSlots;   // 요구 UI 슬롯들 (레시피 requires 표시)
    [SerializeField] private TempInventorySlot[] rewardSlots;    // 보상 UI 슬롯들 (레시피 rewards 표시)
    [SerializeField] private TempInventorySlot offerSlotUI;
    [SerializeField] private ItemIconTable iconTable;            // 아이콘 매핑

    // 커스텀 에디터 읽기용
    public ItemId DebugOfferItemId => offerItemId;
    public int DebugOfferAmount => offerAmount;

    // 레시피 풀
    private List<TradeRecipe> recipePool = new List<TradeRecipe>();

    // 커스텀 에디터에서 읽기용
    public InventoryData DebugInventory => InventoryManager.Instance != null ? InventoryManager.Instance.Data : null;
    public TradeRecipe DebugCurrentRecipe => currentNpcRecipe;

    protected override void Initialize()
    {
        base.Initialize();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        tradePanel.SetActive(false);

        BuildRecipePool();
    }


    public void OpenTradeUI()
    {
        tradePanel.SetActive(true);
        Cursor.visible = true;
        RefreshAllUI();
    }

    public void CloseTradeUI()
    {
        tradePanel.SetActive(false);
        Cursor.visible = false;
        RefreshAllUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && IsOpen())
        {
            CloseTradeUI();
        }
    }

    public bool IsOpen()
    {
        return tradePanel != null && tradePanel.activeSelf;
    }

    /// <summary>
    /// 16개 슬롯을 채우도록 랜덤 인벤토리 생성
    /// </summary>
    public void DebugFillInventoryAllTypes()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[TradeUI] InventoryManager.Instance 없음");
            return;
        }

        var inv = InventoryManager.Instance.Data;
        inv.Clear();

        // 아이템 종류 목록
        List<ItemId> itemTypes = new List<ItemId>();
        foreach (ItemId id in System.Enum.GetValues(typeof(ItemId)))
        {
            itemTypes.Add(id);
        }

        // 16개 슬롯을 채우기 위해 아이템을 반복하여 추가
        int slotsToFill = Mathf.Min(16, inventorySlots != null ? inventorySlots.Length : 16);
        
        for (int i = 0; i < slotsToFill; i++)
        {
            // 아이템 종류를 순환하면서 선택 (다양성을 위해)
            ItemId id = itemTypes[i % itemTypes.Count];
            
            // 랜덤 수량 생성
            int amt;
            if (id is ItemId.Water or ItemId.Armor or ItemId.RedJuice or ItemId.Badge)
            {
                amt = Random.Range(1, 5); // 희귀 아이템: 1~4개
            }
            else
            {
                amt = Random.Range(3, 21); // 일반 아이템: 3~20개
            }
            
            // 인벤토리에 추가
            inv.Add(id, amt);
        }

        Debug.Log($"[TradeUI] 인벤토리 더미 생성 완료 ({slotsToFill}개 슬롯 채움)");
        RefreshAllUI();
    }

    /// <summary>
    /// 레시피 풀에서 랜덤으로 NPC 레시피 선택
    /// </summary>
    public void DebugRandomizeNpcRecipe()
    {
        if (recipePool == null || recipePool.Count == 0)
        {
            Debug.LogWarning("[TradeUI] 레시피 풀이 비어 있음");
            return;
        }

        currentNpcRecipe = recipePool[Random.Range(0, recipePool.Count)];
        Debug.Log($"[TradeUI] NPC 레시피 선택: {currentNpcRecipe.recipeName}");
        RefreshAllUI();
    }

    /// <summary>
    /// 교역 1회 실행: 요구량 검사 → 차감 → 보상 지급
    /// </summary>
    public void DebugTradeOnce()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[TradeUI] InventoryManager.Instance 없음");
            return;
        }

        var inv = InventoryManager.Instance.Data;

        if (currentNpcRecipe == null)
        {
            Debug.LogWarning("[TradeUI] currentNpcRecipe 없음");
            return;
        }

        if (!inv.HasAll(currentNpcRecipe.requires, out string reason))
        {
            Debug.LogWarning($"[TradeUI] 교역 실패: {reason}");
            return;
        }

        inv.ConsumeAll(currentNpcRecipe.requires);
        inv.AddAll(currentNpcRecipe.rewards);

        Debug.Log($"[TradeUI] 교역 성공: {currentNpcRecipe.recipeName}");
        RefreshAllUI();
    }

    // =========================================================
    // Recipe Pool (현재 7종 아이템에 맞춘 더미 레시피)
    // =========================================================

    private void BuildRecipePool()
    {
        recipePool = new List<TradeRecipe>();

        recipePool.Add(new TradeRecipe
        {
            recipeName = "흰 부스러기 10 → 물 1",
            requires = new List<ItemStack> { new ItemStack(ItemId.WhiteDebris, 10) },
            rewards = new List<ItemStack> { new ItemStack(ItemId.Water, 1) }
        });

        recipePool.Add(new TradeRecipe
        {
            recipeName = "금화 5 → 갑옷 1",
            requires = new List<ItemStack> { new ItemStack(ItemId.GoldCoin, 5) },
            rewards = new List<ItemStack> { new ItemStack(ItemId.Armor, 1) }
        });

        recipePool.Add(new TradeRecipe
        {
            recipeName = "물 3 → 붉은 주스 1",
            requires = new List<ItemStack> { new ItemStack(ItemId.Water, 3) },
            rewards = new List<ItemStack> { new ItemStack(ItemId.RedJuice, 1) }
        });

    }

    public void DebugSetOffer(ItemId id, int amount)
    {
        offerItemId = id;
        offerAmount = Mathf.Max(0, amount);
        RefreshAllUI();
    }

    public void DebugClearOffer()
    {
        // 오퍼 슬롯의 아이템을 인벤토리로 반환
        ReturnOfferToInventory();
        RefreshAllUI();
    }

    public void DebugTradeUsingOfferSlot()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[TradeUI] InventoryManager.Instance 없음");
            return;
        }
        var inv = InventoryManager.Instance.Data;

        if (currentNpcRecipe == null)
        {
            Debug.LogWarning("[TradeUI] currentNpcRecipe 없음");
            return;
        }

        if (currentNpcRecipe.requires == null || currentNpcRecipe.requires.Count == 0)
        {
            Debug.LogWarning("[TradeUI] 레시피 requires 비어있음");
            return;
        }

        if (currentNpcRecipe.requires.Count != 1)
        {
            Debug.LogWarning("[TradeUI] 오퍼 슬롯 1개 버전은 requires가 1개인 레시피만 지원함");
            return;
        }

        ItemStack req = currentNpcRecipe.requires[0];

        if (offerItemId != req.itemId)
        {
            Debug.LogWarning($"[TradeUI] 교역 실패: 올려둔 아이템 불일치 (올림 {ItemName.ToKorean(offerItemId)} / 요구 {ItemName.ToKorean(req.itemId)})");
            return;
        }

        if (offerAmount < req.amount)
        {
            Debug.LogWarning($"[TradeUI] 교역 실패: 올려둔 수량 부족 (올림 {offerAmount} / 요구 {req.amount})");
            return;
        }

        // 오퍼 슬롯에 있는 아이템은 이미 인벤토리에서 차감됨
        // 보상만 지급
        inv.AddAll(currentNpcRecipe.rewards);

        Debug.Log($"[TradeUI] 교역 성공(Offer 1): {currentNpcRecipe.recipeName}");

        // 오퍼 슬롯 초기화 (인벤토리 복구 없이)
        offerItemId = ItemId.WhiteDebris;
        offerAmount = 0;
        
        RefreshAllUI();
    }
    public void SetNpcRecipe(TradeRecipe recipe)
    {
        currentNpcRecipe = recipe;
        RefreshAllUI();
    }

    public void RefreshAllUI()
    {
        RefreshInventoryUI();
        RefreshRecipeUI();
        RefreshOfferUI();
    }

    /// <summary>
    /// 인벤토리 UI 갱신 (최대 16개 슬롯 지원)
    /// 같은 아이템이 여러 슬롯에 나뉘어 표시될 수 있음
    /// </summary>
    private void RefreshInventoryUI()
    {
        if (inventorySlots == null || inventorySlots.Length == 0) return;
        if (InventoryManager.Instance == null) return;

        var inv = InventoryManager.Instance.Data;
        var items = inv.Items;

        // 모든 슬롯을 인벤토리 데이터로 채움 (최대 16개)
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            var slot = inventorySlots[i];
            if (slot == null) continue;

            if (items != null && i < items.Count)
            {
                // 인벤토리에 있는 아이템 표시
                var itemStack = items[i];
                Sprite icon = iconTable != null ? iconTable.GetIcon(itemStack.itemId) : null;
                slot.Set(itemStack.itemId, itemStack.amount, icon);
            }
            else
            {
                // 빈 슬롯
                slot.Clear();
            }
        }
    }

    private void RefreshRecipeUI()
    {
        if (currentNpcRecipe == null) return;

        // 요구
        if (requireSlots != null)
        {
            for (int i = 0; i < requireSlots.Length; i++)
            {
                var slot = requireSlots[i];
                if (slot == null) continue;

                // 레시피 슬롯은 드래그 불가능하게 설정
                slot.SetDraggable(false);

                if (currentNpcRecipe.requires == null || i >= currentNpcRecipe.requires.Count)
                {
                    slot.Clear();
                    continue;
                }

                var r = currentNpcRecipe.requires[i];
                Sprite icon = iconTable != null ? iconTable.GetIcon(r.itemId) : null;
                slot.Set(r.itemId, r.amount, icon);
            }
        }

        // 보상
        if (rewardSlots != null)
        {
            for (int i = 0; i < rewardSlots.Length; i++)
            {
                var slot = rewardSlots[i];
                if (slot == null) continue;

                // 레시피 슬롯은 드래그 불가능하게 설정
                slot.SetDraggable(false);

                if (currentNpcRecipe.rewards == null || i >= currentNpcRecipe.rewards.Count)
                {
                    slot.Clear();
                    continue;
                }

                var r = currentNpcRecipe.rewards[i];
                Sprite icon = iconTable != null ? iconTable.GetIcon(r.itemId) : null;
                slot.Set(r.itemId, r.amount, icon);
            }
        }
    }
    private void RefreshOfferUI()
    {
        if (offerSlotUI == null) return;

        // offerAmount == 0 이면 빈칸으로 보이게
        if (offerAmount <= 0)
        {
            offerSlotUI.Clear();
            return;
        }

        Sprite icon = iconTable != null ? iconTable.GetIcon(offerItemId) : null;
        offerSlotUI.Set(offerItemId, offerAmount, icon);
    }

    public void OnClickTradeButton()
    {
        DebugTradeUsingOfferSlot();
        RefreshAllUI();
    }

    /// <summary>
    /// 인벤토리에서 아이템을 오퍼 슬롯에 추가
    /// </summary>
    public void AddToOfferSlot(ItemId itemId, int amount)
    {
        if (InventoryManager.Instance == null) return;
        var inv = InventoryManager.Instance.Data;

        int availableAmount = inv.Get(itemId);
        if (availableAmount <= 0)
        {
            Debug.LogWarning($"[TradeUI] 인벤토리에 {ItemName.ToKorean(itemId)}이(가) 없습니다.");
            return;
        }

        if (offerAmount == 0)
        {
            offerItemId = itemId;
        }
        else if (offerItemId != itemId)
        {
            Debug.LogWarning("[TradeUI] 오퍼 슬롯에는 같은 종류의 아이템만 올릴 수 있습니다.");
            return;
        }

        int amountToAdd = Mathf.Min(amount, availableAmount - offerAmount);
        if (amountToAdd <= 0)
        {
            Debug.LogWarning("[TradeUI] 더 이상 올릴 수 없습니다.");
            return;
        }

        offerAmount += amountToAdd;
        Debug.Log($"[TradeUI] {ItemName.ToKorean(itemId)} x{amountToAdd} 오퍼 슬롯에 추가");
        RefreshAllUI();
    }

    /// <summary>
    /// 오퍼 슬롯에서 아이템 제거
    /// </summary>
    public void RemoveFromOfferSlot(int amount)
    {
        if (offerAmount <= 0) return;

        int amountToRemove = Mathf.Min(amount, offerAmount);
        offerAmount -= amountToRemove;

        if (offerAmount <= 0)
        {
            offerAmount = 0;
            Debug.Log("[TradeUI] 오퍼 슬롯 비움");
        }
        else
        {
            Debug.Log($"[TradeUI] {ItemName.ToKorean(offerItemId)} x{amountToRemove} 제거");
        }

        RefreshAllUI();
    }

    /// <summary>
    /// 슬롯 타입 확인 (인벤토리 슬롯인지, 오퍼 슬롯인지)
    /// </summary>
    public bool IsInventorySlot(TempInventorySlot slot)
    {
        if (inventorySlots == null || slot == null) return false;
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == slot) return true;
        }
        return false;
    }

    public bool IsOfferSlot(TempInventorySlot slot)
    {
        return slot != null && slot == offerSlotUI;
    }

    /// <summary>
    /// 드래그 앤 드롭으로 슬롯 간 이동 처리
    /// </summary>
    public void HandleSlotSwap(TempInventorySlot fromSlot, TempInventorySlot toSlot)
    {
        if (fromSlot == null || toSlot == null || fromSlot == toSlot) return;

        bool fromIsInventory = IsInventorySlot(fromSlot);
        bool fromIsOffer = IsOfferSlot(fromSlot);
        bool fromIsRecipe = IsRecipeSlot(fromSlot);
        
        bool toIsInventory = IsInventorySlot(toSlot);
        bool toIsOffer = IsOfferSlot(toSlot);
        bool toIsRecipe = IsRecipeSlot(toSlot);

        // 레시피 슬롯은 드래그 불가
        if (fromIsRecipe)
        {
            Debug.LogWarning("[TradeUI] 레시피 슬롯에서는 드래그할 수 없습니다.");
            return;
        }

        // 레시피 슬롯으로는 드롭 불가
        if (toIsRecipe)
        {
            Debug.LogWarning("[TradeUI] 레시피 슬롯으로는 드롭할 수 없습니다.");
            return;
        }

        // 인벤토리 → 인벤토리 (스왑)
        if (fromIsInventory && toIsInventory)
        {
            SwapInventorySlots(fromSlot, toSlot);
            RefreshAllUI();
            return;
        }

        // 인벤토리 → 오퍼 슬롯
        if (fromIsInventory && toIsOffer)
        {
            if (!fromSlot.HasItem()) return;

            // 레시피 검증 및 요구 수량 확인
            if (!TryMoveToOfferSlot(fromSlot.ItemId))
            {
                return;
            }

            RefreshAllUI();
        }
        // 오퍼 슬롯 → 인벤토리
        else if (fromIsOffer && toIsInventory)
        {
            if (offerAmount > 0)
            {
                ReturnOfferToInventory();
                RefreshAllUI();
            }
        }
    }

    /// <summary>
    /// 레시피 슬롯인지 확인 (요구 또는 보상 슬롯)
    /// </summary>
    private bool IsRecipeSlot(TempInventorySlot slot)
    {
        if (slot == null) return false;

        // 요구 슬롯 확인
        if (requireSlots != null)
        {
            for (int i = 0; i < requireSlots.Length; i++)
            {
                if (requireSlots[i] == slot) return true;
            }
        }

        // 보상 슬롯 확인
        if (rewardSlots != null)
        {
            for (int i = 0; i < rewardSlots.Length; i++)
            {
                if (rewardSlots[i] == slot) return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 인벤토리 슬롯 간 아이템 스왑
    /// </summary>
    private void SwapInventorySlots(TempInventorySlot slotA, TempInventorySlot slotB)
    {
        // UI 레벨에서만 스왑 (슬롯 데이터 교환)
        slotA.SwapWith(slotB);
        
        Debug.Log($"[TradeUI] 슬롯 스왑 완료");
    }

    /// <summary>
    /// 슬롯의 인벤토리 인덱스 찾기
    /// </summary>
    private int GetInventorySlotIndex(TempInventorySlot slot)
    {
        if (inventorySlots == null || slot == null) return -1;

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == slot)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 인벤토리에서 오퍼 슬롯으로 아이템 이동 시도
    /// </summary>
    private bool TryMoveToOfferSlot(ItemId itemId)
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[TradeUI] InventoryManager.Instance 없음");
            return false;
        }

        var inv = InventoryManager.Instance.Data;

        // 현재 레시피 확인
        if (currentNpcRecipe == null)
        {
            Debug.LogWarning("[TradeUI] 현재 레시피가 없습니다.");
            return false;
        }

        if (currentNpcRecipe.requires == null || currentNpcRecipe.requires.Count == 0)
        {
            Debug.LogWarning("[TradeUI] 레시피 요구사항이 비어있습니다.");
            return false;
        }

        // 단일 요구사항 레시피만 지원
        if (currentNpcRecipe.requires.Count != 1)
        {
            Debug.LogWarning("[TradeUI] 오퍼 슬롯은 요구사항이 1개인 레시피만 지원합니다.");
            return false;
        }

        ItemStack requiredItem = currentNpcRecipe.requires[0];

        // 레시피가 요구하는 아이템이 아닌 경우
        if (itemId != requiredItem.itemId)
        {
            Debug.LogWarning($"[TradeUI] 이 레시피는 {ItemName.ToKorean(requiredItem.itemId)}을(를) 요구합니다. (시도한 아이템: {ItemName.ToKorean(itemId)})");
            return false;
        }

        // 이미 오퍼 슬롯에 있는 경우
        if (offerAmount > 0)
        {
            Debug.LogWarning($"[TradeUI] 이미 오퍼 슬롯에 아이템이 있습니다. 먼저 제거해주세요.");
            return false;
        }

        // 인벤토리에 충분한 수량이 있는지 확인
        int availableAmount = inv.Get(itemId);
        int requiredAmount = requiredItem.amount;

        if (availableAmount < requiredAmount)
        {
            Debug.LogWarning($"[TradeUI] {ItemName.ToKorean(itemId)} 부족 (보유: {availableAmount} / 필요: {requiredAmount})");
            return false;
        }

        // 인벤토리에서 차감하고 오퍼 슬롯에 추가
        inv.Add(itemId, -requiredAmount);
        offerItemId = itemId;
        offerAmount = requiredAmount;

        Debug.Log($"[TradeUI] {ItemName.ToKorean(itemId)} x{requiredAmount}를 오퍼 슬롯에 올렸습니다.");

        return true;
    }

    /// <summary>
    /// 오퍼 슬롯의 아이템을 인벤토리로 반환
    /// </summary>
    private void ReturnOfferToInventory()
    {
        if (offerAmount <= 0) return;
        if (InventoryManager.Instance == null) return;

        var inv = InventoryManager.Instance.Data;

        // 인벤토리에 아이템 복구
        inv.Add(offerItemId, offerAmount);

        Debug.Log($"[TradeUI] {ItemName.ToKorean(offerItemId)} x{offerAmount}를 인벤토리로 반환했습니다.");

        // 오퍼 슬롯 초기화
        offerItemId = ItemId.WhiteDebris;
        offerAmount = 0;
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(TradeUIManager))]
public class TradeUIManagerEditor : Editor
{
    private bool showRecipe = true;
    private bool showInventory = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TradeUIManager mgr = (TradeUIManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Play 모드에서만 디버그 버튼이 동작합니다.", MessageType.Info);
            return;
        }

        // ---------- 버튼 ----------

        if (GUILayout.Button("랜덤 인벤토리 생성 (모든 아이템 포함)"))
        {
            mgr.DebugFillInventoryAllTypes();
        }

        if (GUILayout.Button("랜덤 NPC 레시피 생성"))
        {
            mgr.DebugRandomizeNpcRecipe();
        }

        if (GUILayout.Button("교역하기 (요구량 검사 → 차감 → 보상 지급)"))
        {
            mgr.DebugTradeOnce();
        }

        EditorGUILayout.Space(8);

        // ---------- 현재 레시피 ----------

        showRecipe = EditorGUILayout.Foldout(showRecipe, "현재 NPC 요구/보상");
        if (showRecipe)
        {
            DrawRecipe(mgr.DebugCurrentRecipe);
        }

        // ---------- 인벤 ----------

        showInventory = EditorGUILayout.Foldout(showInventory, "플레이어 인벤토리");
        if (showInventory)
        {
            DrawInventory(mgr.DebugInventory);
        }
    }

    private void DrawRecipe(TradeRecipe recipe)
    {
        if (recipe == null)
        {
            EditorGUILayout.HelpBox("현재 선택된 레시피가 없습니다.", MessageType.Warning);
            return;
        }

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField(string.IsNullOrEmpty(recipe.recipeName) ? "(이름 없음)" : recipe.recipeName, EditorStyles.boldLabel);

        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("요구 아이템", EditorStyles.miniBoldLabel);
        if (recipe.requires == null || recipe.requires.Count == 0)
        {
            EditorGUILayout.LabelField("- 없음");
        }
        else
        {
            foreach (var r in recipe.requires)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(ItemName.ToKorean(r.itemId), GUILayout.Width(120));
                EditorGUILayout.LabelField($"x {r.amount}");
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("보상 아이템", EditorStyles.miniBoldLabel);
        if (recipe.rewards == null || recipe.rewards.Count == 0)
        {
            EditorGUILayout.LabelField("- 없음");
        }
        else
        {
            foreach (var r in recipe.rewards)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(ItemName.ToKorean(r.itemId), GUILayout.Width(120));
                EditorGUILayout.LabelField($"x {r.amount}");
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawInventory(InventoryData inv)
    {
        if (inv == null)
        {
            EditorGUILayout.HelpBox("인벤토리 데이터가 없습니다.", MessageType.Warning);
            return;
        }

        EditorGUILayout.BeginVertical("box");

        var items = inv.Items;
        if (items == null || items.Count == 0)
        {
            EditorGUILayout.LabelField("(비어 있음)");
        }
        else
        {
            foreach (var it in items)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(ItemName.ToKorean(it.itemId), GUILayout.Width(120));
                EditorGUILayout.LabelField($"보유: {it.amount}");
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndVertical();
    }
}
#endif