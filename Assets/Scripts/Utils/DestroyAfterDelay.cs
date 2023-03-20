using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
    [SerializeField] private float lifetime;
    
    void Start() {
        lifetime += Time.time;
    }

    public void Init(float time) {
        lifetime = time;
    }

    void FixedUpdate()
    {
        if (Time.time > lifetime)
            Destroy(gameObject);
    }
}
