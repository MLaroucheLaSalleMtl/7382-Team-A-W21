using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : LivingEntity
{
    [Header("Layers")]
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask levelMask;

    [Header("Movement")]
    [Range(10f,500f)] [SerializeField] private float moveSpeed = 250;
    [Range(10f, 1500f)] [SerializeField] private float dashSpeed = 1000;
    [SerializeField] private float dashTime = 0.25f;
    [SerializeField] private float stunTime = 1f;
    [Space]
    [SerializeField][Tooltip("Doesn't make a huge difference if everything is on 45 degree increments but just incase!")] private float wallAngleThreshold = 120;

    [Header("Stats")]
    public int maxHealth = 10;
    public int maxStamina = 20;
    public int stamina = 20;
    public int dashCost = 5;
    [Tooltip("How long before regend")] public float regenTime = 3f;
    [Tooltip("How long between each point regenerated")] public float regenSpeed = 1f;

    [Header("Other")]
    [SerializeField] private GameObject Canvas;
    [SerializeField] private Vector2 interact_distance = new Vector2();

    //Internals
    private float sinceLastDrain = 3f;
    private UIBehaviour UIScript;
    private bool interact = false;
    private bool attacking; // could probably use delegates and stuff
    private const float timer = 0.5f;
    private IEnumerator attack_cooldown;
    private bool dashing;
    private ContactFilter2D filter2D;


    //----------debug----------//

    //----------Input System----------//
    public void OnMove(InputAction.CallbackContext context) 
    {

        this.gotoPoint = context.ReadValue<Vector2>();
    }
    public void OnDash(InputAction.CallbackContext context)
    {

        this.dashing = context.performed;
    }
    public void OnInteract(InputAction.CallbackContext context) 
    {
        interact = context.performed;
    }
    public void OnAttack(InputAction.CallbackContext context) 
    {
        attacking = context.performed;
    }
    //----------Move----------//
    private void Move()
    {
        this.body.velocity = (this.gotoPoint.normalized * moveSpeed) * Time.fixedDeltaTime;
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
        interact = false;
    }
    //----------Walking Animation----------//
    private void Animate_Direction()
    {
        if (!interrupt)
        {
            Move();
            if (this.gotoPoint != Vector2.zero)
            {
                this.anim.SetFloat("Hor", this.gotoPoint.x);
                this.anim.SetFloat("Ver", this.gotoPoint.y);
                this.anim.SetBool("Mov", true);
            }
            else
            {
                this.anim.SetBool("Mov", false);
            }
        }
    }

    //----------Custom Methods----------//

    private void Regen()
    {
        if (sinceLastDrain >= regenTime && stamina < maxStamina)
        {
            stamina++;
            UIScript.SetBar(false, stamina, maxStamina);  //Update UI
        }
    }

    //----------Built In Function----------//
    void Start()
    {
        base.Start();
        this.body = GetComponent<Rigidbody2D>();
        //Set Hp, animation and other things from Living_Entity...
        this.anim = GetComponent<Animator>();
        UIScript = Canvas.GetComponent<UIBehaviour>();
        filter2D = new ContactFilter2D();
        filter2D.layerMask = levelMask;
        filter2D.useLayerMask = true;
        InvokeRepeating("Regen", 0, regenSpeed);
    }
    void FixedUpdate()
    {
        if (attacking)
        {
            if (attack_cooldown == null)
            {
                attack_cooldown = Attack_Cooldown();
                StartCoroutine(attack_cooldown);
            }
        }
        else
            Animate_Direction();
        if (interact) 
        {
            Interact();
        }
        if (dashing)
        {
            if ( interrupt || stamina < dashCost || gotoPoint.magnitude == 0)
            {
                dashing = false;
            }
            else
            {
                stamina -= dashCost;
                UIScript.SetBar(false, stamina, maxStamina);  //Update UI
                StartCoroutine(Dash(dashTime));
            }
        }
        if (sinceLastDrain < regenTime && !dashing)
        {
            sinceLastDrain += Time.fixedDeltaTime;
        }

        //Update dash light
        UIScript.SetDashLight(!dashing && stamina >= dashCost && !interrupt); //Takes a bool (hence all the comparisons)

    }

    //----------Inherited----------//

    public override void Hurt(int damage,Transform hitting)
    {
        if (!this.invincible && this.hp > 0 && !dashing) 
        {
            base.Hurt(damage, hitting);
            StartCoroutine(invFrames(1));
            UIScript.SetBar(true, this.hp, maxHealth);
        }
    }
    protected override void Death()
    {
        base.Death();
        Debug.Log("Game Over");
        //Show Game Over screen
    }

    //----------Coroutines----------//
    private IEnumerator Attack_Cooldown() 
    {
        while (attacking) 
        {
            interrupt = true;
            Attack();
            body.velocity = Vector2.zero;
            yield return new WaitForSeconds(timer);
        }
        attack_cooldown = null;
        interrupt = false;

    }
    private IEnumerator Dash(float dashTime)
    {
        
        interrupt = true;
        invincible = true;

        sinceLastDrain = 0f;
        Vector2 dashDir = gotoPoint.normalized; //So that controls don't affect the dash once it's started
        float count = 0;
        bool hasHitWall = false; //to remember if we've bumped a wall too hard
        List<ContactPoint2D> contacts; //to handle the impacts

        while (count < dashTime)
        {
            
            //Flicker waaaaay faster during dash
            if (invincible)
            {
                base.myRenderer.enabled = !base.myRenderer.enabled;
            }
            else
            {
                base.myRenderer.enabled = true;
            }
            
            //Erase previous impacts
            contacts = new List<ContactPoint2D>();

            //If we've hit at least one wall
            if (body.GetContacts(filter2D,contacts) >= 1)
            {
                Vector2 avg = Vector2.zero; //Used to get average collision point (used to bounce if not a 'perpendicular enough' impact)
                foreach(ContactPoint2D Point in contacts)
                {
                    //Check if angle is within the stun range
                    if (Mathf.Abs(Vector2.Angle(dashDir, Point.normal) - 180) <= wallAngleThreshold/2)
                    {
                        //HERE IS WHERE YOU WOULD CALL GETTING
                        dashTime = stunTime;
                        count = 0f;
                        hasHitWall = true;
                        this.body.velocity = Vector2.zero;
                        invincible = false; //We aren't invisible once we've stopped moving
                    }
                    avg += (Point.point - (Vector2)this.transform.position);
                }
                avg /= contacts.Count;
                dashDir = -avg.normalized; //Otherwise bounce (Using average of all collision points)
            }

            //If we've been stunned then don't move
            if (!hasHitWall)
            {
                this.body.velocity = (dashDir * dashSpeed) * Time.fixedDeltaTime;
            }

            yield return new WaitForEndOfFrame();
            count += Time.deltaTime;
        }

        //Everything returns to normal
        base.myRenderer.enabled = true;
        invincible = false;
        interrupt = false;
        dashing = false;

    }
}
