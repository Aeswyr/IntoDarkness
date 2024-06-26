using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class LevelController : MonoBehaviour
{
    [SerializeField] private GameObject parallax;
    [SerializeField] private GameObject parallaxNoY;
    private UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera pixelPerfect;
    void Start() {
        pixelPerfect = Camera.main.GetComponent<UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera>();
        CinemachineCore.CameraUpdatedEvent.AddListener(UpdateParallax);
    }
    public void UpdateParallax(CinemachineBrain arg0) {
        var cam = Camera.main.transform.position;
        cam.z = 0;
        cam = pixelPerfect.RoundToPixel(cam);

        parallax.transform.localPosition = cam;

        Vector3 pos = parallaxNoY.transform.localPosition;
        pos.x = cam.x;
        parallaxNoY.transform.localPosition = pos;
    }

    public void LateUpdate() {

    }
}
