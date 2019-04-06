public class XUTimer
{
    System.Diagnostics.Stopwatch _stopWatch = new System.Diagnostics.Stopwatch();
    public float duration = float.PositiveInfinity;

    public XUTimer(float duration = float.PositiveInfinity)
    {
        this.duration = duration;
    }

    public void Restart()
    {
        _stopWatch.Restart();
    }
    public void Restart(float newDuration)
    {
        duration = newDuration;
        _stopWatch.Restart();
    }

    public void Start()
    {
        _stopWatch.Start();
    }

    public void Start(float newDuration)
    {
        duration = newDuration;
        _stopWatch.Start();
    }

    public void Stop()
    {
        _stopWatch.Stop();
    }

    public bool expired => timeElapsed >= duration;
    public long timeElapsedMs => _stopWatch.ElapsedMilliseconds;
    public float timeElapsed => (float)(.001f * _stopWatch.ElapsedMilliseconds);
}
