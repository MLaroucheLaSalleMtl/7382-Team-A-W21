using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : BaseEnemyAI
{
    // Start is called before the first frame update
    void Start()
    {
        this.approach = 0f;     //Slime is a melee enemy
        this.speed = 4f;
        this.attack_damage = 3;
        base.Start();
    }

    public override void Attack()
    {
        MeleeAttack();
    }
}
