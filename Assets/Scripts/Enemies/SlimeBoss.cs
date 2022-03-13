using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBoss : BaseEnemyAI
{

    [SerializeField] private GameObject minion;
    [SerializeField] private List<Door> exitDoors;
    // Start is called before the first frame update
    new void Start()
    {
        this.hp = 6;
        this.moveSpeed = 2f;
        this.attack_damage = 3;
        base.Start();
    }

    public override void Hurt(int damage, Transform hitting)
    {
        if (hitting.GetComponent<Throwable>() && !invincible)
        {
            this.transform.localScale *= 0.83333f;
            this.attackReach *= 0.83333f;
            this.moveSpeed+=0.1f;
            this.hp--;
            if (hp <= 0)
                this.Death();
            else
            {
                StartCoroutine(invFrames(2));
                GameObject newBlob = Instantiate(minion, this.transform.position, new Quaternion(), null);
                foreach(Door exit in exitDoors)
                {
                    exit.enemies.Add(newBlob);
                }
                LivingEntity ent = newBlob.GetComponent<LivingEntity>();
                ent.Start();
                ent.StartCoroutine(ent.invFrames(1));
                ent.hp = 1;
                manager.player.canvas.GetComponent<UIBehaviour>().SetBar(UIBars.Boss, hp, 6); //UPDATE UI
            }
        }
    }

    public override void Attack()
    {
        MeleeAttack();
    }
}
