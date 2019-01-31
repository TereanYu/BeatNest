using System;
using UnityEngine;
using System.Collections.Generic;

public static class Util
{
    private static LomontFFT fft = new LomontFFT();

    private static float[] magnitude = new float[0];
    private static float[] upsampledSignal = new float[0];
    private static float[] mono = new float[0];

    public static void GetSpectrum(float[] samples)
    {
        fft.RealFFT(samples, true);
    }

    public static float[] GetSpectrumMagnitude(float[] spectrum)
    {
        if (magnitude.Length != spectrum.Length / 2)
            magnitude = new float[spectrum.Length / 2];

        GetSpectrumMagnitude(spectrum, magnitude);

        return magnitude;
    }

    public static void GetSpectrumMagnitude(float[] spectrum, float[] spectrumMagnitude)
    {
        if (spectrumMagnitude.Length != spectrum.Length / 2)
            throw new Exception("SpectrumMagnitude length has to be half of spectrum length.");

        for (int i = 0; i < spectrumMagnitude.Length - 2; i++)
        {
            int ii = (i * 2) + 2;
            float re = spectrum[ii];
            float im = spectrum[ii + 1];
            spectrumMagnitude[i] = Mathf.Sqrt((re * re) + (im * im));
        }

        spectrumMagnitude[spectrumMagnitude.Length - 2] = spectrum[0];
        spectrumMagnitude[spectrumMagnitude.Length - 1] = spectrum[1];
    }

    public static float[] GetMono(float[] samples, int channels)
    {
        if (mono.Length != samples.Length / channels)
            mono = new float[samples.Length / channels];

        GetMono(samples, mono, channels);

        return mono;
    }

    public static void GetMono(float[] samples, float[] mono, int channels = 0)
    {
        if (channels == 0)
            channels = samples.Length / mono.Length;

        if (samples.Length % mono.Length != 0)
            throw new ArgumentException("Sample length is not a multiple of mono length.");

        if (mono.Length * channels != samples.Length)
            throw new ArgumentException("Mono length does not match samples length for " + channels + " channels");

        for (int i = 0; i < mono.Length; i++)
        {
            float mean = 0;

            for (int ii = 0; ii < channels; ii++)
                mean += samples[i * channels + ii];

            mean /= channels;

            mono[i] = mean * 1.4f;
        }
    }

    public static float Sum(IList<float> input, int start, int end)
    {
        float output = 0;

        for (int i = start; i < end; i++)
        {
            output += input[i];
        }

        return output;
    }

    public static void Smooth(IList<float> input, int windowSize)
    {
        //Note: this is "incorrect" (smoothing happens in place, so it will use already smoothed values of preceding indices)
        //but it gives better results when used with beat tracking

        for (int i = 0; i < input.Count; i++)
        {
            input[i] = Average(input, i, windowSize);
        }
    }
    
    public static float Average(IList<float> input, int index, int windowSize)
    {
        float average = 0;
        for (int i = index - (windowSize / 2); i < index + (windowSize / 2); i++)
        {
            if (i > 0 && i < input.Count)
                average += input[i];
        }

        return average / windowSize;
    }

    public static float[] UpsampleSingnal(float[] signal, int multiplier)
    {
        if (upsampledSignal.Length != signal.Length * multiplier)
            upsampledSignal = new float[signal.Length * multiplier];

        UpsampleSingnal(signal, upsampledSignal, multiplier);

        return upsampledSignal;
    }

    public static void UpsampleSingnal(float[] signal, float[] upsampledSignal, int multiplier)
    {
        if (upsampledSignal.Length != signal.Length * multiplier)
            throw new ArgumentException("UpsampledSignal does not match signal length and multiplier");

        for (int i = 0; i < signal.Length - 1; i++)
        {
            for (int ii = 0; ii < multiplier; ii++)
            {
                float f = (float)ii / (float)multiplier;
                upsampledSignal[(i * multiplier) + ii] = Mathf.Lerp(signal[i], signal[i + 1], f);
            }
        }
    }

    public static int GetBestIndex(IList<float> input, int start = 0, int end = 0)
    {
        int best = start;

        if (end == 0)
            end = input.Count;
        
        for (int i = start; i < end; i++)
            if (input[i] > input[best])
                best = i;

        return best;
    }
}
