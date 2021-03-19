using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class optionsMenu : MonoBehaviour
{
    [SerializeField] private Dropdown resolutionList;
    [SerializeField] private AudioMixer mixer;
    private Resolution[] resolutions;
    private void updateResolution ()
    {
        List<string> dropOptions = new List<string>();
        int count = 0;
        int position = 0;
        foreach (Resolution res in resolutions)
        {
            string name = res.ToString();
            dropOptions.Add(name);
            count++;
            if (Screen.currentResolution.ToString() == name)
            {
                position = count;
            }
        }
        resolutionList.ClearOptions();
        resolutionList.AddOptions(dropOptions);
        resolutionList.SetValueWithoutNotify(position);
    }

    public void testApply()
    {
        Resolution newRes = resolutions[resolutionList.value];
        Screen.SetResolution(newRes.width, newRes.height, Screen.fullScreenMode, newRes.refreshRate);
        GameManager.instance.ReturnToMain();
        GameManager.instance.DisableMenu(gameObject, false);
    }

    public void changeSFX(float value)
    {
        mixer.SetFloat("SFXVol", value);
    }

    public void changeMusic(float value)
    {
        mixer.SetFloat("MusicVol", value);
    }


    // Start is called before the first frame update
    void Start()
    {
        resolutions = Screen.resolutions;
        updateResolution();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
