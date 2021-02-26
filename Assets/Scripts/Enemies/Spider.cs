using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : BaseEnemyAI
{
    // Start is called before the first frame update
    void Start()
    {
        this.hp = 4;
        this.approach = 7f;             //Spider is a ranged enemy
        this.moveSpeed = 2f;
        this.attack_damage = 2;
        this.cooldownTimer = 0.8f;      //Time between attacks (in seconds) while in range
        this.projectileSpeed = 4f;
        this.fov = 180;
        base.Start();
    }

    public override void Attack()
    {
        RangedAttack();
    }
}
