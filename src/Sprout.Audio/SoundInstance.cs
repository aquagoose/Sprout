using MixrSharp;

namespace Sprout.Audio;

public class SoundInstance : IDisposable
{
    private readonly AudioSource _source;

    internal event OnFinishedPlaying FinishedPlaying;

    public float Volume
    {
        get => _source.Volume;
        set => _source.Volume = value;
    }

    public double Speed
    {
        get => _source.Speed;
        set => _source.Speed = value;
    }

    public void Play()
    {
        _source.Play();
    }

    public void Pause()
    {
        _source.Pause();
    }

    internal SoundInstance(AudioSource source)
    {
        _source = source;
        FinishedPlaying = delegate { };
        _source.StateChanged += SourceOnStateChanged;
    }

    private void SourceOnStateChanged(SourceState state)
    {
        if (state == SourceState.Stopped)
            FinishedPlaying(this);
    }

    public void Dispose()
    {
        _source.Dispose();
    }

    internal delegate void OnFinishedPlaying(SoundInstance instance);
}