using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonBoss : BaseEnemyAI
{
    // Start is called before the first frame update
    void Start()
    {
        this.hp = 20;
        this.approach = 6f;             //Ghost is a ranged enemy
        this.moveSpeed = 3f;
        this.attack_damage = 4;
        this.cooldownTimer = 1f;        //Time between attacks (in seconds) while in range
        this.projectileSpeed = 5f;
        this.fov = 360;                 //300 degrees of vision
        base.Start();
    }

    public override void Attack()
    {
        RangedAttack();
    }
    protected override void Death()
    {
        manager.bossesSlain++;
        base.Death();
    }
}
