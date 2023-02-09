using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/DialogueObject")]

public class DialogueObject : ScriptableObject
{
    [SerializeField] private DialogueLine[] dialogue;
    [SerializeField] private string dialogueName;
    [SerializeField] private bool freezePlayer;

    public string Name => name;
    public bool FreezePlayer => freezePlayer;
    public DialogueLine[] Dialogue => dialogue;
}

[System.Serializable]
public class DialogueLine
{
    [SerializeField] [TextArea] private string line;
    [SerializeField] private DialoguePortrait dialoguePortrait;
    [SerializeField] private DialoguePortraitPosition position;
    public string Line => line;
    public DialoguePortrait DialoguePortrait => dialoguePortrait;
    public DialoguePortraitPosition Position => position;
}

public enum DialoguePortrait
{
    PLAYER,
    RAT_BOSS,
    CAT_BOSS,
    LOBBY_NPC_1,
}
public enum DialoguePortraitPosition
{
    LEFT,
    RIGHT,
}