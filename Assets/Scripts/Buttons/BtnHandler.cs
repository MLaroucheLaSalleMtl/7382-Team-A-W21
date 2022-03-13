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

    //Button click behaviour 
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.selectedObject.GetComponent<Button>() != null)
        {
            if (manager)
                manager.PlayButtonAudio();  //Play button click sound effect
            GetComponent<Button>().onClick.Invoke();    //Trigger button press on click, not release
        }
        Input.ResetInputAxes();   //Prevents double selection  
    }

    //Puts focus on the button the mouse hovers
    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<Selectable>().Select();    
    }
}
