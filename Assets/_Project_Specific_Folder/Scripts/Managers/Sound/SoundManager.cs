using UnityEngine;
using DG.Tweening;
using KobGamesSDKSlim;
using KobGamesSDKSlim.SoundManagerV1;
using UnityEngine.Rendering;

namespace KobGamesSDKSlim
{
    [ExecutionOrder(eExecutionOrder.SoundManager)]
    public class SoundManager : SoundManagerBase
    {
        protected override void OnAwakeEvent()
        {
            base.OnAwakeEvent();
            SetSFXMute(false);
        }

        private AudioClip m_CustomClipSFX;
        private float m_Pitch, m_Volume;
        private bool m_Play;
        public void PlayCustomClip(AudioClip i_CustomClipSFX, float i_Pitch, float i_Volume = 1)
        {
            //PlaySFX(i_CustomClipSFX, i_Pitch, i_Volume);
            m_Pitch = i_Pitch;
            m_Volume = i_Volume;
            m_CustomClipSFX = i_CustomClipSFX;
            m_Play = true;
        }

        private void LateUpdate()
        {
            if (m_Play)
            {
                PlaySFX(m_CustomClipSFX, m_Pitch, 1);
                m_Play = false;
            }
        }
    }
}
