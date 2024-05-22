using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
#if UNITY_IOS || UNITY_EDITOR
using UnityEngine.iOS;
#endif
using UnityEngine.UI;

namespace KobGamesSDKSlim
{
    public enum ePopupMode
    {
        LevelLoad = 1,
        LevelComplete = 2
    }

    public class RateUsPopUp : MonoBehaviour
    {
        public Button ButtonLater;
        public Button ButtonNo;
        public Button ButtonYes;

        public ePopupMode RateUsPopupMode { get { return GameSettings.Instance.RateUs.RateUsPopupMode; } }

        public List<int> RateUsLevels { get { return GameSettings.Instance.RateUs.RateUsLevels; } }

        [ShowInInspector, ReadOnly]
        public string RateUsLevelsBlackList
        {
            get { return PlayerPrefs.GetString(nameof(RateUsLevelsBlackList), ""); }
            set { PlayerPrefs.SetString(nameof(RateUsLevelsBlackList), value); }
        }

        [ShowInInspector, ReadOnly, InfoBox("Won't allow to show if player hit 'yes' / 'no'")]
        public bool IsAllowedToShow
        {
            get { return PlayerPrefs.GetInt(nameof(IsAllowedToShow), 1) == 1; }
            set { PlayerPrefs.SetInt(nameof(IsAllowedToShow), value == true ? 1 : 0); }
        }

        public bool IsRateUsEnabled { get { return GameSettings.Instance.RateUs.IsRateUsEnabled; } }

        public Canvas CanvasRoot;

        public RateUsPopupSchema[] Schemas;

        [ValueDropdown(nameof(Schemas)), OnValueChanged(nameof(OnSelectedSchema))]
        public RateUsPopupSchema SelectedSchema;

        [OnValueChanged(nameof(onStarsDefaultStateChange))]
        public bool StarsOnByDefault = false;

        [FoldoutGroup("References"), ValidateInput(nameof(mustNotBeNull), "$Message")]
        public Image PanelBg;
        [FoldoutGroup("References"), ValidateInput(nameof(mustNotBeNull), "$Message")]
        public Image TitleBg;
        [FoldoutGroup("References"), ValidateInput(nameof(mustNotBeNull), "$Message")]
        public TextMeshProUGUI TitleText;
        [FoldoutGroup("References"), ValidateInput(nameof(mustNotBeNull), "$Message")]
        public Image[] Stars;
        [FoldoutGroup("References"), ValidateInput(nameof(mustNotBeNull), "$Message")]
        public Button[] StarsButtons;
        [FoldoutGroup("References"), ValidateInput(nameof(mustNotBeNull), "$Message")]
        public Image YesButtonBg;
        [FoldoutGroup("References"), ValidateInput(nameof(mustNotBeNull), "$Message")]
        public TextMeshProUGUI YesButtonTextColor;

        #region Editor Validations
        private bool mustNotBeNull(dynamic i_Object)
        {
            return i_Object != null;
        }

        private void OnValidate()
        {
            if (PanelBg == null || TitleBg == null || TitleText == null || Stars == null || YesButtonBg == null || YesButtonTextColor == null || StarsButtons == null)
            {
                Debug.LogError($"{nameof(RateUsPopUp)}: One of the fields is missing a reference, please check.");
            }
        }
        #endregion

        [HideInInspector]
        public string Message = "This field is missing a reference.";

        private void onStarsDefaultStateChange()
        {
            updateStarsSprites();

            Managers.Instance.ApplyPrefabInstance();

            m_RepaintCanvas = true;
        }

        public bool IsFiveStars
        {
            get
            {
                bool result = true;

                foreach (var star in Stars)
                {
                    if (star.sprite == SelectedSchema.StarOff)
                    {
                        result = false;
                        break;
                    }
                }

                return result;
            }
        }

