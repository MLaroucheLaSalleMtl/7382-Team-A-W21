using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private GameObject spawnPoint;

    private void Awake()
    {
        int i = PlayerPrefs.GetInt("Checkpoint", 0);     //Read checkpoint data from the registry
        if (i > 0)                                       //If the player has reached the checkpoint
            spawnPoint.transform.position = this.transform.position;    //Move them to the checkpoint
    }

    //When player reaches the checkpoint
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerPrefs.SetInt("Checkpoint", 1);   //Reset checkpoint in RAM
        PlayerPrefs.Save();                    //Save the data to the disk
        GetComponentInChildren<Animator>().SetBool("Lit", true);    //Play animation
        GetComponentInChildren<AudioSource>().Play();               //Play sound effect
        Destroy(GetComponent<BoxCollider2D>());
    }
}
