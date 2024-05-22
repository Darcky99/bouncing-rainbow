using System;
using MoreMountains.NiceVibrations;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public class HapticManager : MonoBehaviour
    {
        public void Haptic(HapticTypes i_Haptic, bool defaultToRegularVibrate = false, bool allowVibrationOnLegacyDevices = true)
        {
            if (Managers.Instance != null && Managers.Instance.IsHapticEnabled && StorageManager.Instance.IsVibrationOn)
            {
                MMVibrationManager.Haptic(i_Haptic, defaultToRegularVibrate, allowVibrationOnLegacyDevices);
            }
        }

        [Obsolete("Legacy. Just a wrapper for older projects.")]
        public void Haptic(Taptics.HapticTypes i_Haptic)
        {
            switch (i_Haptic)
            {
                case Taptics.HapticTypes.Warning:
                    Haptic(HapticTypes.Warning);
                    break;
                case Taptics.HapticTypes.Failure:
                    Haptic(HapticTypes.Failure);
                    break;
                case Taptics.HapticTypes.Success:
                    Haptic(HapticTypes.Success);
                    break;
                case Taptics.HapticTypes.LightImpact:
                    Haptic(HapticTypes.LightImpact);
                    break;
                case Taptics.HapticTypes.MediumImpact:
                    Haptic(HapticTypes.MediumImpact);
                    break;
                case Taptics.HapticTypes.HeavyImpact:
                    Haptic(HapticTypes.HeavyImpact);
                    break;
                case Taptics.HapticTypes.Selection:
                    Haptic(HapticTypes.Selection);
                    break;
                default:
                    Haptic(HapticTypes.Success);
                    break;
            }
        }

        // Dummy function so Unity will add to AndroidManifest.xml
        // <uses-permission android:name="android.permission.VIBRATE" />
        public void DummyFunction()
        {
            Handheld.Vibrate();
        }
    }
}