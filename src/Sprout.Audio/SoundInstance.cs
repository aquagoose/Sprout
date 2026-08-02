using System.Diagnostics;
using MixrSharp;

namespace Sprout.Audio;

public struct SoundInstance
{
    private readonly Sound.SourceInstance _source;
    private readonly uint _instanceID;

    public float Volume
    {
        get
        {
            Debug.Assert(_source.IsValid && _source.CurrentID == _instanceID,
                "This sound instance is invalid and is not associated with any playing sound.");
            return _source.Source.Volume;
        }
        set
        {
            Debug.Assert(_source.IsValid && _source.CurrentID == _instanceID,
                "This sound instance is invalid and is not associated with any playing sound.");
            _source.Source.Volume = value;
        }
    }

    public double Speed
    {
        get
        {
            Debug.Assert(_source.IsValid && _source.CurrentID == _instanceID,
                "This sound instance is invalid and is not associated with any playing sound.");
            return _source.Source.Speed;
        }
        set
        {
            Debug.Assert(_source.IsValid && _source.CurrentID == _instanceID,
                "This sound instance is invalid and is not associated with any playing sound.");
            _source.Source.Speed = value;
        }
    }

    public void Resume()
    {
        Debug.Assert(_source.IsValid && _source.CurrentID == _instanceID,
            "This sound instance is invalid and is not associated with any playing sound.");
        _source.Source.Play();
    }

    public void Pause()
    {
        Debug.Assert(_source.IsValid && _source.CurrentID == _instanceID,
            "This sound instance is invalid and is not associated with any playing sound.");
        _source.Source.Pause();
    }

    public void Stop()
    {
        Debug.Assert(_source.IsValid && _source.CurrentID == _instanceID,
            "This sound instance is invalid and is not associated with any playing sound.");
        _source.Source.Stop();
    }

    internal SoundInstance(Sound.SourceInstance source, uint instanceId)
    {
        _source = source;
        _instanceID = instanceId;
    }
}