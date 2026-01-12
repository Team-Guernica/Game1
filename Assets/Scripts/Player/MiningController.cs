using UnityEngine;

public class MiningController : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform origin;       
    public Pickaxe pickaxe;        
    public GameObject hitboxObject; // 스윙 시 켤 Hitbox 오브젝트

    [Header("Mining Layers")]
    public LayerMask mineableMask; 

    [Header("Hybrid Settings")]
    public float hitboxRange = 1.0f;

    [Tooltip("히트박스가 켜져있는 시간")]
    public float hitboxActiveTime = 0.12f;


    float _lastMineTime = -999f;

    // “이번 스윙에서 히트박스가 이미 맞췄는지” 플래그 (중복 방지용)
    bool _hitRegisteredThisSwing = false;
    int _swingId = 0;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (origin == null) origin = transform;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryMineHybrid();
        }
    }

    void TryMineHybrid()
    {
        if (pickaxe == null) return;

        // 쿨다운
        if (Time.time < _lastMineTime + pickaxe.Cooldown)
            return;

        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 start = origin.position;

        // 스윙 시작
        _swingId++;
        _hitRegisteredThisSwing = false;

       
        if (hitboxObject != null)
        {
            StartCoroutine(EnableHitboxForAWhile(_swingId));
        }

        // 클릭 위치가 히트박스 범위 밖이면, 그 방향으로 레이캐스트 채굴
        float dist = Vector2.Distance(start, mouseWorld);
        bool outsideHitbox = dist > hitboxRange;

        if (outsideHitbox)
        {
            TryRaycastMine(mouseWorld);
            _lastMineTime = Time.time;
            return;
        }


        // 근접은 히트박스에만 맡김
        _lastMineTime = Time.time;
    }

    void TryRaycastMine(Vector2 mouseWorld)
    {
        if (pickaxe == null) return;

        Vector2 start = origin.position;
        Vector2 dir = (mouseWorld - start);
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        // 이미 히트박스가 이번 스윙에 맞췄으면 중복 방지
        if (_hitRegisteredThisSwing) return;

        RaycastHit2D hit = Physics2D.Raycast(start, dir, pickaxe.Reach, mineableMask);
        if (!hit.collider) return;

        IMineable mineable = hit.collider.GetComponent<IMineable>();
        if (mineable == null) return;

        mineable.TakeHit(pickaxe.Power, hit.point);
    }

    System.Collections.IEnumerator EnableHitboxForAWhile(int swingId)
    {
        // 다른 스윙이 시작됐으면 중단
        if (swingId != _swingId) yield break;

        hitboxObject.SetActive(true);
        yield return new WaitForSeconds(hitboxActiveTime);

        if (swingId != _swingId) yield break;

        hitboxObject.SetActive(false);
    }

    System.Collections.IEnumerator RaycastFallbackAfterWindow(int swingId, Vector2 mouseWorld, float wait)
    {
        yield return new WaitForSeconds(wait);

        
        if (swingId != _swingId) yield break;
        if (_hitRegisteredThisSwing) yield break;

        TryRaycastMine(mouseWorld);
    }

    public void NotifyHitThisSwing()
    {
        _hitRegisteredThisSwing = true;
    }
}

//mineableMask = Mineable 레이어만 체크
//origin = HandSlot(손 위치) 넣기
//hitboxObject = Pickaxe의 Hitbox 자식 오브젝트 넣기