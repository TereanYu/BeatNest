using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class Segmenter
{
    public ReadOnlyDictionary<int, float> changes { get; private set; }
    public ReadOnlyCollection<int> changeIndices { get; private set; }

    private Dictionary<int, float> _changes;
    private List<int> _changeIndices;

    private ReadOnlyCollection<float> magnitudeSmooth;
    private ReadOnlyCollection<float> magnitudeAvg;

    private int changeStart = 0;
    private float changeSign = 0;

    public Segmenter(AnalysisData analysis)
    {
        magnitudeSmooth = analysis.magnitudeSmooth;
        magnitudeAvg = analysis.magnitudeAvg;

        _changes = new Dictionary<int, float>();
        _changeIndices = new List<int>();

        changes = new ReadOnlyDictionary<int, float>(_changes);
        changeIndices = _changeIndices.AsReadOnly();
    }

    public void Init()
    {
        changeStart = 0;
        
        _changes.Clear();
        _changeIndices.Clear();
    }

    public void Init(IDictionary<int, float> changes)
    {
        _changeIndices.Clear();
        _changes.Clear();

        foreach (KeyValuePair<int, float> item in changes)
        {
            _changes.Add(item.Key, item.Value);
            _changeIndices.Add(item.Key);
        }

        _changeIndices.Sort();
    }
    
    public void DetectChanges(int index)
    {
        if (index < 0)
            return;

        float dif = magnitudeAvg[index + 1] - magnitudeAvg[index];
        
        if (dif >= .05f && changeStart == 0)        
            changeStart = index;        

        if (dif <= -.08f && changeStart == 0)        
            changeStart = index;

        if (changeStart == index)
            changeSign = Mathf.Sign(dif);

        if (dif * changeSign < .04f * changeSign && changeStart != 0)
        {
            int requiredLength = 22;

            if (dif * changeSign > .04f * -changeSign)
                requiredLength = 12;

            int bestIndex = changeStart + (index - changeStart) / 2;
            float best = magnitudeSmooth[bestIndex + 1] - magnitudeSmooth[bestIndex];

            for (int i = changeStart; i < index; i++)
            {
                float current = magnitudeSmooth[i + 1] - magnitudeSmooth[i];

                if (current * changeSign > best * changeSign)
                {
                    bestIndex = i;
                    best = current;
                }
            }

            float length = magnitudeAvg[index] - magnitudeAvg[changeStart];
            length = Mathf.Sqrt(Mathf.Pow(length, 2) + Mathf.Pow((index - changeStart) * .1f, 2));

            if (length > requiredLength)
            {
                _changes.Add(bestIndex, magnitudeAvg[index] * changeSign);
                _changeIndices.Add(bestIndex);
            }

            changeStart = 0;
        }
    }

    public int PrevChangeIndex(int index)
    {
        if (_changeIndices.Count == 0)
            return 0;

        int prevChange = _changeIndices.BinarySearch(index);
        prevChange = Mathf.Max(prevChange, ~prevChange);
        prevChange = Mathf.Clamp(prevChange - 1, 0, _changeIndices.Count - 1);

        prevChange = _changeIndices[prevChange];
        return prevChange;
    }

    public int NextChangeIndex(int index)
    {
        if (_changeIndices.Count == 0)
            return 0;

        int nextChange = _changeIndices.BinarySearch(index);
        nextChange = Mathf.Max(nextChange, ~nextChange);
        nextChange = Mathf.Clamp(nextChange, 0, _changeIndices.Count - 1);

        nextChange = _changeIndices[nextChange];
        return nextChange;
    }

    public float PrevChange(int index)
    {
        if (_changes.Count == 0)
            return 0;

        int prevChange = PrevChangeIndex(index);

        return _changes[prevChange];
    }

    public float NextChange(int index)
    {
        if (_changes.Count == 0)
            return 0;

        int nextChange = NextChangeIndex(index);

        return _changes[nextChange];
    }
}
