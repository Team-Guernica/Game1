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
    /// 모든 아이템을 인벤토리에 넣고 랜덤 수량 부여
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

        foreach (ItemId id in System.Enum.GetValues(typeof(ItemId)))
        {
            int amt = Random.Range(5, 31);

            if (id is ItemId.Water or ItemId.Armor or ItemId.RedJuice or ItemId.Badge)
                amt = Random.Range(0, 4);

            inv.Set(id, amt);
        }

        Debug.Log("[TradeUI] 인벤토리 더미 생성 완료");
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
        offerAmount = 0;
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

        if (!inv.Has(req.itemId, req.amount))
        {
            Debug.LogWarning($"[TradeUI] 교역 실패: 인벤 부족 (보유 {inv.Get(req.itemId)} / 요구 {req.amount})");
            return;
        }

        inv.Add(req.itemId, -req.amount);
        inv.AddAll(currentNpcRecipe.rewards);

        Debug.Log($"[TradeUI] 교역 성공(Offer 1): {currentNpcRecipe.recipeName}");

        DebugClearOffer();
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

    private void RefreshInventoryUI()
    {
        if (inventorySlots == null || inventorySlots.Length == 0) return;
        if (InventoryManager.Instance == null) return;

        var inv = InventoryManager.Instance.Data;

        // 슬롯 i번째에 enum i번째 아이템을 고정 배치(간단한 임시 방식)
        var ids = (ItemId[])System.Enum.GetValues(typeof(ItemId));

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            var slot = inventorySlots[i];
            if (slot == null) continue;

            if (i >= ids.Length)
            {
                slot.Clear();
                continue;
            }

            ItemId id = ids[i];
            int amt = inv.Get(id);
            Sprite icon = iconTable != null ? iconTable.GetIcon(id) : null;

            slot.Set(id, amt, icon);
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