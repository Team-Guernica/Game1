using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMining : BaseBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Transform interactOrigin;
    [SerializeField] private Tilemap targetTilemap;

    [Header("Mine")]
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private float range = 1.2f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float cooldown = 0.08f;    // protect rapid mining

    private float lastMineTime;

    protected override void Initialize()
    {
        base.Initialize();
        movement = GetComponent<PlayerMovement>();
        interactOrigin = transform;
    }

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            Debug.Log("E pressed");
            TryMine();
        }
    }

    private void TryMine()
    {
        if (Time.time < lastMineTime + cooldown) return;
        lastMineTime = Time.time;

        if (targetTilemap == null)
        {
            Debug.LogError("targetTilemap is null");
            return;
        }

        Vector2 origin = interactOrigin.position;
        Vector2 dir = movement != null ? movement.AimDir : Vector2.down;
        if (dir.sqrMagnitude < 0.001f) dir = Vector2.down;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, range, platformLayer);
        if (!hit.collider) return;

        Vector3 fixedWorld = hit.point - (Vector2)dir * 0.05f;
        Vector3Int cell = targetTilemap.WorldToCell(fixedWorld);


        TileBase tile = targetTilemap.GetTile(cell);

        if (tile == null)
        {
            Vector3Int step = new Vector3Int(
                dir.x > 0.5f ? 1 : (dir.x < -0.5f ? -1 : 0),
                dir.y > 0.5f ? 1 : (dir.y < -0.5f ? -1 : 0),
                0
            );

            Vector3Int candidate = cell + step;
            TileBase t2 = targetTilemap.GetTile(candidate);

            if (t2 != null)
            {
                cell = candidate;
                tile = t2;
            }
        }

        Debug.Log($"Mine final cell={cell}, tile={(tile ? tile.name : "null")}");

        if (tile == null) return;

        targetTilemap.SetTile(cell, null);
        targetTilemap.RefreshTile(cell);

        Debug.Log($"Mine final cell={cell}, tile={(tile ? tile.name : "null")}");

        if (tile == null) return;

        targetTilemap.SetTile(cell, null);
        targetTilemap.RefreshTile(cell);
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (interactOrigin == null) return;

        Vector2 origin = interactOrigin.position;
        Vector2 dir = Vector2.down;
        var pm = GetComponent<PlayerMovement>();
        if (pm != null && pm.AimDir.sqrMagnitude > 0.001f) dir = pm.AimDir;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + dir * range);

        Vector3 targetWorld = (Vector2)origin + dir * range;
        Gizmos.DrawWireSphere(targetWorld, 0.08f);
    }

    protected override void OnBindField()
    {
        base.OnBindField();
    }
#endif
}
