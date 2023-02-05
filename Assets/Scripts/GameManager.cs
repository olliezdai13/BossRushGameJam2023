using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager gameManager;
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
    }

    //[Header("PreFightIdle")]
    //public float _preFightIdleDuration;

    private void Start()
    {
        //StartCoroutine(WaitAndSignalStart(_preFightIdleDuration));
    }

    //IEnumerator WaitAndSignalStart(float time)
    //{
    //    yield return new WaitForSeconds(time);
    //    EventManager.TriggerEvent("onPreFightIdleEnd", new Dictionary<string, object> {});
    //}

}
