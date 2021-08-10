using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : Interactable
{
    [SerializeField] Spikes[] spikes;   //Spikes affected by this lever

    //When the player interacts with a lever
    public override void Interaction()
    {
        anim.SetBool("LeverOn", !(anim.GetBool("LeverOn")));  //Flip the lever in the opposite direction
        foreach (Spikes spike in spikes)
            spike.InvertSpikes();           //Raise or lower spikes associated with this lever 
    }
}
