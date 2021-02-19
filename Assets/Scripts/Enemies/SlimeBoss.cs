using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBoss : BaseEnemyAI
{

    [SerializeField] private GameObject minion;
    // Start is called before the first frame update
    void Start()
    {
        this.hp = 6;
        this.approach = 0f;
        this.moveSpeed = 2f;
        this.attack_damage = 3;
        base.threshold = 2;
        base.Start();
    }

    public override void Hurt(int damage, Transform hitting)
    {
        if (hitting.name == "Bomb(Clone)" && !invincible)
        {
            this.transform.localScale *= 0.83333f;
            this.attackReach *= 0.83333f;
            base.threshold *= 0.83333f;
            this.moveSpeed+=0.1f;
            this.hp--;
            if (hp <= 0)
                this.Death();
            else
            {
                StartCoroutine(invFrames(2));
                GameObject newBlob = Instantiate(minion, this.transform.position, new Quaternion(), null);
                LivingEntity ent = newBlob.GetComponent<LivingEntity>();
                ent.StartCoroutine(ent.invFrames(1));
                ent.hp = 1;
                ent.Start();
            }
            
        }
    }
    public override void Attack()
    {
        MeleeAttack();
    }
}
