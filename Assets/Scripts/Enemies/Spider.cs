using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : BaseEnemyAI
{
    // Start is called before the first frame update
    void Start()
    {
        this.approach = 5f;             //Spider is a ranged enemy
        this.moveSpeed = 4f;
        this.attack_damage = 1;
        this.cooldownTimer = 0.8f;      //Time between attacks (in seconds) while in range
        this.projectileSpeed = 4f;
        base.Start();
    }

    public override void Attack()
    {
        RangedAttack();
    }
}
