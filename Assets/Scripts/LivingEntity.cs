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

public class LivingEntity : MonoBehaviour
{
    protected int hp = 10; //Health
    protected GameManager manager;
    protected Animator anim;
    public Rigidbody2D body;
    [Tooltip("Which way should I start facing?")] [SerializeField] protected Direction direction = Direction.DOWN;
    protected int attack_damage;
    [HideInInspector] public Vector2 gotoPoint;     //Where to walk to
    protected bool interrupt = false;               //Pauses follow/roaming AI behaviour
    private const float timer = 0.5f;
    IEnumerator knockback;
    protected bool invincible = false;
    protected Renderer myRenderer;
    


    // Start is called before the first frame update
    public void Start()
    {
        this.manager = GameManager.instance;
        this.body = GetComponent<Rigidbody2D>();
        this.myRenderer = GetComponent<Renderer>();
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


    //defult melee attack for both player and monster (replaced with animations)
    public virtual void Attack() 
    {
        //Player's attack is made together with the animation
        this.anim.SetTrigger("Attack");
    }

    public virtual void Hurt(int damage, Transform hitting) 
    {
        int multiplier = 1; //debug reasons
        //stop entity's velocity so you can push them...
        interrupt = true;

        body.velocity = Vector2.zero;
        hp -= damage;
        Vector2 distance = gameObject.transform.position - hitting.transform.position;
        distance = distance.normalized * damage * multiplier;
        body.velocity = (distance);
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
        //Debug.Log($"current distance : {body.velocity}");
        Debug.Log("current HP: " + this.hp);

    }
    protected virtual void Death() 
    {
        this.interrupt = true;                  //Disable movement
        //Play death animation
    }

    //Coroutines
    private IEnumerator KnockBackCoolDown() 
    {
        while (interrupt) 
        {
            yield return new WaitForSeconds(timer);
            interrupt = false;
        }
        knockback = null;
    }

    protected IEnumerator invFrames(int seconds)
    {
        Debug.Log("Invince");
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
