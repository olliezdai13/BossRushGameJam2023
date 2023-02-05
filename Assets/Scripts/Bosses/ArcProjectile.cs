using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ArcProjectile : MonoBehaviour
{
    private Rigidbody2D _rb;
    private CinemachineImpulseSource _impulseSource;
    public GameObject _vfxBreakPrefab;
    void Start()
    {
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 direction, float force)
    {
        Debug.Log("Init " + direction);
        if (!_rb) _rb = GetComponent<Rigidbody2D>();
        _rb.AddForce(direction * force, ForceMode2D.Impulse);
        _rb.AddTorque(1.5f, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _impulseSource.GenerateImpulseWithForce(.1f); 
            Instantiate(_vfxBreakPrefab, transform.position, Quaternion.identity);
            EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.RatBoulderBreak } });
            Destroy(gameObject);
        }
    }
}
