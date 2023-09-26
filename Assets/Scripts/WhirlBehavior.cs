using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhirlBehavior : MonoBehaviour
{
    public float spinSpeed = 1f;
    private SpriteRenderer _renderer;
    private CircleCollider2D _hitbox;
    private Animator _animator;
    public AnimationCurve fadeCurve = new AnimationCurve(
      new Keyframe(0, 0, 1, 1),
      new Keyframe(1, 1, 1, 1)
    );

    private void OnEnable()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _hitbox = GetComponent<CircleCollider2D>();
        _animator = GetComponent<Animator>();
        EventManager.StartListening("onWhirlDespawn", OnWhirlDespawn);
    }

    private void OnDisable()
    {
        EventManager.StopListening("onWhirlDespawn", OnWhirlDespawn);
    }

    private void Update()
    {
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
    }

    private void OnWhirlDespawn(Dictionary<string, object> data)
    {
        _hitbox.enabled = false;
        //StartCoroutine(FadeOut(1));
        Destroy(gameObject, .5f);
    }

    //IEnumerator FadeOut(float duration)
    //{
    //    float journey = 0f;
    //    while (journey <= duration)
    //    {
    //        journey = journey + Time.deltaTime;
    //        float percent = Mathf.Clamp01(journey / duration);
    //        float curvePercent = fadeCurve.Evaluate(percent);
    //        _renderer.color = new Color(
    //            _renderer.color.r,
    //            _renderer.color.g,
    //            _renderer.color.b,
    //            Mathf.LerpUnclamped(1, 0, curvePercent));

    //        yield return new WaitForEndOfFrame();
    //    }
    //}
}
