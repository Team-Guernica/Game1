using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraFollow : BaseBehaviour
{

    [Header("Follow Target")]
    [SerializeField] private Transform target;
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;
    [SerializeField] private float savedY;


    private Coroutine lerpRoutine;

    protected override void Initialize()
    {
        base.Initialize();

        savedY = transform.position.y;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 currentPos = transform.position;
        Vector3 targetPos = target.position;

        float newX = currentPos.x;
        float newY = currentPos.y;

        if (followY)
        {
            newY = targetPos.y;
            savedY = newY;
        }
        else
        {
            newY = savedY;
        }

        Vector3 desiredPos = new Vector3(newX, newY, -10);
        transform.position = desiredPos;
    }

    public void SetFollowYTrue() => followY = true;
    public void SetFollowYFalse() => followY = false;

    public void SmoothMoveY(float startY, float duration = 0.5f)
    {
        if (target == null)
        {
            Debug.LogWarning("[CameraFollow] SmoothMoveY 호출했는데 target 이 없습니다.");
            return;
        }

        if (lerpRoutine != null)
        {
            StopCoroutine(lerpRoutine);
        }

        lerpRoutine = StartCoroutine(LerpYRoutine(startY, duration));
    }

    private IEnumerator LerpYRoutine(float startY, float duration)
    {
        followY = false;

        float time = 0f;
        float endY = target.position.y;

        // 시작값을 먼저 savedY에 세팅
        savedY = startY;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            // ★ transform.position 대신 savedY만 갱신
            savedY = Mathf.Lerp(startY, endY, t);

            // LateUpdate가 savedY를 써서 실제 카메라 위치를 움직임
            yield return null;
        }

        savedY = endY;
        followY = true;
        lerpRoutine = null;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CameraFollow))]
public class CameraFollowEditor : Editor
{
    // 디버그용 입력값 (에디터 전용)
    private float debugStartY = -5f;
    private float debugDuration = 0.5f;

    public override void OnInspectorGUI()
    {

        // 기본 인스펙터 먼저 그리기
        base.OnInspectorGUI();

        CameraFollow cam = (CameraFollow)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);

        // Play 모드 여부 안내
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Play 모드에서만 디버그 버튼이 동작합니다.", MessageType.Info);
            return;
        }

        // 디버그용 StartY, Duration 입력
        debugStartY = EditorGUILayout.FloatField("Debug Start Y", debugStartY);
        debugDuration = EditorGUILayout.FloatField("Debug Duration", debugDuration);

        if (GUILayout.Button($"SmoothMoveY (StartY = {debugStartY}, dur = {debugDuration})"))
        {
            cam.SmoothMoveY(debugStartY, debugDuration);
        }

        if (GUILayout.Button($"SmoothMoveY (CurrentY → PlayerY, dur = {debugDuration})"))
        {
            cam.SmoothMoveY(cam.transform.position.y, debugDuration);
        }
    }
}
#endif