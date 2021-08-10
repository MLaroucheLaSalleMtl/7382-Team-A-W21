using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public enum WeaponSlot
{
    None = 0,
    Shield = 1,
    Bomb = 2,
    Bow = 3,
    Mystery = 4
}
[RequireComponent(typeof(Rigidbody2D))]
public class Player : LivingEntity
{
    [Header("Layers")]
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask levelMask;
    [SerializeField] private LayerMask pickMask; // without this then overlay will detect player instead of pickup

    [Header("Movement")]
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
    public int shieldCost = 5;
    private readonly Vector2 originalSize = new Vector2(0.525f, 0.86f);
    private CapsuleCollider2D bodySize; // used for bombs
    [Tooltip("How long before regend")] public float regenTime = 1.5f;
    [Tooltip("How long between each point regenerated")] public float regenSpeed = 0.5f;

    [Header("Other")]
    [SerializeField] public Canvas canvas;
    [SerializeField] private Vector2 interact_distance = new Vector2();

    //Internals
    private float sinceLastDrain = 3f;
    private bool interact = false;
    private bool attacking; // could probably use delegates and stuff
    private IEnumerator attack_cooldown;
    private bool dashing;
    private ContactFilter2D filter2D;

    public UIBehaviour UIScript;

    //Defend variables
    public GameObject shield;
    public Sprite[] shieldSprites = new Sprite[4];
    private float originalSpeed;

    //Sword box
    public GameObject sword;

    //Secondary Weapon
    public int weaponLock = 0;
    private WeaponSlot slot;
    private bool sAttack = false;
    private bool attack_cancel = false; //primary used for bow (when charging it will not let player attack)
    private bool pickup = false;
    private bool itemCooldown = false;
    private const float ITEM_CD_TIMER = 1f;

    //Bomb
    [SerializeField] private GameObject prefab_bomb;
    [SerializeField] private Transform bomb_spawn;
    public const float tossRange = 5f;
    private GameObject clone_throw; // used for both bombs and throwable stuff(pots rocks etc...)
    public delegate void SecondaryAttack();
    private SecondaryAttack secondaryAttack;
    public SecondaryAttack SecondaryAttack1 { get => secondaryAttack; set => secondaryAttack = value; }
    //Bow
    [SerializeField] private ChargeBar bowChargeImage;
    [SerializeField] private GameObject prefab_arrow;
    [SerializeField] private int bowDamage;
    [SerializeField] private float bowSpeed; // multiplier
    private const float timerCap = 3;
    private IEnumerator bowCharge;
    private const int baseBowDamage = 1;
    private const float baseBowSpeed = 2;
    //MysteryItem
    [SerializeField] private GameObject prefab_split;
    [SerializeField] private int mysteryDamage; // changeable in editor
    private Vector2 chacheCloneShot;
    private GameObject clone_shot;
    private const float mysterySpeed = 20f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem stunnedSparkles;
    [SerializeField] private ParticleSystem dashPuffs;

    [Header("Audio")]
    [SerializeField] private AudioSource throwNoise;
    [SerializeField] private AudioSource bowChargeNoise;
    //----------debug----------//

