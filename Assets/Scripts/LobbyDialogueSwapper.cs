using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls NPC position and dialogue in the lobby
public class LobbyDialogueSwapper : MonoBehaviour
{
    private DialogueTrigger dialogueTrigger;
    public DialogueObject preRatDialogue;
    public DialogueObject preCatDialogue;
    public DialogueObject preOwlDialogue;
    public Transform preRatSpot;
    public Transform preCatSpot;
    public Transform preOwlSpot;
    void Start()
    {
        dialogueTrigger = GetComponent<DialogueTrigger>();
        if (GameData.justLost)
        {
            gameObject.SetActive(false);
        }
        else if (GameData.lobbySpawn == LobbySpawn.MAIN)
        {
            if (preRatSpot != null) transform.position = preRatSpot.position;
            dialogueTrigger.dialogue = preRatDialogue;
        }
        else if (GameData.lobbySpawn == LobbySpawn.RAT)
        {
            if (preCatSpot != null) transform.position = preCatSpot.position;
            dialogueTrigger.dialogue = preCatDialogue;
        }
        else if (GameData.lobbySpawn == LobbySpawn.CAT)
        {
            if (preOwlSpot != null) transform.position = preOwlSpot.position;
            dialogueTrigger.dialogue = preOwlDialogue;
        }
        //    if (!GameData.beatRat && !GameData.beatCat && !GameData.beatOwl)
        //    {
        //        dialogueTrigger.dialogue = preRatDialogue;
        //    }
        //    else if (GameData.beatRat && !GameData.beatCat && !GameData.beatOwl)
        //    {
        //        dialogueTrigger.dialogue = preCatDialogue;
        //    }
        //    else if (GameData.beatCat && !GameData.beatOwl)
        //    {
        //        dialogueTrigger.dialogue = preOwlDialogue;
        //    }
    }
}
