using UnityEngine;

public class MineableObject : MonoBehaviour, IMineable
{
    public float maxHp = 3f;
    public float hardness = 0f;

    float currentHp;
    bool beingMined;

    void Awake()
    {
        currentHp = maxHp;
    }

    public void TakeHit(float power, Vector2 hitPoint)
    {
        beingMined = true;

        float dmg = Mathf.Max(0.1f, power - hardness);
        currentHp -= dmg * Time.fixedDeltaTime;

        if (currentHp <= 0)
            Break();
    }

    public void ResetMining()
    {
        if (beingMined)
        {
            currentHp = maxHp;
            beingMined = false;
        }
    }

    void Break()
    {
        Destroy(gameObject);
    }
}