using UnityEngine;

public class Pickaxe : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] float basePower = 1f;
    [SerializeField] float baseReach = 1.5f;
    [SerializeField] float baseCooldown = 0.35f;

    [Header("Upgrade")]
    [SerializeField] int upgradeLevel = 0;
    [SerializeField] float powerPerLevel = 0.5f;
    [SerializeField] float reachPerLevel = 0.1f;

    // === 최종 계산된 값 ===
    public float Power { get; private set; }
    public float Reach { get; private set; }
    public float Cooldown => baseCooldown;

    void Awake()
    {
        RecalculateStats();
    }

    /// <summary>
    /// 업그레이드 조건 충족 시 호출
    /// </summary>
    public void Upgrade(int amount = 1)
    {
        upgradeLevel += amount;
        RecalculateStats();
    }

    void RecalculateStats()
    {
        Power = basePower + upgradeLevel * powerPerLevel;
        Reach = baseReach + upgradeLevel * reachPerLevel;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Inspector에서 값 바꿔도 즉시 반영
        RecalculateStats();
    }
#endif
}