using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BtnHandler : MonoBehaviour, IPointerEnterHandler, IDeselectHandler, IPointerDownHandler
{
    public void OnDeselect(BaseEventData eventData)
    {
        GetComponent<Selectable>().OnPointerExit(null); //When I use keyboard, deselect previous button
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.selectedObject.GetComponent<Button>() != null)
            GetComponent<Button>().onClick.Invoke();    //Trigger button press on click, not release
        Input.ResetInputAxes();     //Avoid double selection
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<Selectable>().Select();    //Puts focus on the button the mouse hovers
    }
}
