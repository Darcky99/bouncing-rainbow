using System;
using Sirenix.OdinInspector;

namespace KobGamesSDKSlim
{
    [Serializable]
    public class WebToolsEditor
    {
        public bool IsWebToolsEnabled;
        [ShowIf("IsWebToolsEnabled"), OnValueChanged(nameof(ManagersReset))]
        public bool IsFeedbackEnabled;
        [ShowIf("IsFeedbackEnabled")]
        public int FeedbackQuestionLimit = 3;
#if ENABLE_FEEDBACK
    [ShowIf("IsFeedbackEnabled"), InlineEditor]
    public FeedbackPopupOpenLogicBase PopupOpenLogic;
#endif
        [ShowIf("IsFeedbackEnabled")]
        public bool IsFeedbackRewardEnabled = false;
        [ShowIf("IsFeedbackRewardEnabled")]
        public int FeedbackRewardAmount = 100;
        [ShowIf("IsFeedbackRewardEnabled")]
        public string FeedbackRewardCurrency = "GEMS!";

        public void ManagersReset()
        {            
            Managers.Instance.Reset();
        }
    }
}
