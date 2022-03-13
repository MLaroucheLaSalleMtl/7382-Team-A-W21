using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class BaseEnemyAI : LivingEntity
{
    [Tooltip("Projectile to shoot if ranged")] [SerializeField] private GameObject enemyProjectile;
    private Rigidbody2D rigid;
    private Transform target;
    private bool attackCD = false;      //Is the attack on cooldown?
    private Collider2D playerHit;       //Collider hit by enemy melee attack (player collider)
    private LayerMask playerMask;
    private LayerMask shieldMask;
    private Vector2 dirToTarget;        //Direction for enemy to reach its target (the player)
    private float distFromTarget;       //Distance between the enemy and its target (the player)
    private bool roamingCD;             //If the enemy moved recently 

    //Stats
    protected float projectileSpeed = 0f;
    protected float cooldownTimer = 1f; //Cooldown between attacks in seconds
    protected float attackReach = 1f;   //Enemy's attack reach
    protected float visionRange = 6f;   //If player is within this range, chase them
    private float despawnTimer = 1f;    //Delay before despawning enemy corpses      
    private float roamingTimer = 10f;   //Max time between enemy roaming movements

    protected new void Start()
    {
        base.Start();
        rigid = GetComponent<Rigidbody2D>();        //Retrieve our rigid body
        target = manager.player.transform;          //Set player as our target
        shieldMask = LayerMask.GetMask("Shield");
        playerMask = LayerMask.GetMask("Player");
    }

    private void Update()
    {
        if (interrupt)  //Stops all enemy behaviour
            return;

        dirToTarget = target.transform.position - this.transform.position;  //Calculate direction and distance to player
        distFromTarget = dirToTarget.magnitude;
        dirToTarget.Normalize();
        Vector2 velocity = dirToTarget * moveSpeed;

        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, target.position - this.transform.position, visionRange, LayerMask.GetMask("Player") | LayerMask.GetMask("Level"));
        if (hit.collider)
            Debug.Log(hit.collider.name);
        if (distFromTarget <= attackReach)  //Attack player if within attack range
            Action();
        else if (distFromTarget >= attackReach && hit.transform == target)    //Else if within vision range, approach player
            rigid.velocity = new Vector2(velocity.x, velocity.y);
        else if (!roamingCD)   
            StartCoroutine(RoamingCoroutine()); //Roaming
    }

    private void FixedUpdate()
    {
        //Update enemy's walk/idle animation
        if (this.rigid.velocity.magnitude > 0.1)
        {
            this.anim.SetFloat("Hor", this.rigid.velocity.x);
            this.anim.SetFloat("Ver", this.rigid.velocity.y);
            this.anim.SetBool("Mov", true);
        }
        else
            this.anim.SetBool("Mov", false);
    }

    //Action to perform when in range of player                             
    protected void Action()
    {
        if(!attackCD)   //Check if attack is on cooldown
            Attack();
    }

    //Enemy melee attack
    protected void MeleeAttack()
    {
        if (this.hp <= 0)           //Enemy can not attack if dead
            return;
        playerHit = Physics2D.OverlapCircle(transform.position, attackReach, shieldMask);
        if (playerHit == null)      //If enemy didn't hit shield
        {
            playerHit = Physics2D.OverlapCircle(transform.position, attackReach, playerMask);
            if (playerHit == null)  //If enemy hit nothing
                return;
            else if (manager.player.stamina <= 0 || playerHit.gameObject == manager.player.gameObject) //If enemy hits the player or player is out of stamina
            {
                manager.player.Hurt(attack_damage, this.gameObject.transform);  //Deal damage to player
                StartCoroutine(AtkCooldownCoroutine());                         //Start attack cooldown
            }
        }  
        else if (manager.player.stamina > 0 && playerHit.gameObject == manager.player.shield)     //If enemy hits shield and player has stamina
        {
            manager.player.Hurt(0, this.gameObject.transform);      //Knock player back
            StartCoroutine(AtkCooldownCoroutine());                 //Start attack cooldown
            this.Hurt(0, manager.player.transform);                 //Knock enemy back
            manager.player.stamina -= manager.player.shieldCost;    //Reduce player stamina
        }
    }

    //Enemy ranged attack
    protected void RangedAttack()
    {
        GameObject projectile = Instantiate(enemyProjectile, transform.position + hitBoxOffset, transform.rotation);    //Create projectile copy
        projectile.GetComponent<Projectile>().damage = this.attack_damage;  //Match the projectile's damage to the enemy's attack
        projectile.GetComponent<Rigidbody2D>().velocity = (target.position - this.transform.position).normalized * projectileSpeed;   //Set projectile velocity
        StartCoroutine(AtkCooldownCoroutine());     //Start attack cooldown
    }

    //Cooldown between regular attacks
    IEnumerator AtkCooldownCoroutine()
    {
        attackCD = true;            //Attack is on cooldown
        float cooldownTimer = this.cooldownTimer;
        while (cooldownTimer > 0)   
        {
            cooldownTimer -= Time.deltaTime;
            yield return null;
        }
        attackCD = false;           //Attack is no longer on cooldown
    }

    //Enemy death
    protected override void Death()
    {
        base.Death();
        anim.SetTrigger("Death");
        StartCoroutine(DeathCoroutine());
    }

    //Wait before despawning enemy corpses
    private IEnumerator DeathCoroutine()
    {
        float seconds = this.despawnTimer;
        while (seconds > 0)
        {
            seconds -= Time.deltaTime;
            yield return null;
        }
        if (this != manager.player) //Don't despawn the player's corpse
            Destroy(gameObject);
    }

    //Roam around when player not within range
    private IEnumerator RoamingCoroutine()
    {
        roamingCD = true;
        Vector2 direction = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;   
        Vector2 velocity = direction * moveSpeed;
        rigid.velocity = new Vector2(velocity.x, velocity.y);   //Move in random direction
        float seconds = Random.Range(0f, this.roamingTimer);    //Cooldown before moving again
        while (seconds > 0)
        {
            seconds -= Time.deltaTime;
            yield return null;
        }
        roamingCD = false; //Reset cooldown
    }
}
