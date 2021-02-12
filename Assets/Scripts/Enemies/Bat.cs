using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : BaseEnemyAI
{
    // Start is called before the first frame update
    void Start()
    {
        this.approach = 0f;     //Bat is a melee enemy
        this.speed = 4f;
        this.attack_damage = 1;
        base.Start();
    }

    public override void Attack()
    {
        MeleeAttack();
    }
}
