using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : BaseEnemyAI
{
    // Start is called before the first frame update
    void Start()
    {
        this.approach = 5f;             //Ghost is a ranged enemy
        this.moveSpeed = 4f;
        this.attack_damage = 4;
        this.cooldownTimer = 1f;        //Time between attacks (in seconds) while in range
        this.projectileSpeed = 4f;
        this.fov = 300;                 //300 degrees of vision
        base.Start();
    }

    public override void Attack()
    {
        RangedAttack();
    }
}