    //----------Input System----------//
    public void OnMove(InputAction.CallbackContext context)
    {

        this.gotoPoint = context.ReadValue<Vector2>();
    }
    //***This function was written by Simon
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started) this.dashing = true;
    }
    //***This function was written by Yan
    public void OnInteract(InputAction.CallbackContext context) 
    {
        interact = context.performed;
    }
    //***This function was written by Yan
    public void OnAttack(InputAction.CallbackContext context) 
    {
        if(!attack_cancel)
            attacking = context.performed;
    }
    //***This function was written by Nicky
    public void OnDefend(InputAction.CallbackContext context)
    {
        //If player has shield and isn't attacking or throwing a bomb
        if (slot > 0  && attack_cooldown == null && (clone_throw == null || clone_throw.transform.parent == null))
        {
            if (context.started)
                RaiseShield();
            if (context.canceled)
                LowerShield();
        }
    }
    //***This function was written by Nicky
    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.started)
            manager.Pause();
    }
    
    //***This function was written by Yan
    public void OnSecondary_Attack(InputAction.CallbackContext context) 
    {
        if(!anim.GetBool("Stunned"))
            switch (slot)
            {
                case WeaponSlot.Bow:
                    if (!shield.activeSelf && attack_cooldown == null && !itemCooldown)
                    {
                        if (context.started)
                            ChargeBow();
                        if (context.canceled && secondaryAttack == ChargeBow)
                            ShootBow();
                    }
                    break;
                case WeaponSlot.Bomb:
                case WeaponSlot.Mystery:
                    sAttack = context.performed;
                    break;
            }
    }
    //***This function was written by Yan
    public void OnPickup(InputAction.CallbackContext context) 
    {
        pickup = context.performed;
    }
    //***This function was written by Yan
    //----------Swap----------//
    public void OnWeaponSwap(InputAction.CallbackContext context) 
    {
        float input = context.ReadValue<float>();
        if (weaponLock > 1 && !attack_cancel && bomb_spawn.childCount < 1)
            if (input > 0)
                slot = (slot > WeaponSlot.Bomb) ? --slot : (WeaponSlot)weaponLock;
            else if (input < 0)
                slot = ((int)slot < weaponLock) ? ++slot : WeaponSlot.Bomb;
        UIScript.UpdateIcons(this.weaponLock);
        UIScript.UpdateSlots(slot);
    }
    //----------Move----------//
    //***This function was written by Yan
    private void Move()
    {
        this.body.velocity = (this.gotoPoint.normalized * moveSpeed) * Time.fixedDeltaTime;
    }
    //----------Interatction----------//
    //***This function was written by Yan
    private void Interact() 
    {
        Vector2 orientation;
        SetPosition(out orientation, out _, interact_distance.x);
        Collider2D hit;
        if (hit = Physics2D.OverlapBox(orientation, interact_distance, 0f, playerMask))
        {
            //add something here to stop the player so they don't move while interacting with objects?
            // other class goes here...
            hit.GetComponent<Interactable>().Interaction();
        }
        interact = false;
    }
    //----------Walking Animation----------//
    //***This function was written by Yan
    private void Animate_Direction()
    {
        if (!interrupt)
        {
            Move();
            Rotate(gotoPoint);
            if (this.gotoPoint != Vector2.zero)
            {
                this.anim.SetFloat("Hor", this.gotoPoint.x);
                this.anim.SetFloat("Ver", this.gotoPoint.y);
                this.anim.SetBool("Mov", true);
            }
            else
                this.anim.SetBool("Mov", false);
        }
    }

    //----------Custom Methods----------//

    private void Regen()
    {
        if (sinceLastDrain >= regenTime && stamina < maxStamina)
        {
            stamina++;
            UIScript.SetBar(UIBars.Stamina, stamina, maxStamina);  //Update UI
        }
    }

    //----------Built In Function----------//
    new void Start()
    {
        base.Start();
        this.body = GetComponent<Rigidbody2D>();
        //Set Hp, animation and other things from Living_Entity...
        //Set Hp, animation and other things from Living_Entity...
        UIScript = canvas.GetComponent<UIBehaviour>();
        filter2D = new ContactFilter2D();
        filter2D.layerMask = levelMask;
        filter2D.useLayerMask = true;
        InvokeRepeating("Regen", 0, regenSpeed);
        this.moveSpeed = 250;
        this.offsetMagnitude = 0.4f; //Offset for the shield
        originalSpeed = moveSpeed;
        bodySize = GetComponent<CapsuleCollider2D>();
    }

    //Activate player's shield                                  ***This function was written by Nicky
    void RaiseShield()
    {
        shield.SetActive(true);
        hitBoxOffset = CalculateOffset(this.offsetMagnitude);
        shield.transform.localPosition = hitBoxOffset;
        moveSpeed = moveSpeed / 2;

        //Handle visuals                                        ***The rest of this function was written by Simon
        SpriteRenderer renderer = shield.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = (GetComponent<Renderer>().sortingOrder + 1);
            switch (direction)
            {
                case Direction.UP:
                    renderer.sprite = shieldSprites[3];
                    renderer.sortingOrder = (GetComponent<Renderer>().sortingOrder - 1);
                    break;
                case Direction.RIGHT:
                    renderer.sprite = shieldSprites[2];
                    break;
                default:
                case Direction.DOWN:
                    renderer.sprite = shieldSprites[0];
                    break;
                case Direction.LEFT:
                    renderer.sprite = shieldSprites[1];
                    break;
            }
        }
    }

    //Deactivate player's shield                                  ***This function was written by Nicky
    void LowerShield()
    {
        shield.SetActive(false);
        shield.transform.localPosition = new Vector3(0, 0, 0);
        moveSpeed = originalSpeed;
    }

    //***This was worked on by Yan and Simon
    void FixedUpdate()
    {
        if (attacking && !attack_cancel)
            Attack();

        if (interact) 
            if (!interrupt)
                Interact();
            else
                interact = false;

        if (dashing)
            if ( interrupt || stamina < dashCost || gotoPoint.magnitude == 0)
                dashing = false;
            else
            {
                stamina -= dashCost;
                UIScript.SetBar(UIBars.Stamina, stamina, maxStamina);  //Update UI
                StartCoroutine(Dash(dashTime));
            }
        if (sinceLastDrain < regenTime && !dashing)
            sinceLastDrain += Time.fixedDeltaTime;
        //Update dash light
        UIScript.SetDashLight(!dashing && stamina >= dashCost && !interrupt); //Takes a bool (hence all the comparisons)
        //move character
        Animate_Direction();
        //Check secondary attack
        if (sAttack && !shield.activeSelf && !itemCooldown)
            Current_SAttack();
        if (pickup && !shield.activeSelf) 
            PickUp();
        //check if you can shield...
        if (stamina < shieldCost)
            LowerShield();
    }
    //----------Secondary Attack----------//
    //***This function was written by Yan
    public void Current_SAttack() 
    {
        switch (slot) 
        {
            case WeaponSlot.Bomb:
                if (secondaryAttack == null)
                    secondaryAttack = Spawn_Bomb;
                break;
            case WeaponSlot.Mystery:
                if (secondaryAttack == null)
                    secondaryAttack = Mystery;
                break;
        }
        if(slot != WeaponSlot.Bow)
            secondaryAttack();
    }
    //----------Interaction Add----------//
    public void InteractionAdd() 
    {
        weaponLock++;
        slot = (WeaponSlot) weaponLock;
        if(UIScript)
            UIScript.UpdateIcons(weaponLock);
    }
    //----------Bomb----------//
    //***This function was written by Yan
    public void Spawn_Bomb() 
    {
        //Change body size
        bodySize.offset = new Vector2(0f, 0.18f);
        bodySize.size = new Vector2(0.525f, 1.25f);
        //check if the player attacked
        clone_throw = Instantiate(prefab_bomb, bomb_spawn);
        secondaryAttack = Throw_Bomb;
        sAttack = false;
        
    }

    //***This function was written by Yan
    public void Throw_Bomb() 
    {
        throwNoise.Play();
        //create distance between bomb spawn and location
        SetPosition(out Vector2 end, out _, tossRange);
        clone_throw.transform.parent = null;
        StartCoroutine(clone_throw.GetComponent<Bombs>().Tossed(bomb_spawn, end));
        secondaryAttack = null;
        sAttack = false;
        StartCoroutine(ItemCooldown());
        bodySize.offset = Vector2.zero;
        bodySize.size = originalSize;
    }
    //----------Pick Up----------//
    //***This function was written by Yan
    public void PickUp() 
    {
        //check if player is attacking, and check if bomb spawn is emtpy
        if (!attacking && attack_cooldown == null && bomb_spawn.childCount <= 0)
        {
            Vector2 orientation;
            Vector2 size = new Vector2(offsetMagnitude, offsetMagnitude);
            SetPosition(out orientation, out _, offsetMagnitude);
            Collider2D hit;
            if (hit = Physics2D.OverlapBox(orientation, size, 0f, pickMask))
            {
                if (hit.GetComponent<Throwable>())
                {
                    bodySize.offset = new Vector2(0f, 0.18f);
                    bodySize.size = new Vector2(0.525f, 1.25f);
                    hit.GetComponent<Throwable>().Pickup(bomb_spawn);
                    hit.GetComponent<Throwable>().pickupSound.Play();
                    clone_throw = hit.gameObject;
                    secondaryAttack = Throw_Bomb;
                }
            }
        }
        pickup = false;
    }
    //----------Orientating sprite and stuff----------//
    //***This function was written by Yan
    public void SetPosition(out Vector2 orientation, out Quaternion rotation , float distance) 
    {
        orientation = transform.position;
        if (anim.GetFloat("Hor") > 0.6 || anim.GetFloat("Hor") < -0.6)
        {
            rotation = (anim.GetFloat("Hor") > 0) ? (Quaternion.Euler(0f, 0f, 0f)) : (Quaternion.Euler(0f, 0f, 180f));
            orientation.x = (anim.GetFloat("Hor") > 0) ? (orientation.x + distance) : (orientation.x - distance);
        }
        else
        {
            rotation = (anim.GetFloat("Ver") > 0) ? Quaternion.Euler(0f, 0f, 90f) : (Quaternion.Euler(0f, 0f, -90f));
            orientation.y = (anim.GetFloat("Ver") > 0) ? (orientation.y + distance) : (orientation.y - distance);
        }
    }
    //----------Bow----------//
    //***This function was written by Yan
    public void ChargeBow() 
    {
        moveSpeed /= 4;
        attack_cancel = true;
        secondaryAttack = ChargeBow;
        if (bowCharge == null)
        {
            bowCharge = Bow_Charge();
            StartCoroutine(bowCharge);
        }
    }
    //***This function was written by Yan
    public void ShootBow() 
    {
        bowChargeImage.ToggleCharge();
        CancelInvoke("ShootBow");
        if (bowCharge != null)
        {
            StopCoroutine(bowCharge);
            bowCharge = null;
        }
        Vector2 orientation;
        Quaternion rotation;
        SetPosition(out orientation, out rotation, offsetMagnitude);
        //add velocity
        GameObject arrow = Instantiate(prefab_arrow, orientation, rotation, null);
        arrow.GetComponent<Projectile>().damage = bowDamage;
        arrow.GetComponent<Rigidbody2D>().velocity = (orientation-(Vector2)transform.position) * bowSpeed * 8f;
        bowDamage = baseBowDamage;
        bowSpeed = baseBowSpeed;
        moveSpeed = originalSpeed;
        attack_cancel = false;

        //cooldown
        StartCoroutine(ItemCooldown());
        secondaryAttack = null;
    }
    //----------Mystery Item----------//
    //***This function was written by Yan
    public void Mystery() 
    {
        //replace the prefab_arrow, i was just using it as a test instead of creating another class for projectile
        if (attack_cooldown == null)
        {
            Vector2 orientation;
            Quaternion rotation;
            SetPosition(out orientation, out rotation, offsetMagnitude);
            clone_shot = Instantiate(prefab_split, orientation, rotation);
            clone_shot.GetComponent<Rigidbody2D>().velocity = (orientation - (Vector2)transform.position) * mysterySpeed;
            secondaryAttack = Mystery_Split;
            clone_shot.GetComponent<Projectile>().damage = mysteryDamage;
            sAttack = false;
            chacheCloneShot = clone_shot.GetComponent<Rigidbody2D>().velocity;
        }
    }
    //***This function was written by Yan
    public void Mystery_Split() 
    {
        Vector2 orientation = chacheCloneShot;
        Vector2 offset = clone_shot.transform.position;
        clone_shot.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        Quaternion rotation = clone_shot.transform.rotation;
        GameObject[] clone_clone_prefab = new GameObject[2];
        clone_clone_prefab[0] = Instantiate(clone_shot, offset, rotation * Quaternion.Euler(0f,0f,-90f));
        clone_clone_prefab[1] = Instantiate(clone_shot, offset, rotation * Quaternion.Euler(0f, 0f, 90f));
        clone_clone_prefab[0].tag = "Untagged";
        clone_clone_prefab[1].tag = "Untagged";
        foreach (GameObject c in clone_clone_prefab)
        {
            c.GetComponent<Projectile>().damage = mysteryDamage;
            c.GetComponent<Collider2D>().enabled = true;
        }
        if (orientation.x > 0 || orientation.x < 0)
        {
            clone_clone_prefab[0].GetComponent<Rigidbody2D>().velocity = new Vector2(0f,mysterySpeed/2);
            clone_clone_prefab[1].GetComponent<Rigidbody2D>().velocity = new Vector2(0f, -mysterySpeed/2);

            Debug.Log(clone_clone_prefab[0].GetComponent<Rigidbody2D>().velocity);
            Debug.Log(clone_clone_prefab[1].GetComponent<Rigidbody2D>().velocity);

        }
        else 
        {
            clone_clone_prefab[0].GetComponent<Rigidbody2D>().velocity = new Vector2(mysterySpeed,0f);
            clone_clone_prefab[1].GetComponent<Rigidbody2D>().velocity = new Vector2(-mysterySpeed,0f);
        }
        secondaryAttack = null;
        sAttack = false;
        Destroy(clone_shot);
    }
    //----------Inherited----------//
    public override void Hurt(int damage,Transform hitting)
    {
        if (!this.invincible && this.hp > 0 && !dashing) 
        {
            base.Hurt(damage, hitting);
            if (damage > 0)
                StartCoroutine(invFrames(1));
            UIScript.SetBar(UIBars.Health, this.hp, maxHealth);
        }
    }
    public override void Attack()
    {
        //if clone_bomb was created and if the bomb was already thrown...
        if (clone_throw && !clone_throw.GetComponent<Throwable>().Thrown && !shield.activeSelf)
            Throw_Bomb();
        else if (!shield.activeSelf)
        {
            Quaternion quaternion;
            SetPosition(out _, out quaternion, 1);
            quaternion.eulerAngles = new Vector3(0, 0, quaternion.eulerAngles.z - 90);
            sword.transform.rotation = quaternion;
            base.Attack();
            if (attack_cooldown == null)
            {
                attack_cooldown = Attack_Cooldown();
                StartCoroutine(attack_cooldown);
            }
        }
        attacking = false;
    }
    //Player death function                     ***This function was written by Nicky
    protected override void Death()
    {
        this.gameObject.GetComponent<Player>().enabled = false;     //Disable player movement
        this.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        anim.SetTrigger("Die");
        manager.GameOver();         //Show Game Over screen + menu
    }

    //----------Coroutines----------//
    private IEnumerator ItemCooldown()
    {
        itemCooldown = true;
        float timer = ITEM_CD_TIMER;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        itemCooldown = false;
        
    }
    //***This function was written by Yan
    private IEnumerator Attack_Cooldown() 
    {
        while (attacking) 
        {
            interrupt = true;
            body.velocity = Vector2.zero;
            attack_cancel = true;
            yield return new WaitForSeconds(timer);
        }
        attack_cancel = false;
        attack_cooldown = null;
        interrupt = false;
    }
    
    //***This function was written by Simon
    private IEnumerator Dash(float dashTime)
    {
        
        interrupt = true;
        invincible = true;
        dashPuffs.Play();
        dashPuffs.gameObject.GetComponent<AudioSource>().Play();
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
                        dashPuffs.Stop();
                        stunnedSparkles.Play();
                        anim.SetBool("Stunned", true);
                        dashTime = stunTime;
                        count = 0f;
                        hasHitWall = true;
                        this.body.velocity = Vector2.zero;
                        invincible = false; //We aren't invisible once we've stopped moving
                        //audio
                        stunnedSparkles.gameObject.GetComponent<AudioSource>().Play();
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
        anim.SetBool("Stunned", false);
        dashPuffs.Stop();
        base.myRenderer.enabled = true;
        stunnedSparkles.Stop();
        invincible = false;
        interrupt = false;
        dashing = false;
    }
    
    //***This function was written by Yan
    private IEnumerator Bow_Charge() 
    {
        float charge = 0f;
        bowChargeImage.ToggleCharge();
        while (charge <= timerCap) 
        {
            if (charge != 0f)
            {
                bowChargeNoise.Play();
                bowDamage++;
                bowSpeed++;
            }
            Debug.Log($"bow damage: {bowDamage}, bow speed: {bowSpeed} , timer: {charge}");
            bowChargeImage.SetValue(charge++, timerCap);
            yield return new WaitForSeconds(timer);
        }
        Invoke("ShootBow", timer);
        bowCharge = null;
    }
}
