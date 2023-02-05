using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    public float time = 1;
    void Start()
    {
        Object.Destroy(gameObject, time);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
