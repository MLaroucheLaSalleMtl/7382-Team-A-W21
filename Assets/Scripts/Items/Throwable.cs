using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    //working on it with more detail after the bomb this is for things like pots and rocks etc...
    [SerializeField] protected float radius = 1.5f;
    [SerializeField] protected int damage = 2;
    protected bool thrown = false;
    protected bool destination_reached = false;
    private const int tossSpeed = 15;
    private GameManager manager;
    protected Rigidbody2D body;
    [HideInInspector]public AudioSource pickupSound;
    [SerializeField] protected AudioSource wallHitSound;
    public bool Thrown { get => thrown; }

    protected void Start()
    {
        manager = GameManager.instance;
        pickupSound = GetComponent<AudioSource>();
        body = GetComponent<Rigidbody2D>();
    }
    public void Pickup(Transform head) 
    {
        thrown = false;
        destination_reached = false;
        body.bodyType = RigidbodyType2D.Kinematic;
        gameObject.layer = LayerMask.NameToLayer("Bomb");
        this.gameObject.transform.position = head.position;
        this.gameObject.transform.parent = head;
    }
    public IEnumerator Tossed(Vector2 start, Vector2 end)
    {
        this.thrown = true;
        Vector2 distance = (end - start);
        body.velocity = distance.normalized * tossSpeed;
        body.isKinematic = false;
        gameObject.layer = LayerMask.NameToLayer("PlayerProjectiles");
        while (!destination_reached && gameObject.activeSelf)
        {
            //check distance
            Debug.Log(Vector2.Distance(transform.position, end));
            if (Vector2.Distance(transform.position, end) < 0.11f) //needs to be 0.11, due to how fixed update works
            {
                transform.position = end;
                body.velocity = Vector2.zero;
                gameObject.layer = LayerMask.NameToLayer("Default");
                destination_reached = true;
            }
            yield return new WaitForFixedUpdate();
        }
        yield return null;
    }
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Level"))
        {
            //check if the player is looking a wall
            //RaycastHit2D hit = Physics2D.Raycast(manager.player.transform.position,)
            wallHitSound.Play();
            body.velocity = Vector2.zero;
            destination_reached = true;
        }
        else if (!(collision.gameObject.layer == LayerMask.NameToLayer("Enemy")))
            body.bodyType = RigidbodyType2D.Static;
        gameObject.layer = LayerMask.NameToLayer("Default");
        
    }

}
