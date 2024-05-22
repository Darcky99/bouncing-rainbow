using UnityEngine;
using UnityEngine.UI;

namespace KobGamesSDKSlim
{
    public class ImageToggle : MonoBehaviour
    {

        private bool m_State;

        public Image OnImage;
        public Image OffImage;

        public void Toggle()
        {
            Set(!m_State);
        }

        public void Set(bool i_State)
        {
            m_State = i_State;

            OffImage.gameObject.SetActive(!m_State);
            OnImage.gameObject.SetActive(m_State);
        }
    }
}
