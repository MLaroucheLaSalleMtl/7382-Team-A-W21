using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBox : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private Door[] doors;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player;
        if (player = collision.gameObject.GetComponent<Player>())
        {
            foreach(GameObject enemy in enemies)
            {
                enemy.SetActive(true);
            }
            foreach(Door door in doors)
            {
                door.CloseDoor();
                door.locked = true;
            }
            Destroy(gameObject);    //Destroy trigger box after use
        }
    }
}
