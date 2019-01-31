using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class VisualizerExample : MonoBehaviour
{
    public RhythmTool rhythmTool;    
    public RhythmEventProvider eventProvider;
    public Text bpmText;
    public GameObject linePrefab;
    public List<AudioClip> audioClips;
    
    private List<Line> lines;	
    private int currentSong;
    private ReadOnlyCollection<float> magnitudeSmooth;

	void Start ()
	{		
		currentSong = -1;
		Application.runInBackground = true;
        
        lines = new List<Line>();

        eventProvider.Onset += OnOnset;
        eventProvider.Beat += OnBeat;
        eventProvider.Change += OnChange;
        eventProvider.SongLoaded += OnSongLoaded;
        eventProvider.SongEnded += OnSongEnded;

        magnitudeSmooth = rhythmTool.low.magnitudeSmooth;

        if (audioClips.Count <= 0)
			Debug.LogWarning ("no songs configured");
		else        
			NextSong();		
	}	
	
	private void OnSongLoaded()
	{          
		rhythmTool.Play ();					
	}

    private void OnSongEnded()
	{		
		NextSong();	
	}
	
	private void NextSong ()
	{
		ClearLines ();
		
		currentSong++;
		
		if (currentSong >= audioClips.Count)
			currentSong = 0;
			
		rhythmTool.audioClip = audioClips [currentSong];	
	}
	
	void Update ()
	{		
		if (Input.GetKeyDown (KeyCode.Space))
			NextSong ();

		if (Input.GetKey (KeyCode.Escape))
			Application.Quit ();			

		if (!rhythmTool.songLoaded)        						
			return;		

        UpdateLines();

        bpmText.text = rhythmTool.bpm.ToString();

        rhythmTool.DrawDebugLines ();
    }

    private void UpdateLines()
    {
        List<Line> toRemove = new List<Line>();
        foreach(Line line in lines)
        {
            if (line.index < rhythmTool.currentFrame || line.index > rhythmTool.currentFrame + eventProvider.offset)
            {
                Destroy(line.gameObject);
                toRemove.Add(line);
            }
        }

        foreach (Line line in toRemove)
            lines.Remove(line);

        float[] cumulativeMagnitudeSmooth = new float[eventProvider.offset + 1];
        float sum = 0;
        for (int i = 0; i < cumulativeMagnitudeSmooth.Length; i++)
        {
            int index = Mathf.Min(rhythmTool.currentFrame + i, rhythmTool.totalFrames-1);

            sum += magnitudeSmooth[index];
            cumulativeMagnitudeSmooth[i] = sum;
        }

        foreach (Line line in lines)
        {
            Vector3 pos = line.transform.position;
            pos.x = cumulativeMagnitudeSmooth[line.index - rhythmTool.currentFrame] * .2f;
            pos.x -= magnitudeSmooth[rhythmTool.currentFrame] * .2f * rhythmTool.interpolation;
            line.transform.position = pos;
        }
    }
        
    private void OnBeat(Beat beat)
    {
        lines.Add(CreateLine(beat.index, Color.white, 20, -40));       
    }

    private void OnChange(int index, float change)
    {
        if (change > 0)
            lines.Add(CreateLine(index, Color.yellow, 20, -60));
    }
    
    private void OnOnset(OnsetType type, Onset onset)
    {
        if (onset.rank < 4 && onset.strength < 5)
            return;

        switch (type)
        {
            case OnsetType.Low:
                lines.Add(CreateLine(onset.index, Color.blue, onset.strength, -20));
                break;
            case OnsetType.Mid:
                lines.Add(CreateLine(onset.index, Color.green, onset.strength, 0));
                break;
            case OnsetType.High:
                lines.Add(CreateLine(onset.index, Color.yellow, onset.strength, 20));
                break;
            case OnsetType.All:
                lines.Add(CreateLine(onset.index, Color.magenta, onset.strength, 40));
                break;
        }
    }

    private Line CreateLine(int index, Color color, float opacity, float yPosition)
    {
        GameObject lineObject = Instantiate(linePrefab) as GameObject;
        lineObject.transform.position = new Vector3(0, yPosition, 0);

        Line line = lineObject.GetComponent<Line>();
        line.Init(color, opacity, index);

        return line;
    }

    private void ClearLines()
    {
        foreach (Line line in lines)
            Destroy(line.gameObject);

        lines.Clear();
    }
}
