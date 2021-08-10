using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : BaseEnemyAI
{
    // Start is called before the first frame update
    new void Start()
    {
        this.moveSpeed = 4f;
        this.attack_damage = 3;
        base.Start();
    }

    public override void Attack()
    {
        MeleeAttack();
    }
}
