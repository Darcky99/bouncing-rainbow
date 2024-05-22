using KobGamesSDKSlim;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SpriteColorChanger : AnimatorBase
{
	[SerializeField] private SpriteRenderer m_SpriteRenderer = null;
	[SerializeField] private Image          m_Image          = null;

	[SerializeField] private Gradient m_Gradient = null;

	[Button]
	private void SetReferences()
	{
		m_SpriteRenderer = GetComponent<SpriteRenderer>();
		m_Image          = GetComponent<Image>();
	}
	
	[Button]
	public override void StartAnimation()
	{
		base.StartAnimation();

		ResetValues();
		
		if (m_SpriteRenderer != null)
		{
			Tween = m_SpriteRenderer.DOGradientColor(m_Gradient, Duration)
				.SetEase(Ease)
				.SetRelative(IsRelative)
				.SetLoops(LoopCount, LoopType)
				.SetUpdate(UpdateType, UpdateFrameIndependant);
		}
		else if (m_Image != null)
		{
			Tween = m_Image.DOGradientColor(m_Gradient, Duration)
				.SetEase(Ease)
				.SetRelative(IsRelative)
				.SetLoops(LoopCount, LoopType)
				.SetUpdate(UpdateType, UpdateFrameIndependant);
		}
		else
		{
			Debug.LogError("ColorChanger does not have valid target for changing color.", gameObject);
			return;
		}

		base.SetId(Tween);

		if (StartTime > 0 && StartTime < 1)
			this.Tween.Goto(Mathf.Lerp(0, Duration, StartTime), true);
	}

	public override void ResetValues()
	{
		base.ResetValues();

		if (m_SpriteRenderer)
		{
			m_SpriteRenderer.color = m_Gradient.Evaluate(0f);
		}
		else if (m_Image)
		{
			m_Image.color = m_Gradient.Evaluate(0f);
		}
	}
}