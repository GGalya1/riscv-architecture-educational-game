using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioSource sourceA;
    [SerializeField] private AudioSource sourceB;

    [Header("Tracks")]
    [SerializeField] private MusicPlaylist playlist;

    private AudioSource _active;
    private AudioSource _standby;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var src in new[] { sourceA, sourceB })
        {
            src.outputAudioMixerGroup = musicMixerGroup;
            src.playOnAwake = false;
            src.volume = 0f;
        }

        _active = sourceA;
        _standby = sourceB;
    }

    /// <summary>
    /// Picks the right track for the scene that is CURRENTLY active (menu / regular level /
    /// processor level) based on GameConstants scene indices.
    /// </summary>
    public void PlayForCurrentScene(float fadeDuration = 1.5f)
    {
        if (playlist == null) return;

        var index = SceneManager.GetActiveScene().buildIndex;

        AudioClip clip;
        if (index == GameConstants.FullProcessorSceneIndex || index == GameConstants.OneTickProcessorSceneIndex)
            clip = playlist.processorMusic;
        else if (index == 0)
            clip = playlist.menuMusic;
        else
            clip = playlist.levelsMusic;

        if (clip != null) PlayLoop(clip, fadeDuration);
    }

    /// <summary>
    /// Crossfades to a single clip that already loops cleanly on its own (no separate intro).
    /// </summary>
    public void PlayLoop(AudioClip clip, float fadeDuration = 1.5f)
    {
        if (_active.clip == clip && _active.isPlaying && _active.volume > 0.001f) return;

        _standby.clip = clip;
        _standby.loop = true;
        _standby.volume = 0f;
        _standby.Play();

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(CrossfadeRoutine(fadeDuration));
    }

    /// <summary>
    /// Plays a non-looping intro once, then hands off to a seamlessly looping section with
    /// sample-accurate timing. Use this to start a track fresh (level start, main menu) rather
    /// than to crossfade away from something already playing.
    /// </summary>
    public void PlayWithIntro(AudioClip intro, AudioClip loop, float fadeInDuration = 1f)
    {
        StopAllCoroutines();
        _active.Stop();
        _standby.Stop();

        var introSource = _active;
        var loopSource = _standby;

        double startTime = AudioSettings.dspTime + 0.1; // small safety buffer for scheduling
        double introLength = (double)intro.samples / intro.frequency;

        introSource.clip = intro;
        introSource.loop = false;
        introSource.volume = 0f;
        introSource.PlayScheduled(startTime);

        loopSource.clip = loop;
        loopSource.loop = true;
        loopSource.volume = 0f;
        loopSource.PlayScheduled(startTime + introLength);

        StartCoroutine(FadeVolume(introSource, 1f, fadeInDuration));
        StartCoroutine(FadeVolume(loopSource, 1f, fadeInDuration));

        // From now on the loop source is the one future PlayLoop()/crossfades should treat
        // as "active" - the intro source will simply stop on its own once it finishes.
        _active = loopSource;
        _standby = introSource;
    }

    /// <summary>
    /// Fades whatever is currently audible down to silence, without touching clips.
    /// Call this at the START of a scene transition, alongside your loadingOverlay fade-out.
    /// </summary>
    public void FadeToSilence(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeVolume(sourceA, 0f, duration));
        StartCoroutine(FadeVolume(sourceB, 0f, duration));
    }

    private IEnumerator CrossfadeRoutine(float duration)
    {
        var from = _active;
        var to = _standby;
        var fromStart = from.volume;
        var t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            var p = duration <= 0f ? 1f : Mathf.Clamp01(t / duration);
            from.volume = Mathf.Lerp(fromStart, 0f, p);
            to.volume = Mathf.Lerp(0f, 1f, p);
            yield return null;
        }

        from.volume = 0f;
        from.Stop();
        to.volume = 1f;

        _active = to;
        _standby = from;
    }

    private IEnumerator FadeVolume(AudioSource src, float target, float duration)
    {
        var start = src.volume;
        var t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(start, target, duration <= 0f ? 1f : t / duration);
            yield return null;
        }

        src.volume = target;
    }
}