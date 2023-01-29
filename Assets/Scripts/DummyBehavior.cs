using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyBehavior : MonoBehaviour, IKnockbackable
{
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb;
    public LayerMask layerMask;
    public float flashDuration = 1f;
    public bool useOwnKnockbackForce = false;
    public float knockbackForce = 1f;
    private float flashTimer = 0f;
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>(); 
    }

    void Update()
    {
        flashTimer += Time.deltaTime;
        if (flashTimer > flashDuration)
        {
            _spriteRenderer.color = Color.white;
        }
    }

    public void Knockback(Vector2 direction, float force, Vector2 _hitPos, Transform _initiator)
    {
        if (useOwnKnockbackForce)
        {
            _rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        } else
        {
            _rb.AddForce(direction * force, ForceMode2D.Impulse);
        }
        _spriteRenderer.color = Color.red;
        flashTimer = 0;
    }
}
