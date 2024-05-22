using UnityEngine;
using Sirenix.OdinInspector;

namespace KobGamesSDKSlim.SoundManagerV1
{
	[ExecutionOrder(eExecutionOrder.SoundManager)]
	public class SoundManagerBase : Singleton<SoundManager>
	{
		[ShowInInspector, ReadOnly, BoxGroup("States")]
		public bool IsMusicMuted
		{
			get
			{
				return StorageManager.Instance.IsMusicMuted;
			}
			set
			{
				StorageManager.Instance.IsMusicMuted = value;
			}
		}

		[ShowInInspector, ReadOnly, BoxGroup("States")]
		public bool IsSFXMuted
		{
			get
			{
				return StorageManager.Instance.IsSFXMuted;
			}
			set
			{
				StorageManager.Instance.IsSFXMuted = value;
			}
		}

		[Header("Audio Sources")]
		public AudioSource MusicSource;
		public AudioSource SFXGameAudioSource;
		public AudioSource SFXUIAudioSource;

		[Header("Clips")]
		public AudioClip UIClickSFX;

		protected override void OnAwakeEvent()
		{
			base.OnAwakeEvent();
			UpdateAudioSourcesMuteState();
			//SetMusicMute(IsMusicMuted);
			//SetSFXMute(IsSFXMuted);
		}

		public override void Start()
		{
			base.Start();
		}

		protected virtual void OnEnable()
		{
			GameManager.OnGameReset += OnGameReset;
		}

		public override void OnDisable()
		{
			base.OnDisable();

			GameManager.OnGameReset -= OnGameReset;
		}

		protected virtual void OnGameReset()
		{
		}

		#region Play Sounds
		public virtual void PlayUIClickSound()
		{
			if (SFXUIAudioSource != null)
				SFXUIAudioSource.PlayOneShot(UIClickSFX);
		}

		public virtual void PlaySFX(AudioClip i_AudioClip, float pitch, float i_VolumeScale = 1)
		{
			if (i_VolumeScale > 0)
			{
				SFXGameAudioSource.pitch = pitch;
				SFXGameAudioSource.PlayOneShot(i_AudioClip, i_VolumeScale);

			}
		}

		public virtual void PlayMusic(AudioClip i_AudioClip)
		{
			MusicSource.clip = i_AudioClip;
			PlayMusic();
		}
		public virtual void PlayMusic()
		{
			MusicSource.Play();
		}
		#endregion

		#region Music Toggle
		[BoxGroup("Toggle")]
		public virtual void ToggleMusicMute()
		{
			SetMusicMute(!IsMusicMuted);
		}

		public virtual void SetMusicMute(bool i_IsMute)
		{
			IsMusicMuted = i_IsMute;
			UpdateAudioSourcesMuteState();

			//Debug.LogError(string.Format("Set Music Mute: {0}", i_MuteValue));
		}
		#endregion

		#region SFX Toggle
		[BoxGroup("Toggle")]
		public virtual void ToggleSFXMute()
		{
			SetSFXMute(!IsSFXMuted);
		}

		public virtual void SetSFXMute(bool i_IsMute)
		{
			IsSFXMuted = i_IsMute;
			UpdateAudioSourcesMuteState();

			//Debug.LogError(string.Format("Set SFX Mute: {0}", i_MuteValue));
		}
		#endregion

		protected void UpdateAudioSourcesMuteState()
		{
			if (MusicSource != null) MusicSource.mute = IsMusicMuted;
			if (SFXUIAudioSource != null) SFXUIAudioSource.mute = SFXGameAudioSource.mute = IsSFXMuted;
		}
	}
}