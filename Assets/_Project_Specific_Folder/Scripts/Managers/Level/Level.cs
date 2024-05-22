using KobGamesSDKSlim;
using KobGamesSDKSlim.MenuManagerV1;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    private GamePlayVariablesEditor m_GameplayVariables => GameConfig.Instance.GamePlay;
    private StorageManager m_StorageManager => StorageManager.Instance;
    private MenuManager m_MenuMangaer => MenuManager.Instance;

    #region Init
    private void OnEnable()
    {
        GameManager.OnLevelStarted += onLevelStarted;
    }
    private void OnDisable()
    {
        GameManager.OnLevelStarted -= onLevelStarted;
    }
    #endregion

    private void onLevelStarted()
    {

    }
}
