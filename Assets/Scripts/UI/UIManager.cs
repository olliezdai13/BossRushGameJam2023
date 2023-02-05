using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private static UIManager uiManager;

    [Header("Boss Healthbar")]
    public Image _healthBarForeground;
    public Image _healthBarBackground;
    public float _bossHpLerpDuration;
    public AnimationCurve _bossHPAnimationCurve = new AnimationCurve(
      new Keyframe(0, 0, 1, 1),
      new Keyframe(1, 1, 1, 1)
    );
    private float _currentBossHp = -999;

    void OnEnable()
    {
        _healthBarBackground.enabled = false;
        _healthBarForeground.enabled = false;
        EventManager.StartListening("onHealthChange", OnPlayerHealthChange);
        EventManager.StartListening("onBossHealthChange", OnBossHealthChange);
        EventManager.StartListening("onDialogueClose", OnDialogueClose);
    }
    void OnDisable()
    {
        EventManager.StopListening("onHealthChange", OnPlayerHealthChange);
        EventManager.StopListening("onBossHealthChange", OnBossHealthChange);
        EventManager.StopListening("onDialogueClose", OnDialogueClose);
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
        _healthBarForeground.fillAmount = 1;
    }

    void OnPlayerHealthChange(Dictionary<string, object> data)
    {
        int hp = (int)data["newHp"];
        int maxhp = (int)data["maxHp"];
        changePlayerHealth(hp, maxhp);
    }
    void OnBossHealthChange(Dictionary<string, object> data)
    {
        int hp = (int)data["newHp"];
        int maxhp = (int)data["maxHp"];
        changeBossHealth(hp, maxhp);
    }

    void changePlayerHealth(int hp, int maxhp)
    {
        //healthText.text = "Player HP: " + hp + "/" + maxhp;
    }

    void changeBossHealth(int hp, int maxhp)
    {
        if (_currentBossHp == -999)
        {
            _currentBossHp = hp;
        }
        StartCoroutine(AnimateHealthbar(_currentBossHp / (float)maxhp, (float)hp / (float)maxhp, _bossHpLerpDuration));
        _currentBossHp = hp;
    }

    IEnumerator AnimateHealthbar(float origin, float target, float duration)
    {
        float journey = 0f;
        while (journey <= duration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);
            float curvePercent = _bossHPAnimationCurve.Evaluate(percent);
            _healthBarForeground.fillAmount = Mathf.LerpUnclamped(origin, target, curvePercent);

            yield return null;
        }
    }
    void OnDialogueClose(Dictionary<string, object> data)
    {
        DialogueObject dialogue = (DialogueObject)data["dialogue"];
        if (dialogue.FreezePlayer)
        {
            _healthBarBackground.enabled = true;
            _healthBarForeground.enabled = true;
        }
    }
}
