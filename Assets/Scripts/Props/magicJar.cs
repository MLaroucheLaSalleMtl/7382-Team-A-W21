using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class magicJar : LivingEntity
{
    
    [HideInInspector] public MagicJarManager magicManager;
    public bool broken = false;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetBool("broken", broken);
    }

    public void BringBack()
    {
        if (broken)
        {
            broken = false;
            anim.SetBool("broken", false);
        }
    }

    public override void Hurt(int damage, Transform hitting)
    {
        if (!broken)
        {
            broken = true;
            anim.SetBool("broken", true);
            if (magicManager) magicManager.checkForSolve(); 
        }
    }

}
