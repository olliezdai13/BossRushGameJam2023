using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    public float biasLeft;
    public float biasRight;
    private BasicMove _movement;
    private Transform _playerTransform;
    private CinemachineVirtualCamera _cinemachine;

    void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _movement = GameObject.FindGameObjectWithTag("Player").GetComponent<BasicMove>();
        _cinemachine = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = _playerTransform.position.x + (_movement.facingRight ? biasRight : -biasLeft);
        transform.position = new Vector3(x, _playerTransform.position.y, 0);
    }
}
