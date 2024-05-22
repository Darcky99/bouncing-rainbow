using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace KobGamesSDKSlim
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] private bool InvertProgress = false;
        [SerializeField, ReadOnly] private Image ProgressBarImage;

# if UNITY_EDITOR
        [Button]
        protected virtual void SetRefs()
        {
            ProgressBarImage = transform.FindDeepChild<Image>("InnerMask");
        }
#endif

        public virtual void Open()
        {
            gameObject.SetActive(true);
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);

        }

        public virtual void SetProgress(float i_Progress)
        {
            if (InvertProgress)
                i_Progress = 1 - i_Progress;

            ProgressBarImage.fillAmount = i_Progress;
        }
    }

}
