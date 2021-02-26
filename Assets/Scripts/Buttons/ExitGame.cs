using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void Exit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;    //Exit play mode if we're in the editor, Quit the game if we're not
        #else
            Application.Quit();
        #endif
    }
}

