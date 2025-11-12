using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DB/Pickaxe Database", fileName = "PickaxeDatabase")]
public class PickaxeDatabase : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public PickaxeType type;
        public GameObject prefab; // "손에 들 장착" 프리팹
        [Header("Stats")]
        public float power;          // 채굴/타격 파워
        public float swingCooldown;  // 쿨타임
        public float activeHitTime;  // 히트박스 유지 시간
    }

    public List<Entry> entries = new();

    Dictionary<PickaxeType, Entry> _map;

    void BuildIfNeeded()
    {
        if (_map != null) return;
        _map = new Dictionary<PickaxeType, Entry>();
        foreach (var e in entries)
            if (!_map.ContainsKey(e.type)) _map.Add(e.type, e);
    }

    public GameObject GetPrefab(PickaxeType type)
    {
        BuildIfNeeded();
        return _map.TryGetValue(type, out var e) ? e.prefab : null;
    }

    public bool TryGetStats(PickaxeType type, out float power, out float cooldown, out float activeTime)
    {
        BuildIfNeeded();
        if (_map.TryGetValue(type, out var e))
        {
            power = e.power;
            cooldown = e.swingCooldown;
            activeTime = e.activeHitTime;
            return true;
        }
        power = cooldown = activeTime = 0f;
        return false;
    }
}