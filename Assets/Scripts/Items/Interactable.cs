using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    // Start is called before the first frame update
    //this interaction is solely for unlocking items if you want to use pop up and sign inherit and overwrite it
    [SerializeField] private Vector3 size = new Vector3(2f,2f,2f);
    private GameManager manager;
    private CapsuleCollider2D open;
    protected Animator anim;
    protected AudioSource audioS;

    private void Awake()
    {
        open = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();
        audioS = GetComponent<AudioSource>();
    }
    private void Start()
    {
        manager = GameManager.instance;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, size);
    }
    public virtual void Interaction() 
    {
        //If this item is a chest
        if (gameObject.CompareTag("Chest"))
        {
            manager.player.InteractionAdd();
            anim.SetTrigger("Open");                //Play chest opening animation
            GetComponent<AudioSource>().Play();     //Play chest opening sound effect
            this.gameObject.layer = LayerMask.NameToLayer("Default");   //Player can no longer interact with open chest
            manager.ItemFound();
        }
        //If this item is a sign post
        else
            manager.ReadSign();
    }
}
