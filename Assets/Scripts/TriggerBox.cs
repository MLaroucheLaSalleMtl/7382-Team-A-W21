using UnityEngine;

public class TriggerBox : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private Door[] doors;

    //Spawns enemies and locks doors when player enters trigger box
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player;
        if (player = collision.gameObject.GetComponent<Player>())
        {
            foreach(GameObject enemy in enemies)    //Spawn enemies
            {
                enemy.SetActive(true);
            }
            foreach(Door door in doors)             //Shut and lock doors
            {
                door.CloseDoor();
                door.locked = true;
            }
            Destroy(gameObject);    //Destroy trigger box after use
        }
    }
}
