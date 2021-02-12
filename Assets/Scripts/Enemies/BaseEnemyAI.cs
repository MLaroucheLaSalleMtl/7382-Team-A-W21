//Simon Choquet 2/4/2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// This implements very basic AI that wanders until it spots a target.
/// After which it will run to the target or the last place the target was spotted.
/// If it loses track of the player it will restart the cycle.
/// However, If it finds itself within the specified range, it will try to maintain this range.
/// 
/// </summary>

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class BaseEnemyAI : LivingEntity
{
    //Inspector fields (public or serialized)
    [Header("AI Behaviour")]
    [Tooltip("What am I looking for?")] public Transform target;
    [Tooltip("What can we NOT see through?")] [SerializeField] private LayerMask blinds;
    [Tooltip("How close should I get to my destination before I say I've arrived? [Do not change if you do not understand]")] [SerializeField] private float threshold = 1f;
    [Header("AI Stats")]
    [Tooltip("How wide is my field of view? [Should keep above 90 if you don't want blind spots]")] [Range(2, 360)] public float fov = 90;
    [Tooltip("How far can I see?")] [Range(1, 100)] public float range = 20;
    [Tooltip("How far can I wander?")] [Range(1, 100)] public float wanderRange = 10;
    [Tooltip("How close do I want to get? Also the auto-detect range!")] [Range(0, 20)] public float approach = 7;
    [Tooltip("How fast should I move?")] public float speed = 3;
    [Space]
    [Header("Enemy Attack Parameters")]
    [Tooltip("Layer of the target we can attack (Player should be on this layer)")] [SerializeField] private LayerMask layerMask;
    [Tooltip("Enemy's attack reach on the x and y-axes if melee")] [SerializeField] private Vector2 attackReach = new Vector2(0.8f, 0.8f);
    [Tooltip("Projectile to shoot if ranged")] [SerializeField] private GameObject enemyProjectile;
    [Tooltip("Projectile speed if ranged")] [SerializeField] protected float projectileSpeed = 0;

    //Hidden fields (private or hidden)
    private bool wasPlayer = false;     //Did we last see the player or are we wandering
    public Rigidbody2D rigid;

    //Variables for enemy attack
    private bool attackCD = false;                  //Is the attack on cooldown?
    protected float cooldownTimer = 1;              //Cooldown between attacks in seconds
    private Collider2D playerHit;                   //Collider hit by enemy melee attack (player collider)
    private Vector3 hitBoxOffset = new Vector3();   //Offset from the enemy of its hitbox 
    private const float offsetMagnitude = 0.3f;     //Magnitude of the enemy hitbox offset

    //Unity Messages
    public void Start()
    {
        Debug.Log("Enemy start");
        base.Start();
        rigid = GetComponent<Rigidbody2D>();        //Retrieve our rigid body
        target = manager.player.transform;          //Set player as our target
        layerMask = LayerMask.GetMask("Player");    //Layer to check for attack hitbox collision
        blinds = LayerMask.GetMask("Level");        //Layer with objects the enemy can't see through
        pickPoint();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, range);
        Gizmos.DrawLine(this.transform.position, this.transform.position+(Quaternion.Euler(0, 0, (int)direction + (fov / 2)) * Vector2.right) * range);
        Gizmos.DrawLine(this.transform.position, this.transform.position+(Quaternion.Euler(0, 0, (int)direction - (fov / 2)) * Vector2.right) * range);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, approach);

        //Hitbox for enemy attack
        Gizmos.color = Color.red;                           
        Gizmos.DrawWireCube(this.transform.position + hitBoxOffset, attackReach);
    }
    
    private void FixedUpdate()
    {
        //If interrupted don't move and don't do anything 
        if (interrupt) return;
        
        //If we haven't arrived to our node walk that way
        if (((gotoPoint - (Vector2)this.transform.position).magnitude > threshold))
        {
            rigid.velocity = (gotoPoint - (Vector2)this.transform.position).normalized * speed; //Head to point
            Rotate(rigid.velocity);
        }
        else 
        {
            pickPoint(); //Once we've arrived pick a new node
        }
        

        if (target)
        {
            //If the target gets too close we automatically face it
            if ((target.position - this.transform.position).magnitude < approach)
            {
                Rotate(target.position - this.transform.position);
            }
            
            //If we can see our target head towards it
            if (canSee(target.position))
            {
                Vector3 dif = target.position - this.transform.position;
                float travelDist = dif.magnitude - approach;
                dif = dif.normalized;
                dif *= travelDist;
                gotoPoint = this.transform.position + dif;
                wasPlayer = true;
                if ((gotoPoint - (Vector2)this.transform.position).magnitude < threshold)
                {
                    rigid.velocity /= 1.25f;
                    Action();
                }
            }
        }

    }

    void Update()
    {

        //For visualisation in edior only
        //Also note the * Vector2.right
        //(Because mathematically rotation should always be starting from the positive x and going CCW. Like the Unit Circle!)
        #if UNITY_EDITOR
            if (target != null)
            {
                Debug.DrawRay(this.transform.position, target.position - this.transform.position, canSee(target.position) ? Color.blue : Color.red); //Player scanline
            };
        #endif

    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    List<ContactPoint2D> conPoints2D = new List<ContactPoint2D>();
    //    collision.GetContacts(conPoints2D);
    //    Vector2 avg = Vector2.zero;
    //        foreach(ContactPoint2D conPoint in conPoints2D)
    //        {
    //            avg += conPoint.point;
    //        }
    //        avg /= conPoints2D.Count;
    //        avg = ((avg - (Vector2)this.transform.position).normalized * Random.Range(0, -wanderRange));
    //        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, avg, avg.magnitude, blinds);
    //        Rotate(avg);
    //        if (hit)
    //        {
    //            avg = hit.point;
    //        }
    //        else
    //        {
    //            avg = avg += (Vector2)this.transform.position;
    //        }
    //    gotoPoint = avg;    
    //}


    //** ALL CUSTOM METHODS HERE **//

    //Pick a new point based on wether the monster want's to wander or not.
    private void pickPoint()
    {
        Vector2 dest;
        if (wasPlayer) //If the last point was chasing a player continue a little farther in that direction (To meet exactly where the player should've been spotted)
        {
            dest = rigid.velocity.normalized;
            dest *= approach;
        }
        else //Or pick a random point (wander)
        {
            dest = new Vector2(Random.value, Random.value);
            dest.Normalize();
            dest *= Random.Range(-wanderRange, wanderRange);
        }
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, dest, dest.magnitude, blinds);
        Rotate(dest);
        if (hit)
        {
            dest = hit.point;
        }
        else
        {
            dest = dest += (Vector2)this.transform.position;
        }
        wasPlayer = false;
        gotoPoint = dest;
    }

    //My process of elimination version of a field of view for ai
    private bool canSee(Vector3 tgt)
    {

        Vector3 dif = tgt - this.transform.position;
        if (dif.magnitude > range) return false; //out of range = can't see

        float angle = Vector2.Angle(Quaternion.Euler(0, 0, (int)direction) * Vector2.right, (Vector2)(this.transform.position - tgt)) - 180; //get angle in field of view
        if (angle < -0.5 * fov) return false; //Out of fov = can't see

        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, dif, dif.magnitude, blinds);
        if (hit.collider) return false;

        return true;
    }

    //Action to perform when in range of player
    protected void Action()
    {
        //Check if attack is on cooldown
        if(!attackCD)
        {
            interrupt = true;          //Pause roaming/follow AI
            switch (direction)         //Decide which side to attack on, based on the direction we're facing
            {
                case Direction.UP:
                    hitBoxOffset = new Vector3(0f, offsetMagnitude, 0f);
                    break;
                case Direction.DOWN:
                    hitBoxOffset = new Vector3(0f, -offsetMagnitude, 0f);
                    break;
                case Direction.LEFT:
                    hitBoxOffset = new Vector3(-offsetMagnitude, 0f, 0f);
                    break;
                case Direction.RIGHT:
                    hitBoxOffset = new Vector3(offsetMagnitude, 0f, 0f);
                    break;
            }
            Attack();
            StartCoroutine(AtkCooldownCoroutine()); //Start attack cooldown
            interrupt = false;                      //Resume AI behaviour
        }
    }

    //Enemy melee attack
    protected void MeleeAttack()
    {
        //If player is inside enemy hitbox
        if (playerHit = Physics2D.OverlapBox(transform.position + hitBoxOffset, attackReach, 0f, layerMask))
        {
            manager.player.Hurt(attack_damage, this.gameObject.transform); //Deal damage to player
        }
        else
        {
            Debug.Log("Melee attack hit nothing");
        }
    }

    //Enemy ranged attack
    protected void RangedAttack()
    {
        GameObject projectile = Instantiate(enemyProjectile, transform.position + hitBoxOffset, transform.rotation);    //Create projectile copy
        projectile.GetComponent<Projectile>().damage = this.attack_damage;  //Match the projectile's damage to the enemy's attack
        projectile.GetComponent<Rigidbody2D>().velocity = (target.position - this.transform.position).normalized * projectileSpeed;   //Set projectile velocity
    }

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

    protected override void Death()
    {
        base.Death();
        StartCoroutine(DeathCoroutine(2f));    //Wait 2 seconds before removing corpse
    }

    private IEnumerator DeathCoroutine(float seconds)
    {
        while (seconds > 0)
        {
            seconds -= Time.deltaTime;
            yield return null;
        }
        if (this != manager.player)
            Destroy(gameObject);
    }
}