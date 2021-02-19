using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{
    private Animator anim;
    [SerializeField] private bool closed;   //Is the door closed?
    public bool locked;                     //Is the door locked?
    [SerializeField] GameObject[] enemies;  //Door unlocks when all enemies are defeated

    private void Start()
    {
        this.anim = GetComponent<Animator>();
    }
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
        this.closed = false;
        anim.SetBool("Closed", false);
        StartCoroutine("WaitForAnim");
        this.GetComponent<BoxCollider2D>().enabled = false;
    }
    public void CloseDoor()
    {
        this.closed = true;
        anim.SetBool("Closed", true);
        
        this.GetComponent<BoxCollider2D>().enabled = true;
    }
    private void Update()
    {
        if (enemies.Length > 0 && locked && closed)
            CheckLocks();       //Check if conditions have been met to open door
    }

    public void CheckLocks()
    {
        foreach(GameObject enemy in enemies)
        {
            if (enemy != null)
                return;
        }
        OpenDoor(); //If all the enemies have been defeated, open door
    }

    //private IEnumerator WaitForAnim()
    //{

    //}
}
