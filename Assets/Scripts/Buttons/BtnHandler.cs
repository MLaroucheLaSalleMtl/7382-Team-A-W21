using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BtnHandler : MonoBehaviour, IPointerEnterHandler, IDeselectHandler, IPointerDownHandler
{
    private GameManager manager;

    private void Start()
    {
        manager = GameManager.instance;
    }

    //Deselect previous button when I use keyboard
    public void OnDeselect(BaseEventData eventData)
    {
        GetComponent<Selectable>().OnPointerExit(null); 
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.selectedObject.GetComponent<Button>() != null)
        {
            if (manager)
                manager.PlayButtonAudio();
            GetComponent<Button>().onClick.Invoke();    //Trigger button press on click, not release
        }
        Input.ResetInputAxes();     //Avoid double selection
    }

    //Puts focus on the button the mouse hovers
    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<Selectable>().Select();    
    }
}
