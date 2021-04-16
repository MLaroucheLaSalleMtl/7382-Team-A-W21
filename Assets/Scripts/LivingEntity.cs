using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
public class LivingEntity : MonoBehaviour
{
    protected GameManager manager;
    protected Animator anim;
    public Rigidbody2D body;
    protected Renderer myRenderer;
    [SerializeField] protected AudioSource hurtSound;       //AudioSource
    public int hp = 10;                                     //Health
    protected int attack_damage;
    protected float moveSpeed = 3;
    [Tooltip("Which way should I start facing?")] [SerializeField] protected Direction direction = Direction.DOWN;
    [HideInInspector] public Vector2 gotoPoint;     //Where to walk to
    protected bool interrupt = false;               //Pauses follow/roaming AI behaviour
    protected const float timer = 0.5f;
    protected IEnumerator knockback;
    protected bool invincible = false;

    protected Vector3 hitBoxOffset = new Vector3(0,0,0);    //Offset from the entity of its hitbox 
    protected float offsetMagnitude = 0f;                   //Magnitude of the entity hitbox offset
    private const float shieldKnockback = 5f;

    // Start is called before the first frame update
    public void Start()
    {
        this.manager = GameManager.instance;
        this.body = GetComponent<Rigidbody2D>();
        this.myRenderer = GetComponent<Renderer>();
        this.anim = GetComponent<Animator>();
        this.hurtSound = GetComponent<AudioSource>();
    }
    //Rotate the direction enumerator based on a Vector2
    protected void Rotate(Vector2 dir)
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

    //defult melee attack for player (replaced with animations)
    //***This function was written by Yan
    public virtual void Attack() 
    {
        //Player's attack is made together with the animation
        this.anim.SetTrigger("Attack");
    }
    //***This function was written by Yan
    public virtual void Hurt(int damage, Transform hitting) 
    {
        //stop entity's velocity so you can push them...
        interrupt = true;
        //play hurt noise
        hurtSound.Play();
        body.velocity = Vector2.zero;
        hp -= damage;
        if (hitting != null)
        {
            Vector2 distance = gameObject.transform.position - hitting.transform.position;
            if (damage > 0)
                distance = distance.normalized * damage;   //Normal hit
            else
                distance = distance.normalized * shieldKnockback;   //Hit blocked by shield
            body.velocity = (distance);
        }
        //If hp reaches 0
        if (this.hp <= 0)
        {
            body.velocity = new Vector2(0, 0);
            Death();
        }
        else if (knockback == null) 
        {
            knockback = KnockBackCoolDown();
            StartCoroutine(knockback);
        }
    }
    protected virtual void Death() 
    {
        this.interrupt = true;                  //Disable movement
    }

    protected Vector3 CalculateOffset(float offsetMagnitude)
    {
        Vector3 offset = new Vector3();
        switch (direction)         //Decide which side to attack on, based on the direction we're facing
        {
            case Direction.UP:
                offset = new Vector3(0f, offsetMagnitude, 0f);
                break;
            case Direction.DOWN:
                offset = new Vector3(0f, -offsetMagnitude, 0f);
                break;
            case Direction.LEFT:
                offset = new Vector3(-offsetMagnitude, 0f, 0f);
                break;
            case Direction.RIGHT:
                offset = new Vector3(offsetMagnitude, 0f, 0f);
                break;
        }
        return offset;
    }

    //Coroutines
    //***This function was written by Yan
    private IEnumerator KnockBackCoolDown() 
    {
        while (interrupt) 
        {
            yield return new WaitForSeconds(timer);
            interrupt = false;
        }
        knockback = null;
    }

    public IEnumerator invFrames(int seconds)
    {
        int count = 0;
        invincible = true;
        while (count < seconds * 10)
        {
            myRenderer.enabled = false;
            yield return new WaitForSeconds(0.05f);
            myRenderer.enabled = true;
            yield return new WaitForSeconds(0.05f);
            count++;
        }
        invincible = false;
    }
}
