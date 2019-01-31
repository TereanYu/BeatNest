using UnityEngine;
using System.Collections.Generic;

public class Analysis
{
    public AnalysisData analysisData { get; private set; }
    public string name { get; private set; }

    private List<int> onsetIndices;
    private Dictionary<int, Onset> _onsets;
    private List<float> _magnitude;
    private List<float> _magnitudeSmooth;
    private List<float> _flux;
    private List<float> _magnitudeAvg;
       
    private List<int> points;

    private int start;
    private int end;

    private int totalFrames;

    public Analysis(int start, int end, string name)
    {
        int spectrumSize = (RhythmTool.fftWindowSize / 2);

        this.name = name;
        this.end = Mathf.Clamp(end, 0, spectrumSize);
        this.start = Mathf.Clamp(start, 0, this.end);

        if (end < start || start < 0 || end < 0 || start >= spectrumSize || end > spectrumSize)
            Debug.LogWarning("Invalid range for analysis " + name + ". Range must be within " + spectrumSize + " and start cannot come after end.");

        _magnitude = new List<float>();
        _flux = new List<float>();
        _magnitudeSmooth = new List<float>();
        _magnitudeAvg = new List<float>();
        _onsets = new Dictionary<int, Onset>(1000);

        analysisData = new AnalysisData(name, _magnitude, _flux, _magnitudeSmooth, _magnitudeAvg, _onsets);

        onsetIndices = new List<int>(1000);

        points = new List<int>();
    }

    public void Init(int totalFrames)
    {
        this.totalFrames = totalFrames;

        onsetIndices.Clear();

        _magnitude.Clear();
        _flux.Clear();
        _magnitudeSmooth.Clear();
        _magnitudeAvg.Clear();
        _onsets.Clear();

        float[] empty = new float[totalFrames];
        _magnitude.AddRange(empty);
        _flux.AddRange(empty);
        _magnitudeSmooth.AddRange(empty);
        _magnitudeAvg.AddRange(empty);

        _magnitude.TrimExcess();
        _flux.TrimExcess();
        _magnitudeSmooth.TrimExcess();
        _magnitudeAvg.TrimExcess();
        
        points.Clear();
        points.AddRange(new int[4]);
    }

    public void Init(AnalysisData data)
    {
        totalFrames = data.magnitude.Count;

        onsetIndices.Clear();

        _magnitude.Clear();
        _flux.Clear();
        _magnitudeSmooth.Clear();
        _magnitudeAvg.Clear();
        _onsets.Clear();

        _magnitude.AddRange(data.magnitude);
        _flux.AddRange(data.flux);
        _magnitudeSmooth.AddRange(data.magnitudeSmooth);
        _magnitudeAvg.AddRange(data.magnitudeAvg);

        _magnitude.TrimExcess();
        _flux.TrimExcess();
        _magnitudeSmooth.TrimExcess();
        _magnitudeAvg.TrimExcess();

        foreach (KeyValuePair<int, Onset> item in data.onsets)
        {
            _onsets.Add(item.Key, item.Value);
        }
    }

    public void Analyze(float[] spectrum, int index)
    {
        _magnitude[index] = Util.Sum(spectrum, start, end);

        Smooth(index, 10, 5);
        Interpolate(index);

        if (index > 1)
            _flux[index] = _magnitude[index] - _magnitude[index - 1];

        FindPeaks(index, 1.9f, 12);
        RankPeaks(index - 12, 50);
    }

    private void Smooth(int index, int windowSize, int iterations)
    {
        _magnitudeSmooth[index] = _magnitude[index];

        for (int i = 1; i < iterations + 1; i++)
        {
            int iterationIndex = Mathf.Max(0, index - i * (windowSize / 2));
            _magnitudeSmooth[iterationIndex] = Util.Average(_magnitudeSmooth, iterationIndex, windowSize);
        }
    }

    private void FindPeaks(int index, float thresholdMultiplier, int thresholdWindowSize)
    {
        int offset = Mathf.Max(index - (thresholdWindowSize / 2) - 1, 0);

        float threshold = Threshold(offset, thresholdMultiplier, thresholdWindowSize);

        if (_flux[offset] > threshold && _flux[offset] > _flux[offset + 1] && _flux[offset] > _flux[offset - 1])
        {
            //garbage
            Onset o = new Onset(offset, _flux[offset], 0);
            _onsets.Add(offset, o);
            onsetIndices.Add(offset);
        }
    }

