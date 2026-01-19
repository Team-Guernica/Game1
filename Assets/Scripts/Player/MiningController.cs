using UnityEngine;

public class MiningController : MonoBehaviour
{
    public Camera cam;
    public Transform origin;
    public Pickaxe pickaxe;

    [Header("8 Direction Hitboxes (N부터 시계방향)")]
    public PickaxeHitbox[] hitboxes; // 크기 8

    int currentDir = -1;
    bool holding;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (origin == null) origin = transform;
        DisableAllHitboxes();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            holding = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            holding = false;
            DisableAllHitboxes(true);
        }

        if (holding)
        {
            UpdateDirectionAndHitbox();
        }
    }

    void UpdateDirectionAndHitbox()
    {
        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouseWorld - (Vector2)origin.position).normalized;

        int dirIndex = GetClosest8Dir(dir);
        if (dirIndex != currentDir)
        {
            DisableAllHitboxes();
            currentDir = dirIndex;
            hitboxes[currentDir].gameObject.SetActive(true);
        }
    }

    int GetClosest8Dir(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle = (angle + 360f + 22.5f) % 360f;
        return (int)(angle / 45f);
    }

    void DisableAllHitboxes(bool resetMining = false)
    {
        foreach (var hb in hitboxes)
        {
            if (hb == null) continue;
            if (resetMining) hb.ForceReset();
            hb.gameObject.SetActive(false);
        }
        currentDir = -1;
    }
}