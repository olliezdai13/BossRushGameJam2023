using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum CatState
{
    IDLE,
    STRANDS, // Sends out random screen-wide slicing strands
    CLAWS, // Sends out player-targeting triple line "claws" of strands
    TELEPORT, // Fades out of existence to a new target (or random spot in bounds)
    SLASH, // Slashes in the direction it's facing in a wide arc
    PROJECTILE // (if conditions apply) Spawns a slow-moving projectile where it is standing (should be preceeded by teleports)
}

[Serializable]
public class CatAction
{
    public CatState state;
    public float time;
    public Transform target;
}
