using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour
{
    private CinemachineImpulseSource _impulseSource;
    private void Start()
    {
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }
    public void DoShake()
    {
        _impulseSource.GenerateImpulseWithForce(.5f);
    }
}