        public int NumberOfStars
        {
            get
            {
                int count = 0;

                foreach (var star in Stars)
                {
                    if (star.sprite == SelectedSchema.StarOn)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        private bool m_RepaintCanvas = false;
        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            if (m_RepaintCanvas)
            {
                CanvasRoot.gameObject.SetActive(false);
                CanvasRoot.gameObject.SetActive(true);

                m_RepaintCanvas = false;
            }
        }

        public void OnSelectedSchema()
        {
#if UNITY_EDITOR
            PanelBg.sprite = SelectedSchema.PanelBg;
            TitleBg.sprite = SelectedSchema.TitleBg;
            TitleText.color = SelectedSchema.TitleTextColor;
            YesButtonBg.sprite = SelectedSchema.YesButtonBg;
            YesButtonTextColor.color = SelectedSchema.YesButtonTextColor;

            updateStarsSprites();

            Managers.Instance.ApplyPrefabInstance();

            //var so = new UnityEditor.SerializedObject(this);
            //so.Update();
            //so.FindProperty("SelectedSchema").objectReferenceValue = SelectedSchema;
            //Debug.LogError(so.FindProperty("SelectedSchema").displayName);
            //Debug.LogError(so.FindProperty("SelectedSchema").objectReferenceValue.name);
            //so.ApplyModifiedProperties();
            //UnityEditor.EditorUtility.SetDirty(SelectedSchema);
#endif
        }

        private void updateStarsSprites()
        {
            foreach (var star in Stars)
            {
                star.sprite = StarsOnByDefault == true ? SelectedSchema.StarOn : SelectedSchema.StarOff;
            }
        }

        private void OnEnable()
        {
            if (ButtonLater != null) ButtonLater.onClick.AddListener(ButtonLaterOnClick);
            if (ButtonNo != null) ButtonNo.onClick.AddListener(ButtonNoOnClick);
            if (ButtonYes != null) ButtonYes.onClick.AddListener(ButtonYesOnClick);

            // If stars are disabled by default, only enable rate us button after first interaction with the stars
            if (!StarsOnByDefault)
            {
                ButtonYes.interactable = false;
            }

            foreach (var starButton in StarsButtons)
            {
                if (starButton != null) starButton.onClick.AddListener(() => StarButtonOnClick(starButton));
            }
        }

        private void OnDisable()
        {
            if (ButtonLater != null) ButtonLater.onClick.RemoveListener(ButtonLaterOnClick);
            if (ButtonNo != null) ButtonNo.onClick.RemoveListener(ButtonNoOnClick);
            if (ButtonYes != null) ButtonYes.onClick.RemoveListener(ButtonYesOnClick);

            foreach (var starButton in StarsButtons)
            {
                if (starButton != null) starButton.onClick.RemoveAllListeners();
            }
        }

        public void StarButtonOnClick(Button i_Button)
        {
            ButtonYes.interactable = true;

            int index = i_Button.transform.GetSiblingIndex();

            for (int i = 0; i <= index; i++)
            {
                if (Stars.Length > i && Stars[i] != null)
                {
                    Stars[i].sprite = SelectedSchema.StarOn;
                }
            }

            for (int i = index + 1; i < Stars.Length; i++)
            {
                if (Stars[i] != null)
                {
                    Stars[i].sprite = SelectedSchema.StarOff;
                }
            }

            //Debug.LogError($"Clicked on {i_Button.name} index: {i_Button.transform.GetSiblingIndex()}");
        }

        public void ButtonLaterOnClick()
        {
            Debug.Log($"{nameof(RateUsPopUp)}: Player clicked on Later");

            Managers.Instance.Analytics.LogRateUsEvent("Later");

            CloseRateUs();
        }

        public void ButtonNoOnClick()
        {
            Debug.Log($"{nameof(RateUsPopUp)}: Player clicked on No");

            IsAllowedToShow = false;

            Managers.Instance.Analytics.LogRateUsEvent("No");

            CloseRateUs();
        }

        public void ButtonYesOnClick()
        {
            Debug.Log($"{nameof(RateUsPopUp)}: Player clicked on Yes");

            IsAllowedToShow = false;

            Managers.Instance.Analytics.LogRateUsEvent("Yes", NumberOfStars);

            if (IsFiveStars)
            {
                GameSettings.Instance.General.OpenAppStoreURL();
            }

            CloseRateUs();
        }

        private bool m_IsShownRateUs = false;
        public bool TryShowRateUsOnSpecificLevel(int i_LevelId, ePopupMode i_PopupMode)
        {
            m_IsShownRateUs = false;
            if (IsRateUsEnabled)
            {
                if (i_PopupMode == RateUsPopupMode)
                {
                    if (RateUsLevels.Contains(i_LevelId) && !isRateUsLevelBlackListed(i_LevelId))
                    {
                        if (IsAllowedToShow)
                        {
                            Debug.Log($"{nameof(RateUsPopUp)}: Showing rate us dialog on level: " + i_LevelId);

                            addRateUsLevelBlackList(i_LevelId);

                            ShowRateUs();

                            m_IsShownRateUs = true;
                        }
                        else
                        {
                            Debug.Log($"{nameof(RateUsPopUp)}: Not allowed to show RateUs popup since player already chosen 'no' or 'yes'");
                        }
                    }
                }
            }

            return m_IsShownRateUs;
        }

        private void addRateUsLevelBlackList(int i_LevelId)
        {
            RateUsLevelsBlackList += $"_{i_LevelId}_";
        }

        private bool isRateUsLevelBlackListed(int i_LevelId)
        {
            bool isBlacklisted = RateUsLevelsBlackList.Contains($"_{i_LevelId}_");

            if (isBlacklisted)
                Debug.Log($"{nameof(RateUsPopUp)}: Not showing rate us dialog since level {i_LevelId} has already been shown");

            return isBlacklisted;
        }

        public void CloseRateUs()
        {
            if (CanvasRoot != null)
            {
                CanvasRoot.gameObject.SetActive(false);
            }
        }

        public void ShowRateUs()
        {
#if UNITY_IOS || UNITY_EDITOR
            if (Device.RequestStoreReview())
            {
                Debug.Log("ShowRateUs(): Device.RequestStoreReview() is available");
                return;
            }
#endif

            // Incase iOS native popup failed, we fallback to default popup
            if (CanvasRoot != null)
            {
                CanvasRoot.gameObject.SetActive(true);
            }
        }
    }
}