using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonBoss : BaseEnemyAI
{
    private const int MAX_HP = 20;

    // Start is called before the first frame update
    void Start()
    {
        this.hp = MAX_HP;
        this.approach = 5f;             //Ranged enemy
        this.moveSpeed = 3f;
        this.attack_damage = 3;
        this.cooldownTimer = 1f;        //Time between attacks (in seconds) while in range
        this.projectileSpeed = 5f;
        this.fov = 360;                 //300 degrees of vision
        base.Start();
    }

    public override void Attack()
    {
        RangedAttack();
    }

    public override void Hurt(int damage, Transform hitting)
    {
        base.Hurt(damage, hitting);
        manager.player.canvas.GetComponent<UIBehaviour>().SetBar(UIBars.Boss, hp, MAX_HP); //Update UI
    }
}
