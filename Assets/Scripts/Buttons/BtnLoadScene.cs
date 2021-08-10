using UnityEngine;
using UnityEngine.SceneManagement;

public class BtnLoadScene : MonoBehaviour
{
    private AsyncOperation async;   //Data that's been loaded

    //Loads a new scene on button click
    public void ButtonLoadScene(int i)
    {
        if (async != null) return;                  //If there's already something loading, don't continue
        Time.timeScale = 1.0f;                      //Reset to default time scale
        Input.ResetInputAxes();                     //Avoid accidental selection on button release
        System.GC.Collect();                        //Clear memory of unused items
        async = SceneManager.LoadSceneAsync(i);     //Load specified scene
        async.allowSceneActivation = false;         //Wait to switch to next scene
    }
    void Update()
    {
        if (async != null && async.progress >= 0.9f)
        {
            if (GameManager.instance)    //If a game manager exists, destroy it
                GameManager.instance.RemoveManager();
            async.allowSceneActivation = true;
        }
    }
}

