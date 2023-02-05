using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUISystem : MonoBehaviour
{
    [SerializeField] GameObject heartPrefab;
    [SerializeField] GameObject heartBrokenPrefab;

    void OnEnable()
    {
        EventManager.StartListening("onHealthChange", OnPlayerHealthChange);
    }
    void OnDisable()
    {
        EventManager.StopListening("onHealthChange", OnPlayerHealthChange);
    }

    public void DrawHearts(int hearts, int maxHearts)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < maxHearts; i++)
        {
            if (i + 1 <= hearts)
            {
                GameObject heart = Instantiate(heartPrefab, transform.position, Quaternion.identity);
                heart.transform.SetParent(transform);
            } else
            {
                GameObject heart = Instantiate(heartBrokenPrefab, transform.position, Quaternion.identity);
                heart.transform.SetParent(transform);
            }
        }
    }
    void OnPlayerHealthChange(Dictionary<string, object> data)
    {
        int hp = (int)data["newHp"];
        int maxhp = (int)data["maxHp"];
        DrawHearts(hp, maxhp);
    }
}
