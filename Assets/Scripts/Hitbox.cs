using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Initiator
{
    PLAYER,
    ENEMY,
    ENVIRONMENT
}
public class Hitbox : MonoBehaviour
{
    public Initiator initiator;
    public LayerMask layerMask;

    // Damage
    [SerializeField] private int _damage;
    public int Damage { get { return _damage; } }

    // Knockback
    [SerializeField] private Vector2 _pushDirection;
    [SerializeField] private float _pushForce;
    public Vector2 PushDirection { get { return _pushDirection; } }
    public float PushForce { get { return _pushForce; } }
    public Transform parent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((layerMask.value & (1 << collision.transform.gameObject.layer)) > 0)
        {
            Debug.Log("Hitbox hit valid layer");
            Vector2 hitPos = collision.gameObject.transform.position;
            EventManager.TriggerEvent("onHitboxCollision", new Dictionary<string, object> { { "hitPosition", hitPos }, { "initiator", initiator } });

            collision.gameObject.TryGetComponent(out IDamageable damageableObject);
            if (damageableObject != null)
            {
                //Debug.Log("Damage hitbox hitting valid IDamageable for " + _damage + " damage.");
                damageableObject.Damage(_damage);
            }

            collision.gameObject.TryGetComponent(out IKnockbackable knockbackableObject);
            if (knockbackableObject != null)
            {
                //Debug.Log("Knockback hitbox hitting valid IKnockbackable");
                knockbackableObject.Knockback(_pushDirection, _pushForce, hitPos, parent);
            }
        }
    }
}
