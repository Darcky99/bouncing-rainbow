using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public abstract class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
    {
        //Making sure no Object can override Awake (Only implementation insde Singleton)
        protected virtual void Awake()
        {
        }
    }
}
