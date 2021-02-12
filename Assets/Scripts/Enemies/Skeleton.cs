using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : BaseEnemyAI
{
    // Start is called before the first frame update
    void Start()
    {
        this.approach = 0f;     //Skeleton is a melee enemy
        this.speed = 3f;
        this.attack_damage = 6;
        base.Start();
    }

    public override void Attack()
    {
        MeleeAttack();
    }
}
