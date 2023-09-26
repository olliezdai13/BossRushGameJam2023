using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopAudio : MonoBehaviour
{
    private AudioSource _audioSource;
    public float ampItUpPitch;
    public float ampItUpDuration;

    private float originalPitch;
    void OnEnable()
    {
        _audioSource = GetComponent<AudioSource>();
        originalPitch = _audioSource.pitch;
        EventManager.StartListening("soundtrackAmpUp", AmpItUp);
    }
    void OnDisable()
    {
        EventManager.StopListening("soundtrackAmpUp", AmpItUp);
    }

    public void Play()
    {
        _audioSource.pitch = originalPitch;
        _audioSource.Play();
    }
    public void Stop()
    {
        _audioSource.Stop();
    }


    void AmpItUp(Dictionary<string, object> data)
    {
        StartCoroutine(LerpPitchUp(ampItUpDuration, ampItUpPitch));
    }

    public AnimationCurve fadeCurve = new AnimationCurve(
      new Keyframe(0, 0, 1, 1),
      new Keyframe(1, 1, 1, 1)
    );
    IEnumerator LerpPitchUp(float duration, float target)
    {
        float journey = 0f;
        while (journey <= duration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);
            float curvePercent = fadeCurve.Evaluate(percent);
            _audioSource.pitch = Mathf.LerpUnclamped(1, target, curvePercent);
            yield return null;
        }
    }
}
