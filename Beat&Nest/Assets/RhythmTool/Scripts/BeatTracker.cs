using UnityEngine;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;
using System.Threading;

public class BeatTracker : IDisposable
{
    public ReadOnlyCollection<int> beatIndices { get; private set; }
    public ReadOnlyDictionary<int, Beat> beats { get; private set; }

    private static Thread main = Thread.CurrentThread;

    private float frameLength;
    private int beatTrackInterval = 20;

    private float[] signalBuffer;
    private float[] currentSignal;
    private float[] upsampledSignal;
    private float[] repetitionScore;
    private float[] gapScore;
    private float[] offsetScore;
    private List<int> peaks = new List<int>();
    private float[] beatHistogram;

    private int bestRepetition;
    private int bestGap;

    private int currentBeatLength;
    private float currentSync;
    private float sync;
        
    private List<int> _beatIndices;
    private Dictionary<int, Beat> _beats;

    private Thread findBeatThread;
    private AutoResetEvent findBeatEvent;
    private bool disposed;

    public BeatTracker()
    {
        _beatIndices = new List<int>(3000);
        _beats = new Dictionary<int, Beat>(3000);

        beatIndices = _beatIndices.AsReadOnly();
        beats = new ReadOnlyDictionary<int, Beat>(_beats);

        signalBuffer = new float[310];
        currentSignal = new float[signalBuffer.Length];

        upsampledSignal = new float[currentSignal.Length * 4];
        repetitionScore = new float[600];
        gapScore = new float[180];
        beatHistogram = new float[91];
        offsetScore = new float[135];

        findBeatEvent = new AutoResetEvent(false);
        findBeatThread = new Thread(FindBeatBackground);
        findBeatThread.Start();
    }
           
    public void Init(float frameLength)
    {
        this.frameLength = frameLength;

        Array.Clear(signalBuffer, 0, signalBuffer.Length);
        Array.Clear(beatHistogram, 0, beatHistogram.Length);

        sync = 0;
        currentSync = 0;
        currentBeatLength = 0;
        bestRepetition = 0;
        bestGap = 0;

        _beatIndices.Clear();
        _beats.Clear();
    }

    public void Init(IDictionary<int, Beat> beats)
    {
        _beatIndices.Clear();
        _beats.Clear();

        foreach (KeyValuePair<int, Beat> item in beats)
        {
            _beats.Add(item.Key, item.Value);
            _beatIndices.Add(item.Key);
        }

        _beatIndices.Sort();
    }

    public void FillStart()
    {
        int count = Mathf.Min(_beats.Count, 20);

        Dictionary<float, int> lengths = new Dictionary<float, int>()
        {
            { 0,0 }
        };

        for(int i = 0; i < count; i++)
        {
            float length = _beats[_beatIndices[i]].length;

            if (lengths.ContainsKey(length))
                lengths[length]++;
            else
                lengths.Add(length, 1);
        }

        float bestLength = 0;
        foreach (var length in lengths.Keys)
            //if (lengths[length] * length > lengths[bestLength] * bestLength)
            if(lengths[length] > lengths[bestLength])
                bestLength = length;

        if (bestLength == 0)
            return;

        int first = 0;        
        for(int i = 0; i < count; i++)
        {
            if (_beats[_beatIndices[i]].length == bestLength)
            {
                first = i;
                break;
            }
        }

        for(int i = 0; i < first; i++)
            _beats.Remove(_beatIndices[i]);

        _beatIndices.RemoveRange(0, first);

        first = _beatIndices[0];
        float bpm = (60f / (frameLength * bestLength));
        
        for (float i = first - bestLength; i > 0; i -= bestLength)
        {
            int index = Mathf.RoundToInt(i);
            _beatIndices.Insert(0, index);
            _beats.Add(index, new Beat(bestLength, bpm, index));
        }       
    }
    
