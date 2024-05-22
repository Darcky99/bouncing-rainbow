using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KobGamesSDKSlim
{
    [Serializable]
    public class GDPRData
    {
        public bool IsGDPRRequired = false;

        public string PrivacyPolicyURL = "";
        public string TermsOfServiceURL = "";

        public TextData Text;

        [Serializable]
        public class TextData
        {
            public string TitleText = "We've updated our Terms";
            public string TermsOfServiceLinkText = "Terms of Service";
            public string PrivacyPolicyLinkText = "Privacy Policy";
            public string AgreeConditionText = "By tapping \"Accept\" you agree to them";
            public string AcceptButtonText = "Accept";
        }
    }
}