using System;

public static class LevelEvents
{
    /// <summary>
    /// Fired once when the player submits a correct solution.
    /// Carries a full snapshot of the result via <see cref="LevelCompletedData"/>.
    /// </summary>
    public static event Action<LevelCompletedData> LevelCompleted;

    /// <summary>
    /// Fired each time the player submits a wrong answer.
    /// Carries the total number of failed tries so far.
    /// </summary>
    public static event Action<int> SolutionFailed;
    
    public static void RaiseLevelCompleted(LevelCompletedData data)
        => LevelCompleted?.Invoke(data);

    public static void RaiseSolutionFailed(int totalFailedTries)
        => SolutionFailed?.Invoke(totalFailedTries);
    
    // Every time when player roll back a tick (roll back 50 times achievement)
    public static event Action<int> PrevTickUsed; 
    
    // A credits button was pressed
    public static event Action CreditsWatched;
    
    public static void RaisePrevTickUsed(int totalTicks) 
        => PrevTickUsed?.Invoke(totalTicks);

    public static void RaiseCreditsWatched() 
        => CreditsWatched?.Invoke();
}
