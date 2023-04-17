using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField] private GameObject parallaxHolder;
    [SerializeField] private GameObject skyParallax;
    public void UpdateParallax(Vector3 targetPos) {
        var cam = Camera.main.transform.position;

        Vector3 pos = parallaxHolder.transform.position;
        pos.x = cam.x;
        parallaxHolder.transform.position = pos;

        Vector3 skyPos = skyParallax.transform.position;
        skyPos.y = cam.y;
        skyParallax.transform.position = skyPos;
    }
}
