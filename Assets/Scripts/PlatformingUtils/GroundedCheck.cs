using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedCheck : MonoBehaviour
{
   //Grounded Raycast
    [SerializeField] private float floorDetectDistance = 1f;
    [SerializeField] private Vector2 floorDetectOffset;
    [SerializeField] private Vector2 footOffset;
    [SerializeField] private Vector2 wallOffset;
    [SerializeField] private LayerMask floorDetectMask;

    public bool CheckGrounded() {

        RaycastHit2D left = Utils.Raycast(transform.position + (Vector3)(floorDetectOffset + footOffset), Vector2.down, floorDetectDistance, floorDetectMask);
        RaycastHit2D right = Utils.Raycast(transform.position + (Vector3)(floorDetectOffset - footOffset), Vector2.down, floorDetectDistance, floorDetectMask);
        return left || right;
    }

    public bool CheckWallhang(int facing) {
        RaycastHit2D top = Utils.Raycast(transform.position + (Vector3)(facing * Vector2.right * wallOffset), facing * Vector2.right, floorDetectDistance, floorDetectMask);
        RaycastHit2D bottom = Utils.Raycast(transform.position + (Vector3)(facing * Vector2.left * -wallOffset), facing * Vector2.right, floorDetectDistance, floorDetectMask);
        return top || bottom;
    }


}