using UnityEngine;

public class Utils {
    public static RaycastHit2D Raycast(Vector3 start, Vector2 dir, float dist, LayerMask mask) {
        RaycastHit2D hit = Physics2D.Raycast(start, dir, dist, mask);
        Debug.DrawRay(start, dir * dist, Color.green);
        return hit;
    }
}