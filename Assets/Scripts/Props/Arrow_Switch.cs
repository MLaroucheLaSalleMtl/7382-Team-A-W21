using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow_Switch : MonoBehaviour
{
    //Variables
    private bool open;          //used for multiple switch needed for doors
    [SerializeField] private bool multiple;
    [SerializeField] private float timer;
    private Animator anim;
    public bool Open { get => open; }           // used for the door script
    //Collision with arrow
    private void Start()
    {
        anim = GetComponent<Animator>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Projectile>() != null && !open) 
        {
            open = true;
            anim.SetBool("switch", true);
            GetComponent<AudioSource>().Play();
            if (multiple)
                Invoke("SettingBack", timer);
        }
    }
    private void SettingBack() 
    {
        anim.SetBool("switch", false);
        open = false;
    }
}
