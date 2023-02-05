using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum RatState
{
    IDLE,
    DASH,
    THROW,
    JUMP,
    SPIN
}

[Serializable]
public class RatAction {
    public RatState state;
    public float time;
    public Transform target;
}
