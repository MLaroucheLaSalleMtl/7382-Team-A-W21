using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class magicJar : LivingEntity
{
    
    [HideInInspector] public MagicJarManager magicManager;
    public bool broken = false;

    new void Start()
    {
        base.anim = GetComponent<Animator>();
        base.anim.SetBool("broken", broken);
    }

    public void BringBack()
    {
        if (broken)
        {
            broken = false;
            base.anim.SetBool("broken", false);
        }
    }

    public override void Hurt(int damage, Transform hitting)
    {
        if (!broken)
        {
            broken = true;
            base.anim.SetBool("broken", true);
            GetComponent<AudioSource>().Play();
            if (magicManager) magicManager.checkForSolve(); 
        }
    }

}
