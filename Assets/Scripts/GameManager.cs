using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public Player player;                   //Reference to the player character
    [SerializeField] private GameObject[] dontDestroy;  //Objects that persist between scenes
    private int dungeonsCleared = 0;
    private bool cooldown = false;          //If the action has been performed recently (On cooldown)

    //Values for pause and save features
    private bool paused = false;

    //Values for displaying messages to the player (Images, text, buttons)
    [SerializeField] public GameObject hud;
    [SerializeField] private GameObject sign;
    [SerializeField] private GameObject note;
    [SerializeField] public GameObject mainMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject controlsMenu;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private Button btn_applyOptions;
    [SerializeField] private Button btn_newGame;
    [SerializeField] private Button btn_mainMenu;
    [SerializeField] private Button btn_resume;
    [SerializeField] Text noteText;
    [SerializeField] Text menuText;
    public bool buttonPressed = false;

    //Make sure there's only one instance of GameManager
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        foreach (GameObject gObject in dontDestroy)
        {
            DontDestroyOnLoad(gObject);
        }
    }
    private void Start()
    {
        StopPlayerMove();
    }

    /////Menu Functions/////

    //Displays the keyboard and gamepad controls
    public void DisplayControls()
    {
        controlsMenu.SetActive(true);
        if(mainMenu)
            DisableMenu(mainMenu);
        DisableMenu(pauseMenu);
        StartCoroutine("ControlsScreen");
    }

    //Display options menu
    public void DisplayOptions()
    {
        EnableMenu(optionsMenu, btn_applyOptions);
        if (mainMenu)
            DisableMenu(mainMenu,false);
        DisableMenu(pauseMenu,false);
    }

    public void ReturnToMain()
    {
        EnableMenu(mainMenu, btn_newGame);
    }

    //Pause game
    public void Pause()
    {
        if(paused)
        {
            Time.timeScale = 1f;                        //Unpause time
            paused = false;
            Cursor.lockState = CursorLockMode.Locked;   //Lock and hide cursor
            Cursor.visible = false;
            DisableMenu(pauseMenu);
            StartPlayerMove();
        }
        else
        {
            Time.timeScale = 0f;                    //Pause time
            paused = true;
            Cursor.lockState = CursorLockMode.None; //Unlock and show cursor
            Cursor.visible = true;
            EnableMenu(pauseMenu, btn_resume);
        }
    }

    /////UI Functions/////

    //When the player reads the signpost in HubWorld
    public void ReadSign()
    {
        if (!cooldown)
        {
            sign.SetActive(true);
            StartCoroutine("WaitForKey");
        }
    }
    //When the player finds a new item
    public void ItemFound()
    {
        Debug.Log("Item found");
        StopPlayerMove();
        
        string str = "You found ";

        switch (player.weaponLock)
        {
            case 1:                 //Shield
                str += "a shield! \nHold left ctrl or left bumper to reflect enemy projectiles and defend against incoming damage.";
                break;
            case 2:                 //Bombs
                str += "bombs! \nPress the right mouse button or Y button on your controller to take out a bomb and then press it again to throw the bomb. \nYou can pick bombs back up with the E key or A button.";
                break;
            case 3:                 //Bow
                str += "a bow! \nUse the scroll wheel or the right and left shoulders on your controller to switch between items. \nCharge up your shots to deal more damage!";
                break;
            case 4:                 //Magic wand
                str += "a magic wand! \nPress the right mouse button or Y button to fire magic at obstacles and hit two targets at once. \nPress it again to split the magic bolt early.";
                break;
        }
        noteText.text = str;
        StartCoroutine("ChestDelay");
    }
    //When the player clears a dungeon
    public void DungeonCleared()
    {
        noteText.text = "You have cleared the dungeon! \n\nThe boss dropped a key, what could it unlock?";
        DisplayNote();
    }
    //Displays menu when player dies
    public void GameOver()
    {
        menuText.text = "Defeat";
        Invoke("EnableMenu", 3f);
    }
 
    /////Helper Functions/////
    
    //For player death's invoke command
    private void EnableMenu()
    {
        EnableMenu(gameOverMenu, btn_mainMenu);
    }
    public void EnableMenu(GameObject menu, Button defaultBtn)
    {
        menu.SetActive(true);
        Button[] buttons = menu.GetComponentsInChildren<Button>();
        foreach (Button btn in buttons)
            btn.enabled = true;
        //Select first button (for keyboard/gamepad)
        EventSystem.current.SetSelectedGameObject(defaultBtn.gameObject, null);
        Cursor.lockState = CursorLockMode.None;   //Unlock and show cursor
        Cursor.visible = true;
        StartCoroutine("WaitForButton");
    }

    public void DisableMenu(GameObject menu, bool cursorOff = true)
    {
        menu.SetActive(false);
        Button[] buttons = menu.GetComponentsInChildren<Button>();
        foreach (Button btn in buttons)
            btn.enabled = false;
        //Reset currently selected button
        EventSystem.current.SetSelectedGameObject(null);
        if (cursorOff == true)
        {
            Cursor.lockState = CursorLockMode.Locked;   //Lock and hide cursor
            Cursor.visible = false;
        }
    }

    private void DisplayNote()
    {
        note.SetActive(true);
        StartCoroutine("WaitForKey");
    }

    IEnumerator ControlsScreen()
    {
        bool unpressed = false;
        float delay = 0.2f;
        //Wait 0.2 seconds
        while (delay > 0 && Time.timeScale != 0)
        {
            delay -= Time.deltaTime;
            yield return null;
        }
        //Wait for key press
        while (true)
        {
            if (!Input.anyKeyDown)      //Make sure key press doesn't trigger twice in one frame
            {
                unpressed = true;
                yield return null;
            }
            else if (unpressed)
                break;
            else
                yield return null;
        }
        controlsMenu.SetActive(false);
        if(mainMenu)
            EnableMenu(mainMenu, btn_newGame);
        else
            EnableMenu(pauseMenu, btn_resume);
    }

    //Coroutine waits for any key/mouse/gamepad press
    private IEnumerator WaitForKey()
    {
        bool unpressed = false;
        StopPlayerMove();
        float delay = 0.5f;
        //Wait 0.5 seconds
        while (delay > 0)
        {
            delay -= Time.deltaTime;
            yield return null;
        }
        //Wait for key press
        while (true)
        {
            if (!Input.anyKeyDown)      //Make sure key press doesn't trigger twice in one frame
            {
                unpressed = true;
                yield return null;
            }
            else if (unpressed)
                break;
            else
                yield return null;
        }
        sign.SetActive(false);
        note.SetActive(false);
        StartCoroutine("SignCooldown");
        StartPlayerMove();
    }
    //Coroutine waits for a menu button to be pressed on screen
    private IEnumerator WaitForButton()
    {
        if (player)
            StopPlayerMove();
        while (!buttonPressed)
            yield return null;
        if (player)
            StartPlayerMove();
        buttonPressed = false;
    }
    //Prevents the player from accessing the sign again immediately
    private IEnumerator SignCooldown()
    {
        cooldown = true;
        float delay = 1f;
        while (delay > 0)
        {
            delay -= Time.deltaTime;
            yield return null;
        }
        cooldown = false;
    }
    //Wait for chest animation to play before showing note
    private IEnumerator ChestDelay()
    {
        float delay = 1f;
        while (delay > 0)
        {
            delay -= Time.deltaTime;
            yield return null;
        }
        DisplayNote();
    }
    //Destroys the game manager and its children
    public void RemoveManager()
    {
        Destroy(gameObject);
    }
    //Disables the player's movement
    public void StopPlayerMove()
    {
        player.enabled = false;
        player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        player.gotoPoint = Vector2.zero;
    }
    //Enables the player's movement
    public void StartPlayerMove()
    {
        player.enabled = true;
        player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
    }
}
