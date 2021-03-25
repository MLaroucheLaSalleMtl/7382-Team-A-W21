using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    [SerializeField] private bool raisedAtStart;    //Whether or not the spikes should start raised
    private Animator anim;

    private void Start()
    {
        this.anim = GetComponent<Animator>();
        this.anim.SetBool("SpikesOn", raisedAtStart);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Player player;
        if (player = collision.collider.GetComponent<Player>()) //If the player walks into a spike
            player.Hurt(1, this.transform);
    }
    public void InvertSpikes()
    {
        anim.SetBool("SpikesOn", !(anim.GetBool("SpikesOn")));  //Invert the bool value of SpikesOn
        GetComponent<AudioSource>().Play();
    }

}
