using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField] private GameObject waterline;
    public void UpdateWaterline(float xPos) {
        Vector3 pos = waterline.transform.position;
        pos.x = xPos;
        waterline.transform.position = pos;
    }
}
