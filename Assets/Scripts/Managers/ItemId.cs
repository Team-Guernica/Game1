using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemId
{
    WhiteDebris,
    OldBrick,
    GoldCoin,

    Water,
    Armor,
    RedJuice,

    Badge
}

public static class ItemName
{
    public static string ToKorean(ItemId id)
    {
        return id switch
        {
            ItemId.WhiteDebris => "흰 부스러기",
            ItemId.OldBrick => "오래된 벽돌",
            ItemId.GoldCoin => "금화",

            ItemId.Water => "물",
            ItemId.Armor => "갑옷",
            ItemId.RedJuice => "붉은 주스",

            ItemId.Badge => "뱃지",
            _ => id.ToString()
        };
    }
}

[Serializable]
public struct ItemStack
{
    public ItemId itemId;
    public int amount;

    public ItemStack(ItemId id, int amt)
    {
        itemId = id;
        amount = amt;
    }
}

[Serializable]
public class InventoryData
{
    [SerializeField] private List<ItemStack> items = new();

    public IReadOnlyList<ItemStack> Items => items;

    public void Clear()
    {
        items.Clear();
    }

    public int Get(ItemId id)
    {
        int idx = items.FindIndex(x => x.itemId == id);
        return idx >= 0 ? items[idx].amount : 0;
    }

    public void Set(ItemId id, int amount)
    {
        amount = Mathf.Max(0, amount);

        int idx = items.FindIndex(x => x.itemId == id);
        if (idx >= 0)
        {
            items[idx] = new ItemStack(id, amount);
        }
        else
        {
            items.Add(new ItemStack(id, amount));
        }
    }

    public void Add(ItemId id, int delta)
    {
        Set(id, Get(id) + delta);
    }

    public bool Has(ItemId id, int need)
    {
        return Get(id) >= need;
    }

    /// <summary>
    /// 필요한 수량이 있으면 차감하고 true, 부족하면 false
    /// </summary>
    public bool TryConsume(ItemId id, int need)
    {
        if (!Has(id, need)) return false;
        Add(id, -need);
        return true;
    }

    /// <summary>
    /// 레시피 요구조건 전체 충족 여부
    /// </summary>
    public bool HasAll(List<ItemStack> requires, out string reason)
    {
        foreach (var r in requires)
        {
            int have = Get(r.itemId);
            if (have < r.amount)
            {
                reason = $"{ItemName.ToKorean(r.itemId)} 부족 (보유 {have} / 요구 {r.amount})";
                return false;
            }
        }

        reason = "";
        return true;
    }

    /// <summary>
    /// 레시피 요구조건을 모두 차감
    /// </summary>
    public void ConsumeAll(List<ItemStack> requires)
    {
        foreach (var r in requires)
        {
            Add(r.itemId, -r.amount);
        }
    }

    /// <summary>
    /// 레시피 보상을 모두 지급
    /// </summary>
    public void AddAll(List<ItemStack> rewards)
    {
        foreach (var r in rewards)
        {
            Add(r.itemId, r.amount);
        }
    }
}

[Serializable]
public class TradeRecipe
{
    public string recipeName;
    public List<ItemStack> requires = new();
    public List<ItemStack> rewards = new();
}
