using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    // Start is called before the first frame update
    //debugging
    [SerializeField] private Vector3 size = new Vector3(2f,2f,2f);
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, size);
    }
    public void Interaction() 
    {
        //Debugging Mode//
        Debug.Log("Interaction!!!");
    }
}
