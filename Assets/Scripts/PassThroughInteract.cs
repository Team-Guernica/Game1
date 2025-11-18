using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PassThroughInteract : MonoBehaviour
{
    [Header("플레이어 태그")]
    public string playerTag = "Player";

    [Header("상호작용 키")]
    public KeyCode interactKey = KeyCode.E;

    [Header("이벤트 (인스펙터에서 연결 가능)")]
    public UnityEvent onPlayerEnter;   // 근처 들어올 때
    public UnityEvent onPlayerExit;    // 나갈 때
    public UnityEvent onInteract;      // E키 눌렀을 때

    private bool isPlayerNear = false;

    private void Update()
    {
        // 플레이어가 근처에 있고, E키를 눌렀을 때
        if (isPlayerNear && Input.GetKeyDown(interactKey))
        {
            Debug.Log("✅ 상호작용이 되었습니다!");
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerNear = true;
            Debug.Log("👣 플레이어가 근처에 왔습니다.");
        }

    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerNear = false;
            Debug.Log("🚶‍♂️ 플레이어가 멀어졌습니다.");
        }
    }
}