    public void FillEnd(int index)
    {
        if (_beatIndices.Count < 1)
            return;

        int lastBeat = _beatIndices[_beatIndices.Count - 1];

        float beatLength = beats[lastBeat].length;

        int numFill = (int)((index - lastBeat) / beatLength);

        for (int i = 1; i < numFill; i++)
        {
            AddBeat(lastBeat + Mathf.RoundToInt(i * beatLength), beatLength);
        }
    }

    private void AddBeat(int index, float beatLength)
    {
        _beatIndices.Add(index);
        
        float currentBPM = beatLength;

        int count = Mathf.Min(_beatIndices.Count, 21);

        if (count > 5)
        {
            float start = _beatIndices[_beatIndices.Count - count];
            float end = _beatIndices[_beatIndices.Count - 1];
            currentBPM = (end - start) / (count - 1);
        }        

        currentBPM = (60f / (frameLength * currentBPM));
        currentBPM = Mathf.Round(currentBPM);
        currentBPM = Mathf.Clamp(currentBPM, 0, 199);

        //garbage
        Beat b = new Beat(beatLength, currentBPM, index);
        _beats.Add(index, b);
    }

    public void TrackBeat(float sample, int index)
    {
        signalBuffer[index % signalBuffer.Length] = sample;

        if (index > currentSignal.Length && index % beatTrackInterval == 0)
        {
            for (int i = 0; i < currentSignal.Length; i++)
            {
                currentSignal[i] = signalBuffer[(i + index + 1) % signalBuffer.Length];
            }

            currentSync = sync;

            if (Thread.CurrentThread == main)
                findBeatEvent.Set();
            else
                FindBeat();
        }

        if (currentBeatLength > 5)
        {
            if (currentBeatLength > 20)
            {
                if (sync > currentBeatLength)
                {
                    sync -= currentBeatLength;
                    AddBeat(index - currentSignal.Length, currentBeatLength / 4f);
                }
            }

            sync += 4;
        }
    }

    private void FindBeatBackground()
    {
        Thread.CurrentThread.IsBackground = true;

        findBeatEvent.WaitOne();

        while (!disposed)
        {
            FindBeat();
            findBeatEvent.WaitOne();            
        }
    }

    private void FindBeat()
    {
        Util.Smooth(currentSignal, 4);
        Util.Smooth(currentSignal, 4);
        
        Array.Clear(currentSignal, 0, 5);
        Array.Clear(currentSignal, currentSignal.Length - 5, 5);

        Util.UpsampleSingnal(currentSignal, upsampledSignal, 4);
        
        UpdateRepetition();

        UpdatePeaks();        

        UpdateBeatLength();

        UpdateOffset();
    }

    private void UpdateRepetition()
    {
        for (int i = 0; i < repetitionScore.Length; i++)
            repetitionScore[i] = RepetitionScore(upsampledSignal, i);

        bestRepetition = Util.GetBestIndex(repetitionScore, 40, repetitionScore.Length - 5);

        float max = repetitionScore[bestRepetition];
        for (int i = 0; i < repetitionScore.Length; i++)
            repetitionScore[i] = repetitionScore[i] / max;

        for (int i = 45; i < gapScore.Length; i++)
            gapScore[i] = GapScore(repetitionScore, i);

        bestGap = Util.GetBestIndex(gapScore, 45);
    }

    private void UpdatePeaks()
    {
        peaks.Clear();

        for (int i = 1; i < repetitionScore.Length - 5; i++)
        {
            int gap = i;

            while (gap < 45)
                gap *= 2;
            while (gap > gapScore.Length - 1)
                gap /= 2;

            if (gapScore[gap / 2] > gapScore[gap])
                gap /= 2;

            float score = repetitionScore[i] * Mathf.Max(0, gapScore[gap]);

            float threshold = .425f;

            if (beatHistogram[currentBeatLength] < 7)
                threshold = .25f;

            if (score > threshold && repetitionScore[i] > repetitionScore[i + 1] && repetitionScore[i] > repetitionScore[i - 1])
                peaks.Add(i);
        }

        if (peaks.Count == 0)
        {
            if (repetitionScore[bestRepetition / 2] > .75f)
                peaks.Add(bestRepetition);
            else if (bestRepetition * 2 < repetitionScore.Length)
                if (repetitionScore[bestRepetition * 2] > .75f)
                    peaks.Add(bestRepetition);
        }

        if (peaks.Count == 0 && currentBeatLength == 0)
            peaks.Add(bestGap);

        peaks.Insert(0, 0);
    }

