using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{
    [SerializeField] private bool closed;   //Is the door closed?
    public bool locked;                     //Is the door locked?
    [SerializeField] private bool dungeonExit;  //Is this door the dungeon exit?
    [SerializeField] GameObject[] enemies;  //Door unlocks when all enemies are defeated

    [SerializeField] private Arrow_Switch[] switches; //used for switches, different from enemies

    public override void Interaction()
    {
        if (closed) //If the door is closed
        {
            if(locked)  //If the door is locked
            {
                Debug.Log("Locked");
                //Play locked sound
            }
            else        //If the door is not locked
            {
                OpenDoor();
            }
        }
    }
    public void OpenDoor()
    {
        Debug.Log("Opening");
        this.closed = false;
        anim.SetBool("Closed", false);
        this.GetComponent<BoxCollider2D>().enabled = false;
        if(dungeonExit)
        {
            GameManager manager = GameManager.instance;
            manager.DungeonCleared();
        }
        //cancel all invokes in switches
        foreach (Arrow_Switch s in switches)
            s.CancelInvoke();
    }
    public void CloseDoor()
    {
        this.closed = true;
        anim.SetBool("Closed", true);
        this.GetComponent<BoxCollider2D>().enabled = true;
    }
    private void Update()
    {
        if ((enemies.Length > 0 || switches.Length > 0) && locked && closed)
            CheckLocks();       //Check if conditions have been met to open door
    }

    public void CheckLocks()
    {
        foreach(GameObject enemy in enemies)
        {
            if (enemy != null)
                return;
        }
        if (switches != null) 
        {
            foreach (Arrow_Switch s in switches)
            {
                if (!s.Open)
                    return;
            }
        }
        OpenDoor(); //If all the enemies have been defeated, open door
    }
}
