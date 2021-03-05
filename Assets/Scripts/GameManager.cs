using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public Player player;                       //Reference to the player character
    [SerializeField] private GameObject[] dontDestroy;  //Objects that persist between scenes
    private int dungeonsCleared = 0;
    private int itemsFound = 0;
    private bool cooldown = false;                //If the action has been performed recently (On cooldown)

    //Values for displaying messages to the player (Image, text, buttons)
    [SerializeField] private GameObject sign;
    [SerializeField] private GameObject note;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject controls;
    [SerializeField] private Button btn_mainMenu;
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
        itemsFound++;
        string str = "You found ";

        switch (itemsFound)
        {
            case 1:                 //Shield
                str += "a shield! \nHold left ctrl or left bumper to reflect enemy projectiles and defend against incoming damage.";
                break;
            case 2:                 //Bombs
                str += "bombs! \nPress the F key or Y button on your controller to take out a bomb and then press it again to throw the bomb. \nYou can pick bombs back up with the E key or A button.";
                break;
            case 3:                 //Bow
                str += "a bow! \nUse the scroll wheel or the right and left shoulders on your controller to switch between items. \nCharge up your shots to deal more damage!";
                break;
            case 4:                 //Magic wand
                str += "a magic wand! \nPress the F key or Y button to fire magic at obstacles and hit two targets at once. \nPress it again to split the magic bolt early.";
                break;
        }
        noteText.text = str;
        StartCoroutine("ChestDelay");
    }
    //When the player clears a dungeon
    public void DungeonCleared()
    {
        dungeonsCleared++;
        if (dungeonsCleared >= 3)   //If all dungeons have been cleared
            Victory();
        else
        {
            noteText.text = "You have cleared the dungeon! \n\nThe boss dropped a strange key, what could it unlock?";
            DisplayNote();
        }
    }
    //Displays menu when player clears all dungeons
    public void Victory()
    {
        menuText.text = "Victory!";
        EnableMenu();
    }
    //Displays menu when player dies
    public void GameOver()
    {
        menuText.text = "Defeat";
        Invoke("EnableMenu", 3f);
    }
    //Displays menu when player completes the demo
    public void DemoComplete()
    {
        menuText.text = "Demo Complete!";
        EnableMenu();
    }
    //Displays the keyboard and gamepad controls
    public void DisplayControls()
    {
        controls.SetActive(true);
        DisableMenu();
        StartCoroutine("ControlsScreen");
    }

    /////Helper Functions/////
    
    IEnumerator ControlsScreen()
    {
        bool unpressed = false;
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
        controls.SetActive(false);
        EnableMenu();
    }

    public void EnableMenu()
    {
        menu.SetActive(true);
        Button[] buttons = menu.GetComponentsInChildren<Button>();
        foreach (Button btn in buttons)
            btn.enabled = true;
        //Select first button (for keyboard/gamepad)
        EventSystem.current.SetSelectedGameObject(btn_mainMenu.gameObject, null);
        StartCoroutine("WaitForButton");
    }

    public void DisableMenu()
    {
        menu.SetActive(false);
        Button[] buttons = menu.GetComponentsInChildren<Button>();
        foreach (Button btn in buttons)
            btn.enabled = false;
        //Reset currently selected button
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void DisplayNote()
    {
        note.SetActive(true);
        StartCoroutine("WaitForKey");
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
    private void StopPlayerMove()
    {
        player.enabled = false;
        player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        player.gotoPoint = Vector2.zero;
    }
    //Enables the player's movement
    private void StartPlayerMove()
    {
        player.enabled = true;
        player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
    }
}
