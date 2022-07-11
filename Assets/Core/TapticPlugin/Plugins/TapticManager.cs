using System.Runtime.InteropServices;
using System.Collections.Generic;

public enum TapticNotification
{
    Success,
    Warning,
    Error
}

public enum TapticImpact
{
    Light,
    Medium,
    Heavy,
    None
}

public static class TapticManager
{
    private static readonly Dictionary<TapticImpact, int> _androidVibroDurations = new Dictionary<TapticImpact, int>() {{TapticImpact.Light, 15}, {TapticImpact.Medium, 25}, {TapticImpact.Heavy, 35}};

    public static void Notification(TapticNotification feedback)
    {
        if (StatePersister.Instance.TapticEnabled)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            _unityTapticNotification((int)feedback);
#elif UNITY_ANDROID && !UNITY_EDITOR
            if (feedback == TapticNotification.Success)
                Vibration.Vibrate(new long[] { 0, 50, 200, 200 }, -1);
            else if (feedback == TapticNotification.Error ||feedback == TapticNotification.Warning)
                Vibration.Vibrate(new long[] { 0, 200, 200, 50 }, -1);
#endif
        }
    }

    public static void Impact(TapticImpact feedback = TapticImpact.Medium)
    {
        if (StatePersister.Instance.TapticEnabled)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            _unityTapticImpact((int)feedback);
#elif UNITY_ANDROID && !UNITY_EDITOR
            int duration;
            _androidVibroDurations.TryGetValue(feedback, out duration);
            Vibration.Vibrate(duration);
#endif
        }
    }

    public static void Selection()
    {
        if (StatePersister.Instance.TapticEnabled)
        {
            _unityTapticSelection();
        }
    }

    static void AndroidVibrate()
    {
        if (StatePersister.Instance.TapticEnabled)
        {
            UnityEngine.Handheld.Vibrate();
        }
    }

    public static bool IsSupport()
    {
        return _unityTapticIsSupport();
    }

    #region DllImport

#if UNITY_IPHONE && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _unityTapticNotification(int type);
    [DllImport("__Internal")]
    private static extern void _unityTapticSelection();
    [DllImport("__Internal")]
    private static extern void _unityTapticImpact(int style);
    [DllImport("__Internal")]
    private static extern bool _unityTapticIsSupport();
#else
    private static void _unityTapticNotification(int type)
    {
    }

    private static void _unityTapticSelection()
    {
    }

    private static void _unityTapticImpact(int style)
    {
    }

    private static bool _unityTapticIsSupport()
    {
        return Vibration.HasVibrator();
    }
#endif

    #endregion // DllImport
}