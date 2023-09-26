using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    public IntroScene[] scenes;
    public MainMenuBehavior mainMenuScene;

    private void Start()
    {
        mainMenuScene.gameObject.SetActive(false);
        StartCoroutine(StepThroughScenes());
    }

    private IEnumerator StepThroughScenes()
    {
        //yield return new WaitForSeconds(20);
        foreach (IntroScene scene in scenes)
        {
            scene.gameObject.SetActive(true);
            if (scene.startMusic) GetComponent<AudioSource>().Play();
            yield return StartCoroutine(scene.StepThroughText());
            scene.gameObject.SetActive(false);
        }
        Image fadeToBlackImage = GameObject.FindGameObjectWithTag("FadeBlackImage").GetComponent<Image>();
        fadeToBlackImage.enabled = false;
        mainMenuScene.gameObject.SetActive(true);
        mainMenuScene.Init();
    }
}
