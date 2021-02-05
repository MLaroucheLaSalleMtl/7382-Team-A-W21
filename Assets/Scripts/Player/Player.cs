using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : LivingEntity
{
    [Header("Movement")]
    [Range(1f,10f)] [SerializeField] private float moveSpeed = 1;
    [SerializeField] private Vector2 move = new Vector2();
    [Space] 
    private Rigidbody2D body;
    [Header("Interaction")]
    private bool interact = false;
    [SerializeField] private Vector2 interact_distance = new Vector2();
    [SerializeField] private LayerMask playerMask;
    //[SerializeField] private ContactFilter2D playerContact;
    [Space]
    [SerializeField] private bool attacking; // could probably use delegates and stuff

    //----------debug----------//

    //----------Input System----------//
    public void OnMove(InputAction.CallbackContext context) 
    {
        move = context.ReadValue<Vector2>();
    }
    public void OnInteract(InputAction.CallbackContext context) 
    {
        interact = context.performed;
    }
    //----------Move----------//
    private void Move()
    {
        body.MovePosition(body.position + move * moveSpeed * Time.fixedDeltaTime);
    }
    //----------Interatction----------//
    private void Interact() 
    {
        Collider2D hit;
        if (hit = Physics2D.OverlapBox(transform.position, interact_distance, 0f, playerMask))
        {
            // other class goes here...
            hit.GetComponent<Interactable>().Interaction();
        }
        else 
        {
            Debug.Log("no interaction...");
        }

    }
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate()
    {
        Move();
        if (interact) 
        {
            Interact();
        }
    }
}
