using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private static UIManager uiManager;
    public TextMeshProUGUI healthText;

    void OnEnable()
    {
        EventManager.StartListening("onHealthChange", OnHealthChange);
    }
    void OnDisable()
    {
        EventManager.StopListening("onHealthChange", OnHealthChange);
    }

    public static UIManager instance
    {
        get
        {
            if (!uiManager)
            {
                uiManager = FindObjectOfType(typeof(UIManager)) as UIManager;

                if (!uiManager)
                {
                    Debug.LogError("There needs to be one active EventManager script on a GameObject in your scene.");
                }
                else
                {
                    uiManager.Init();

                    //  Sets this to not be destroyed when reloading scene
                    DontDestroyOnLoad(uiManager);
                }
            }
            return uiManager;
        }
    }

    void Init()
    {
    }

    void OnHealthChange(Dictionary<string, object> data)
    {
        int hp = (int)data["newHp"];
        changeHealth(hp);
    }

    void changeHealth(int hp)
    {
        healthText.text = "HP: " + hp;
    }
}
