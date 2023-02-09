using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager gameManager;
    private Image fadeToBlackImage;
    public float fadeOutTime;
    public float fadeInTime;
    public float fadeOutTimeToLobby;
    public AnimationCurve fadeCurve = new AnimationCurve(
      new Keyframe(0, 0, 1, 1),
      new Keyframe(1, 1, 1, 1)
    );
    public static GameManager instance
    {
        get
        {
            if (!gameManager)
            {
                gameManager = FindObjectOfType(typeof(GameManager)) as GameManager;

                if (!gameManager)
                {
                    Debug.LogError("There needs to be one active EventManager script on a GameObject in your scene.");
                }
                else
                {
                    gameManager.Init();

                    //  Sets this to not be destroyed when reloading scene
                    DontDestroyOnLoad(gameManager);
                }
            }
            return gameManager;
        }
    }
    void Init()
    {
        fadeToBlackImage = GameObject.FindGameObjectWithTag("FadeBlackImage").GetComponent<Image>();
    }

    public void EnterBoss1()
    {
        StartCoroutine(FadeToLevel(gameManager.fadeOutTime, "Boss1"));
    }
    public void EnterBoss2()
    {
        StartCoroutine(FadeToLevel(gameManager.fadeOutTime, "Boss2"));
    }
    public void EnterBoss3()
    {
        StartCoroutine(FadeToLevel(gameManager.fadeOutTime, "Boss3"));
    }
    public void EnterLobby()
    {
        StartCoroutine(FadeToLevel(gameManager.fadeOutTimeToLobby, "Lobby"));
    }

    public void FadeInFromBlack()
    {
        fadeToBlackImage = GameObject.FindGameObjectWithTag("FadeBlackImage").GetComponent<Image>();
        fadeToBlackImage.color = new Color(
                fadeToBlackImage.color.r,
                fadeToBlackImage.color.g,
                fadeToBlackImage.color.b,
                1);

        StartCoroutine(FadeIn(gameManager.fadeInTime));
    }
    IEnumerator FadeToLevel(float duration, string sceneName)
    {
        float adjustedDuration = duration;
        float journey = 0f;
        while (journey <= adjustedDuration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / adjustedDuration);
            float curvePercent = fadeCurve.Evaluate(percent);
            fadeToBlackImage.color = new Color(
                fadeToBlackImage.color.r, 
                fadeToBlackImage.color.g, 
                fadeToBlackImage.color.b, 
                Mathf.LerpUnclamped(0, 1, curvePercent));

            yield return null;
        }
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(sceneName);
    }
    IEnumerator FadeIn(float duration)
    {
        float adjustedDuration = Mathf.Abs(0 - 1) * duration;
        float journey = 0f;
        while (journey <= adjustedDuration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / adjustedDuration);
            float curvePercent = fadeCurve.Evaluate(percent);
            fadeToBlackImage.color = new Color(
                fadeToBlackImage.color.r,
                fadeToBlackImage.color.g,
                fadeToBlackImage.color.b,
                Mathf.LerpUnclamped(1, 0, curvePercent));

            yield return null;
        }
    }
}
