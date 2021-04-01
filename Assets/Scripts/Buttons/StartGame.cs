using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    private GameManager manager;
    private AsyncOperation async;   //Data that's been loaded

    void Start()
    {
        manager = GameManager.instance;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void PrepStart()
    {
        Input.ResetInputAxes();     //Avoid accidental selection on button release
        System.GC.Collect();        //Clear memory of unused items
    }

    public void StartNewGame()
    {
        PrepStart();
        async = SceneManager.LoadSceneAsync(3);     //Load hub world
        async.allowSceneActivation = false;         //Wait to switch to next scene
    }

    public void ContinueSavedGame()
    {
        PrepStart();
        int i = PlayerPrefs.GetInt("Level", 3);     //Read save game data from the registry
        int check = PlayerPrefs.GetInt("Checkpoint", 0);
        if (i > 6 || (i == 5 && check > 0))          //1st dungeon - Add shield
            manager.player.InteractionAdd();        
        if (i > 10 || (i == 9 && check > 0))         //Second dungeon - Add bombs
            manager.player.InteractionAdd();        
        if(i == 13 && check > 0)                       //Third dungeon - Add bow and wand
        {
            manager.player.InteractionAdd();        
            manager.player.InteractionAdd();        
        }
        async = SceneManager.LoadSceneAsync(i);     //Load saved scene
        async.allowSceneActivation = false;         //Wait to switch to next scene
    }

    void Update()
    {
        if (async != null && async.progress >= 0.9f)
        {
            manager.DisableMenu(manager.mainMenu);
            Cursor.lockState = CursorLockMode.Locked;   //Lock and hide cursor
            Cursor.visible = false;
            manager.StartPlayerMove();
            manager.hud.SetActive(true);
            async.allowSceneActivation = true;
            Destroy(manager.mainMenu);  //Destroy the main menu once we're done with it
        }
    }
}
