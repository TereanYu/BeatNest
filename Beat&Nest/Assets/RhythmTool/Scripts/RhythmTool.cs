using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine;

/// <summary>
/// The main Component for analyzing songs.
/// </summary>
[RequireComponent(typeof(AudioSource))]
[AddComponentMenu("Audio/RhythmTool")]
public class RhythmTool : MonoBehaviour, IEventSequence
{	
    /// <summary>
    /// Occurs when a new song has been loaded and is ready for playback. 
    /// </summary>
    public event Action SongLoaded;

    /// <summary>
    /// Occurs when a song has finished playback.
    /// </summary>
    public event Action SongEnded;

    /// <summary>
    /// Occurs for every frame that has passed.
    /// </summary>
    public event Action<int, int> FrameChanged;

    /// <summary>
    /// Occurs every update when a song is playing.
    /// </summary>
    public event Action<int, float> FrameUpdate;

    /// <summary>
    /// Occurs when a song has stopped or restarted playback.
    /// </summary>
    public event Action Reset;

    /// <summary>
    /// The number of samples to use for FFT per frame.
    /// </summary>
    public static int fftWindowSize { get { return 2048; } }

    /// <summary>
    /// The number of samples the FFT window moves for each frame.
    /// </summary>
    public static int frameSpacing { get { return 1470; } }

    /// <summary>
    /// The number of frames that are a part of the analysis at any time.
    /// </summary>
    public static int analysisWidth { get { return 325; } }

    /// <summary>
    /// Analysis results for the default low frequency range analysis. 
    /// 0hz - 645hz
    /// </summary>
    public AnalysisData low { get { return _low.analysisData; } }

    /// <summary>
    /// Analysis results for the default mid frequency range analysis. 
    /// 645hz - 7500hz
    /// </summary>
    public AnalysisData mid { get { return _mid.analysisData; } }

    /// <summary>
    /// Analysis results for the default high frequency range analysis. 
    /// 7500hz - 20000hz
    /// </summary>
    public AnalysisData high { get { return _high.analysisData; } }

    /// <summary>
    /// Analysis results for the default wide frequency range analysis. 
    /// 0hz - 7500hz
    /// </summary>
    public AnalysisData all { get { return _all.analysisData; } }

    /// <summary>
    /// A collection of beats with the frame index as key and Beat as value.
    /// </summary>
    public ReadOnlyDictionary<int, Beat> beats { get { return beatTracker.beats; } }

    /// <summary>
    /// A collection of segment changes with the frame index as key and average volume as value.
    /// A positive value indicates an increase in volume compared to the previous segment and a negative value indicates a decrease.
    /// </summary>
    public ReadOnlyDictionary<int, float> changes { get { return segmenter.changes; } }

    /// <summary>
    /// The most probable BPM currently.
    /// </summary>
    public float bpm { get; private set; }

    /// <summary>
    /// The length of the beat (in frames) of the beat that is currently occuring.
    /// </summary>
    public float beatLength { get; private set; }

    /// <summary>
    /// The current sample that is being played.
    /// </summary>
    public float currentSample { get; private set; }
   
    /// <summary>
    /// The total number of frames for the current song.
    /// </summary>
    public int totalFrames { get; private set; }
    
    /// <summary>
    /// The frame index of the last analyzed frame.
    /// </summary>
    public int lastFrame { get; private set; }

    /// <summary>
    /// The index of the frame that corresponds with the current time in the song.
    /// </summary>
    public int currentFrame { get; private set; }

    /// <summary>
    /// The time in between the current frame and the next.
    /// </summary>
    public float interpolation { get; private set; }

    /// <summary>
    /// The length of a frame in seconds.
    /// </summary>
    public float frameLength { get; private set; }

    /// <summary>
    /// Is a song loaded and ready to be played?
    /// </summary>
    public bool songLoaded { get; private set; }

    /// <summary>
    /// Is the analysis done?
    /// </summary>
    public bool analysisDone { get; private set; }

