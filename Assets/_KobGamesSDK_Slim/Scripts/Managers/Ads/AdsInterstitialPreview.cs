using System;
using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public class AdsInterstitialPreview : MonoBehaviour
    {
        [Title("Configs")]
        public bool IsEnabled = true;
        public int StepCount = 3;
        public float DelayPerStep = 0.5f;

        [Title("Refs")]
        public GameObject Root;
        public TMP_Text CountText;

        private Action m_ShowAdCallback;
        private Action m_CloseCallback;

        [HorizontalGroup("Screen")]
        [Button, VerticalGroup("Screen/Left")]
        public void Show()
        {
            Show(null);
        }

        public void Show(Action i_ShowAdCallback, Action i_CloseCallback = null)
        {
            if (!IsEnabled)
            {
                i_ShowAdCallback.InvokeSafe();
                i_CloseCallback.InvokeSafe();
            }
            else
            {
                m_ShowAdCallback = i_ShowAdCallback;
                m_CloseCallback = i_CloseCallback;
                CountText.text = $"{StepCount}";

                Root.SetActive(true);

                StopAllCoroutines();
                StartCoroutine(startStepsCountDown());
            }
        }

        [Button, VerticalGroup("Screen/Right")]
        public void Close()
        {
            m_CloseCallback.InvokeSafe();
            m_CloseCallback = null;

            Root.SetActive(false);

            StopAllCoroutines();
        }

        private float m_DelayNow = 0;
        private IEnumerator startStepsCountDown()
        {
            for (int step = StepCount; step >= 0; step--)
            {
                CountText.text = $"{step}";
                m_DelayNow = DelayPerStep;
                while (m_DelayNow > 0)
                {
                    yield return null;
                    m_DelayNow -= Time.unscaledDeltaTime;
                }

                // On Counter == 1 we invoke m_ShowAdCallback since ads SDKs takes a little time before they show
                // Interstitials on device
                if (step == 1)
                {
                    m_ShowAdCallback.InvokeSafe();
                    m_ShowAdCallback = null;
                }
            }

            Close();
        }

        [Button]
        public void SetRefs()
        {
            Root = this.gameObject.FindObject<GameObject>("Root");
            CountText = this.transform.FindDeepChild("Count").GetComponent<TMP_Text>();
        }
    }
}