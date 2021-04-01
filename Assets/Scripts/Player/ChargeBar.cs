using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargeBar : MonoBehaviour
{
    [SerializeField] private Image filledImage;
    void Start()
    {
        filledImage = GetComponent<Image>();
        ToggleCharge();
    }

    public void ToggleCharge() 
    {
        if (transform.parent.parent.parent.gameObject.activeSelf)
            transform.parent.parent.parent.gameObject.SetActive(false);
        else 
            transform.parent.parent.parent.gameObject.SetActive(true);
    }

    public void SetValue(float amount ,float max) 
    {
        filledImage.fillAmount = amount / max;
    }
}
