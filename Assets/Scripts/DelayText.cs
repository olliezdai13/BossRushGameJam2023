using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayText : MonoBehaviour
{
    public float delay;
    private Animator animator;
    private AudioSource audioSource;
    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        // Disable stuff?
    }

    public void Go()
    {
        Invoke("Animate", delay);
    }

    private void Animate()
    {
        animator.SetTrigger("go");
        //audioSource.Play();
    }

    // This is actually more like OnSoundPlay
    public void OnFinishedAnimating()
    {
        // Do screenshake
        audioSource.Play();
    }
}
