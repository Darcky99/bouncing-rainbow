using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;

namespace KobGamesSDKSlim.UI
{
    /// <summary>
    /// Coin Display Refs
    /// </summary>
    public class CoinContainer : MonoBehaviour
    {
        [Title("Refs")]
        [SerializeField, ReadOnly] protected TextMeshProUGUI m_CoinText;
        [SerializeField, ReadOnly] protected RectTransform m_CoinIcon;
        [SerializeField, ReadOnly] protected RectTransform m_CoinMoveTarget;

        [SerializeField, ReadOnly] protected Scaler m_TextScaler;
        [SerializeField, ReadOnly] protected Scaler m_IconScaler;

        [SerializeField] private bool m_IsUseTextFormat;

        public RectTransform CoinMoveTarget { get => m_CoinMoveTarget; }

        private string m_TextFormat;

        [Button]
        protected virtual void SetRefs()
        {
            m_CoinText = transform.FindDeepChild<TextMeshProUGUI>("CoinText");
            m_CoinIcon = transform.FindDeepChild<RectTransform>("CoinIcon");
            m_CoinMoveTarget = transform.FindDeepChild<RectTransform>("CoinMoveTarget");

            m_TextScaler = m_CoinText.GetComponent<Scaler>();
            m_IconScaler = m_CoinIcon.GetComponent<Scaler>();
        }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() {}

        private void updateCoinsText(int i_CoinsAmount)
        {
        }

        public virtual void SetCoins(int i_Value, bool i_PopAnim)
        {
            if (m_IsUseTextFormat)
            {
                if (m_TextFormat == null)
                {
                    m_TextFormat = m_CoinText.text;
                }

                m_CoinText.SetText(string.Format(m_TextFormat, i_Value));
            }
            else
            {
                m_CoinText.SetText(i_Value.ToString());
            }

            if (i_PopAnim)
                PopAnim();
        }

        public void PopAnim()
        {
            m_IconScaler.StopAnimation();
            m_IconScaler.StartAnimation();
        }
    }
}

