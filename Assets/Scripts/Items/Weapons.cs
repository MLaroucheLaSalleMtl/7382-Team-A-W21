using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : MonoBehaviour
{
    //debugging test
    public static int damage = 3;
    private const float timer = 2f;
    [SerializeField] private AudioSource hitWallSound;

    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 9 || collision.gameObject.layer == 8)
        {
            collision.GetComponent<LivingEntity>().Hurt(damage, gameObject.transform.parent.transform);
        }
        else if (collision.CompareTag("Breakable"))
        {
            //the and can be removed just wanted to be double sure it's an interactable layer
            //play animation

            //destroy
            Destroy(collision.gameObject,timer);
        }
        else
        {
            //hit a rock or something / unbreakable (prob add push back)
            Debug.Log("hitting rock");
            hitWallSound.Play();
        }
    }
    //Note: remember to remake the animation for attacking
}