    /// <summary>
    /// Is the song playing right now?
    /// </summary>
    public bool isPlaying
    {
        get
        {
            return audioSource.isPlaying;
        }
    }

    /// <summary>
    /// The volume of the AudioSource.
    /// </summary>
    public float volume
    {
        get
        {
            return audioSource.volume;
        }
        set
        {
            audioSource.volume = value;
        }
    }

    /// <summary>
    /// The pitch of the AudioSource.
    /// </summary>
    public float pitch
    {
        get
        {
            return audioSource.pitch;
        }
        set
        {
            audioSource.pitch = value;
        }
    }

    /// <summary>
    /// The AudioClip to analyze and play.
    /// </summary>
    public AudioClip audioClip
    {
        get
        {
            return _audioClip;
        }
        set
        {
            _audioClip = value;
            OnSongChanged();
        }
    }

    /// <summary>
    /// How many frames ahead the song is analyzed.
    /// See <see cref="preAnalyze"/> for analyzing the whole song in advance.
    /// </summary>
    public int lead
    {
        get
        {
            return _lead;
        }
        set
        {
            _lead = Mathf.Clamp(value, 1, 1800);
        }
    }

    /// <summary>
    /// Perform beat tracking?
    /// </summary>
    public bool trackBeat
    {
        get
        {
            return _trackBeat;
        }
        set
        {
            _trackBeat = value;
        }
    }

    /// <summary>
    /// Analyze the entire song in advance?
    /// </summary>
    public bool preAnalyze
    {
        get
        {
            return _preAnalyze;
        }
        set
        {
            _preAnalyze = value;
        }
    }

    /// <summary>
    /// Cache analysis results? 
    /// </summary>
    public bool cacheAnalysis
    {
        get { return _cacheAnalysis; }
        set { _cacheAnalysis = value; }
    }

    private AudioSource audioSource;
    private int channels;
    private float[] samples;
    private float[] monoSamples;
    private float[] spectrum;

    private List<Analysis> analyses;
    private Analysis _low;
    private Analysis _mid;
    private Analysis _high;
    private Analysis _all;

    private BeatTracker beatTracker;
    private Segmenter segmenter;

    private Thread analyze;
    private ManualResetEvent stop = new ManualResetEvent(false);
    private int lastDataFrame;
    private string cachePath;

