using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomNumberTargetRotation : MonoBehaviour
{

    [SerializeField] public Transform target;
    void Updates() //This is only active while example: Room number is visible.
    {
        /*
        Vector3 targetPos = target.position;
        targetPos.x = 0; targetPos.z = 0;
        transform.LookAt(targetPos);
        */
    }
}
