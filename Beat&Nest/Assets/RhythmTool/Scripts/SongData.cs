using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[System.Serializable]
public class SongData
{
	public List<AnalysisData> analyses { get; private set; }

	public ReadOnlyDictionary<int, Beat> beats { get; private set; }

	public ReadOnlyDictionary<int, float> changes { get; private set; }
    
	public int length { get; private set; }
    
	public SongData (int length, List<AnalysisData> analyses, ReadOnlyDictionary<int, Beat> beats, ReadOnlyDictionary<int, float> changes)
	{
		this.analyses = analyses;
		this.length = length;
		this.beats = beats;
		this.changes = changes;
	}
    
    public void Save(string path)
    {
        Save(path, this);
    }       

    public static SongData Load(string path)
    {
        if (!File.Exists(path))
            return null;

        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var formatter = new BinaryFormatter();
            SongData songData = (SongData)formatter.Deserialize(stream);
            return songData;
        }
    }

    public static void Save(string path, SongData songData)
    {
        using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, songData);
        }
    }
}

