using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Switches to the next scene when the player walks through the door
public class SceneSelector : MonoBehaviour
{
    private AsyncOperation async;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        async = SceneManager.LoadSceneAsync(currentScene.buildIndex + 1);   //Load the next scene
    }

    private void Update()
    {
        if (async != null && async.progress >= 0.9f)    //If the scene to load is specified and 90% loaded
            async.allowSceneActivation = true;          //Switch scene
    }
}
