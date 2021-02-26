using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UIBars
{
    Health,
    Stamina,
    Boss
}
public class UIBehaviour : MonoBehaviour
{
    //Editor Settings
    [Header("Attachements")]
    [Tooltip("The health bar's rectTransform compnonent")] [SerializeField] private RectTransform healthBar;
    [Tooltip("The health bar's rectTransform component")] [SerializeField] private RectTransform staminaBar;
    [Tooltip("The boss bar's rectTransform component")] [SerializeField] private RectTransform bossBar;
    [Tooltip("The dash light's image component")] [SerializeField] private Image dashLight;
    [Tooltip("Dash light's lit icon")] [SerializeField] private Sprite dashLit;
    [Tooltip("Dash light's unlit icon")] [SerializeField] private Sprite dashUnlit;
    [Tooltip("Item slot icons lit")] [SerializeField] private Sprite[] slotSpritesLit = new Sprite[3];
    [Tooltip("Item slot icons unlit")] [SerializeField] private Sprite[] slotSpritesUnlit = new Sprite[3];
    [Tooltip("Quickswap bar Icons")] [SerializeField] private Image[] slotIcons = new Image[3];
    [Tooltip("Slots in game")] [SerializeField] private Image[] slotImages = new Image[3];
    [Tooltip("The boss game object")] [SerializeField] public GameObject boss;
    [Tooltip("Boss bar name component")] [SerializeField] public Text barName;
    [Header("Settings")]
    [Tooltip("How quickly in seconds do the bars fill or empty")] [SerializeField] private float updateSpeed = 0.25f;

    //Internals 
    private float maxHealthWidth;
    private float maxStaminaWidth;
    private float maxBossWidth;
    private float timeSinceSwap = 1f;
    private Animator animator;
    //Built in methods
    public void Start()
    {
        Debug.Log("Start UI behaviour");
        maxHealthWidth = healthBar.sizeDelta.x;
        maxStaminaWidth = staminaBar.sizeDelta.x;
        maxBossWidth = bossBar.sizeDelta.x;
        if (boss != null) barName.text = boss.name;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (timeSinceSwap < 1f)
        {
            timeSinceSwap += Time.deltaTime;
            animator.SetBool("QSwapVisible", true);
        }
        else
        {
            animator.SetBool("QSwapVisible", false);
        }
        if(boss != null)
        {
            if (boss.activeSelf)
            {
                animator.SetBool("bossBar", true);
            }
            else
            {
                animator.SetBool("bossBar", false);
            }
        }
        else
        {
            animator.SetBool("bossBar", false);
        }

    }

    //Custom methods
    
    public void UpdateIcons(int item)
    {
        Debug.Log(item);
        if (item-2 >= 0)
        {
            for (int i = 0; i < 3; i++)
            {
                if (item-2 >= i)
                {
                    slotIcons[i].enabled = true;
                }
                else
                {
                    slotIcons[i].enabled = false;
                }
            }
        }
    }

    public void UpdateSlots(WeaponSlot slot)
    {
        if ((int)slot > 1 && (int)slot < 5)
        {
            for(int i = 0; i < 3; i++)
            {
                slotImages[i].sprite = slotSpritesUnlit[i];
            }
            slotImages[(int)slot - 2].sprite = slotSpritesLit[(int)slot - 2];
            timeSinceSwap = 0f;
        }
    }
    public void SetDashLight(bool lit)
    {
        dashLight.sprite = lit ? dashLit : dashUnlit;
    }

    public void SetBar(UIBars bar, float num, float outof)
    {
        switch (bar)
        {
            case UIBars.Health:
                StartCoroutine(changeBar(healthBar,maxHealthWidth, (num > 0 ? num : 0) / outof));
                break;
            case UIBars.Stamina:
                StartCoroutine(changeBar(staminaBar, maxStaminaWidth, (num > 0 ? num : 0) / outof));
                break;
            case UIBars.Boss:
                StartCoroutine(changeBar(bossBar, maxBossWidth, (num > 0 ? num : 0) / outof));
                break;
            default:
                break;
        }
        
    }

    //Coroutines
    private IEnumerator changeBar(RectTransform bar,float width, float fraction)
    {
        float count = 0;
        float dif = (fraction * width) - bar.sizeDelta.x;
        float start = bar.sizeDelta.x;
        while (count < updateSpeed)
        {

            bar.sizeDelta= new Vector2(start + (dif*count)/updateSpeed, 12);

            count += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();

        }
    }

}
