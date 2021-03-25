using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGhostProjectile : Projectile
{
    //made this class, because having shilds reflect bullet kinda ruin the point of bullet hell, also this is made with the intent to use object pooling so you can't destroy the gameobject
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<LivingEntity>() && collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            collision.collider.GetComponent<LivingEntity>().Hurt(damage, this.gameObject.transform); 
            gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            gameObject.SetActive(false);
            
        }
        //If we hit anything not enemy
        else if (!(collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))) 
        {
            if (collision.collider.name == "Shield")
            {
                DrainStamina();
                collision.collider.gameObject.GetComponent<AudioSource>().Play();
            }
            gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            gameObject.SetActive(false);
        }
    }
    public IEnumerator StopnStart(GameObject shot)
    {
        Vector2 originalVelocity = shot.GetComponent<Rigidbody2D>().velocity;
        while (gameObject.activeSelf) 
        {
            yield return new WaitForSeconds(0.25f);
            shot.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            yield return new WaitForSeconds(0.25f);
            shot.GetComponent<Rigidbody2D>().velocity = originalVelocity;
        }
        yield return null;
    }
    //use this move when u make a circle num shot is 1
    public IEnumerator TargetPlayer(GameObject shot, float shotspeed) 
    {
        yield return new WaitForSeconds(0.5f);
        shot.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        yield return new WaitForSeconds(4.7f);
        shot.GetComponent<Rigidbody2D>().velocity = (manager.player.transform.position - gameObject.transform.position).normalized * shotspeed;
        yield return null;
    }
}
