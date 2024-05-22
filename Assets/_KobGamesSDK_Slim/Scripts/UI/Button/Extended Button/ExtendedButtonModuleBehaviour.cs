using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace KobGamesSDKSlim
{
	public enum eExtendedButtonModuleType
	{
		ColorMultiply,
		ColorSet,
		SpriteSwap
	}

	public class ExtendedButtonModuleBehaviour : MonoBehaviour
	{
		private eButtonState m_CurrState = eButtonState.Normal;
		
		[SerializeField] private eExtendedButtonModuleType m_Type;

		[SerializeField, HideIf(nameof(m_Type), eExtendedButtonModuleType.SpriteSwap)]
		private bool m_CopyColorStatesFromParent = true;

		[SerializeField, ShowIf(nameof(m_ShowColorStates))]
		private ColorBlock m_ColorStates;
		private bool m_ShowColorStates => !m_CopyColorStatesFromParent && m_Type != eExtendedButtonModuleType.SpriteSwap;

		[SerializeField, ShowIf(nameof(m_Type), eExtendedButtonModuleType.SpriteSwap)]
		private SpriteState m_SpriteState;

		[SerializeField] private bool m_CopyImageMaterialFromParent = true;

		
		[Header("Refs")] [SerializeField, ReadOnly]
		private Image m_Image;

		[SerializeField, ReadOnly] private Color  m_ImageOriginalColor;
		[SerializeField, ReadOnly] private Sprite m_ImageOriginalSprite;

		[SerializeField, ReadOnly] private Text  m_Text;
		[SerializeField, ReadOnly] private Color m_TextOriginalColor;

		[SerializeField, ReadOnly] private TextMeshProUGUI m_TextsTMP;
		[SerializeField, ReadOnly] private Color           m_TextsTMPOriginalColor;
		[SerializeField, ReadOnly] private Color           m_TextsTMPOutlineOriginalColor;


#if UNITY_EDITOR
		//Needs to be public due to custom Editor
		[Button]
		public virtual void SetRefs()
		{
			ExtendedButton parentButton = GetComponentsInParent<ExtendedButton>(true).First();

			if (parentButton == null)
			{
				Debug.LogError("Couldn't find parent Button - " + gameObject.name, gameObject);
				return;
			}

			m_Image    = GetComponent<Image>();
			m_Text     = GetComponent<Text>();
			m_TextsTMP = GetComponent<TextMeshProUGUI>();


			parentButton.UpdateModuleData(this);

			if (!parentButton.interactable)
				return;

			setOriginalColorData();

			if (m_Image != null) m_ImageOriginalSprite = m_Image.sprite;


			UnityEditor.EditorUtility.SetDirty(this);
		}
#endif

		private void setOriginalColorData()
		{
			if (m_Image != null) m_ImageOriginalColor = m_Image.color;
			if (m_Text != null) m_TextOriginalColor   = m_Text.color;
			if (m_TextsTMP != null)
			{
				m_TextsTMPOriginalColor = m_TextsTMP.color;

				if (m_TextsTMP.fontSharedMaterial != null)
					m_TextsTMPOutlineOriginalColor = m_TextsTMP.fontSharedMaterial.GetColor(ShaderUtilities.ID_OutlineColor);
			}
		}

#if UNITY_EDITOR
		protected void OnValidate()
		{
			SetRefs();

			if (m_Image == null && m_Type == eExtendedButtonModuleType.SpriteSwap)
			{
				Debug.LogError("Can't set module as SpriteSwap if it doesn't have an image");
				m_Type = eExtendedButtonModuleType.ColorMultiply;
			}
		}
#endif

		public void SetData(Material i_Material, ColorBlock i_ColorBlock)
		{
			if (m_Image != null && m_CopyImageMaterialFromParent) m_Image.material = i_Material;
			if (m_CopyColorStatesFromParent) m_ColorStates                         = i_ColorBlock;
		}
		
		public void SetState(eButtonState i_ButtonState)
		{
			if (m_CurrState == eButtonState.Normal)
				setOriginalColorData();
			
			switch (m_Type)
			{
				case eExtendedButtonModuleType.ColorMultiply:
					setColorMultiplyState(i_ButtonState);
					break;
				case eExtendedButtonModuleType.ColorSet:
					setColorState(i_ButtonState);
					break;
				case eExtendedButtonModuleType.SpriteSwap:
					setSpriteState(i_ButtonState);
					break;
				default:
					break;
			}
			
			m_CurrState = i_ButtonState;
		}

		private void setColorMultiplyState(eButtonState i_ButtonState)
		{
			Color multiplyColor = Color.white;

			switch (i_ButtonState)
			{
				case eButtonState.Disabled:
					multiplyColor = m_ColorStates.disabledColor;
					break;
				default:
					break;
			}

			if (m_Image != null) m_Image.color = m_ImageOriginalColor * multiplyColor;
			if (m_Text != null) m_Text.color   = m_TextOriginalColor * multiplyColor;
			if (m_TextsTMP != null)
			{
				m_TextsTMP.color = m_TextsTMPOriginalColor * multiplyColor;
				m_TextsTMP.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, m_TextsTMPOutlineOriginalColor * multiplyColor);
			}
		}

		private void setColorState(eButtonState i_ButtonState)
		{
			switch (i_ButtonState)
			{
				case eButtonState.Normal:
					setItemsColor(m_ColorStates.normalColor);
					break;
				case eButtonState.Disabled:
					setItemsColor(m_ColorStates.disabledColor);
					break;
				default:
					break;
			}
		}

		private void setSpriteState(eButtonState i_ButtonState)
		{
			switch (i_ButtonState)
			{
				case eButtonState.Normal:
					m_Image.sprite = m_ImageOriginalSprite;
					break;
				case eButtonState.Disabled:
					m_Image.sprite = m_SpriteState.disabledSprite;
					break;
				default:
					break;
			}
		}

		private void setItemsColor(Color i_Color)
		{
			if (m_Image != null) m_Image.color = i_Color;
			if (m_Text != null) m_Text.color   = i_Color;
			if (m_TextsTMP != null)
			{
				m_TextsTMP.color = i_Color;
				//TODO - what happens with outlines?
				// m_TextsTMP.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, i_Color);
			}
		}


		
	}
}