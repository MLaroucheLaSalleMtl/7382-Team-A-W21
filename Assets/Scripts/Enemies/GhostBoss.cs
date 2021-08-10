using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GhostBoss : LivingEntity
{
    //----------Delegate----------//
    private delegate void Decision();
    private Decision decision;
    //----------Other Variables----------//
    //[SerializeField] private GameObject canvas;
    private const float cooldown = 1f;
    private readonly float[] angleMove = { -45f, -135f, 135f, 45f };
    private bool ready = false;
    private bool moving;
    private const int HP_MAX = 10; //change if necesary...
    private float delayPerShot;
    private float duration;
    //----------Variable----------//
    [Header("Direction")]
    Vector2 movement;
    [Space]
    [Header("Projectile")]
    [SerializeField] private List<float> angles;
    private int bulletnum = 4;
    private const float bulletSpeed = 5f;
    private int index;
    [SerializeField] private float offset = 1;
    //----------Movement Selection----------//
    //makes sure the node found is not the same one
    private void StartMovement() 
    {
        body.velocity = Vector2.zero;
        int angleIndex = (int)Random.Range(0f, angleMove.Length - 1);
        float posX = transform.position.x + Mathf.Sin((angleMove[angleIndex] * Mathf.PI) / 180f);
        float posY = transform.position.y + Mathf.Cos((angleMove[angleIndex] * Mathf.PI) / 180f);
        movement = (new Vector2(posX, posY) - (Vector2)transform.position).normalized * moveSpeed;
        body.velocity = movement;
        ready = true;
    }
    //----------Attack Patern----------//
    public void StandardPattern(int numBullet, float rotationSpeed, float speed, out GameObject[] shots) 
    {
        //off set positive = clockwise, negative = counterclockwise
        shots = new GameObject[numBullet]; //all shots fired within this volley...(can add special characteristic to shots)

        float anglenum = 360f / numBullet;
        for (int i = 0; i < numBullet; i++) 
        {
            float positionX = transform.position.x + Mathf.Sin((angles[index] * Mathf.PI) / 180f);
            float positionY = transform.position.y + Mathf.Cos((angles[index] * Mathf.PI) / 180f);
            Vector2 projectileMove = (new Vector2(positionX, positionY) - (Vector2)transform.position).normalized;
            GameObject temp = ObjectPooling.instance.GetBullet();
            Shot(temp, projectileMove, speed);
            angles[index] += anglenum + rotationSpeed;
            shots[i] = temp;
        }
        if (Mathf.Abs(angles[index]) >= 360f)
            angles[index] %= 360f;
        index++;
        if (angles.Count < index + 1)
            angles.Add(0f);
    }
    public void Action() 
    {

        ready = false;
        if (Random.Range(0f, 1f) < 0.5f)
        {
            moving = true;
            ResumeMove();
        }
        else
        {
            moving = false;
            body.velocity = Vector2.zero;
        }
        Attack();
    }
    //normal attack bullet
    public void TargetSpiral() 
    {
        StandardPattern(2 * bulletnum, 2 * offset, 2 * bulletSpeed, out _);
        angles.RemoveAt(index);
        index = 0;
    }
    public void TargetAttack() 
    {
        if (manager.player.hp > 0) // makes sure there's no null reference
        {
            Vector2 middle_shot = (manager.player.transform.position - transform.position).normalized;
            Vector2 second_shot = Rotate(15f, middle_shot);
            Vector2 third_shot = Rotate(-15f, middle_shot);

            Shot(ObjectPooling.instance.GetBullet(), middle_shot, 2 * bulletSpeed);
            Shot(ObjectPooling.instance.GetBullet(), second_shot, 2 * bulletSpeed);
            Shot(ObjectPooling.instance.GetBullet(), third_shot, 2 * bulletSpeed);
        }
    }
    public Vector2 Rotate(float angle, Vector2 vector) 
    {
        Vector2 shot = new Vector2
        {
            x = vector.x * Mathf.Cos((angle * Mathf.PI) / 180f) - vector.y * Mathf.Sin((angle * Mathf.PI) / 180f),
            y = vector.x * Mathf.Sin((angle * Mathf.PI) / 180f) + vector.y * Mathf.Cos((angle * Mathf.PI) / 180f)
        };
        return shot;
    }
    public void RefreshAngles() 
    {
        for (int i = 0; i < angles.Count; i++)
            angles[i] = 0f;
    }
    public void Shot(GameObject temp, Vector2 dir, float speed) 
    {
        temp.SetActive(true);
        temp.transform.position = transform.position;
        temp.GetComponent<Rigidbody2D>().velocity = dir * speed;
    }
    //----------Paterns that boss uses when standing still for x seconds----------//
    public void StaticPattern_Flower() 
    {
        GameObject[] shot_volley;
        StandardPattern(bulletnum, 2 * offset, bulletSpeed, out shot_volley);
        StopGimmick(shot_volley);
        StandardPattern(bulletnum, -2 * offset, bulletSpeed, out shot_volley);
        StopGimmick(shot_volley);
        StandardPattern(bulletnum - 1, 4 * offset, bulletSpeed + (bulletSpeed / 4), out _);
        StandardPattern(bulletnum - 1, -4 * offset, bulletSpeed + (bulletSpeed / 4), out _);
        angles.RemoveAt(index);
        index = 0;
    }
    public void StaticPatern_CircleTrack() 
    {
        GameObject[] volley;
        StandardPattern(bulletnum, -1, bulletSpeed, out _);
        StandardPattern(bulletnum % (bulletnum - 1), 15 * offset, bulletSpeed, out volley);
        TargetGimmick(volley);
        angles.RemoveAt(index);
        index = 0;
    }
    //----------Shot Characteristics----------//
    public void StopGimmick(GameObject[] volley) 
    {
        foreach (GameObject g in volley)
            g.GetComponent<BossGhostProjectile>().StartCoroutine(g.GetComponent<BossGhostProjectile>().StopnStart(g));
    }
    public void TargetGimmick(GameObject[] volley) 
    {
        foreach (GameObject g in volley)
            g.GetComponent<BossGhostProjectile>().StartCoroutine(g.GetComponent<BossGhostProjectile>().TargetPlayer(g, bulletSpeed * 2));
    }
    //----------Iheritance----------//
    public override void Attack()
    {
        int result;
        //float delay = (decision != StaticPattern_Flower) ? 0.2f : 0.45f;
        //add stuff in delegate here
        if (moving)
        {
            result = Random.Range(0, 3);
            switch (result)
            {
                case 0:
                    decision = TargetAttack;
                    delayPerShot = 0.2f;
                    break;
                case 1:
                    decision = TargetSpiral;
                    delayPerShot = 0.2f;
                    StartCoroutine(SpiralGimmick());
                    break;
                default:
                    decision += TargetAttack;
                    decision += TargetSpiral;
                    delayPerShot = 0.2f;
                    StartCoroutine(SpiralGimmick());
                    break;
            }
            duration = 25f;
        }
        else
        {
            //if you want to have it teleport to middle of stage and attack
            transform.localPosition = new Vector2(-0.2f, -5f);
            result = Random.Range(0, 3);
            switch (result)
            {
                case 0:
                    decision = StaticPattern_Flower;
                    delayPerShot = 0.45f;
                    break;
                case 1:
                    decision = StaticPatern_CircleTrack;
                    delayPerShot = 0.2f;
                    break;
                case 2:
                    decision += StaticPattern_Flower;
                    decision += TargetAttack;
                    delayPerShot = 0.45f;
                    break;
                default:
                    decision += StaticPatern_CircleTrack;
                    decision += TargetAttack;
                    delayPerShot = 0.2f;
                    break;
            }
            duration = 50f;
        }
    }
    public override void Hurt(int damage, Transform hitting)
    {
        if (hitting.gameObject.GetComponent<Projectile>() != null)
        {
            base.Hurt(damage, hitting);
            //if you want we can make it so it use it's old vector but i though i might be better to have it find a new angle to travers
            if (moving)
                Invoke("StartMovement", timer);
            else
                body.velocity = Vector2.zero;
            manager.player.canvas.GetComponent<UIBehaviour>().SetBar(UIBars.Boss, hp, HP_MAX); //UPDATE UI
        }
    }
    protected override void Death()
    {
        base.Death();
        //gameObject.SetActive(false);
        ObjectPooling.instance.TurnOffBullets();
        Destroy(gameObject);
    }
    //----------Coroutine----------//
    private IEnumerator Attacking() 
    {
        float count = 0;
        while (count < duration && manager.player.hp > 0) // make sure it attacks if the player is alive
        {
            yield return new WaitForSeconds(delayPerShot);
            decision();
            count++;
        }
        decision = null;
        Invoke("Delay", cooldown);
        RefreshAngles();
        yield return null;
    }
    private IEnumerator SpiralGimmick()
    {
        while (!ready && moving)
        {
            yield return new WaitForSeconds(2.5f);
            offset *= -1;
        }
        offset = Mathf.Abs(offset);
        yield return null;
    }
    //----------Built In Functions----------//
    new private void Start()
    {
        angles = new List<float>();
        angles.Add(0f);
        base.Start();
        this.hp = HP_MAX;
        this.moveSpeed = 2.5f;
        Invoke("StartMovement",1f);
        //Debugging for each shots
        //InvokeRepeating("StaticPatern_CircleTrack", 0.5f, betweenShot);
    }
    public void FixedUpdate()
    {
        if (ready)
        {
            Action();
            StartCoroutine(Attacking());
        }
    }
    private void Delay() 
    {
        ready = true;
    }
    private void ResumeMove() 
    {
        body.velocity = movement;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Level"))
        {
            body.velocity = Vector2.Reflect(movement.normalized, collision.GetContact(0).normal) * moveSpeed;
            //set movement to be the reflection
            movement = body.velocity;
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //touch boss = death
            manager.player.Hurt(100, transform);
        }
        else if (collision.gameObject.GetComponent<Throwable>()) 
        {
            body.velocity = Vector2.zero;
            if (moving)
                StartMovement();
        }
    }
}
