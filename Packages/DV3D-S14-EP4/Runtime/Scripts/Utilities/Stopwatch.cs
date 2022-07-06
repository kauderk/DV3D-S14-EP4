using UnityEngine;

public class Stopwatch
{
    public float Duration { get; private set; } = 0;

    private float _timestamp = 0f;
    private bool _isElapsed = false;

    public bool IsElapsed
    {
        get
        {
            // Preventing from using UnityEngine.Time which is quiet expensive performance wise
            if (_isElapsed)
                return true;

            _isElapsed = Time.time - _timestamp > Duration;

            return _isElapsed;
        }
    }

    public Stopwatch(float duration) =>
        Duration = duration;

    public void Reset(float duration_ = 0f)
    {
        Duration = duration_ <= 0 ? Duration : duration_;
        _timestamp = Time.time;
        _isElapsed = false;
    }
}