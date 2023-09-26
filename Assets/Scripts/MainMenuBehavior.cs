using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuBehavior : MonoBehaviour
{
    public DelayText text1;
    public DelayText text2;
    public DelayText text3;
    public DelayText text4;

    public float characterShowDelay;
    public float playShowDelay;
    public Image characterImage;
    public GameObject playButton;
    private AudioSource audioSource;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // Disable everything, to be enabled on Init()
        characterImage.enabled = false;
        playButton.GetComponent<Button>().enabled = false;
        playButton.GetComponent<Image>().enabled = false;
        playButton.GetComponentInChildren<TMP_Text>().enabled = false;
    }
    public void Init()
    {
        text1.Go();
        text2.Go();
        text3.Go();
        text4.Go();
        Invoke("ShowCharacter", characterShowDelay);
        Invoke("ShowPlay", playShowDelay);
    }

    private void ShowCharacter()
    {
        //audioSource.Play();
        characterImage.enabled = true;
    }
    private void ShowPlay()
    {
        audioSource.Play();
        playButton.GetComponent<Button>().enabled = true;
        playButton.GetComponent<Image>().enabled = true;
        playButton.GetComponentInChildren<TMP_Text>().enabled = true;
    }
}
