using UnityEngine;

public class InventoryManager : BaseBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Player Inventory")]
    [SerializeField] private InventoryData inventory = new InventoryData();

    public InventoryData Data => inventory;

    protected override void Initialize()
    {
        base.Initialize();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // 편의 함수들
    public int Get(ItemId id) => inventory.Get(id);
    public bool Has(ItemId id, int need) => inventory.Has(id, need);
    public void Add(ItemId id, int delta) => inventory.Add(id, delta);
    public void Clear() => inventory.Clear();
}
