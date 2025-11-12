using System.Collections;
using UnityEngine;

public class Pickaxe : MonoBehaviour
{
    [Header("구성")]
    public Transform pivot;         // 회전축(없으면 transform)
    public GameObject hitboxObject; // 자식 Hitbox
    [Header("전투/채굴")]
    public float power = 1f;
    public float swingCooldown = 0.35f;
    public float activeHitTime = 0.12f;

    float _lastSwing = -999f;
    bool _swinging;

    void Awake()
    {
        if (pivot == null) pivot = transform;
        if (hitboxObject == null)
            hitboxObject = transform.Find("Hitbox")?.gameObject;
        if (hitboxObject) hitboxObject.SetActive(false);
    }

    public void ApplyStats(float p, float cd, float active)
    {
        power = p; swingCooldown = cd; activeHitTime = active;
    }

    public void TrySwing()
    {
        if (_swinging) return;
        if (Time.time < _lastSwing + swingCooldown) return;
        StartCoroutine(SwingRoutine());
    }

    IEnumerator SwingRoutine()
    {
        _swinging = true;
        _lastSwing = Time.time;

        // 히트박스 ON
        if (hitboxObject) hitboxObject.SetActive(true);

        // 간단한 손맛용 회전
        float dur = Mathf.Max(activeHitTime * 2f, 0.2f);
        float t = 0f;
        float startZ = -40f, endZ = 60f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float a = Mathf.SmoothStep(0, 1, t / dur);
            float z = Mathf.Lerp(startZ, endZ, a);
            pivot.localRotation = Quaternion.Euler(0, 0, z);
            yield return null;
        }

        // 히트박스 OFF + 원위치
        if (hitboxObject) hitboxObject.SetActive(false);
        pivot.localRotation = Quaternion.identity;

        _swinging = false;
    }
}