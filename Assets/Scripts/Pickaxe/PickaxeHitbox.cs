using UnityEngine;

public class PickaxeHitbox : MonoBehaviour
{
    Pickaxe owner;

    void Awake()
    {
        owner = GetComponentInParent<Pickaxe>();
        if (owner == null)
            Debug.LogError("PickaxeHitbox: Pickaxe not found in parent");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (owner == null) return;

        IMineable mineable = other.GetComponent<IMineable>();
        if (mineable == null) return;

        if (mineable != null && owner != null)
        {
            var controller = GetComponentInParent<MiningController>();
            if (controller != null) controller.NotifyHitThisSwing();

            Vector2 hitPoint = other.ClosestPoint(transform.position);
            mineable.TakeHit(owner.Power, hitPoint);
        }

       
    }
}


/*필수 구조
 * Pickaxe
 └─ Hitbox
      ├─ BoxCollider2D (IsTrigger = ON)
      └─ PickaxeHitbox 
*/