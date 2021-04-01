using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Automatically loads the next scene on Start (For pre-loader and loading screen)
public class AutoLoadScene : MonoBehaviour
{
    private AsyncOperation async;

    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.buildIndex >= 3)    //If the player has already entered the first dungeon
        {
            PlayerPrefs.SetInt("Level", currentScene.buildIndex);   //Save current scene index in RAM
            PlayerPrefs.Save();                                     //Save the data to the disk
        }
        async = SceneManager.LoadSceneAsync(currentScene.buildIndex + 1);   //Load the next scene
    }

    private void Update()
    {
        if (async != null && async.progress >= 0.9f)    //If the scene to load is specified and 90% loaded
            async.allowSceneActivation = true;          //Switch scene
    }
}
