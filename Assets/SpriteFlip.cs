using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFlip : MonoBehaviour
{
    SpriteRenderer _renderer;
    BasicMove _movement;
    public bool invert;
    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _movement = GetComponent<BasicMove>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_movement.facingRight)
        {
            _renderer.flipX = invert;
        } else
        {
            _renderer.flipX = !invert;
        }
    }
}
