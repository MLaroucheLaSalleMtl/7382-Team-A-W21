using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : BaseEnemyAI
{
    // Start is called before the first frame update
    void Start()
    {
        this.hp = 4;
        this.approach = 0.5f;     //Bat is a melee enemy
        this.moveSpeed = 3f;
        this.attack_damage = 1;
        base.Start();
    }

    public override void Attack()
    {
        MeleeAttack();
    }
}
