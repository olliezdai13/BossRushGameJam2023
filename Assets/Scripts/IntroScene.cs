using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroScene : MonoBehaviour
{
    public bool startMusic;
    private Image img;
    private TypewriterEffect typewriterEffect;
    public List<TMP_Text> texts;
    private List<string> textsStrings;
    public float postTextWaitTime;
    public float fadeInTime;
    public float fadeOutTime;
    private Image fadeToBlackImage;
    public AnimationCurve fadeCurve = new AnimationCurve(
      new Keyframe(0, 0, 1, 1),
      new Keyframe(1, 1, 1, 1)
    );

    public void Start()
    {
        textsStrings = new List<string>();
        img = GetComponent<Image>();
        img.enabled = false;
        typewriterEffect = GetComponent<TypewriterEffect>();
        foreach(TMP_Text text in texts)
        {
            textsStrings.Add(text.text);
            text.enabled = false;
        }
        fadeToBlackImage = GameObject.FindGameObjectWithTag("FadeBlackImage").GetComponent<Image>();
    }

    public IEnumerator StepThroughText()
    {
        img.enabled = true;
        yield return StartCoroutine(FadeIn(fadeInTime));
        for (int i = 0; i < texts.Count; i++)
        {
            EnableIndex(i);
            yield return typewriterEffect.Run(textsStrings[i], texts[i]);
            yield return new WaitForSeconds(postTextWaitTime);
        }
        yield return StartCoroutine(FadeOut(fadeOutTime));
        DisableAllTexts();
        img.enabled = false;
    }

    public void DisableAllTexts()
    {
        foreach (TMP_Text text in texts)
        {
            text.enabled = false;
        }
    }

    private void EnableIndex(int j)
    {
        DisableAllTexts();
        for (int i = 0; i < texts.Count; i++)
        {
            if (i == j)
            {
                texts[j].enabled = true;
            } else
            {
                texts[i].enabled = false;
            }
        }
    }

    IEnumerator FadeIn(float duration)
    {
        float journey = 0f;
        while (journey <= duration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);
            float curvePercent = fadeCurve.Evaluate(percent);
            fadeToBlackImage.color = new Color(
                fadeToBlackImage.color.r,
                fadeToBlackImage.color.g,
                fadeToBlackImage.color.b,
                Mathf.LerpUnclamped(1, 0, curvePercent));

            yield return null;
        }
    }
    IEnumerator FadeOut(float duration)
    {
        float journey = 0f;
        while (journey <= duration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);
            float curvePercent = fadeCurve.Evaluate(percent);
            fadeToBlackImage.color = new Color(
                fadeToBlackImage.color.r,
                fadeToBlackImage.color.g,
                fadeToBlackImage.color.b,
                Mathf.LerpUnclamped(0, 1, curvePercent));

            yield return null;
        }
    }
}
