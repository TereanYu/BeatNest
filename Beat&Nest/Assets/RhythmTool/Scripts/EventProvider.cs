using System;
using UnityEngine;

/// <summary>
/// EventProvider base class.
/// </summary>
/// <typeparam name="T">Type that implements IEventSequence</typeparam>
public abstract class EventProvider<T> : MonoBehaviour where T : MonoBehaviour, IEventSequence
{
    /// <summary>
    /// Occurs every Update.
    /// </summary>
    public event Action<int, float> TimingUpdate;

    /// <summary>
    /// Occurs for every frame that has passed.
    /// </summary>
    public event Action<int, int> FrameChanged;

    /// <summary>
    /// Occurs when an IEventSequence has been loaded or restarted.
    /// </summary>
    public event Action Reset;

    /// <summary>
    /// The IEventSequence that will be used by this EventProvider.
    /// </summary>
    public T target
    {
        get
        {
            return _target;
        }
        set
        {
            OnTargetChanged(_target, value);
            _target = value;
        }
    }

    /// <summary>
    /// The frame index that is currently occuring.
    /// </summary>
    public int currentFrame
    {
        get
        {
            if (target == null)
                return 0;

            return target.currentFrame;
        }
    }

    /// <summary>
    /// The current time in between the current frame and the next frame.
    /// </summary>
    public float interpolation
    {
        get
        {
            if (target == null)
                return 0;

            return target.interpolation;
        }
    }

    /// <summary>
    /// The total number of frames for the IEventSequence.
    /// </summary>
    public int totalFrames
    {
        get
        {
            if (target == null)
                return 0;

            return target.totalFrames;
        }
    }

    /// <summary>
    /// The offset for this EventProvider.
    /// </summary>
    public int offset { get; private set; }
    
    /// <summary>
    /// The max offset that is allowed for this EventProvider.
    /// </summary>
    public virtual int maxOffset { get { return 300; } }

    /// <summary>
    /// The targeted offset for this EventProvider.
    /// </summary>
    public int targetOffset
    {
        get
        {
            return _targetOffset;
        }
        set
        {
            _targetOffset = Mathf.Clamp(value, 0, maxOffset);
        }
    }

    [SerializeField]
    private int _targetOffset;

    [SerializeField]
    private T _target;

    protected virtual void Awake()
    {
        target = _target;
    }
    
    protected virtual void OnTargetChanged(T oldTarget, T newTarget)
    {
        if (oldTarget != null)
        {
            oldTarget.FrameUpdate -= OnTimingUpdate;
            oldTarget.FrameChanged -= OnFrameChanged;
            oldTarget.Reset -= OnReset;
        }

        if (newTarget != null)
        {
            newTarget.FrameUpdate += OnTimingUpdate;
            newTarget.FrameChanged += OnFrameChanged;
            newTarget.Reset += OnReset;
        }

        OnReset();
    }

    private void OnTimingUpdate(int currentFrame, float interpolation)
    {
        if (TimingUpdate != null)
            TimingUpdate(currentFrame, interpolation);
    }

    private void OnFrameChanged(int currentFrame, int lastFrame)
    {
        while (offset < targetOffset)
        {
            if (FrameChanged != null)
                FrameChanged(currentFrame + offset, lastFrame);

            offset++;
        }

        offset = Mathf.Min(offset, targetOffset);

        if (FrameChanged != null)
            FrameChanged(currentFrame + offset, lastFrame);
    }

    private void OnReset()
    {
        if (Reset != null)
            Reset();

        offset = 0;
    }

    void OnDestroy()
    {
        target = null;
    }
}
