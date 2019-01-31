using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// Stores the results of an analysis in readonly collections
/// </summary>
[System.Serializable]
public class AnalysisData
{
    /// <summary>
    /// The Analysis' name.
    /// </summary>
	public string name { get; private set; }

    /// <summary>
    /// The combined magnitude of all frequencies in the specified frequency range. Essentially the loudness.
    /// </summary>
	public ReadOnlyCollection<float> magnitude { get; private set; }

    /// <summary>
    /// The difference between a frame’s and the previous frame’s magnitudes.
    /// </summary>
	public ReadOnlyCollection<float> flux { get; private set; }

    /// <summary>
    /// A smoothed version of magnitude.
    /// </summary>
	public ReadOnlyCollection<float> magnitudeSmooth { get; private set; }

    /// <summary>
    /// A version of magnitudeSmooth that interpolates from trough to trough and peak to peak. 
    /// This is like a smooth version of magnitude, but without big variations.
    /// </summary>
	public ReadOnlyCollection<float> magnitudeAvg { get; private set; }

    /// <summary>
    /// Detected peaks that represent the beginning of a note, sound or beat.
    /// </summary>
	public ReadOnlyDictionary<int, Onset> onsets { get; private set; }

	public AnalysisData (string name, List<float> magnitude, List<float> flux, List<float> magnitudeSmooth, List<float> magnitudeAvg, Dictionary<int, Onset> onsets)
	{
		this.name = name;
		this.magnitude = magnitude.AsReadOnly ();
		this.flux = flux.AsReadOnly ();
		this.magnitudeSmooth = magnitudeSmooth.AsReadOnly ();
		this.magnitudeAvg = magnitudeAvg.AsReadOnly ();
		this.onsets = new ReadOnlyDictionary<int, Onset> (onsets);
	}
}