    //[SerializeField]
    private AudioClip _audioClip;
    [SerializeField]
    private bool _trackBeat = true;
    [SerializeField]
    private int _lead = 300;
    [SerializeField]
    private bool _preAnalyze = false;
    [SerializeField]
    private bool _cacheAnalysis = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioClip != null)
            OnSongChanged();

        analyses = new List<Analysis>();

        _low = new Analysis(0, 30, "low"); //0hz - 645hz
        _mid = new Analysis(30, 350, "mid"); //645hz - 7500hz
        _high = new Analysis(370, 900, "high"); //7500hz - 20000hz
        _all = new Analysis(0, 350, "all"); //0hz - 7500hz

        analyses.Add(_low);
        analyses.Add(_mid);
        analyses.Add(_high);
        analyses.Add(_all);

        beatTracker = new BeatTracker();
        segmenter = new Segmenter(all);
    }

    void Update()
    {
        UpdateTiming();
        UpdateAnalysis();
        UpdateEvents();
    }

    void OnDestroy()
    {
        beatTracker.Dispose();
    }

    private void UpdateTiming()
    {
        if (!isPlaying)
            return;

        currentSample = Mathf.Clamp(currentSample + audioClip.frequency * Time.unscaledDeltaTime * pitch,
            audioSource.timeSamples - frameSpacing / 2,
            audioSource.timeSamples + frameSpacing / 2);

        interpolation = currentSample / frameSpacing;
        currentFrame = (int)interpolation;
        interpolation -= currentFrame;

        Beat nextBeat = NextBeat(currentFrame);
        beatLength = nextBeat.length;
        bpm = nextBeat.bpm;

        if (currentFrame >= totalFrames - 1)
            OnSongEnded();
    }

    private void UpdateAnalysis()
    {
        if (!songLoaded || analysisDone)
            return;

        int targetFrame = Mathf.Min(currentFrame + lead + analysisWidth, totalFrames);

        for (int i = lastFrame + 1; i < targetFrame; i++)
        {
            audioClip.GetData(samples, Mathf.Max((i * frameSpacing) - (samples.Length / 2), 0)); 
            Analyze(samples, i);
            lastFrame = i;
        }

        if (lastFrame == totalFrames - 1)
            OnAnalysisDone();
    }

    private void UpdateEvents()
    {
        if (!isPlaying)
            return;

        if (FrameUpdate != null)
            FrameUpdate(currentFrame, interpolation);

        for (int i = lastDataFrame + 1; i < currentFrame + 1; i++)
        {
            if (FrameChanged != null)
                FrameChanged(i, lastFrame);

            lastDataFrame = i;
        }
    }

    private void Analyze(float[] samples, int index)
    {
        Util.GetMono(samples, monoSamples, channels);
        Util.GetSpectrum(monoSamples);
        Util.GetSpectrumMagnitude(monoSamples, spectrum);

        foreach (Analysis analysis in analyses)
            analysis.Analyze(spectrum, index);

        if (_trackBeat)
            beatTracker.TrackBeat(all.flux[index], index);

        segmenter.DetectChanges(index - 200);
    }

    private void Analyze(float[] samples)
    {
        int count = samples.Length / frameSpacing / channels;

        for (int i = lastFrame; i < count && !stop.WaitOne(0); i++)
        {
            int ii = Mathf.Max((i * frameSpacing * channels) - (fftWindowSize * channels), 0); 
            Array.Copy(samples, ii, this.samples, 0, fftWindowSize * channels);

            Analyze(this.samples, i);

            lastFrame = i;
        }
    }

    IEnumerator Initialize()
    {
        yield return StartCoroutine(StopAnalyze());

        cachePath = "";
        songLoaded = false;
        analysisDone = false;

        totalFrames = 0;
        frameLength = 0;
        lastFrame = 0;
        lastDataFrame = 0;

        if (audioClip == null)
            yield break;

        cachePath = Path.Combine(Application.persistentDataPath, audioClip.name + ".rthm");

        channels = audioClip.channels;
        samples = new float[fftWindowSize * channels];
        monoSamples = new float[fftWindowSize];
        spectrum = new float[fftWindowSize / 2];
        totalFrames = audioClip.samples / frameSpacing;
        frameLength = 1 / ((float)audioSource.clip.frequency / frameSpacing);
        
        if (!(cacheAnalysis && preAnalyze && TryLoad()))
            yield return StartCoroutine(StartAnalyze());

        OnSongLoaded();
    }

    IEnumerator StopAnalyze()
    {
        stop.Set();

        while (analyze != null && analyze.IsAlive)
            yield return null;

        stop.Reset();
    }

    IEnumerator StartAnalyze()
    {
        foreach (Analysis analysis in analyses)
        {
            analysis.Init(totalFrames);
        }

        beatTracker.Init(frameLength);
        segmenter.Init();

        int count = preAnalyze ? totalFrames : lead + analysisWidth;
        float[] samples = new float[count * frameSpacing * channels];
        audioClip.GetData(samples, 0);

        analyze = new Thread(() => Analyze(samples));
        analyze.Start();

        while (analyze.IsAlive)
            yield return null;

        if (_trackBeat)
            beatTracker.FillStart();

        if (lastFrame == totalFrames - 1)
            OnAnalysisDone();
    }

    private void OnSongChanged()
    {
        if (!Application.isPlaying)
            return;

        StopAllCoroutines();
        Stop();

        audioSource.clip = audioClip;

        StartCoroutine(Initialize());
    }

    private void OnSongLoaded()
    {
        songLoaded = true;

        if (SongLoaded != null)
            SongLoaded();
    }

    private void OnSongEnded()
    {
        if (SongEnded != null)
            SongEnded();
    }

    private void OnAnalysisDone()
    {
        if (_trackBeat)
            beatTracker.FillEnd(totalFrames);

        analysisDone = true;

        if (cacheAnalysis && preAnalyze)
            Save();
    }

    private void OnReset()
    {
        lastDataFrame = 0;
        currentFrame = 0;

        if (Reset != null)
            Reset();
    }

    private void Save()
    {
        List<AnalysisData> data = new List<AnalysisData>();

        foreach (Analysis analysis in analyses)
            data.Add(analysis.analysisData);

        SongData songData = new SongData(totalFrames, data, beats, changes);
        songData.Save(cachePath);
    }

    private bool TryLoad()
    {
        if (!File.Exists(cachePath))
            return false;

        try
        {
            SongData songData = SongData.Load(cachePath);

            if (songData.length != totalFrames)
                return false;

            lastFrame = totalFrames;
            analysisDone = true;

            foreach (AnalysisData data in songData.analyses)
            {
                foreach (Analysis analysis in analyses)
                {
                    if (analysis.name == data.name)
                    {
                        analysis.Init(data);
                    }
                }
            }

            beatTracker.Init(songData.beats);
            segmenter.Init(songData.changes);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not load SongData for song: " + e.Message);
            return false;
        }
    }
    
    /// <summary>
    /// Start playing the song.
    /// </summary>
    public void Play()
    {
        if (!songLoaded)
            return;

        if (audioSource.isPlaying || currentFrame >= totalFrames - 1)
            OnReset();

        audioSource.Play();
    }

    /// <summary>
    /// Stop playing the song.
    /// </summary>
    public void Stop()
    {
        if (!songLoaded)
            return;

        OnReset();

        audioSource.Stop();
    }

    /// <summary>
    /// Pause the song.
    /// </summary>
    public void Pause()
    {
        audioSource.Pause();
    }

    /// <summary>
    /// Unpause the paused song.
    /// </summary>
    public void UnPause()
    {
        audioSource.UnPause();
    }
    
    /// <summary>
    /// Add a custom Analysis.
    /// </summary>
    /// <param name="start">Start of frequency range.</param>
    /// <param name="end">End of frequency range.</param>
    /// <param name="name">The Analysis' name.</param>
    /// <returns></returns>
    public AnalysisData AddAnalysis(int start, int end, string name)
    {
        foreach (Analysis analysis in analyses)
        {
            if (analysis.name == name)
            {
                Debug.LogWarning("Analysis with name " + name + " already exists");
                return null;
            }
        }

        Analysis a = new Analysis(start, end, name);
        a.Init(totalFrames);
        analyses.Add(a);

        return a.analysisData;
    }

    /// <summary>
    /// Returns AnalysisData for the Analysis with name.
    /// </summary>
    /// <param name="name">The Analyis' name.</param>
    /// <returns>The AnalysysData of an Analysis with name. Null if Analysis was not found.</returns>
    public AnalysisData GetAnalysis(string name)
    {
        foreach (Analysis analysis in analyses)
        {
            if (analysis.name == name)
                return analysis.analysisData;
        }

        Debug.LogWarning("Analysis with name " + name + " was not found");

        return null;
    }

    /// <summary>
    /// Get the next beat closest to index.
    /// </summary>
    /// <param name="index">The frame index to look for the next beat.</param>
    /// <returns>The next beat.</returns>
    public Beat NextBeat(int index)
    {
        return beatTracker.NextBeat(index);
    }

    /// <summary>
    /// Get the previous beat closest to index.
    /// </summary>
    /// <param name="index">The frame index to look for the previous beat.</param>
    /// <returns>The previous beat.</returns>
    public Beat PrevBeat(int index)
    {
        return beatTracker.PrevBeat(index);
    }

    /// <summary>
    /// Get the next beat index closest to index.
    /// </summary>
    /// <param name="index">The frame index to look for the next beat index.</param>
    /// <returns>The next beat's index.</returns>
    public int NextBeatIndex(int index)
    {
        return beatTracker.NextBeatIndex(index);
    }

    /// <summary>
    /// Get the next beat index closest to currentFrame.
    /// </summary>
    /// <returns>The next beat's index.</returns>
    public int NextBeatIndex()
    {
        return NextBeatIndex(currentFrame);
    }
    
    /// <summary>
    /// Get the previous beat index closest to index.
    /// </summary>
    /// <param name="index">The frame index to look for the previous beat index.</param>
    /// <returns>The previous beat's index.</returns>
    public int PrevBeatIndex(int index)
    {
        return beatTracker.PrevBeatIndex(index);
    }

    /// <summary>
    /// Get the previous beat index closest to currentFrame.
    /// </summary>
    /// <returns>The previous beat's index.</returns>
    public int PrevBeatIndex()
    {
        return PrevBeatIndex(currentFrame);
    }

    /// <summary>
    /// Get the normalized time between the previous and next beat indices for index.
    /// </summary>
    /// <param name="index">The index to get the beat time for.</param>
    /// <returns>A value ranging from 0 to 1 representing the time between the previous and next beat indices.</returns>
    public float BeatTime(float index)
    {
        return beatTracker.BeatTime(index);
    }

    /// <summary>
    /// Get the normalized time between the previous and next beat indices for currentFrame.
    /// </summary>
    /// <returns>A value ranging from 0 to 1 representing the time between the previous and next beat indices.</returns>
    public float BeatTime()
    {
        return BeatTime(currentFrame + interpolation);
    }

    /// <summary>
    /// Get the next segment change closes to index.
    /// </summary>
    /// <param name="index">The frame index to look for the next change.</param>
    /// <returns>The volume for the next segment change. A positive value indicates an increase compared to the previous segment and a negative value indicates a decrease.</returns>
    public float NextChange(int index)
    {
        return segmenter.NextChange(index);
    }

    /// <summary>
    /// Get the previous segment change closes to index.
    /// </summary>
    /// <param name="index">The frame index to look for the previous change.</param>
    /// <returns>The volume for the previous segment change. A positive value indicates an increase compared to the previous segment and a negative value indicates a decrease.</returns>
    public float PrevChange(int index)
    {
        return segmenter.PrevChange(index);
    }

    /// <summary>
    /// Get the frame index of the next segment change closes to index.
    /// </summary>
    /// <param name="index">The frame index to look for the next change</param>
    /// <returns></returns>
    public int NextChangeIndex(int index)
    {
        return segmenter.NextChangeIndex(index);
    }

    /// <summary>
    /// Get the frame index of the previous segment change closes to index.
    /// </summary>
    /// <param name="index">The frame index to look for the previous change</param>
    /// <returns></returns>
    public int PrevChangeIndex(int index)
    {
        return segmenter.PrevChangeIndex(index);
    }

    /// <summary>
    /// Get the spectrum for the audio that's currently playing.
    /// </summary>
    /// <param name="samples">Array to fill with the spectrum. Length must be a power of 2</param>
    /// <param name="channel">The channel to use</param>
    /// <param name="window">The window type to use.</param>
    public void GetSpectrum(float[] samples, int channel, FFTWindow window)
    {
        audioSource.GetSpectrumData(samples, channel, window);
    }

    /// <summary>
    /// Draw analysis results in graphs for the next 300 frames.
    /// </summary>
    public void DrawDebugLines()
    {
        if (!Application.isEditor)
            //return;

        if (!songLoaded)
            return;

        for (int i = 0; i < analyses.Count; i++)
        {
            analyses[i].DrawDebugLines(currentFrame, i);
        }

        if (_trackBeat)
        {
            beatTracker.DrawDebugLines(currentFrame);
        }
    }    
}
