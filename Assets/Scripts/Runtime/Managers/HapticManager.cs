using UnityEngine;

namespace MarbleRace.Runtime.Managers
{
    public static class HapticManager
    {
        public static void LightTap()
        {
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }

        public static void MediumImpact()
        {
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }

        public static void HeavyImpact()
        {
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }
    }
}
