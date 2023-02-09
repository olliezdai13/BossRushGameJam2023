using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceedOnDeath : MonoBehaviour
{
    public DialogueObject dialogue;
    public float bossDeadContinueDelay = 2f;
    public BossDoor.BossDoorType boss;
    void OnEnable()
    {
        EventManager.StartListening("onBossHealthChange", OnBossHealthChange);
        EventManager.StartListening("onHealthChange", OnPlayerHealthChange);
    }
    void OnDisable()
    {
        EventManager.StopListening("onBossHealthChange", OnBossHealthChange);
        EventManager.StopListening("onHealthChange", OnPlayerHealthChange);
    }

    void OnBossHealthChange(Dictionary<string, object> data)
    {
        int hp = (int)data["newHp"];
        if (hp <= 0)
        {
            StartCoroutine(BossDie());
        }
    }
    void OnPlayerHealthChange(Dictionary<string, object> data)
    {
        int hp = (int)data["newHp"];
        if (hp <= 0)
        {
            PlayerDie();
        }
    }

    private IEnumerator BossDie()
    {
        yield return new WaitForSeconds(bossDeadContinueDelay);
        if (boss == BossDoor.BossDoorType.RAT)
        {
            GameData.beatRat = true;
            GameData.timesWonToRat++;
        }
        if (boss == BossDoor.BossDoorType.CAT)
        {
            GameData.beatCat = true;
            GameData.timesWonToCat++;
        }
        if (boss == BossDoor.BossDoorType.OWL)
        {
            GameData.beatOwl = true;
            GameData.timesWonToOwl++;
        }
        GameObject ui = GameObject.FindGameObjectWithTag("UI");
        DialogueManager dialogueManager = ui.GetComponent<DialogueManager>();
        if (dialogueManager && dialogue)
        {
            dialogueManager.ShowDialogue(dialogue, onEnd: () => 
            {
                GameManager.instance.EnterLobby();
            }
            );
        }
    }

    void PlayerDie()
    {
        // This will probs stop the BossDie() coroutine if the player killed the boss then died to a boulder
        // Effectively making player death override boss death
        StopAllCoroutines();
        if (boss == BossDoor.BossDoorType.RAT)
        {
            GameData.timesLostToRat++;
        }
        if (boss == BossDoor.BossDoorType.CAT)
        {
            GameData.timesLostToCat++;
        }
        if (boss == BossDoor.BossDoorType.OWL)
        {
            GameData.timesLostToOwl++;
        }
        GameManager.instance.EnterLobby();
    }
}
