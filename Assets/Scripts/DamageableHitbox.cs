using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableHitbox : MonoBehaviour, IDamageable
{
    public GameObject damageableParent;
    private IDamageable damageable;
    private void Start()
    {
        damageable = damageableParent.GetComponent<IDamageable>();
    }
    public void Damage(int amount)
    {
        if (damageable != null)
        {
            damageable.Damage(amount);
        }
    }
}
