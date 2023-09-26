using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strand : MonoBehaviour
{
    // References
    private BoxCollider2D hitbox;
    private Animator animator;

    // Public Configs
    public float _hitboxDisableTime = 0.2f;

    public bool flickerStarted = false;

    private bool _includeHitbox;

    // Call this when you spawn a strand
    public Strand Init(bool includeHitbox)
    {
        _includeHitbox = includeHitbox;
        animator = GetComponentInChildren<Animator>();
        hitbox = GetComponentInChildren<BoxCollider2D>();
        hitbox.enabled = false;
        return this;
    }

    public void Slide()
    {
        animator.SetTrigger("slide");
    }

    public void Fade()
    {
        animator.SetTrigger("fade");
    }

    public void Flicker()
    {
        animator.SetTrigger("flicker");
    }

    public void Attack()
    {
        animator.SetTrigger("attack");
        if (_includeHitbox) hitbox.enabled = true;
        Invoke("DisableHitboxes", _hitboxDisableTime);
    }

    public void DisableHitboxes()
    {
        if (_includeHitbox) hitbox.enabled = false;
    }
}
