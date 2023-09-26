using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // 0 lobby, 1 rat, 2 cat, 3 owl
    public int currentLevel;
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
        currentLevel = 1;
        GameData.lobbySpawn = LobbySpawn.RAT;
        StartCoroutine(FadeToLevel(gameManager.fadeOutTime, "Boss1"));
        GameData.justWon = false;
        GameData.justLost = false;
    }
    public void EnterBoss2()
    {
        currentLevel = 2;
        GameData.lobbySpawn = LobbySpawn.CAT;
        StartCoroutine(FadeToLevel(gameManager.fadeOutTime, "Boss2"));
        GameData.justWon = false;
        GameData.justLost = false;
    }
    public void EnterBoss3()
    {
        currentLevel = 3;
        GameData.lobbySpawn = LobbySpawn.OWL;
        StartCoroutine(FadeToLevel(gameManager.fadeOutTime, "Boss3"));
        GameData.justWon = false;
        GameData.justLost = false;
    }
    public void EnterLobby(float time = 0)
    {
        currentLevel = 0;
        StartCoroutine(FadeToLevel(time == 0 ? gameManager.fadeOutTimeToLobby : time, "Lobby"));
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
        EventManager.TriggerEvent("stopSoundtrack", new Dictionary<string, object> { });
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

    private float fixedDeltaTime;
    void Awake()
    {
        // Make a copy of the fixedDeltaTime, it defaults to 0.02f, but it can be changed in the editor
        this.fixedDeltaTime = Time.fixedDeltaTime;
    }
    private void Update()
    {
        if (Input.GetKey(KeyCode.Alpha0) && Input.GetKey(KeyCode.Alpha1))
        {
            GameData.beatRat = true;
            GameData.beatCat = true;
            GameData.beatOwl = true;
            GameData.hasKey = true;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (Time.timeScale == 1.0f)
                Time.timeScale = 0.1f;
            else
                Time.timeScale = 1.0f;
            // Adjust fixed delta time according to timescale
            // The fixed delta time will now be 0.02 real-time seconds per frame
            Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
        }
    }
}
