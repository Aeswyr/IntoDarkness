using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private CinemachineVirtualCamera cam;
    [SerializeField] private LevelController level;


    public void SetCameraFollow(Transform target) {
        cam.Follow = target;
    }

    public LevelController GetLevel() {
        return level;
    }
}
