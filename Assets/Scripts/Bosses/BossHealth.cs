using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealth : MonoBehaviour, IDamageable
{
    public int maxHealth;
    [SerializeField] private int _currentHealth;
    public int CurrentHealth { get { return _currentHealth; } }

    private void Start()
    {
        _currentHealth = maxHealth;
        EventManager.TriggerEvent("onBossHealthChange", new Dictionary<string, object> { { "newHp", _currentHealth }, { "maxHp", maxHealth } });
    }

    public void Damage(int amount)
    {
        if (_currentHealth > 0)
        {
            _currentHealth -= amount;
            EventManager.TriggerEvent("onBossHealthChange", new Dictionary<string, object> { { "newHp", _currentHealth }, { "maxHp", maxHealth } });
            EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.PlayerAttackLands } });

        }
        if (_currentHealth < 0)
        {
            _currentHealth = 0;
        }
    }
}
