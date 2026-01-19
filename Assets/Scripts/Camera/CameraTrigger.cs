using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField] private CameraFollow cameraFollow;

    private void Start()
    {
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            cameraFollow.SetFollowYFalse();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            cameraFollow.SetFollowYTrue();
        }
    }

}
