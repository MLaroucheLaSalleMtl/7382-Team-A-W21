using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIBehaviour : MonoBehaviour
{
    //Editor Settings
    [Header("Attachements")]
    [Tooltip("The health bar's rectTransform compnonent")] [SerializeField] private RectTransform healthBar;
    [Tooltip("The health bar's rectTransform component")] [SerializeField] private RectTransform staminaBar;
    [Tooltip("The dash light's image component")] [SerializeField] private Image dashLight;
    [Tooltip("Dash light's lit icon")] [SerializeField] private Sprite dashLit;
    [Tooltip("Dash light's unlit icon")] [SerializeField] private Sprite dashUnlit;
    [Header("Settings")]
    [Tooltip("How quickly in seconds do the bars fill or empty")] [SerializeField] private float updateSpeed = 0.25f;

    //Internals 
    private float maxHealthWidth;
    private float maxStaminaWidth;

    //Built in methods
    private void Start()
    {
        maxHealthWidth = healthBar.sizeDelta.x;
        maxStaminaWidth = staminaBar.sizeDelta.x;
    }

    //Custom methods
    public void SetDashLight(bool lit)
    {
        dashLight.sprite = lit ? dashLit : dashUnlit;
    }

    public void SetBar(bool isHealthBar, float num, float outof)
    {
        StartCoroutine(changeBar(isHealthBar ? healthBar : staminaBar, isHealthBar ? maxHealthWidth:maxStaminaWidth, (num>0?num:0) / outof));
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
