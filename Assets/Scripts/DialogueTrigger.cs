using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueObject dialogue;
    private bool triggered = false;
    public bool changeSoundtrackWhenClose = true;
    public SoundtrackNames onCloseSoundtrack;
    
    public bool Triggered => triggered;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!triggered)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                GameObject ui = GameObject.FindGameObjectWithTag("UI");
                DialogueManager dialogueManager = ui.GetComponent<DialogueManager>();
                if (dialogueManager && dialogue)
                {
                    triggered = true;
                    dialogueManager.ShowDialogue(dialogue, onEnd: () =>
                    {
                        if (changeSoundtrackWhenClose)
                        {
                            EventManager.TriggerEvent("soundtrack", new Dictionary<string, object> { { "name", onCloseSoundtrack } });
                        }
                    });
                }
            }
        }
    }
}
