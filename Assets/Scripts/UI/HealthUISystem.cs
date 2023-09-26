using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUISystem : MonoBehaviour
{
    [SerializeField] Sprite heartSprite;
    [SerializeField] Sprite heartBrokenSprite;

    [SerializeField] GameObject[] hearts;

    void OnEnable()
    {
        EventManager.StartListening("onHealthChange", OnPlayerHealthChange);
    }
    void OnDisable()
    {
        EventManager.StopListening("onHealthChange", OnPlayerHealthChange);
    }

    void OnPlayerHealthChange(Dictionary<string, object> data)
    {
        int hp = (int)data["newHp"];
        int maxhp = (int)data["maxHp"];
        UpdateHearts(hp);
    }

    void UpdateHearts(int hp)
    {
        int hpLeft = hp;
        if (hearts.Length < hp)
        {
            Debug.LogError("NOT ENOUGH HEARTS FOR " + hp + " HP");
        } else
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                if (hpLeft > 0)
                {
                    hpLeft--;
                    hearts[i].GetComponent<Image>().sprite = heartSprite;
                } else
                {
                    hearts[i].GetComponent<Image>().sprite = heartBrokenSprite;
                }
            }
        }
    }
}
