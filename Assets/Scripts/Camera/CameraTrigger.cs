using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraTrigger : MonoBehaviour
{
    [Header("Gate Settings")]
    [SerializeField] private float gateMinCameraY = -10f; // 이 값보다 카메라가 더 내려가지 못함
    [SerializeField] private bool applyOnce = true;       // 한 번만 적용하고 끝낼지 여부

    private bool _applied = false;

    private void Reset()
    {
        // 게이트용이라서 기본값을 트리거로 세팅
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_applied && applyOnce) return;
        if (!other.CompareTag("Player")) return;

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        CameraFollow camFollow = mainCam.GetComponent<CameraFollow>();
        if (camFollow == null) return;

        // 카메라가 이 Y 값 아래로 내려가지 못하게 설정
        camFollow.SetVerticalClamp(gateMinCameraY, Mathf.Infinity);

        _applied = true;

        if (applyOnce)
        {
            // 필요하다면 게이트 제거 or 비활성화
            // Destroy(gameObject);
            GetComponent<Collider2D>().enabled = false;
        }
    }

}
