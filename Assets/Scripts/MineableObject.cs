using UnityEngine;

public interface IMineable
{
    void TakeHit(float power, Vector2 hitPoint);
}

[RequireComponent(typeof(Collider2D))]
public class MineableObject : MonoBehaviour, IMineable
{
    [Header("채굴 설정")]
    public float maxHp = 3f;
    public float hardness = 0f;          
    public GameObject dropPrefab;       

    float currentHp;

    void Awake()
    {
        currentHp = maxHp;
    }

    public void TakeHit(float power, Vector2 hitPoint)
    {
        float dmg = Mathf.Max(0.1f, power - hardness);
        currentHp -= dmg;
        Debug.Log($"{name} hit! {dmg} damage → HP {currentHp}");

        // 맞을 때 간단한 반응
        //transform.localScale = Vector3.one * Random.Range(0.95f, 1.05f);

        if (currentHp <= 0)
            Break();
    }

    void Break()
    {
        Debug.Log($"{name} destroyed");
        if (dropPrefab)
        {
            Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}