using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBoss : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private Door door;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player;
        if (player = collision.gameObject.GetComponent<Player>())
        {
            foreach(GameObject enemy in enemies)
            {
                enemy.SetActive(true);
            }
        }
        door.CloseDoor();
        door.locked = true;
        Destroy(gameObject);
    }
}
