
public class Ghost : BaseEnemyAI
{
    // Start is called before the first frame update
    new void Start()
    {
        this.moveSpeed = 4f;
        this.attack_damage = 2;
        this.attackReach = 5;
        this.cooldownTimer = 1f;        //Time between attacks (in seconds) while in range
        this.projectileSpeed = 4f;
        base.Start();
    }

    public override void Attack()
    {
        RangedAttack();
    }
}
