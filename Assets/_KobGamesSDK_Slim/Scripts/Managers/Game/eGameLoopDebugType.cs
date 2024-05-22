using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KobGamesSDKSlim.Debugging
{
    public enum eGameLoopDebugType
    {
        Complete,
        FailedWithRevive,
        Failed,
        PrevLevel,
        NextLevel,
        ResetLevel,
        ResetGame,
        ToggleGDPR
    }
}
