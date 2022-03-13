using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    // Start is called before the first frame update
    //this interaction is solely for unlocking items if you want to use pop up and sign inherit and overwrite it
    [SerializeField] private Vector3 size = new Vector3(2f,2f,2f);
    protected GameManager manager;
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
    //If this item is a sign post
    public virtual void Interaction() 
    {
        manager.ReadSign();
    }
}
