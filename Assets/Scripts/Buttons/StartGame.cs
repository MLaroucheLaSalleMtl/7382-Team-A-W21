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

    public void StartNewGame()
    {
        Input.ResetInputAxes();     //Avoid accidental selection on button release
        System.GC.Collect();        //Clear memory of unused items
        async = SceneManager.LoadSceneAsync(3);     //Load hub world
        async.allowSceneActivation = false;         //Wait to switch to next scene
    }

    public void ContinueSavedGame()
    {
        Input.ResetInputAxes();     //Avoid accidental selection on button release
        System.GC.Collect();        //Clear memory of unused items
        async = SceneManager.LoadSceneAsync(3);     //Load next scene
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
