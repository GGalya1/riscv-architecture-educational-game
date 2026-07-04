using System;

/// <summary>
/// Platform-agnostic interface for all achievement services.
/// Implement this for any platform (Google Play, Apple GameCenter, Steam, etc.).
/// </summary>
public interface IAchievementService
{
    bool IsAuthenticated { get; }

    /// <summary>
    /// Initialize and sign in. Calls onResult(true) on success, onResult(false) on failure or unsupported platform.
    /// </summary>
    void Initialize(Action<bool> onResult = null);

    /// <summary>Unlocks a standard (non-incremental) achievement.</summary>
    void Unlock(string achievementId);

    /// <summary>Increments an incremental achievement by <paramref name="steps"/>.</summary>
    void Increment(string achievementId, int steps = 1);

    /// <summary>Opens the platform's achievements overlay UI (unknown, if needed for anything but google play).</summary>
    void ShowUI();
}