    private void UpdateBeatLength()
    {
        for (int i = 0; i < peaks.Count; i++)
        {
            if (i + 1 < peaks.Count)
            {
                int gap = peaks[i + 1] - peaks[i];

                if (gap > 3)
                {
                    while (gap < 45)
                    { //45 = 160bpm / 40 = 180 bpm
                        gap *= 2;
                    }
                    while (gap > 90)
                    { //90 = 80bpm / 120 = 60bpm
                        gap /= 2;
                    }

                    beatHistogram[gap]++;
                }
            }
        }

        currentBeatLength = Util.GetBestIndex(beatHistogram, 1);

        if (beatHistogram[currentBeatLength] > 15)
        {
            for (int i = 0; i < beatHistogram.Length; i++)
            {
                beatHistogram[i] = Mathf.Clamp(beatHistogram[i] - 7, 0, 7);
            }
        }
    }

    private void UpdateOffset()
    {
        int end = currentBeatLength + currentBeatLength / 2;

        for (int i = 0; i < end; i++)
            offsetScore[i] = OffsetScore(upsampledSignal, currentBeatLength, i);
        
        float offset = Util.GetBestIndex(offsetScore, 0, end);

        offset %= currentBeatLength;
        offset = currentBeatLength - offset;
        offset -= currentSync;
        offset += 2;

        float sign = Mathf.Sign(offset);
        offset = Math.Abs(offset);

        if (offset > currentBeatLength / 2f)
            sign *= -1;                

        sync += Mathf.Min(2, offset) * sign;
    }
    
    private float RepetitionScore(float[] signal, int offset)
    {
        float score = 0;

        for (int i = 0; i < signal.Length - offset; i++)
            score += (signal[i] * signal[i + offset]);        

        return (score / (signal.Length - offset));
    }

    private float GapScore(float[] signal, int gap)
    {
        float score = 0;        
        
        for(int i = gap; i < signal.Length; i += gap)        
            score += signal[i];                

        return score / (signal.Length / gap);
    }

    private float OffsetScore(float[] signal, int gap, int offset)
    {
        float score = 0;

        for (int i = 0; i < signal.Length - offset; i += gap)            
            score += signal[i + offset];

        return score /= (signal.Length - offset) / gap;
    }
    
    public Beat NextBeat(int index)
    {
        if (_beats.Count == 0)
            return new Beat(0, 0, 0);

        int nextBeat = NextBeatIndex(index);

        return _beats[nextBeat];
    }

    public Beat PrevBeat(int index)
    {
        if (_beats.Count == 0)
            return new Beat(0, 0, 0);

        int prevBeat = PrevBeatIndex(index);

        return _beats[prevBeat];
    }

    public int NextBeatIndex(int index)
    {
        if (_beatIndices.Count == 0)
            return 0;

        int nextBeat = _beatIndices.BinarySearch(index);
        nextBeat = Mathf.Max(nextBeat, ~nextBeat);
        nextBeat = Mathf.Clamp(nextBeat, 0, _beatIndices.Count - 1);

        nextBeat = _beatIndices[nextBeat];
        return nextBeat;
    }

    public int PrevBeatIndex(int index)
    {
        if (_beatIndices.Count == 0)
            return 0;

        int prevBeat = _beatIndices.BinarySearch(index);
        prevBeat = Mathf.Max(prevBeat, ~prevBeat);
        prevBeat = Mathf.Clamp(prevBeat - 1, 0, _beatIndices.Count - 1);

        prevBeat = _beatIndices[prevBeat];
        return prevBeat;
    }

