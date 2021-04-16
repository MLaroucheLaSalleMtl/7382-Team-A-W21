using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Defines which dungeon entrances are open in the hub world based off which items the player has collected so far
public class DungeonSelector : MonoBehaviour
{
    [SerializeField] private Door[] doors;
    private GameManager manager;

    void Start()
    {
        manager = GameManager.instance;
        switch(manager.player.weaponLock)
        {
            //Player has 0 items -> Open first dungeon
            case 0:
                doors[0].OpenDoor();
                break;
            //Player has 1 item (shield) -> Unlock second dungeon
            case 1:
                doors[1].locked = false;
                break;
            //Player has found 2 items (shield + bomb) -> Unlock third dungeon
            case 2:
                doors[2].locked = false;
                break;
        }
        PlayerPrefs.SetInt("Checkpoint", 0);   //Reset checkpoint in RAM
        PlayerPrefs.Save();                    //Save the data to the disk
    }
}
