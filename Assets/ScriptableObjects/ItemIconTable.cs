using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Trade/Item Icon Table")]
public class ItemIconTable : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public ItemId id;
        public Sprite icon;
    }

    [SerializeField] private Entry[] entries;

    public Sprite GetIcon(ItemId id)
    {
        if (entries == null) return null;
        for (int i = 0; i < entries.Length; i++)
            if (entries[i].id == id) return entries[i].icon;
        return null;
    }
}