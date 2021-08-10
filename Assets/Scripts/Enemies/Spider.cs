
public class Spider : BaseEnemyAI
{
    // Start is called before the first frame update
    new void Start()
    {
        this.hp = 4;
        this.moveSpeed = 2f;
        this.attack_damage = 2;
        this.attackReach = 5;
        this.cooldownTimer = 0.8f;      //Time between attacks (in seconds) while in range
        this.projectileSpeed = 4f;
        base.Start();
    }

    public override void Attack()
    {
        RangedAttack();
    }
}
