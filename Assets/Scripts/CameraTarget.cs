using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    // ADJUSTABLE VALUES
    public float biasLeft;
    public float biasRight;

    // REFERENCES
    private PlayerController _player;
    private Transform _playerTransform;

    void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = _playerTransform.position.x + (_player.FacingRight ? biasRight : -biasLeft);
        transform.position = new Vector3(x, _playerTransform.position.y, 0);
    }
}
