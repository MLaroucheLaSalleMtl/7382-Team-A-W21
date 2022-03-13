using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CutsceneDialogue : MonoBehaviour
{
    GameManager manager;
    private const int DIALOGUE_OPTIONS = 8;
    private int currentText = 0;
    [SerializeField] private Text nameplateText;
    [SerializeField] private Text dialogueText;
    [SerializeField] private GameObject victoryMenu;
    [SerializeField] private Button defaultBtn;

    private void Start()
    {
        if (manager = GameManager.instance)
            if (manager.player)
                manager.StopPlayerMove();   //Disable player movement during cutscene
        Cursor.lockState = CursorLockMode.Locked;   //Unlock and show cursor
        Cursor.visible = false;
    }

    //Prints next line of cutscene dialogue on screen
    public void ProgressDialogue()
    {
        switch(currentText)
        {
            case 0:
                nameplateText.text = "Adlez";
                dialogueText.text = "You came to save me!";
                nameplateText.gameObject.SetActive(true);
                dialogueText.gameObject.SetActive(true);
                break;
            case 1:
                nameplateText.text = "Hero";
                dialogueText.text = "Of course, my love!";
                break;
            case 2:
                nameplateText.text = "Adlez";
                dialogueText.text = "Wait, what?";
                break;
            case 3:
                nameplateText.text = "Hero";
                dialogueText.text = "Huh?";
                break;
            case 4:
                nameplateText.text = "Adlez";
                dialogueText.text = "Uh, thanks for saving me but...";
                break;
            case 5:
                dialogueText.text = "I only see you as a friend.";
                break;
            case 6:
                nameplateText.text = "Hero";
                dialogueText.text = "...";
                break;
            case 7:
                dialogueText.text = "Well this is awkward.";
                break;
            default:
                EnableMenu();   //Enable end game menu
                break;
        }
        GetComponent<AudioSource>().Play();     //Play sound effect
        if (currentText < DIALOGUE_OPTIONS)     //If there's more dialogue to print
        {
            currentText++;
            StartCoroutine(WaitForKey());       
        }
    }

    //Coroutine waits for any key/mouse/gamepad press
    private IEnumerator WaitForKey()
    {
        bool unpressed = false;
        float delay = 0.3f;
        while (delay > 0)   //Wait 0.3 seconds
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
        ProgressDialogue();
    }

    //Enables the end game menu
    public void EnableMenu()
    {
        victoryMenu.SetActive(true);
        Button[] buttons = victoryMenu.GetComponentsInChildren<Button>();
        foreach (Button btn in buttons)     //Enable the buttons
            btn.enabled = true;
        EventSystem.current.SetSelectedGameObject(defaultBtn.gameObject, null);     //Select first button 
        Cursor.lockState = CursorLockMode.None;     //Unlock and show cursor
        Cursor.visible = true;
    }
}
