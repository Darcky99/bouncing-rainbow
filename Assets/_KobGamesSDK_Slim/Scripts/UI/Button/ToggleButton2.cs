using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace KobGamesSDKSlim.UI
{
    public class ToggleButton2 : MonoBehaviour
    {
        [SerializeField, ReadOnly] private ExtendedButton m_Button;
        [SerializeField, ReadOnly] private TextMeshProUGUI m_Title;
        [SerializeField, ReadOnly] private ImageToggle m_ToggleImage;

        public delegate void ToggleCallback(bool i_State);
        public event ToggleCallback OnToggle = delegate { };

        private bool m_isOn;

        [Button]
        protected void SetRefs()
        {
            m_Button = GetComponentInChildren<ExtendedButton>();
            m_Title = transform.FindDeepChild<TextMeshProUGUI>("Title");
            m_ToggleImage = GetComponentInChildren<ImageToggle>();

        }

        protected virtual void OnEnable()
        {
            m_Button.Setup(OnButtonDown);

            SetState(m_isOn);
        }

        public virtual void Set(string i_Title, bool i_State, ToggleCallback i_Callback)
        {
            m_Title.text = i_Title;
            SetState(i_State);
            OnToggle = i_Callback;
        }

        protected virtual void SetState(bool i_SetTrue)
        {
            m_ToggleImage.Set(i_SetTrue);
            m_isOn = i_SetTrue;

            OnToggle?.Invoke(m_isOn);
        }

        protected virtual void OnButtonDown()
        {
            SetState(!m_isOn);
        }
    }
}