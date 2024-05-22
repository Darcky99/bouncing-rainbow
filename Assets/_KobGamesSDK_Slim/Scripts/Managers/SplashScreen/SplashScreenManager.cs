using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace KobGamesSDKSlim
{
    public class SplashScreenManager : Singleton<SplashScreenManager>
    {
        [HideInInspector] public Action OnSplashScreenShownStart = delegate {};
        [HideInInspector] public Action OnSplashScreenShownFinished = delegate { };

        public bool ShowSplash = false;
        public bool ShowSplashOnEditor = false;

        [ReadOnly] public bool IsFinished = true;

        [Title("Config")]
        public float LogoFadeDuration = 1.75f;
        public float LogoFadeDelay = 1;
        public float BackgroundFadeDuration = 1.25f;
        public float BackgroundFadeDelay = 0;

        private Canvas m_SplashCanvas;
        private Image m_SplashBackground;
        private Image m_SplashLogo;

        protected override void OnAwakeEvent()
        {
            base.OnAwakeEvent();

            initRefs();

            if (Application.isEditor && !ShowSplashOnEditor || !Application.isEditor && !ShowSplash)
                return;

            showSplash();
        }        

        private void showSplash()
        {
            IsFinished = false;

            m_SplashBackground?.gameObject.SetActive(true);

            Sequence seq = DOTween.Sequence();

            seq.InsertCallback(0.025f, () => OnSplashScreenShownStart.InvokeSafe())
               .AppendInterval(LogoFadeDelay)
               .Append(m_SplashLogo?.DOFade(0, LogoFadeDuration).OnComplete(GC.Collect))
               .AppendInterval(BackgroundFadeDelay)
               .InsertCallback(LogoFadeDelay + LogoFadeDuration + BackgroundFadeDelay + BackgroundFadeDuration / 2, setIsFinished)
               .Append(m_SplashBackground?.DOFade(0, BackgroundFadeDuration))
               .OnComplete(onSequenceCompleted)
               .OnKill(onSequenceCompleted);
        }

        private void onSequenceCompleted()
        {
            m_SplashCanvas?.gameObject.SetActive(false);
            OnSplashScreenShownFinished.InvokeSafe();
        }

        private void setIsFinished()
        {
            IsFinished = true;
        }

        private void Reset()
        {
            initRefs();
        }

        private void OnValidate()
        {
            if (base.m_IsDestroyed)
                return;

            initRefs();
        }

        private void initRefs()
        {
            if (m_SplashCanvas == null)
            {
                m_SplashCanvas = this.GetComponentInChildren<Canvas>();
            }

            if (m_SplashBackground == null)
            {
                m_SplashBackground = this.transform.FindDeepChild<Image>("SplashBackground");
            }

            if (m_SplashLogo == null)
            {
                m_SplashLogo = this.transform.FindDeepChild<Image>("SplashLogo");
            }
        }
    }
}