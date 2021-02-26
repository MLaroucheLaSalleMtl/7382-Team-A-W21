using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoComplete : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameManager.instance.DemoComplete();
    }
}
