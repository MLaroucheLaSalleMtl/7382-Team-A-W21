using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    //----------Singleton Value----------//
    public static ObjectPooling instance;
    //----------Final Boss Bullet Goes Here----------//
    [SerializeField] private GameObject prefab;
    [SerializeField] private List<GameObject> objectpooling;
    [Range(1f, 1000f)] [SerializeField] private int bulletNum;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
            Destroy(gameObject);
        objectpooling = new List<GameObject>();
        for(int i = 0; i < bulletNum; i++) 
        {
            GameObject temp = Instantiate(prefab);
            temp.SetActive(false);
            objectpooling.Add(temp);
        }
    }

    public GameObject GetBullet() 
    {
        for (int i = 0; i < objectpooling.Count; i++) 
        {
            if (!objectpooling[i].activeInHierarchy)
                return objectpooling[i];
        }
        //if there's no space add more space till equalibrium
        GameObject temp = Instantiate(prefab);
        temp.SetActive(false);
        objectpooling.Add(temp);
        return temp;
    }
    public void TurnOffBullets() 
    {
        foreach (GameObject g in objectpooling)
            g.SetActive(false);
    }
}
