using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlphaDecay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private AnimationCurve alpha;

    float startTime;
    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var color = sprite.color;
        color.a = alpha.Evaluate(Time.time - startTime);

        sprite.color = color;
    }
}
