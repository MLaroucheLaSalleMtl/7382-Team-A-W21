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
    private bool ready = true;
    private bool moving;
    private const int HP_MAX = 10; //change if necesary...
    //----------Variable----------//
    [Header("Direction")]
    [SerializeField] private List<Transform> node;
    [SerializeField] private Transform previous_node;
    [Space]
    [Header("Projectile")]
    [SerializeField] private int bulletnum = 1;
    private const float bulletSpeed = 5f;
    private int index;
    [SerializeField] private List<float> angles;
    //[SerializeField] private float offset; // used for reverse rotation didnt use keeping incase...
    //----------Movement Selection----------//
    //makes sure the node found is not the same one
    private Transform FindNode() 
    {
        Transform Destination = node[Random.Range(0, node.Count)];
        if (previous_node != null) 
        {
            return (previous_node == Destination) ? FindNode() : (previous_node = Destination);
        }
        Debug.Log($"going to : {Destination.name}");
        return Destination;
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
            Vector2 projectileMove = (new Vector2(positionX, positionY) - (Vector2)transform.position).normalized * speed;
            GameObject temp = ObjectPooling.instance.GetBullet();
            temp.transform.position = transform.position;
            temp.SetActive(true);
            temp.GetComponent<Rigidbody2D>().velocity = projectileMove;
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
        if(Random.Range(0f, 1f) < 0.5f)
        {
            moving = true;
            StartCoroutine(Move());
        }
        else 
            moving = false;
        Attack();
    }
    //normal attack bullet
    public void TargetAttack() 
    {
        Vector2 middle_shot = (manager.player.transform.position - transform.position).normalized;
        Vector2 second_shot = Rotate(25f, middle_shot);
        Vector2 third_shot = Rotate(-25f, middle_shot);
        
        Shot(ObjectPooling.instance.GetBullet(),middle_shot,bulletSpeed);
        Shot(ObjectPooling.instance.GetBullet(), second_shot, bulletSpeed);
        Shot(ObjectPooling.instance.GetBullet(), third_shot, bulletSpeed);
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
        StandardPattern(bulletnum, 5, bulletSpeed, out shot_volley);
        StopGimmick(shot_volley);
        StandardPattern(bulletnum, -5, bulletSpeed, out shot_volley);
        StopGimmick(shot_volley);
        StandardPattern(2, 10, bulletSpeed + (bulletSpeed / 4), out _);
        StandardPattern(2, -10, bulletSpeed + (bulletSpeed / 4), out _);
        angles.RemoveAt(index);
        index = 0;
    }
    public void StaticPatern_CircleTrack() 
    {
        GameObject[] volley;
        StandardPattern(4, -1, bulletSpeed, out _);
        StandardPattern(1, 15, bulletSpeed, out volley);
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
        //play animation
        //base.Attack();
        //add stuff in delegate here
        if (moving)
        {
            decision = TargetAttack;
        }
        else 
        {
            if (Random.Range(0f, 1f) < 0.5f)
                decision = StaticPattern_Flower;
            else
                decision = StaticPatern_CircleTrack;
        }
    }
    public override void Hurt(int damage, Transform hitting)
    {
        if(hitting.gameObject.GetComponent<Projectile>() != null)
            base.Hurt(damage, hitting);
        this.body.velocity = Vector2.zero;
        manager.player.canvas.GetComponent<UIBehaviour>().SetBar(UIBars.Boss, hp, HP_MAX); //UPDATE UI
    }
    protected override void Death()
    {
        base.Death();
        gameObject.SetActive(false);
    }
    //----------Coroutine----------//
    private IEnumerator Move() 
    {
        Vector2 startpoint = transform.position;
        float currentLocal = 0f;
        //gotoPoint = node[Random.Range(0, node.Count)].position;
        gotoPoint = FindNode().position;
        while (currentLocal <= 1 && gameObject.activeSelf) 
        {
            transform.position = Vector2.Lerp(startpoint, gotoPoint, currentLocal);
            currentLocal += 0.01f;
            yield return new WaitForSeconds(0.02f);
        }
        Debug.Log("Destination Reached");
        yield return null;
    }
    private IEnumerator Attacking() 
    {
        float count = 0;
        float timer = (decision != TargetAttack) ? 50f : 10f;
        float delay = (decision == StaticPatern_CircleTrack) ? 0.2f : 0.5f;
        //add more to condition if there's more attack
        while (count < timer) 
        {
            yield return new WaitForSeconds(delay);
            decision();
            count++;
        }
        decision = null;
        ready = true;
        RefreshAngles();
        yield return null;
    }
    //----------Built In Functions----------//
    private void Start()
    {
        angles = new List<float>();
        angles.Add(0f);
        base.Start();
        this.hp = HP_MAX;
        

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
}
