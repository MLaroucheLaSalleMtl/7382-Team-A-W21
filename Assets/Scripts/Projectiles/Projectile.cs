using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 0;          //How much damage the projectile deals if it hits a player/enemy
    private Rigidbody2D rigid;
    private LivingEntity entity;
    private Vector2 startVelocity;  //Velocity when first fired
    protected GameManager manager;
    protected int drainAmount = 2;  // didn't use constant, because if other projectile could have more drain etc...

    private void Start()                            
    {
        manager = GameManager.instance;
        rigid = this.GetComponent<Rigidbody2D>();
        startVelocity = rigid.velocity;
    }

    //When the projectile hits something   
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
                  
        CheckSplit();   //Check if projectile splits

        if (collision.collider.name == "Shield")    //If we hit the player's shield
        {
            this.gameObject.layer = LayerMask.NameToLayer("PlayerProjectiles");
            rigid.velocity = (Vector2.Reflect(startVelocity, collision.GetContact(0).normal));  //Reflect projectile off shield
            DrainStamina();
            collision.collider.gameObject.GetComponent<AudioSource>().Play();
        }
        else if (entity = collision.collider.GetComponent<LivingEntity>())  //If we hit the player or an enemy
        {
            entity.Hurt(damage, this.gameObject.transform);
            Destroy(gameObject);    //Destroy the projectile
        }
        else                    //If we hit anything else
            Destroy(gameObject);    //Destroy the projectile
    }

    //Check if projectile splits
    private void CheckSplit()
    {
        if (gameObject.CompareTag("Split"))
            manager.player.GetComponent<Player>().Mystery_Split();
    }

    //Drain player's stamina on hit
    protected void DrainStamina() 
    {
        if (manager.player.stamina >= drainAmount)
            manager.player.stamina -= drainAmount;
        else 
            manager.player.stamina = 0;
        manager.player.canvas.GetComponent<UIBehaviour>().SetBar(UIBars.Stamina, manager.player.stamina, manager.player.maxStamina);
    }
}
