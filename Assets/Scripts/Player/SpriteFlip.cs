using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFlip : MonoBehaviour
{
    SpriteRenderer _renderer;
    PlayerController _player;
    public bool invert;
    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _player = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_player.FacingRight)
        {
            _renderer.flipX = invert;
        } else
        {
            _renderer.flipX = !invert;
        }
    }
}
