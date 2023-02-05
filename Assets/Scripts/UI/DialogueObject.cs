using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/DialogueObject")]

public class DialogueObject : ScriptableObject
{
    [SerializeField] [TextArea] private string[] dialogue;
    [SerializeField] private string dialogueName;
    [SerializeField] private bool freezePlayer;

    public string Name => name;
    public bool FreezePlayer => freezePlayer;
    public string[] Dialogue => dialogue;
}
