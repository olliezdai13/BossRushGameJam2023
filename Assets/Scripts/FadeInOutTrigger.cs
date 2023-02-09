using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FadeInOutTrigger : MonoBehaviour
{
    private Coroutine co1;
    public float fadeInTime;
    public float fadeOutTime;
    public AnimationCurve fadeCurve = new AnimationCurve(
      new Keyframe(0, 0, 1, 1),
      new Keyframe(1, 1, 1, 1)
    );

    private TMP_Text text;

    private void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (co1 != null) StopCoroutine(co1);
            co1 = StartCoroutine(Fade(text.alpha, 1, fadeInTime));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (co1 != null) StopCoroutine(co1);
            co1 = StartCoroutine(Fade(text.alpha, 0, fadeOutTime));
        }
    }

    IEnumerator Fade(float origin, float target, float duration)
    {
        float adjustedDuration = Mathf.Abs(target - origin) * duration;
        float journey = 0f;
        while (journey <= adjustedDuration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / adjustedDuration);
            float curvePercent = fadeCurve.Evaluate(percent);
            text.alpha = Mathf.LerpUnclamped(origin, target, curvePercent);

            yield return null;
        }
    }
}