    private float Threshold(int index, float multiplier, int windowSize)
    {
        int start = Mathf.Max(0, index - windowSize / 2);
        int end = Mathf.Min(_flux.Count - 1, index + windowSize / 2);

        float mean = 0;
        for (int i = start; i <= end; i++)
            mean += Mathf.Abs(_flux[i]);
        mean /= (end - start);

        return Mathf.Clamp(mean * multiplier, 3, 70);
    }

    private void RankPeaks(int index, int windowSize)
    {
        int offset = Mathf.Max(0, index - windowSize);

        if (!_onsets.ContainsKey(offset))
            return;

        int onsetIndex = onsetIndices.IndexOf(offset);

        int rank = onsetIndices.Count - onsetIndex;

        for (int i = 5; i > 0; i--)
        {
            if (onsetIndex - i > 0 && onsetIndex + i < onsetIndices.Count)
            {
                float c = _flux[offset];
                float p = _flux[onsetIndices[onsetIndex - i]];
                float n = _flux[onsetIndices[onsetIndex + i]];

                if (c > p && c > n)
                {
                    rank = 6 - i;
                }

                if (onsetIndices[onsetIndex - i] < offset - windowSize / 2 && onsetIndices[onsetIndex + i] > offset + windowSize / 2)
                    rank = 6 - i;
            }
        }

        _onsets[offset].rank = rank;
    }

    private void Interpolate(int index)
    {
        int offset = Mathf.Clamp(index - 30, 1, totalFrames - 1);

        if ((_magnitudeSmooth[offset] > _magnitudeSmooth[offset - 1] && _magnitudeSmooth[offset] > _magnitudeSmooth[offset + 1])
            || (_magnitudeSmooth[offset] < _magnitudeSmooth[offset - 1] && _magnitudeSmooth[offset] < _magnitudeSmooth[offset + 1]))           
        {              
            points.Add(offset);

            int a1 = points[0];
            int a2 = points[2];

            int b1 = points[1];
            int b2 = points[3];

            int aLength = a2 - a1;
            int bLength = b2 - b1;
            int abLength = a2 - b1;
            
            int ab = b1 - a1;
            int ba = a2 - b1;
            
            float a = Mathf.Lerp(_magnitudeSmooth[a1], _magnitudeSmooth[a2], (float)ab / aLength);
            float b = Mathf.Lerp(_magnitudeSmooth[b1], _magnitudeSmooth[b2], (float)ba / bLength);

            a = (a + _magnitudeSmooth[b1]) / 2f;
            b = (b + _magnitudeSmooth[a2]) / 2f;

            for(int i = 0; i < abLength; i++)
            {
                _magnitudeAvg[i + b1] = Mathf.Lerp(a, b, (float)i/abLength);
            }

            points.RemoveAt(0);
        }

        if (index == totalFrames - 1)
        {
            int start = points[0];
            int length = totalFrames - start;

            for (int i = 0; i < length; i++)
            {
                _magnitudeAvg[start + i] = Mathf.Lerp(_magnitudeAvg[start], 0, (float)i / length);
            }
        }
    }
    
    public void DrawDebugLines(int index, int h)
    {
        for (int i = 0; i < 299; i++)
        {
            if (i + 1 + index > totalFrames - 1)
                break;
            Vector3 s = new Vector3(i, _magnitude[i + index] + h * 100, 0);
            Vector3 e = new Vector3(i + 1, _magnitude[i + 1 + index] + h * 100, 0);
            Debug.DrawLine(s, e, Color.red);

            s = new Vector3(i, _magnitudeSmooth[i + index] + h * 100, 0);
            e = new Vector3(i + 1, _magnitudeSmooth[i + 1 + index] + h * 100, 0);
            Debug.DrawLine(s, e, Color.red);

            s = new Vector3(i, _magnitudeAvg[i + index] + h * 100, 0);
            e = new Vector3(i + 1, _magnitudeAvg[i + 1 + index] + h * 100, 0);
            Debug.DrawLine(s, e, Color.black);

            s = new Vector3(i, _flux[i + index] + h * 100, 0);
            e = new Vector3(i + 1, _flux[i + 1 + index] + h * 100, 0);
            Debug.DrawLine(s, e, Color.blue);
            
            if (_onsets.ContainsKey(i + index))
            {
                Onset onset = _onsets[i + index];

                s = new Vector3(i, h * 100, -1);
                e = new Vector3(i, onset.strength + h * 100, -1);
                Debug.DrawLine(s, e, Color.green);

                s = new Vector3(i, h * 100, 0);
                e = new Vector3(i, -onset.rank + h * 100, 0);
                Debug.DrawLine(s, e, Color.white);
            }
        }
    }
}
