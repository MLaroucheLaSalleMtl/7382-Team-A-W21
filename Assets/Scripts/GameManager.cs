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
        itemsFound++;
        string str = "You found ";

        switch (itemsFound)
        {
            case 1:                 //Shield
                str += "a shield! \nHold left ctrl or left bumper to reflect enemy projectiles.";
                break;
            case 2:                 //Bombs
                str += "bombs! \nPress the _ button to use a bomb and _ to throw it.";
                break;
            case 3:                 //Bow
                str += "a bow and arrows! \nPress _ to switch between items, and _ to fire arrows at your enemies.";
                break;
            case 4:                 //Magic wand
                str += "a magic wand! \nPress _ to fire magic at obstacles and hit two targets at once.";
                break;
        }
        noteText.text = str;
        player.enabled = false;
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

    public void Victory()
    {
        menuText.text = "Victory!";
        EnableMenu();
    }

    public void GameOver()
    {
        menuText.text = "Defeat";
        Invoke("EnableMenu", 3f);
    }

    public void DemoComplete()
    {
        menuText.text = "Demo Complete!";
        EnableMenu();
    }

    /////Helper Functions/////
    private void EnableMenu()
    {
        menu.SetActive(true);
        Button[] buttons = menu.GetComponentsInChildren<Button>();
        foreach (Button btn in buttons)
            btn.enabled = true;
        //Select first button (for keyboard/gamepad)
        EventSystem.current.SetSelectedGameObject(btn_mainMenu.gameObject, null);
        StartCoroutine("WaitForButton");
    }

    private void DisableMenu()
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
        player.enabled = false;
        float delay = 1f;
        //Wait 1 second
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
        player.enabled = true;
    }

    //Coroutine waits for a menu button to be pressed on screen
    private IEnumerator WaitForButton()
    {
        player.enabled = false;
        while (!buttonPressed)
            yield return null;
        player.enabled = true;
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
}
