using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingTextController : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;

    [SerializeField] private AnimationCurve scale;
    [SerializeField] private AnimationCurve alpha;
    [SerializeField] private AnimationCurve yPos;

    float startTime;
    Vector3 basePos;
    Vector3 baseScale;
    void Start() {
        startTime = Time.time;
        basePos = transform.position;
        baseScale = transform.localScale;
    }
    public void SetText(string text, Color color) {
        this.text.text = text;
        this.text.color = color;
    }

    void FixedUpdate() {
        float time = Time.time - startTime;
        if (time >= scale.keys[scale.keys.Length - 1].time
            || time >= yPos.keys[yPos.keys.Length - 1].time)
            Destroy(gameObject);
        else {
            transform.position = basePos + yPos.Evaluate(time) * Vector3.up;
            transform.localScale = scale.Evaluate(time) * baseScale;
            Color color = text.color;
            color.a = alpha.Evaluate(time);
            text.color = color;
        }
    }
}