    public float BeatTime(float index)
    {
        int nextBeat = NextBeatIndex((int)Mathf.Ceil(index));
        int prevBeat = PrevBeatIndex((int)Mathf.Ceil(index));

        int l = nextBeat - prevBeat;

        if (l == 0)
            return 0;

        return 1 - ((nextBeat - index) / l);
    }

    public void Dispose()
    {
        disposed = true;
        findBeatEvent.Set();
    }

    public void DrawDebugLines(int currentFrame)
    {
        for (int i = currentFrame; i < currentFrame + 300; i++)
        {
            if (_beats.ContainsKey(i))
            {
                Debug.DrawLine(new Vector3(i - currentFrame, 0, -1), new Vector3(i - currentFrame, 400, 1), Color.black);
            }
        }

        //for (int i = 0; i < signalBuffer.Length - 1; i++)
        //{
        //    Vector3 start = new Vector3(i, signalBuffer[i], 0);
        //    Vector3 end = new Vector3((i + 1), signalBuffer[i + 1], 0);
        //    start += Vector3.down * 250;
        //    end += Vector3.down * 250;
        //    Debug.DrawLine(start, end, Color.black);
        //}

        //for (int i = 0; i < currentSignal.Length - 1; i++)
        //{
        //    Vector3 start = new Vector3(i, currentSignal[i], 0);
        //    Vector3 end = new Vector3((i + 1), currentSignal[i + 1], 0);
        //    start += Vector3.down * 300;
        //    end += Vector3.down * 300;
        //    Debug.DrawLine(start, end);
        //}

        for (int i = 0; i < repetitionScore.Length - 1; i++)
        {
            Vector3 start = new Vector3(i, repetitionScore[i] * 10, 0);
            Vector3 end = new Vector3((i + 1), repetitionScore[i + 1] * 10, 0);
            start += Vector3.down * 100;
            end += Vector3.down * 100;
            Debug.DrawLine(start, end, Color.gray);
        }

        for (int i = 0; i < gapScore.Length - 1; i++)
        {
            Vector3 start = new Vector3(i, gapScore[i] * 10, 0);
            Vector3 end = new Vector3((i + 1), gapScore[i + 1] * 10, 0);
            start += Vector3.down * 100;
            end += Vector3.down * 100;
            Debug.DrawLine(start, end, Color.black);
        }

        for (int i = 0; i < beatHistogram.Length; i++)
        {
            Vector3 start = new Vector3(i, 0, 0);
            Vector3 end = new Vector3(i, beatHistogram[i], 0);
            start += Vector3.down * 150;
            end += Vector3.down * 150;
            Debug.DrawLine(start, end, Color.blue);
        }

        for (int i = 0; i < offsetScore.Length - 1; i++)
        {
            Vector3 start = new Vector3(i, offsetScore[i], 0);
            Vector3 end = new Vector3((i + 1), offsetScore[i + 1], 0);
            start += Vector3.down * 200;
            end += Vector3.down * 200;
            Debug.DrawLine(start, end, Color.red);
        }

        foreach (int peak in peaks)
        {
            Debug.DrawLine(new Vector3(peak, -100, 0), new Vector3(peak, -50, 0));

            int t = peak;

            if (t > 0)
            {
                while (t < 45)
                    t *= 2;
                while (t > 149)
                    t /= 2;
            }

            Debug.DrawLine(new Vector3(t, -100, 0), new Vector3(t, -90, 0), Color.black);
        }

        Debug.DrawLine(new Vector3(bestRepetition, -100, 0), new Vector3(bestRepetition, -70, 0), Color.yellow);

        int b = bestRepetition;

        if (b > 0)
        {
            while (b < 45)
                b *= 2;
            while (b > gapScore.Length - 1)
                b /= 2;
        }

        Debug.DrawLine(new Vector3(b, -100, 0), new Vector3(b, -80, 0), Color.yellow);
    }
}
