using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : Interactable
{
    //When the player interacts with a chest
    public override void Interaction()
    {
        manager.player.InteractionAdd();
        anim.SetTrigger("Open");                //Play chest opening animation              
        GetComponent<AudioSource>().Play();     //Play chest opening sound effect       
        this.gameObject.layer = LayerMask.NameToLayer("Default");   //Player can no longer interact with open chest
        manager.ItemFound();                    //Display item found message
    }
}
