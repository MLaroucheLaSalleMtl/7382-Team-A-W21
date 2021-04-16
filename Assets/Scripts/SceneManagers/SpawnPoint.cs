using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Initializes dungeon specific variables and restores player to full health
public class SpawnPoint : MonoBehaviour
{
    private Player player;
    private GameManager manager;
    [SerializeField] private GameObject boss;
    void Start()
    {
        manager = GameManager.instance;
        if(manager)
        {
            this.player = manager.player;
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
            SceneManager.MoveGameObjectToScene(manager.gameObject, SceneManager.GetActiveScene());
            manager.StartPlayerMove();
            manager.obscure.SetActive(false);   //Stop hiding the player (for loading screen)
        }
    }
}
