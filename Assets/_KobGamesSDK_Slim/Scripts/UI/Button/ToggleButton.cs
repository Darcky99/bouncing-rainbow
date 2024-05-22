using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace KobGamesSDKSlim.UI
{
    public class ToggleButton : MonoBehaviour
    {
        [SerializeField, ReadOnly] private Button m_Button;
        [SerializeField, ReadOnly] private TextMeshProUGUI m_Title;
        [SerializeField, ReadOnly] private ImageToggle m_ToggleImage;
        [SerializeField, ReadOnly] private Transform m_OffPosition;
        [SerializeField, ReadOnly] private Transform m_OnPosition;

        public delegate void ToggleCallback(bool i_State);
        public event ToggleCallback OnToggle = delegate { };

        private bool m_isOn;

        private Transform m_DummyTransform;

        [Button]
        protected void SetRefs()
        {
            m_Button = GetComponentInChildren<Button>();
            m_Title = transform.FindDeepChild<TextMeshProUGUI>("Title");
            m_ToggleImage = GetComponentInChildren<ImageToggle>();
            m_OffPosition = transform.FindDeepChild<Transform>("offPosition");
            m_OnPosition = transform.FindDeepChild<Transform>("onPosition");

        }

        protected virtual void OnEnable()
        {
            m_Button.onClick.AddListener(OnButtonDown);

            //forcing just to make sure animations doesn't break the system
            SetState(m_isOn, false);
        }

        protected virtual void OnDisable()
        {
            m_Button.onClick.RemoveAllListeners();
        }

        public virtual void Set(string i_Title, bool i_State, ToggleCallback i_Callback)
        {
            m_Title.text = i_Title;
            SetState(i_State, false);

            OnToggle = i_Callback;
        }

        protected virtual void SetState(bool i_SetTrue, bool i_Animate)
        {
            DOTween.Kill(this);

            m_DummyTransform = i_SetTrue ? m_OnPosition : m_OffPosition;

            if (i_Animate)
                m_ToggleImage.transform.DOLocalMove(m_DummyTransform.transform.localPosition, .25f)
                    .SetEase(Ease.OutCirc)
                    .SetId(this);
            else
                m_ToggleImage.transform.localPosition = m_DummyTransform.transform.localPosition;


            m_ToggleImage.Set(i_SetTrue);
            m_isOn = i_SetTrue;

            OnToggle?.Invoke(m_isOn);
        }

        protected virtual void OnButtonDown()
        {
            DOTween.Kill(this);
            SetState(!m_isOn, true);
        }
    }
}