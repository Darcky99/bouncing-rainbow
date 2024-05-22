using DG.Tweening;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace KobGamesSDKSlim
{
	public class AnimatorBase : MonoBehaviour
	{
		
		[ReadOnly] public bool m_IsPlaying;
		[ReadOnly] public Tween Tween;

		[Space]

		public bool PlayOnEnable;
		public bool RandomStartTime = false;
		[Range(0, 1)] public float StartTime = 0;

		[Space]
		public bool UseDotweenID = false;

		[HideIf(nameof(UseDotweenID))]
		public int Id = k_NoId;
		protected const int k_NoId = -9999;
		[ShowIf(nameof(UseDotweenID)), ValueDropdown(nameof(DotweenIDs))]
		public int DoTweenID = DOTweenIDs.Default;

		protected int FinalID { get => UseDotweenID ? DoTweenID : Id; }


		public float Duration = 1;
		public Ease Ease = Ease.Linear;
		public bool IsRelative;

		[Space]

		public UpdateType UpdateType = UpdateType.Normal;
		public bool UpdateFrameIndependant = false;

		[Space]

		public int LoopCount = 0;
		private bool m_UsingLoops => LoopCount > 0 || LoopCount == -1;
		[ShowIf(nameof(m_UsingLoops))]
		public LoopType LoopType;

		private ValueDropdownList<int> DotweenIDs()
        {
			return DOTweenIDs.ValuesDropdownData;
        }

		protected virtual void Awake()
		{
			if (RandomStartTime)
			{
				StartTime = Random.Range(0.0f, 1.0f);
			}
		}

		protected virtual void Start()
		{
			SetRestartAction();
		}

		protected virtual void SetRestartAction()
		{
		}

		protected virtual void OnEnable()
		{
			GameManager.OnGameReset += OnGameReset;

			ResetValues();
			if (PlayOnEnable)
				StartAnimation();
		}

		protected virtual void OnDisable()
		{
			GameManager.OnGameReset -= OnGameReset;

			StopAnimation();
		}

		protected virtual void OnGameReset()
        {
			if (!DOTWeenIDsBase.IdsBase.Contains(FinalID))
			{
				ResetValues();
				//Debug.LogError(FinalID + "		" + DOTWeenIDsBase.IdsBase.Contains(FinalID), gameObject);
			}
		}

		[Button]
		public virtual void ResetValues()
		{
			StopAnimation();
		}

		public virtual void StartAnimation()
		{
			Tween?.Kill(true);
			m_IsPlaying = true;
		}

		public virtual void StopAnimation()
		{
			m_IsPlaying = false;
			Tween?.Kill(true);
		}

		public virtual void RestartAnimation()
		{
			ResetValues();
			StartAnimation();
		}

		public virtual void PauseAnimation()
        {
			Tween?.Pause();
        }

		public void ContinueAnimation()
        {
			Tween?.Play();
        }

		public virtual void SetId(Tween i_Tween)
        {
			if (FinalID != k_NoId)
			{
				if (i_Tween != null)
				{
					i_Tween.SetId(FinalID);
				}
			}
        }
	}
}