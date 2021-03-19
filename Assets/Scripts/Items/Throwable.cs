using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    //working on it with more detail after the bomb this is for things like pots and rocks etc...
    [SerializeField] protected float radius = 1.5f;
    [SerializeField] protected int damage = 2;
    [SerializeField] protected LayerMask wallDetect;
    protected bool thrown = false;
    protected bool hit = false;
    protected bool destination_reached = false;
    private GameManager manager;
    public bool Thrown { get => thrown; }

    private void Start()
    {
        manager = GameManager.instance;
    }
    public void Pickup(Transform head) 
    {
        thrown = false;
        gameObject.layer = LayerMask.NameToLayer("PlayerProjectiles");
        this.gameObject.transform.position = head.position;
        this.gameObject.transform.parent = head;
    }
    public IEnumerator Tossed(Transform start, Vector2 end)
    {
        this.thrown = true;
        float currentLocal = 0.0f;
        while (currentLocal < 1 && !hit)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, end.normalized, 1f, wallDetect);
            if (hit)
            {
                Debug.Log(hit.collider.name);
                //get current object's position
                end = transform.position;
                currentLocal = 0.9f;
            }
            //10% of the way to destination player can collide with it
            if (currentLocal > 0.1f)
                gameObject.layer = LayerMask.NameToLayer("Default");
            transform.position = Vector2.Lerp(start.position, end, currentLocal);
            currentLocal += 0.05f;
            yield return new WaitForSeconds(0.01f);
        }
        destination_reached = true;
        yield return null;
    }
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Generic object thrown");
    }

}
