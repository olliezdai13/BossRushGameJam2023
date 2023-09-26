using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text textLeft;
    [SerializeField] private TMP_Text textRight;
    [SerializeField] private Image portraitLeft;
    [SerializeField] private Image portraitRight;
    [SerializeField] private Image continueArrow;
    [SerializeField] private Image skipIcon;
    [SerializeField] private Image staticSkipIcon;
    [SerializeField] private DialogueObject testDialogue;

    private TypewriterEffect typewriterEffect;
    private TMP_Text activeTextLabel;
    private Image activePortrait;
    private float skipTimer;
    public float skipHoldTime;
    private bool isTimedDialogue = false;

    // Sprites
    public Sprite spritePlayer;
    public Sprite spriteRatBoss;
    public Sprite spriteCatBoss;
    public Sprite spriteLobbyNpc1;

    private void Start()
    {
        skipTimer = 0;
        typewriterEffect = GetComponent<TypewriterEffect>();
        if (continueArrow) continueArrow.enabled = false;
        CloseDialogueBox();
    }

    private void Update()
    {
        if (!isTimedDialogue)
        {
            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E) || Input.GetMouseButton(0) || Input.GetKey(KeyCode.Period))
            {
                if (skipTimer >= skipHoldTime)
                {
                    skipTimer = skipHoldTime;
                } else
                {
                    skipTimer += Time.deltaTime;
                }
            }
            else
            {
                if (skipTimer <= 0)
                {
                    skipTimer = 0;
                } else
                {
                    skipTimer -= Time.deltaTime;
                }
            }
        }
        skipIcon.fillAmount = skipTimer / skipHoldTime;
    }

    public void ShowDialogue(DialogueObject dialogueObject, Action onEnd = null, float closeTime = 0) {
        dialogueBox.SetActive(true);
        EventManager.TriggerEvent("onDialogueOpen", new Dictionary<string, object> { { "dialogue", dialogueObject } });
        StartCoroutine(StepThroughDialogue(dialogueObject, onEnd: onEnd, closeTime: closeTime));
    }

    private IEnumerator StepThroughDialogue(DialogueObject dialogueObject, Action onEnd = null, float closeTime = 0)
    {
        if (closeTime != 0)
        {
            isTimedDialogue = true;
            skipTimer = 0;
            staticSkipIcon.enabled = false;
        }
        foreach (DialogueLine dialogueLine in dialogueObject.Dialogue)
        {
            skipTimer = 0;
            activeTextLabel = enableText(dialogueLine.Position);
            activePortrait = enablePortrait(dialogueLine.Position, dialogueLine.DialoguePortrait);
            yield return typewriterEffect.Run(dialogueLine.Line, activeTextLabel);
            if (closeTime == 0)
            {
                continueArrow.enabled = true;
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Period) 
                || skipTimer >= skipHoldTime);
                continueArrow.enabled = false;
                if (skipTimer >= skipHoldTime)
                {
                    break;
                }
            } else
            { 
                yield return new WaitForSeconds(closeTime);
            }
        }
        if (onEnd != null)
        {
            onEnd();
        }
        CloseDialogueBox();
        isTimedDialogue = false;
        staticSkipIcon.enabled = true;
        yield return new WaitForSeconds(0.25f);
        EventManager.TriggerEvent("onDialogueClose", new Dictionary<string, object> { { "dialogue", dialogueObject } });
    }

    private void CloseDialogueBox()
    {
        dialogueBox.SetActive(false);
        textLeft.text = string.Empty;
        textRight.text = string.Empty;
        portraitLeft.sprite = null;
        portraitRight.sprite = null;
    }

    private TMP_Text enableText(DialoguePortraitPosition position)
    {
        if (position == DialoguePortraitPosition.LEFT)
        {
            textRight.enabled = false;
            textLeft.enabled = true;
            return textLeft;
        }
        else
        {
            textLeft.enabled = false;
            textRight.enabled = true;
            return textRight;
        }
    }
    private Image enablePortrait(DialoguePortraitPosition position, DialoguePortrait portrait)
    {
        Image tmpImage;
        if (position == DialoguePortraitPosition.LEFT)
        {
            portraitRight.enabled = false;
            portraitLeft.enabled = true;
            tmpImage = portraitLeft;
        }
        else
        {
            portraitRight.enabled = true;
            portraitLeft.enabled = false;
            tmpImage = portraitRight;
        }

        if (portrait == DialoguePortrait.RAT_BOSS)
        {
            tmpImage.sprite = spriteRatBoss;
        }
        else if (portrait == DialoguePortrait.LOBBY_NPC_1)
        {
            tmpImage.sprite = spriteLobbyNpc1;
        }
        else if (portrait == DialoguePortrait.CAT_BOSS)
        {
            tmpImage.sprite = spriteCatBoss;
        }
        else
        {
            tmpImage.sprite = spritePlayer;
        }
        return tmpImage;
    }
}
