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

    //If the player walks into the spikes
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Player player;
        if (player = collision.collider.GetComponent<Player>())     //Damage the player
            player.Hurt(1, this.transform);
    }

    //Raise or lower the spikes
    public void InvertSpikes()
    {
        anim.SetBool("SpikesOn", !(anim.GetBool("SpikesOn")));      //Invert the bool value of SpikesOn
        GetComponent<AudioSource>().Play();
    }
}
