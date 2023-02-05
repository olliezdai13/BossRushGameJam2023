using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable, IHealable
{
    public int maxHealth;
    [SerializeField] private int _currentHealth;
    public int CurrentHealth { get { return _currentHealth; } }

    private void Start()
    {
        _currentHealth = maxHealth;
        EventManager.TriggerEvent("onHealthChange", new Dictionary<string, object> { { "newHp", _currentHealth }, { "maxHp", maxHealth } });
    }

    public void Damage(int amount)
    {
        _currentHealth -= amount;
        if (_currentHealth < 0)
        {
            _currentHealth = 0;
        }

        EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.PlayerHit } });
        EventManager.TriggerEvent("onHealthChange", new Dictionary<string, object> { { "newHp", _currentHealth }, { "maxHp", maxHealth } });
    }

    public void Heal(int amount)
    {
        _currentHealth += amount;
        if (_currentHealth > maxHealth)
        {
            _currentHealth = maxHealth;
        }
        EventManager.TriggerEvent("onHealthChange", new Dictionary<string, object> { { "newHp", _currentHealth }, { "maxHp", maxHealth } });
    }
}
