using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicJarManager : MonoBehaviour
{
    public List<magicJar> jars;
    [SerializeField] private List<bool> solution;
    [SerializeField] private Door exitSolve;
    // Start is called before the first frame update
    void Start()
    {
        foreach(magicJar jar in jars)
        {
            if (jar.magicManager == null)
            {
                jar.magicManager = this;
            }
        }
    }

    public void checkForSolve()
    {
        bool solved = true;
        for (int i = 0; i < jars.Count; i++)
        {
            if (jars[i].broken != solution[i])
            {
                Debug.Log("Jar" + (i + 1) + " is wrong");
                solved = false;
            }
        }
        if (solved)
        {
            if (exitSolve) exitSolve.OpenDoor();
        }
        else
        {
            bool allBroken = true;
            foreach(magicJar jar in jars)
            {
                if (!jar.broken) allBroken = false;
            }
            if (allBroken)
            {
                foreach (magicJar jar in jars)
                {
                    jar.BringBack();
                }
            }
        }
    }

}
