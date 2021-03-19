using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 0;  //How much damage the projectile deals if it hits a player/enemy
    private Rigidbody2D rigid;
    private LivingEntity entity;
    private Vector2 startVelocity;  //Velocity when first fired
    protected GameManager manager;

    private void Start()
    {
        manager = GameManager.instance;
        rigid = this.GetComponent<Rigidbody2D>();
        startVelocity = rigid.velocity;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        //check if it needs to split first
        CheckSplit();
        //If we hit the player's shield
        if (collision.collider.name == "Shield")
        {
            this.gameObject.layer = LayerMask.NameToLayer("PlayerProjectiles");
            rigid.velocity = (Vector2.Reflect(startVelocity, collision.GetContact(0).normal));  //Reflect projectile off shield
        }
        //If we hit the player or an enemy
        else if (entity = collision.collider.GetComponent<LivingEntity>())
        {
            entity.Hurt(damage, this.gameObject.transform);
            Destroy(gameObject);    //Destroy the projectile
        }
        //If we hit anything else
        else
        {
            Destroy(gameObject);    //Destroy the projectile
        }
    }
    //check if projectile is the split one
    private void CheckSplit()
    {
        if (gameObject.CompareTag("Split"))
        {
            manager.player.GetComponent<Player>().Mystery_Split();
        }
    }
}
