using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Switches to the next scene when the player walks through a door
public class SceneSelector : MonoBehaviour
{
    private AsyncOperation async;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.GetComponent<Player>())
        {
            GameManager.instance.obscure.SetActive(true);                       //Hide player while in loading screen
            Scene currentScene = SceneManager.GetActiveScene();
            if(currentScene.buildIndex < 14)                                    //Remove manager & player before ending cutscene
                DontDestroyOnLoad(GameManager.instance.gameObject);
            async = SceneManager.LoadSceneAsync(currentScene.buildIndex + 1);   //Load the next scene
            async.allowSceneActivation = false;                                 //Wait to switch to next scene
        }
    }

    private void Update()
    {
        if (async != null && async.progress >= 0.9f)    //If the scene to load is specified and 90% loaded
            async.allowSceneActivation = true;          //Switch scene
    }
}
