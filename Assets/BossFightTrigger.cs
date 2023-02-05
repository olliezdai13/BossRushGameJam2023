using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFightTrigger : MonoBehaviour
{
    public DialogueObject dialogue;
    private bool triggered = false;
    
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
                    dialogueManager.ShowDialogue(dialogue);
                }
            }
        }
    }
}
