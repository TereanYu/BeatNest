

/// <summary>
/// Represents a beat.
/// </summary>
[System.Serializable]
public class Beat
{
	/// <summary>
	/// Length of this beat
	/// </summary>
	public float length;

	/// <summary>
	/// Most probable bpm during this beat
	/// </summary>
	public float bpm;

	/// <summary>
	/// Frame index at which this beat occurs
	/// </summary>
	public int index;

	/// <summary>
	/// Initialize a new Beat.
	/// </summary>
	/// <param name="length">The number of frames between Beats during this beat.</param>
	/// <param name="bpm">The BPM that is most likely during this beat.</param>
	/// <param name="index">The index at which this beat occurs.</param>
	public Beat (float length, float bpm, int index)
	{
		this.length = length;
		this.bpm = bpm;
		this.index = index;
	}
}
