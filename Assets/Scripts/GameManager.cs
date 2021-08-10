using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;  //Reference to the game manger
    public Player player;                       //Reference to the player character
    [SerializeField] private GameObject[] dontDestroy;  //Objects that persist between scenes
    private bool cooldown = false;              //If the action has been performed recently (On cooldown)
    private bool paused = false;                //If the game is paused
    private const float SHORT_DELAY = 0.2f;     //0.2 second delay upon activating certain UI screens
    private const float LONG_DELAY = 1f;        //1 second delay upon sign/chest activations

    //Values for displaying messages to the player (Images, text, buttons)
    [SerializeField] public GameObject hud;
    [SerializeField] private GameObject sign;
    [SerializeField] private GameObject note;
    [SerializeField] public GameObject obscure;
    [SerializeField] public GameObject mainMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject controlsMenu;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private Slider sfxSlide;
    [SerializeField] private Slider musicSlide;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private Button btn_applyOptions;
    [SerializeField] private Button btn_newGame;
    [SerializeField] private Button btn_mainMenu;
    [SerializeField] private Button btn_resume;
    [SerializeField] Text noteText;
    [SerializeField] Text menuText;
    public bool buttonPressed = false;

    //Variables for audio
    private AudioSource audioS;
    [SerializeField] AudioClip audioObtainItem;
    [SerializeField] AudioClip audioProgressText;

    //Make sure there's only one instance of GameManager
    private void Awake()
    {
        if (instance == null)   
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        foreach (GameObject gObject in dontDestroy)
            DontDestroyOnLoad(gObject);
    }

    private void Start()
    {
        StopPlayerMove();
        audioS = GetComponent<AudioSource>();
        if (optionsMenu && optionsMenu.GetComponent<optionsMenu>())
        {
            optionsMenu.GetComponent<optionsMenu>().mixer.SetFloat("MusicVol", PlayerPrefs.GetFloat("music", 0));
            optionsMenu.GetComponent<optionsMenu>().mixer.SetFloat("SFXVol", PlayerPrefs.GetFloat("sfx", 0));
        }
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
        DisableMenu(mainMenu);
        sfxSlide.value = PlayerPrefs.GetFloat("sfx", 0);
        musicSlide.value = PlayerPrefs.GetFloat("music", 0);
        EnableMenu(optionsMenu, btn_applyOptions);
    }

    //Returns to the main menu
    public void ReturnToMain()
    {
        DisableMenu(optionsMenu);
        EnableMenu(mainMenu, btn_newGame);
    }

    //Pauses and unpauses the game
    public void Pause()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.buildIndex > 3 && currentScene.buildIndex < 15 && player.hp > 0)
        {
            if (paused)
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
                str += "a shield! \nHold space or left bumper to reflect enemy projectiles. Reflecting projectiles drains stamina.";
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
        Invoke("EnableMenu", 3f);   //Wait for death animation to play
    }
 
    /////Helper Functions/////
    
    //For player death's invoke command
    private void EnableMenu()
    {
        EnableMenu(gameOverMenu, btn_mainMenu);
    }

    //Enables the specified menu with default button selected
    public void EnableMenu(GameObject menu, Button defaultBtn)
    {
        menu.SetActive(true);
        Button[] buttons = menu.GetComponentsInChildren<Button>();
        foreach (Button btn in buttons)             //Enable the buttons
            btn.enabled = true;
        EventSystem.current.SetSelectedGameObject(defaultBtn.gameObject, null);     //Select first button
        Cursor.lockState = CursorLockMode.None;     //Unlock and show cursor
        Cursor.visible = true;
        StartCoroutine("WaitForButton");
    }

    //Disables the specified menu
    public void DisableMenu(GameObject menu)
    {
        menu.SetActive(false);
        Button[] buttons = menu.GetComponentsInChildren<Button>();
        foreach (Button btn in buttons)                     //Disable all the buttons
            btn.enabled = false;
        EventSystem.current.SetSelectedGameObject(null);    //Reset currently selected button
        Cursor.lockState = CursorLockMode.Locked;           //Lock and hide cursor
        Cursor.visible = false;
    }

    //Show note on screen
    private void DisplayNote()
    {
        note.SetActive(true);
        StartCoroutine("WaitForKey");
    }

    //Displays the controls screen
    IEnumerator ControlsScreen()
    {
        bool unpressed = false;
        float delay = SHORT_DELAY;
        while (delay > 0 && Time.timeScale != 0)    //Wait 0.2 seconds
        {
            delay -= Time.deltaTime;
            yield return null;
        }
        while (true)                    //Wait for key press
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
        controlsMenu.SetActive(false);      //Hide controls menu
        audioS.clip = audioProgressText;    //Play sound effect
        audioS.Play();
        if (mainMenu)
            EnableMenu(mainMenu, btn_newGame);
        else
            EnableMenu(pauseMenu, btn_resume);
    }

    //Coroutine waits for any key/mouse/gamepad press
    private IEnumerator WaitForKey()
    {
        bool unpressed = false;
        StopPlayerMove();
        float delay = SHORT_DELAY;
        while (delay > 0)               
        {
            delay -= Time.deltaTime;
            yield return null;
        }
        while (true)                    //Wait for key press
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
        audioS.clip = audioProgressText;
        audioS.Play();
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
        float delay = LONG_DELAY;
        while (delay > 0)           //Wait 1 second
        {
            delay -= Time.deltaTime;
            yield return null;
        }
        cooldown = false;
    }

    //Wait for chest animation to play before showing note
    private IEnumerator ChestDelay()
    {
        float delay = LONG_DELAY;
        while (delay > 0)
        {
            delay -= Time.deltaTime;
            yield return null;
        }
        DisplayNote();
        audioS.clip = audioObtainItem;  //Play item obtained sound effect
        audioS.Play();
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

    //Play a sound effect when player clicks a button
    public void PlayButtonAudio()
    {
        audioS.clip = audioProgressText;
        audioS.Play();
    }
}
