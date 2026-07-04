using System;

/// <summary>
/// No-op achievement service, currently used on all non-Android platforms.
///
/// All methods are intentionally empty - completely safe to call anywhere.
/// No platform SDK required.
/// </summary>
public class NullAchievementService : IAchievementService
{
    public bool IsAuthenticated => false;

    public void Initialize(Action<bool> onResult = null)
    {
        // Signal "not authenticated" without blocking anything
        onResult?.Invoke(false);
    }

    public void Unlock(string achievementId) { }

    public void Increment(string achievementId, int steps = 1) { }

    public void ShowUI() { }
}