using UnityEngine;

public class PickaxeHitbox : MonoBehaviour
{
    Pickaxe owner;
    MineableObject currentTarget;

    void Awake()
    {
        owner = GetComponentInParent<Pickaxe>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        MineableObject mineable = other.GetComponent<MineableObject>();
        if (mineable == null) return;

        currentTarget = mineable;
        Vector2 hitPoint = other.ClosestPoint(transform.position);
        mineable.TakeHit(owner.Power, hitPoint);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        MineableObject mineable = other.GetComponent<MineableObject>();
        if (mineable == currentTarget)
        {
            mineable.ResetMining();
            currentTarget = null;
        }
    }

    public void ForceReset()
    {
        if (currentTarget != null)
        {
            currentTarget.ResetMining();
            currentTarget = null;
        }
    }
}


/*필수 구조
 * Pickaxe
 └─ Hitbox
      ├─ BoxCollider2D (IsTrigger = ON)
      └─ PickaxeHitbox 
*/