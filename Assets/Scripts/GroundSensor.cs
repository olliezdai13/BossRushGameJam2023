using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    [SerializeField] private Transform[] m_sensors;
    [SerializeField] private LayerMask m_layerMask;
    [SerializeField] private float m_groundCheckDistance = 0.1f;

    public bool IsGrounded;

    // REFERENCES
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponentInParent<Animator>();
    }

    void FixedUpdate()
    {
        IsGrounded = RaycastFromAllSensors();
        if (IsGrounded)
        {
            _animator.SetBool("grounded", true);
        } else
        {
            _animator.SetBool("grounded", false);
        }
    }

    private bool RaycastFromAllSensors()
    {
        foreach (var sensor in m_sensors)
        {
            if (RaycastFromSensor(sensor)) return true;
        }
        return false;
    }

    private bool RaycastFromSensor(Transform sensor)
    {
        var pos = sensor.position;
        var down = -sensor.up;
        RaycastHit2D hit = Physics2D.Raycast(pos, down, m_groundCheckDistance, m_layerMask);
        Debug.DrawRay(pos, down * m_groundCheckDistance);
        if (hit.collider != null)
        {
            return true;
        }
        return false;
    }
}
