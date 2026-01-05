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
        // Mining cooldown
        if (Time.time < lastMineTime + cooldown) return;
        lastMineTime = Time.time;

        // player position and aim direction
        Vector2 origin = interactOrigin.position;
        Vector2 dir = movement != null ? movement.AimDir : Vector2.down;
        if (dir.sqrMagnitude < 0.001f) dir = Vector2.down;

        // aim direction raycast
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, range, platformLayer);
        if (!hit.collider) return;

        Tilemap tm = hit.collider.GetComponent<Tilemap>() ?? hit.collider.GetComponentInParent<Tilemap>();
        if (tm == null) return;

        // raycast to tilemap cell world point
        Vector3Int cell = tm.WorldToCell(hit.point);
        TileBase tile = tm.GetTile(cell);

        // when hit point is edge , offset correction

        // reverse hit.normal and correct position offset
        if (tile == null)
        {
            float epsilon = 0.001f; //
            Vector2 inside = hit.point - hit.normal * epsilon;

            Vector3Int cell2 = tm.WorldToCell(inside);
            TileBase tile2 = tm.GetTile(cell2);

            if (tile2 != null)
            {
                cell = cell2;
                tile = tile2;
            }
        }

        Debug.Log($"Mine: hitPoint={hit.point}, normal={hit.normal}, cell={cell}, tile={(tile ? tile.name : "null")}");

        if (tile == null) return;

        tm.SetTile(cell, null);
        tm.RefreshTile(cell);
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
