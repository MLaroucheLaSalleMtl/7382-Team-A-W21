using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{
    [SerializeField] private bool closed;       //Is the door closed?
    public bool locked;                         //Is the door locked?
    [SerializeField] private bool dungeonExit;  //Is this door the dungeon exit?
    public List<GameObject> enemies;            //Door unlocks when all enemies are defeated
    [SerializeField] private Arrow_Switch[] switches; //used for switches, different from enemies

    //Variables for audio
    [SerializeField] AudioClip audioOpening;
    [SerializeField] AudioClip audioClosing;
    [SerializeField] AudioClip audioLocked;
    [SerializeField] AudioClip audioRoomCleared;

    //When you interact with a door
    public override void Interaction()
    {
        if (closed) //If the door is closed
        {
            if(locked)  //If the door is locked
            {
                audioS.clip = audioLocked;
                audioS.Play();
            }
            else        //If the door is not locked
                OpenDoor();
        }
    }

    public void OpenDoor()
    {
        this.closed = false;
        anim.SetBool("Closed", false);  //Play door opening animation
        audioS.clip = audioOpening;     //Play door opening sound effect
        audioS.Play();
        this.GetComponent<BoxCollider2D>().enabled = false; //Allow player to enter door
        if(dungeonExit)
        {
            GameManager manager = GameManager.instance;
            manager.DungeonCleared();
        }
        //cancel all invokes in switches                    ***This foreach loop was written by Yan
        foreach (Arrow_Switch s in switches)
            s.CancelInvoke();
    }

    public void CloseDoor()
    {
        this.closed = true;
        anim.SetBool("Closed", true);   //Play door closing animation
        audioS.clip = audioClosing;     //Play door closing sound effect
        audioS.Play();
        this.GetComponent<BoxCollider2D>().enabled = true;  //Stop player from walking through door
    }

    private void Update()
    {
        if ((enemies.Count > 0 || switches.Length > 0) && locked && closed)
            CheckLocks();       //Check if conditions have been met to open door
    }

    //Checks if conditions have been met to open door
    public void CheckLocks()
    {
        foreach(GameObject enemy in enemies)    //All enemies have been defeated
        {
            if (enemy != null)
                return;
        }
        if (switches != null)                   //All switches are active           ***This foreach loop was written by Yan
        {
            foreach (Arrow_Switch s in switches)
            {
                if (!s.Open)
                    return;
            }
        }
        OpenDoor();     //If all the enemies have been defeated, open door
        audioS.clip = audioRoomCleared; //Play room completion sound effect
        audioS.Play();
    }
}
