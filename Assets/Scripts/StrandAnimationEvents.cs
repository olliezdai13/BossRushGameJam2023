using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrandAnimationEvents : MonoBehaviour
{
    public Strand parentStrand;

    public void SetFlickerStarted()
    {
        parentStrand.flickerStarted = true;
    }
}
