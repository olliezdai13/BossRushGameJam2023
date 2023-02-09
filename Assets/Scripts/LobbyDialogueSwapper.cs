using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyDialogueSwapper : MonoBehaviour
{
    private DialogueTrigger dialogueTrigger;
    public DialogueObject preRatDialogue;
    public DialogueObject preCatDialogue;
    public DialogueObject preOwlDialogue;
    void Start()
    {
        dialogueTrigger = GetComponent<DialogueTrigger>();
        if (!GameData.beatRat && !GameData.beatCat && !GameData.beatOwl)
        {
            dialogueTrigger.dialogue = preRatDialogue;
        }
        else if (GameData.beatRat && !GameData.beatCat && !GameData.beatOwl)
        {
            dialogueTrigger.dialogue = preCatDialogue;
        }
        else if (GameData.beatCat && !GameData.beatOwl)
        {
            dialogueTrigger.dialogue = preOwlDialogue;
        }
    }
}
