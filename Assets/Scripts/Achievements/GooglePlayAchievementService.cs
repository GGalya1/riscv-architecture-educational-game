// This entire class is compiled ONLY for Android device builds.
#if UNITY_ANDROID && !UNITY_EDITOR

using System;
using GooglePlayGames;

/// <summary>
/// Google Play Games Services achievement implementation.
/// </summary>
public class GooglePlayAchievementService : IAchievementService
{
    public bool IsAuthenticated =>
        PlayGamesPlatform.Instance != null &&
        PlayGamesPlatform.Instance.IsAuthenticated();

    public void Initialize(Action<bool> onResult = null)
    {
        // Activate the plugin (must be called once before anything else)
        PlayGamesPlatform.Activate();

        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            bool isSuccess = (status == GooglePlayGames.BasicApi.SignInStatus.Success);
            
            CustomLog.LogEditor(isSuccess
                ? "[GPGS] Sign-in successful."
                : $"[GPGS] Sign-in failed. Status: {status}");
            
            onResult?.Invoke(isSuccess);
        });
    }

    public void Unlock(string achievementId)
    {
        if (!IsAuthenticated || string.IsNullOrEmpty(achievementId)) return;

        // ReportProgress to 100% == unlock for standard achievements
        PlayGamesPlatform.Instance.ReportProgress(achievementId, 100.0f, success =>
        {
            if (!success)
                CustomLog.LogEditorError($"[GPGS] Failed to unlock achievement: {achievementId}");
        });
    }

    public void Increment(string achievementId, int steps = 1)
    {
        if (!IsAuthenticated || string.IsNullOrEmpty(achievementId)) return;

        PlayGamesPlatform.Instance.IncrementAchievement(achievementId, steps, success =>
        {
            if (!success)
                CustomLog.LogEditorError($"[GPGS] Failed to increment achievement: {achievementId}");
        });
    }

    public void ShowUI()
    {
        if (!IsAuthenticated) return;
        PlayGamesPlatform.Instance.ShowAchievementsUI();
    }
}

#endif
