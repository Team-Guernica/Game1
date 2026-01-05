using UnityEngine;

public class PickaxeHitbox : MonoBehaviour
{
    Pickaxe owner;   //  곡괭이 본체

    void Awake()
    {
        // 부모 오브젝트에서 Pickaxe 가져오기
        owner = GetComponentInParent<Pickaxe>();

        if (owner == null)
        {
            Debug.LogError("Pickaxe component not found");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IMineable mineable = other.GetComponent<IMineable>();

        if (mineable != null && owner != null)
        {
            // 실제 충돌 지점 계산
            Vector2 hitPoint = other.ClosestPoint(transform.position);

            
            mineable.TakeHit(owner.power, hitPoint);
        }
    }
}