using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bombs : Throwable
{
    //needs 2 colliders for player to walk into and one to trigger if it hits an enemy
    [SerializeField] protected const float timer = 5f;
    private void Start()
    {
        Invoke("Hit", timer);
    }
    public void Hit()
    {
        Collider2D[] hit;
        hit = Physics2D.OverlapCircleAll(transform.position, this.radius,this.mask);
        foreach (Collider2D c in hit) 
        {
            //what it hit
            if (c.gameObject.GetComponent<LivingEntity>() != null)
            {
                c.gameObject.GetComponent<LivingEntity>().Hurt(this.damage, gameObject.transform);
                //check if player is holding this bomb
                if (c.gameObject.name == "Player") 
                {
                    Debug.Log("hitting player");
                    //check if he's holding the bomb by seeing if this game object's parent is not null
                    if (c.gameObject.GetComponent<Player>().SecondaryAttack1 != null && transform.parent != null)
                        c.gameObject.GetComponent<Player>().SecondaryAttack1 = null;
                }
            }
            else if (c.CompareTag("Breakable"))
                Destroy(c.gameObject, 1f);
        }
        //check if player is holding the bomb
        //play animation and then destroy this object
        gameObject.SetActive(false);
        Destroy(gameObject, timer);
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        //if you want to have it hit interactable and also explode on impact put ||
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && thrown && !destination_reached)
        {
            hit = true;
            CancelInvoke("Boom");
            Hit();
        }
    }
    //on contact

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
