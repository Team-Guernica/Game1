using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : BaseBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 4.0f;
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private float deceleration = 30f;
    [SerializeField] private Vector2 moveInput;

    [Header("Interact (8-dir)")]
    [SerializeField] private Transform interactOrigin;
    [SerializeField] private float interactRange = 1.2f;
    // 8Direction Aim
    public Vector2 AimDir { get; private set; } = Vector2.down;
    [SerializeField] private int facing = 1;        // 1: right, -1: left

    private Rigidbody2D rb;




    protected override void Initialize()
    {
        base.Initialize();
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ReadMoveInput();
        UpdateFacing();
        UpdateAimDirection();
    }
    private void FixedUpdate()
    {
        ApplyMove();
        BlockUpwardMotion();
    }

    private void ReadMoveInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        moveInput = new Vector2(x, 0f);
    }
    private void UpdateFacing()
    {
        if (moveInput.x > 0.01f) facing = 1;
        else if (moveInput.x < -0.01f) facing = -1;

    }
    private void ApplyMove()
    {
        float targetVx = moveInput.x * moveSpeed;

        float vx = rb.velocity.x;
        float ax = (Mathf.Abs(targetVx) > 0.01f) ? acceleration : deceleration;

        vx = Mathf.MoveTowards(vx, targetVx, ax * Time.fixedDeltaTime);

        rb.velocity = new Vector2(vx, rb.velocity.y); // control x
    }

    private void BlockUpwardMotion()
    {
        if (rb.velocity.y > 0f)
            rb.velocity = new Vector2(rb.velocity.x, 0f);
    }

    private void UpdateAimDirection()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 inputDir = new Vector2(x, y);

        // no input will default aim direction
        if (inputDir.sqrMagnitude < 0.01f)
        {
            return;
        }

        AimDir = Quantize8(inputDir.normalized);
    }

    private Vector2 Quantize8(Vector2 dir)
    {

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle = (angle + 360f) % 360f;

        int sector = Mathf.RoundToInt(angle / 45f) % 8;
        // 0: (1,0), 1:(1,1), 2:(0,1), 3:(-1,1), 4:(-1,0), 5:(-1,-1), 6:(0,-1), 7:(1,-1)
        Vector2[] dirs =
        {
            Vector2.right,
            (Vector2.right + Vector2.up).normalized,
            Vector2.up,
            (Vector2.left + Vector2.up).normalized,
            Vector2.left,
            (Vector2.left + Vector2.down).normalized,
            Vector2.down,
            (Vector2.right + Vector2.down).normalized
        };
        return dirs[sector];
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = interactOrigin != null ? interactOrigin.position : transform.position;

        Vector2 dir = (AimDir.sqrMagnitude > 0.001f) ? AimDir : Vector2.down;

        float range = interactRange > 0f ? interactRange : 1.2f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + (Vector3)(dir * range));

        Gizmos.DrawWireSphere(origin + (Vector3)(dir * range), 0.08f);
    }
    
    // 이동을 연속적으로 할 것인지 한턴에 한번만 할 것인지
    // 이동하며 타겟 방향의 땅을 지울 텐데 땅을 지우는 에임은 마우스로 할 것인가 키보드로 할 것인가?

    protected override void OnBindField()
    {
        base.OnBindField();
    }
#endif 
}
