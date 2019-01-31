using UnityEngine;
using System.Collections;

public class EventsExample : MonoBehaviour
{
    public Transform transform1;

    public Transform transform2;

    public RhythmTool rhythmTool;

    public RhythmEventProvider eventProvider;

    public AudioClip audioClip;

    void Start()
    {
        eventProvider.SongLoaded += OnSongLoaded;
        eventProvider.Beat += OnBeat;
        eventProvider.SubBeat += OnSubBeat;

        rhythmTool.audioClip = audioClip;
    }

    private void OnSongLoaded()
    {
        rhythmTool.Play();
    }

    private void OnBeat(Beat beat)
    {
        transform1.localScale = Random.insideUnitSphere;
    }

    private void OnSubBeat(Beat beat, int count)
    {
        if(count == 0 || count == 2)
            transform2.localScale = Random.insideUnitSphere;
    }
}
