using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInvincibility : MonoBehaviour
{
    public bool IsHpInvincible = false;
    public float invincibilityTimePerHit;
    [SerializeField] private float _hpInvincibilityDuration;
    private Collider2D hitboxToDisable;
    private SpriteRenderer _renderer;
    public float flashDuration = 0.25f;
    private float flashTimer = 0;
    private bool flashOn;
    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        hitboxToDisable = GetComponent<Collider2D>();
        _hpInvincibilityDuration = 0;
        flashTimer = 0;
    }

    void Update()
    {
        if (flashTimer >= flashDuration)
        {
            flashTimer = 0;
            flashOn = !flashOn;
            if (flashOn)
            {
                _renderer.color = Color.gray;
            } else
            {
                _renderer.color = Color.white;
            }
        }
        if (IsHpInvincible)
        {
            flashTimer += Time.deltaTime;
        }
        if (_hpInvincibilityDuration > 0)
        {
            _hpInvincibilityDuration -= Time.deltaTime;
        }
        else
        {
            IsHpInvincible = false;
            flashTimer = 0;
            _renderer.color = Color.white;
            _hpInvincibilityDuration = 0;
        }
        if (IsHpInvincible)
        {
            hitboxToDisable.enabled = false;
        } else
        {
            hitboxToDisable.enabled = true;
        }
    }
    void OnEnable()
    {
        EventManager.StartListening("onHealthChange", OnHealthChange);
    }

    void OnDisable()
    {
        EventManager.StopListening("onHealthChange", OnHealthChange);
    }

    void OnHealthChange(Dictionary<string, object> data)
    {
        bool activateInvincibility = (bool)data["activateInvincibility"];
        if (activateInvincibility)
        {
            IsHpInvincible = true;
            _hpInvincibilityDuration = _hpInvincibilityDuration + invincibilityTimePerHit;
            if (_renderer) _renderer.color = Color.gray;
        }
    }
}
