using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : BaseBehaviour
{

    [Header("Follow Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;

    [Header("Vertical Clamp")]
    [SerializeField] private bool useVerticalClamp = false;
    [SerializeField] private float minY = Mathf.NegativeInfinity;  // 카메라가 이 y보다 아래로는 못 내려감
    [SerializeField] private float maxY = Mathf.Infinity;          // 필요하면 위쪽 한계도 설정 가능

    protected override void Initialize()
    {
        base.Initialize();
        if (target == null)
        {
            // 에디터에서 안 넣었으면 태그로라도 찾아보기
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 currentPos = transform.position;
        Vector3 targetPos = target.position + offset;

        // X, Y 각각 따라갈지 여부
        float newX = followX ? targetPos.x : currentPos.x;
        float newY = followY ? targetPos.y : currentPos.y;

        // 수직 클램프 적용 (예: 땅 깊이 한계)
        if (useVerticalClamp)
        {
            newY = Mathf.Clamp(newY, minY, maxY);
        }

        Vector3 desired = new Vector3(newX, newY, targetPos.z);

        // 부드럽게 따라가기
        Vector3 smoothed = Vector3.Lerp(currentPos, desired, followSpeed * Time.deltaTime);
        transform.position = smoothed;
    }

    /// <summary>
    /// 카메라의 수직 이동 한계를 설정 (예: 게이트 통과 시)
    /// </summary>
    public void SetVerticalClamp(float min, float max)
    {
        useVerticalClamp = true;
        minY = min;
        maxY = max;
    }

    /// <summary>
    /// 카메라 수직 한계 해제
    /// </summary>
    public void ClearVerticalClamp()
    {
        useVerticalClamp = false;
        minY = Mathf.NegativeInfinity;
        maxY = Mathf.Infinity;
    }

    public void FreezeYAtCurrent()
    {
        followY = false;

        // 현재 카메라 위치와 target 사이 Y 오프셋을 재계산해서 고정
        if (target != null)
        {
            float currentY = transform.position.y;
            float targetY = target.position.y;

            // offset.y는 target 기준이므로 이렇게 바꿔주면 이후 y는 고정됨
            offset.y = currentY - targetY;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!useVerticalClamp) return;

        Gizmos.color = Color.green;

        // 뷰 안에서 보이도록 대략적인 선 그리기
        float lineLength = 20f;
        Vector3 left = new Vector3(transform.position.x - lineLength, minY, 0f);
        Vector3 right = new Vector3(transform.position.x + lineLength, minY, 0f);
        Gizmos.DrawLine(left, right);
    }
#endif

}
