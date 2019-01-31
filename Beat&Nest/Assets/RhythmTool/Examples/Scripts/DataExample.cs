using UnityEngine;

public class DataExample : MonoBehaviour
{	
	public RhythmTool rhythmTool;

    public AudioClip audioClip;

	private AnalysisData low;
	
	void Start ()
	{
        rhythmTool.SongLoaded += OnSongLoaded;

        low = rhythmTool.low;

        rhythmTool.audioClip = audioClip;
	}
	
	private void OnSongLoaded()
	{
        rhythmTool.Play ();	
	}	
	
	void Update ()
	{
        if (!rhythmTool.songLoaded)
            return;

		for(int i = 0; i < 100; i++)
        {
            int frameIndex = Mathf.Min(rhythmTool.currentFrame + i, rhythmTool.totalFrames);
            
            float x = i - rhythmTool.interpolation;

            Vector3 start = new Vector3(x, low.magnitude[frameIndex], 0);
            Vector3 end = new Vector3(x+1, low.magnitude[frameIndex+1], 0);
            Debug.DrawLine(start, end, Color.black);
            
            if(rhythmTool.beats.ContainsKey(frameIndex))
            { 
				start = new Vector3(x,0,0);
				end = new Vector3(x,10,0);				
				Debug.DrawLine(start, end, Color.white);
			}           
		}

		Debug.DrawLine(Vector3.zero,Vector3.up * 30,Color.red);
	}
}
