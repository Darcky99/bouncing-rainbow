using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public class IphoneXDiagram : MonoBehaviour
    {
        private Canvas Canvas;

        private float IphoneXAspectRatio = 2.165333f;

        private void Awake()
        {
            Canvas = GetComponentInChildren<Canvas>();
        }

        void Update()
        {
            bool shouldEnable = Math.Round(Screen.height / (float)Screen.width, 2) == Math.Round(IphoneXAspectRatio, 2);

            if(Canvas.enabled != shouldEnable)
                Canvas.enabled = shouldEnable;
        }
}
}

