using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace KobGamesSDKSlim
{
    [Serializable]
    public class FirebaseRemoteConfig : FirebaseRemoteConfigBase
    {
        //[Title("RemoteConfig Project Variables")]
        //[InlineProperty] public FirebaseBoolValue NewVariable = new FirebaseBoolValue(nameof(NewVariable), true);

        public override void SetDefaults()
        {
#if ENABLE_FIREBASE
            //base.ConfigDefaults.Add(NewVariable.Key, NewVariable.Value);
#endif
            base.SetDefaults();
        }

        protected override void RefrectPropertiesSafe()
        {
#if ENABLE_FIREBASE
            base.RefrectPropertiesSafe();

            try
            {
                //NewVariable.Value = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(NewVariable.Key).BooleanValue;

                //Debug.Log($@"Firebase-Project-Fetch-Refrect: 
                //        {NewVariable.Key} = {NewVariable.Value}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{nameof(FirebaseRemoteConfig)}-{Utils.GetFuncName()}(), Exception: {ex.Message}");
            }
#endif
        }

        
    }
}