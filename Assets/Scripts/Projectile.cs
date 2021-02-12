using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 0;  //How much damage the projectile deals if it hits a player/enemy
    
    private void OnTriggerEnter2D(Collider2D hit)
    {
        //If we hit the player or an enemy
        LivingEntity entity = hit.GetComponent<LivingEntity>(); 
        if (entity)
        {
            entity.Hurt(damage, this.gameObject.transform);
        }

        //If player reflects an enemy projectile (Hits shield)
        if(false)           
        {
            gameObject.layer = 10;
        }

        Destroy(gameObject);    //Destroy the projectile when it hits something
    }
}
