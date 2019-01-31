using UnityEngine;

public class AnalyzeExample : MonoBehaviour
{
    public RhythmTool rhythmTool;

	public AudioClip audioClip;

	void Start ()
	{
        rhythmTool.SongLoaded += OnSongLoaded;

        rhythmTool.audioClip = audioClip;
    }

    private void OnSongLoaded()
	{
		rhythmTool.Play ();	
	}
    	
	void Update ()
	{		
		rhythmTool.DrawDebugLines ();		
	}
}
