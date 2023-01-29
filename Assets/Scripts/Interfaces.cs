using UnityEngine;

public interface IDamageable
{
    void Damage(int amount);
}
public interface IKnockbackable
{
    void Knockback(Vector2 direction, float force, Vector2 hitPos, Transform initiator);
}
public interface IHealable
{
    void Heal(int amount);
}