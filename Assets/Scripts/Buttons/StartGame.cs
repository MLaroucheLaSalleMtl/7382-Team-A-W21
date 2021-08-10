using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    private GameManager manager;
    private AsyncOperation async;   //Data that's been loaded
    private int sceneToLoad = 3;    //Build index of the scene we're loading

    void Start()
    {
        manager = GameManager.instance;             //Cache game manager
        Cursor.lockState = CursorLockMode.None;     //Unlock and show the cursor
        Cursor.visible = true;
    }

    //Starts a new game
    public void StartNewGame()
    {
        if (async != null) return;  //If there's already something loading, don't continue
        PrepStart();
        async = SceneManager.LoadSceneAsync(3);     //Load hub world
        async.allowSceneActivation = false;         //Wait to switch to next scene
    }

    //Loads the game from player's last checkpoint
    public void ContinueSavedGame()
    {
        if (async != null) return;  //If there's already something loading, don't continue
        PrepStart();
        sceneToLoad = PlayerPrefs.GetInt("Level", 3);       //Read save game data from the registry
        async = SceneManager.LoadSceneAsync(sceneToLoad);   //Load saved scene
        async.allowSceneActivation = false;                 //Wait to switch to next scene
    }

    //Adds items to the player based on the scene they're entering
    void AddItems()
    {
        int check = PlayerPrefs.GetInt("Checkpoint", 0);
        if (sceneToLoad > 6 || (sceneToLoad == 5 && check > 0))     //First dungeon - Add shield
            manager.player.InteractionAdd();
        if (sceneToLoad > 10 || (sceneToLoad == 9 && check > 0))    //Second dungeon - Add bombs
            manager.player.InteractionAdd();
        if (sceneToLoad == 13 && check > 0)                         //Third dungeon - Add bow and wand
        {
            manager.player.InteractionAdd();
            manager.player.InteractionAdd();
        }
    }

    //Clean up memory before loading next scene
    private void PrepStart()
    {
        Input.ResetInputAxes();     //Avoid accidental selection on button release
        System.GC.Collect();        //Clear memory of unused items
    }

    void Update()
    {
        if (async != null && async.progress >= 0.9f)
        {
            AddItems();
            manager.DisableMenu(manager.mainMenu);
            Cursor.lockState = CursorLockMode.Locked;   //Lock and hide cursor
            Cursor.visible = false;
            manager.hud.SetActive(true);
            Destroy(manager.mainMenu);      //Destroy the main menu once we're done with it
            async.allowSceneActivation = true;
        }
    }
}
