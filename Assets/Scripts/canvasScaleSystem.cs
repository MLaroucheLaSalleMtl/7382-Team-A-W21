using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class canvasScaleSystem : MonoBehaviour
{
    private CanvasScaler canvasScaler;
    private int lastSize = 0;
    // Start is called before the first frame update
    void Start()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        lastSize = Screen.height;
        canvasScaler.scaleFactor = ((float)lastSize / 290f);
    }

    // Update is called once per frame
    void Update()
    {
        
        if (lastSize != Screen.height)
        {
            Debug.Log("Rescaling...");
            lastSize = Screen.height;
            canvasScaler.scaleFactor = ((float)lastSize / 290f);
        }
        
    }
}
