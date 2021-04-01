using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : MonoBehaviour
{
    //debugging test
    public static int damage = 3;
    private const float timer = 2f;
    [SerializeField] private AudioSource hitWallSound;
    private GameManager manager;
    // Start is called before the first frame update
    private void Start()
    {
        manager = GameManager.instance;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Level") || collision.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            Debug.Log("Hit");
            //hit a rock or something / unbreakable (prob add push back)
            hitWallSound.Play();
            manager.player.Hurt(0, transform);
        }
        else if (collision.gameObject.CompareTag("Breakable"))
        {
            Destroy(collision.gameObject, timer);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            collision.gameObject.GetComponent<LivingEntity>().Hurt(damage, transform.parent);
        }
    }
}
