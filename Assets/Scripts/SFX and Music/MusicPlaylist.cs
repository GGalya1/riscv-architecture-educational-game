using UnityEngine;

/// <summary>
/// Holds the three background-music tracks as data, separate from MusicManager (which only
/// knows HOW to play music - crossfading, gapless looping, sync with scene transitions).
/// Create one instance via Assets -> Create -> Audio -> Music Playlist and assign it on the
/// MusicManager component.
/// </summary>
[CreateAssetMenu(fileName = "MusicPlaylist", menuName = "Audio/Music Playlist")]
public class MusicPlaylist : ScriptableObject
{
    public AudioClip menuMusic;
    public AudioClip levelsMusic;
    public AudioClip processorMusic;
}