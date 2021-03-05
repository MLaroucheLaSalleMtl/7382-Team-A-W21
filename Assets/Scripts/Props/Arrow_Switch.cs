using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow_Switch : MonoBehaviour
{
    //Variables
    private bool open;          //used for multiple switch needed for doors
    [SerializeField] private bool multiple;
    [SerializeField] private float timer;
    public bool Open { get => open; }           // used for the door script
    //Collision with arrow
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Projectile>() != null && !open) 
        {
            open = true;
            if (multiple)
                Invoke("SettingBack", timer);
        }
    }
    private void SettingBack() 
    {
        Debug.Log("switching back");
        open = false;
    }
}
