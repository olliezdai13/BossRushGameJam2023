using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioGroup : MonoBehaviour
{
    public AudioClip[] _clips;
    private AudioSource _audioSource;
    private float _originalVolume;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _originalVolume = _audioSource.volume;
    }
    public void PlayRandom()
    {
        _audioSource.volume = _originalVolume;
        if (_clips.Length > 0) _audioSource.PlayOneShot(_clips[Random.Range(0, _clips.Length)]);
    }
    public void PlayRandom(float volumeOverride)
    {
        _audioSource.volume = volumeOverride;
        if (_clips.Length > 0) _audioSource.PlayOneShot(_clips[Random.Range(0, _clips.Length)]);
    }
}
