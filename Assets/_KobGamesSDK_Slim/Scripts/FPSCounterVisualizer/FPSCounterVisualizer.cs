using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KobGamesSDKSlim;

//TODO - Replace FPSObject with the new Unity 2020.2 features :D
public class FPSCounterVisualizer : MonoBehaviour
{
    public Button ToggleButton;
    public GameObject FPSObject;

    public void Awake()
    {
        ToggleButton.onClick.AddListener(()=> { FPSObject.SetActive(!FPSObject.activeSelf); });   
    }

    public void Update()
    {
        ToggleButton.gameObject.SetActive(GameConfig.Instance.Debug.ShowFPSCounter);
        if(!ToggleButton.gameObject.activeSelf)
            FPSObject.gameObject.SetActive(false);
    }
}
