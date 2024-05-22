using KobGamesSDKSlim;
using KobGamesSDKSlim.MenuManagerV1;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class VolumeButton : MonoBehaviour
{
    [SerializeField] private ExtendedButton m_ExtendedButton;
    private SoundManager m_SoundManager => SoundManager.Instance;

    [Button]
    private void setRefs()
    {
        m_ExtendedButton = GetComponent<ExtendedButton>();
        m_Image = transform.GetChild(0).GetComponent<Image>();
    }
    private void OnEnable()
    {
        m_IsMuted = m_SoundManager.IsSFXMuted;
        updateIcon();
        m_ExtendedButton.Setup(OnVolumeButtonDown);
    }

    private bool m_IsMuted; // false --- !false ---> true | //true --- !true ---> false
    private void OnVolumeButtonDown()
    {
        m_SoundManager.SetMusicMute(!m_IsMuted);
        m_SoundManager.SetSFXMute(!m_IsMuted);
        m_IsMuted = !m_IsMuted;
        updateIcon();
    }
    [SerializeField] private Image m_Image;
    [SerializeField] private Sprite m_VolumeOn, m_VolumeOff;
    private void updateIcon()
    {
        m_Image.sprite = m_IsMuted ? m_VolumeOff : m_VolumeOn;
    }
}
