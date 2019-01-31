using System;
using UnityEngine;

/// <summary>
/// The frequency range of an onset.
/// </summary>
public enum OnsetType
{
    Low,
    Mid,
    High,
    All
}

/// <summary>
/// Component that provides events for a RhythmTool Component.
/// </summary>
[AddComponentMenu("Audio/Rhythm Event Provider")]
public class RhythmEventProvider : EventProvider<RhythmTool>
{
    /// <summary>
    /// Occurs every Beat.
    /// </summary>
    public event Action<Beat> Beat;

    /// <summary>
    /// Occurs every quarter Beat.
    /// </summary>
    public event Action<Beat, int> SubBeat = delegate { };

    /// <summary>
    /// Occurs every Onset.
    /// </summary>
    public event Action<OnsetType, Onset> Onset;

    /// <summary>
    /// Occurs every time a change happens.
    /// </summary>
    public event Action<int, float> Change;

    /// <summary>
    /// Occurs when a new song has been loaded and is ready to be played.
    /// </summary>
    public event Action SongLoaded;
    
    /// <summary>
    /// Occurs when a song has finished playing.
    /// </summary>
    public event Action SongEnded;
   
    /// <summary>
    /// The max offset that is allowed for this EventProvider.
    /// </summary>
    public override int maxOffset
    {
        get
        {
            if (target == null)
                return 0;

            return target.lead - 1;
        }
    }

    private float lastBeatTime;
       
    protected override void Awake()    
    {
        base.Awake();

        lastBeatTime = 0;

        TimingUpdate += OnTimingUpdate;
        FrameChanged += OnFrameChanged;
        Reset += OnReset;
    }

    protected override void OnTargetChanged(RhythmTool oldTarget, RhythmTool newTarget)
    {
        if (oldTarget != null)
        {
            oldTarget.SongLoaded -= OnSongLoaded;
            oldTarget.SongEnded -= OnSongEnded;
        }

        if (newTarget != null)
        {
            newTarget.SongLoaded += OnSongLoaded;
            newTarget.SongEnded += OnSongEnded;
        }

        base.OnTargetChanged(oldTarget, newTarget);
    }

    private void OnTimingUpdate(int currentFrame, float interpolation)
    {
        float beatTime = target.BeatTime(currentFrame + interpolation + offset);
        Beat beat = target.PrevBeat(currentFrame + offset);

        if (lastBeatTime > beatTime)
            SubBeat(beat, 0);
        if (lastBeatTime < .5f && beatTime >= .5f)
            SubBeat(beat, 2);
        if (lastBeatTime < .25f && beatTime >= .25f)
            SubBeat(beat, 1);
        if (lastBeatTime < .75f && beatTime >= .75f)
            SubBeat(beat, 3);

        lastBeatTime = beatTime;
    }

    private void OnFrameChanged(int currentFrame, int lastFrame)
    {
        if (target == null)
            return;

        if (Beat != null)
        {
            Beat beat;
            if (target.beats.TryGetValue(currentFrame, out beat))
                Beat(beat);
        }

        if (Onset != null)
        {
            Onset onset;
            if (target.low.onsets.TryGetValue(currentFrame, out onset))
                Onset(OnsetType.Low, onset);
            if (target.mid.onsets.TryGetValue(currentFrame, out onset))
                Onset(OnsetType.Mid, onset);
            if (target.high.onsets.TryGetValue(currentFrame, out onset))
                Onset(OnsetType.High, onset);
            if (target.all.onsets.TryGetValue(currentFrame, out onset))
                Onset(OnsetType.All, onset);
        }

        if (Change != null)
        {
            float change;
            if (target.changes.TryGetValue(currentFrame, out change))
                Change(currentFrame, change);
        }       
    }

    private void OnReset()
    {
        lastBeatTime = 0;
    }

    private void OnSongLoaded()
    {
        if (SongLoaded != null)
            SongLoaded();
    }

    private void OnSongEnded()
    {
        if (SongEnded != null)
            SongEnded();
    }    
}
