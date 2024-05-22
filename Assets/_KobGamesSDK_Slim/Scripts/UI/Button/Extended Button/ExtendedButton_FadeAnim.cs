using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using KobGamesSDKSlim.Animation;
using System;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;


namespace KobGamesSDKSlim
{
    public class ExtendedButton_FadeAnim : ExtendedButton
    {
        private string FadeDelayID { get => gameObject.GetInstanceID().ToString(); }
        private string FadeAnimID { get => gameObject.GetInstanceID() + "" + gameObject.GetInstanceID(); }

        /// <summary>
        /// When you Setup a button it will automatically Reset its state when disabled
        /// It will setup a fade animation according to TweenData
        /// </summary>
        /// <param name="i_TweenData"></param>
        /// <param name="i_OnClick"></param>
        /// <param name="i_OnDownEvent"></param>
        /// <param name="i_OnUpEvent"></param>
        /// <param name="i_OnBeginDragEvent"></param>
        /// <param name="i_OnDragEvent"></param>
        /// <param name="i_OnEndDragEvent"></param>
        public void Setup(TweenData i_TweenData,
            UnityAction i_OnClick = null,
            Action<PointerEventData> i_OnDownEvent = null,
            Action<PointerEventData> i_OnUpEvent = null,
            Action<PointerEventData> i_OnBeginDragEvent = null,
            Action<PointerEventData> i_OnDragEvent = null,
            Action<PointerEventData> i_OnEndDragEvent = null,
            InteractivityChangedCallback i_OnInteractivityChanged = null)
        {
            if (m_IsSetup)
            {
                Debug.LogError("Button already Setup. Skiping!!!");
                return;
            }


            m_IsSetup = true;
            SetEvents(i_OnClick, i_OnDownEvent, i_OnUpEvent, i_OnBeginDragEvent, i_OnDragEvent, i_OnEndDragEvent, i_OnInteractivityChanged);


            interactable = false;
            SetGraphicsVisibility(false);

            DOTween.Kill(FadeDelayID, FadeAnimID);
            DOVirtual.DelayedCall(i_TweenData.Delay, OnFadeAnimDelayCallback)
                .SetId(FadeDelayID);
            FadeButton(true, i_TweenData);
        }


        protected virtual void OnFadeAnimDelayCallback()
        {
            SetInteractable(true);
        }

        public virtual void FadeButton(bool i_Enable, TweenData i_TweenData, TweenCallback i_OnComplete = null)
        {
            DOTween.Kill(FadeAnimID);

            //Debug.LogError("i_TweenData " + i_TweenData.Delay + "   " + i_TweenData.Duration, gameObject);

            m_CanvasGroup.DOFade(Convert.ToInt32(i_Enable), i_TweenData.Duration)
                .SetEase(i_TweenData.Ease)
                .SetDelay(i_TweenData.Delay)
                .SetId(this)
                .OnComplete(i_OnComplete);
        }


        public override void ResetState()
        {
            DOTween.Complete(FadeDelayID);
            DOTween.Complete(FadeAnimID);

            base.ResetState();
        }

  

        #region Debug
#if UNITY_EDITOR
        [ContextMenu("Animate Assets In")]
        private void AnimateAssetsInExample()
        {
            FadeButton(true, new TweenData(.1f, 1, Ease.InOutSine, 0));
        }
        [ContextMenu("Animate Assets Out")]
        private void AnimateAssetsOutExample()
        {
            FadeButton(false, new TweenData(.1f, 1, Ease.InOutSine, 0));
        }
#endif
        #endregion
    }
}
