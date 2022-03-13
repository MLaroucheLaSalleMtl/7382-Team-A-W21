using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : BaseEnemyAI
{
    // Start is called before the first frame update
    new void Start()
    {
        this.hp = 4;
        this.moveSpeed = 3f;
        this.attack_damage = 1;
        base.Start();
    }

    public override void Attack()
    {
        MeleeAttack();
    }
}
