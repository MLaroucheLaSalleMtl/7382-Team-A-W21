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
public enum Direction
{
    [InspectorName("Right")]
    RIGHT = 0,
    [InspectorName("Up")]
    UP = 90,
    [InspectorName("Left")]
    LEFT = 180,
    [InspectorName("Down")]
    DOWN = 270,
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class BaseEnemyAI : LivingEntity
{

    //Inspector fields (public or serialized)
    [Header("AI Behaviour")]
    [Tooltip("What am I looking for?")] public Transform target;
    [Tooltip("Which way should I start facing?")] [SerializeField] private Direction direction = Direction.RIGHT;
    [Tooltip("What can we NOT see through?")] [SerializeField] private LayerMask blinds;
    [Tooltip("How close should I get to my destination before I sat I've arrived? [Do not change if you do not understand]")] [SerializeField] private float threshold = 1.5f;
    [Header("AI Stats")]
    [Tooltip("How wide is my field of view? [Should keep above 90 if you don't want blind spots]")] [Range(2, 360)] public float fov = 90;
    [Tooltip("How far can I see?")] [Range(1, 100)] public float range = 20;
    [Tooltip("How far can I wander?")] [Range(1, 100)] public float wanderRange = 10;
    [Tooltip("How close do I want to get? Also the auto-detect range!")] [Range(0, 20)] public float approach = 7;
    [Tooltip("How fast should I move?")] public float speed = 3;

    //Hidden fields (private or hidden)
    [HideInInspector] public Vector2 gotoPoint; //Public so that other scripts/classes may affect this
    private Rigidbody2D rigid;
    private bool wasPlayer = false;
    

    //Unity Messages
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>(); //Retrieve our rigid body
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
    }

    private void FixedUpdate()
    {

        if (((gotoPoint - (Vector2)this.transform.position).magnitude > threshold))
        {
            rigid.velocity = (gotoPoint - (Vector2)this.transform.position).normalized * speed; //Head to point
            Rotate(rigid.velocity);
        }
        else
        {
            pickPoint();
        }
        if (target)
        {
            if ((target.position - this.transform.position).magnitude < approach)
            {
                Rotate(target.position - this.transform.position);
            }
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Level"))
        {
            List<ContactPoint2D> conPoints2D = new List<ContactPoint2D>();
            collision.GetContacts(conPoints2D);
            Vector2 avg = Vector2.zero;
            foreach(ContactPoint2D conPoint in conPoints2D)
            {
                avg += conPoint.point;
            }
            avg /= conPoints2D.Count;
            avg = ((avg - (Vector2)this.transform.position).normalized * Random.Range(0, -wanderRange));
            RaycastHit2D hit = Physics2D.Raycast(this.transform.position, avg, avg.magnitude, blinds);
            Rotate(avg);
            if (hit)
            {
                avg = hit.point;
            }
            else
            {
                avg = avg += (Vector2)this.transform.position;
            }
            gotoPoint = avg;
        }
            
    }

    //Custom methods

    //Please override with deriving class for what ever result you want (Used for attacks)
    protected void Action()
    {
        Debug.Log("Do something");
    }

    //Pick a new point
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

    //Rotate the direction enumerator based on a Vector2
    private void Rotate(Vector2 dir)
    {
        if (dir == Vector2.zero) return;
        dir = dir.normalized;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x > 0)
            {
                this.direction = Direction.RIGHT;
            }
            else
            {
                this.direction = Direction.LEFT;
            }
        }
        else
        {
            if (dir.y > 0)
            {
                this.direction = Direction.UP;
            }
            else
            {
                this.direction = Direction.DOWN;
            }
        }
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
}
