using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    // Start is called before the first frame update
    //this interaction is solely for unlocking items if you want to use pop up and sign inherit and overwrite it
    [SerializeField] private Vector3 size = new Vector3(2f,2f,2f);
    private GameManager manager;
    CapsuleCollider2D open;
    private void Start()
    {
        manager = GameManager.instance;
        open = GetComponent<CapsuleCollider2D>();

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, size);
    }
    public virtual void Interaction() 
    {
        if (gameObject.CompareTag("Chest"))
            manager.player.InteractionAdd();
        //play animation?

        //also note remove colliders thx
        open.enabled = false;
    }
}
