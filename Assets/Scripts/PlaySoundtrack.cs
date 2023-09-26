using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundtrack : MonoBehaviour
{
    public SoundtrackNames stName;
    void Start()
    {

        EventManager.TriggerEvent("soundtrack", new Dictionary<string, object> { { "name", stName } });

    }
}
