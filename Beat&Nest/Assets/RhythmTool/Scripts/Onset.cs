
/// <summary>
/// Represents a detected peak.
/// </summary>
[System.Serializable]
public class Onset
{
	/// <summary>
	/// Frame index of this onset.
	/// </summary>
	public int index;

	/// <summary>
	/// Strength of this onset.
	/// </summary>
	public float strength;

	/// <summary>
	/// Rank of this onset. Lower rank (5 is best) indicates surrounding onsets are stronger.
	/// </summary>
	public int rank;

	/// <summary>
	/// Initialize a new Onset.
	/// </summary>
	/// <param name="index">The frame index at which this Onset occurs.</param>
	/// <param name="strength">The strength of this Onset.</param>
	/// <param name="rank">This Onset's rank based on strength compared to neighboring Onsets.</param>
	public Onset (int index, float strength, int rank)
	{
		this.index = index;
		this.rank = rank;
		this.strength = strength;
	}
        
	public static explicit operator float (Onset x)
	{
		if (x == null)
			return 0;

		return x.strength;
	}
}
