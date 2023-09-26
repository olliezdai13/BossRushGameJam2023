using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnChanger : MonoBehaviour
{
    public Transform mainSpawn;
    public Transform ratSpawn;
    public Transform catSpawn;
    public Transform owlSpawn;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (mainSpawn != null && GameData.lobbySpawn == LobbySpawn.MAIN)
        {
            player.transform.position = mainSpawn.position;
        }
        if (ratSpawn != null && GameData.lobbySpawn == LobbySpawn.RAT)
        {
            player.transform.position = ratSpawn.position;
        }
        if (catSpawn != null && GameData.lobbySpawn == LobbySpawn.CAT)
        {
            player.transform.position = catSpawn.position;
        }
        if (owlSpawn != null && GameData.lobbySpawn == LobbySpawn.OWL)
        {
            player.transform.position = owlSpawn.position;
        }
    }
}
