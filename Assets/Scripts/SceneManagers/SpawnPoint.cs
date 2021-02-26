using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Initializes dungeon specific variables and restores player to full health
public class SpawnPoint : MonoBehaviour
{
    private Player player;
    [SerializeField] private GameObject boss;
    void Start()
    {
        this.player = GameManager.instance.player;
        player.transform.position = this.transform.position;    //Move player to spawn point
        player.hp = player.maxHealth;                           //Return player to full health and stamina
        player.stamina = player.maxStamina;
        if (player.UIScript)                                    //Update player health and stamina bar
        {
            player.UIScript.SetBar(UIBars.Stamina, player.maxStamina, player.maxStamina);
            player.UIScript.SetBar(UIBars.Health, player.maxHealth, player.maxHealth);
        }
        player.canvas.GetComponent<UIBehaviour>().boss = this.boss; //Set boss for this dungeon
        player.canvas.GetComponent<UIBehaviour>().Start();
    }
}
