using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KobGamesSDKSlim.MenuManagerV1;
using UnityEngine.UI;
using KobGamesSDKSlim.GameManagerV1;
using KobGamesSDKSlim;
using Sirenix.OdinInspector;
using TMPro;

public class Screen_LevelFailed : MenuScreenBase
{
    [SerializeField]           private TextMeshProUGUI[] m_LevelNumber;
    [SerializeField, ReadOnly] private ExtendedButton    m_TapToRestart;

    [Button]
    protected override void SetRefs()
    {
        base.SetRefs();

        m_TapToRestart = transform.FindDeepChild<ExtendedButton>("Tap To Restart");
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        m_TapToRestart.Setup(OnTapToRestart);

        foreach (var levelNumber in m_LevelNumber)
        {
            levelNumber.text = "LEVEL " + StorageManager.Instance.CurrentLevel;
        }
    }


    private void OnTapToRestart()
    {
        GameManagerBase.Instance.ResetGame();
        Close();
    }

}
