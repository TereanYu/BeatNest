using System;

public interface IEventSequence
{
	event Action<int, float> FrameUpdate;
	event Action<int, int> FrameChanged;
	event Action Reset;

	int currentFrame { get; }

	float interpolation { get; }

	int totalFrames { get; }
}

