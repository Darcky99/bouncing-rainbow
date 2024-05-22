using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;

namespace KobGamesSDKSlim.Collectable
{
    public class CollectableContainer : MonoBehaviour
    {
        [Title("Refs")]
        [SerializeField, ReadOnly] protected TextMeshProUGUI m_CollectableText;
        [SerializeField, ReadOnly] protected RectTransform m_CollectableIcon;
        [SerializeField, ReadOnly] protected RectTransform m_MoveTarget;

        [SerializeField, ReadOnly] protected Scaler m_TextScaler;
        [SerializeField, ReadOnly] protected Scaler m_IconScaler;

        [SerializeField, PropertyOrder(1)] private bool m_IsUseTextFormat;
        [SerializeField, PropertyOrder(1)] private bool m_IsHideBigNumbers = true;

        public RectTransform MoveTarget { get => m_MoveTarget; }

        private string m_TextFormat;

        [Button, PropertyOrder(2)]
        protected virtual void SetRefs()
        {
            m_CollectableText = transform.FindDeepChild<TextMeshProUGUI>("Collectable Text");
            if (m_CollectableText == null)
            {
                //Legacy
                m_CollectableText = transform.FindDeepChild<TextMeshProUGUI>("CoinText");
            }

            m_CollectableIcon = transform.FindDeepChild<RectTransform>("Collectable Icon");
            if (m_CollectableIcon == null)
            {
                //Legacy
                m_CollectableIcon = transform.FindDeepChild<RectTransform>("CoinIcon");
            }

            m_MoveTarget = transform.FindDeepChild<RectTransform>("Collectable Move Target");
            if (m_MoveTarget == null)
            {
                //Legacy
                m_MoveTarget = transform.FindDeepChild<RectTransform>("CoinMoveTarget");
            }


            m_TextScaler = m_CollectableText.GetComponent<Scaler>();
            m_IconScaler = m_CollectableIcon.GetComponent<Scaler>();
        }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() {}

        protected virtual void SetCollectableValue(int i_Value, bool i_PopAnim)
        {
            if (m_IsUseTextFormat)
            {
                if (m_TextFormat == null)
                {
                    m_TextFormat = m_CollectableText.text;
                }

                m_CollectableText.SetText(string.Format(m_TextFormat,
                    m_IsHideBigNumbers ? (object)hideBigNumber(i_Value) : i_Value));
            }
            else
            {
                m_CollectableText.SetText(i_Value.ToString());
            }

            if (i_PopAnim)
                popAnim();
        }

        private string hideBigNumber(int i_Num)
        {
            if (i_Num >= 100000000)
            {
                return (i_Num / 1000000D).ToString("0.#M");
            }
            if (i_Num >= 1000000)
            {
                return (i_Num / 1000000D).ToString("0.##M");
            }
            if (i_Num >= 100000)
            {
                return (i_Num / 1000D).ToString("0.#k");
            }
            if (i_Num >= 10000)
            {
                return (i_Num / 1000D).ToString("0.##k");
            }

            return i_Num.ToString();
        }

        private void popAnim()
        {
            m_IconScaler.StopAnimation();
            m_IconScaler.StartAnimation();
        }
    }
}

