using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using KobGamesSDKSlim;

public class VibrationButton : MonoBehaviour
{
    [SerializeField, ReadOnly] private ExtendedButton m_VibrationButton;
    [SerializeField, ReadOnly] private ImageToggle m_VibrationImageToggle;

    [Button]
    private void SetRefs()
    {
        m_VibrationButton = GetComponentInChildren<ExtendedButton>();
        m_VibrationImageToggle = GetComponentInChildren<ImageToggle>();

    }

    private void OnEnable()
    {
        m_VibrationButton.Setup(OnVibrationButtonClick);

        UpdateVibrationButtonImage();
    }

    private void OnVibrationButtonClick()
    {
        //TODO- this should be on Haptics Manager
        StorageManager.Instance.IsVibrationOn = !StorageManager.Instance.IsVibrationOn;
        UpdateVibrationButtonImage();
    }

    private void UpdateVibrationButtonImage()
    {
        m_VibrationImageToggle.Set(StorageManager.Instance.IsVibrationOn);
    }
}
