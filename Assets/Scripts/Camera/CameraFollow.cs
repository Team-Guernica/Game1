using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : BaseBehaviour
{

    [Header("Follow Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;
    [SerializeField] private float savedY;

    [Header("Vertical Clamp")]
    [SerializeField] private bool useVerticalClamp = false;

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

    public void SetFollowYTrue()
    {
        followY = true;
    }

    public void SetFollowYFalse()
    {
        followY = false;
    }

}
