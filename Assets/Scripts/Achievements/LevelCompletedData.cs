/// <summary>
/// Immutable snapshot of everything relevant about a completed level.
/// Passed to all subscribers of <see cref="LevelEvents.LevelCompleted"/>.
/// </summary>
public readonly struct LevelCompletedData
{
    /// <summary>Build index of the scene that was just completed.</summary>
    public readonly int SceneIndex;

    /// <summary>Number of wrong answers the player submitted before solving the level.</summary>
    public readonly int FallenTries;

    /// <summary>Stars earned on this attempt (0–3).</summary>
    public readonly int EarnedStars;

    /// <summary>True if there are no more levels after this one.</summary>
    public readonly bool IsLastLevel;

    public LevelCompletedData(int sceneIndex, int fallenTries, int earnedStars, bool isLastLevel)
    {
        SceneIndex  = sceneIndex;
        FallenTries = fallenTries;
        EarnedStars = earnedStars;
        IsLastLevel = isLastLevel;
    }
}