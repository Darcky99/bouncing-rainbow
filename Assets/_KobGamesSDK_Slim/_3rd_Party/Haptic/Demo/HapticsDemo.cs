using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.NiceVibrations;
using System;
using KobGamesSDKSlim;

public class HapticsDemo : MonoBehaviour
{
    private Button OriginalButton;

    void Start()
    {
        OriginalButton = GetComponentInChildren<Button>();

        for (int i = 0; i < Enum.GetNames(typeof(HapticTypes)).Length; i++)
        {
            GameObject go;
            if (i != 0)
                go = Instantiate(OriginalButton, OriginalButton.transform.parent).gameObject;
            else
                go = OriginalButton.gameObject;

            HapticTypes type = (HapticTypes)i;

            go.GetComponent<Button>().onClick.RemoveAllListeners();
            go.GetComponent<Button>().onClick.AddListener(()=>Managers.Instance.HapticManager.Haptic(type));
            go.GetComponentInChildren<Text>().text = ((HapticTypes)i).ToString();
        }
    }
}
