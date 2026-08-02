using MixrSharp;
using MixrSharp.Stream;

namespace Sprout.Audio;

public class Sound : IDisposable
{
    private readonly Context _context;
    private readonly AudioFormat _format;
    private readonly AudioBuffer _buffer;

    private readonly List<SourceInstance> _activeSources;
    private readonly Queue<SourceInstance> _sourcePool;
    
    internal Sound(Context context, string path)
    {
        _context = context;
        
        using AudioStream stream = StreamUtils.CreateStream(path);
        _format = stream.Format;
        
        byte[] buffer = stream.GetPcm();
        _buffer = _context.CreateBuffer(buffer);

        _activeSources = [];
        _sourcePool = [];
    }

    public SoundInstance Play(float volume = 1.0f, double speed = 1.0)
    {
        if (!_sourcePool.TryDequeue(out SourceInstance source))
        {
            SourceDescription description = new()
            {
                Format = _format,
                Type = SourceType.Pcm
            };
            source = new SourceInstance(_context.CreateSource(description));
        }

        source.IsValid = true;
        source.CurrentID++;

        _activeSources.Add(source);
        source.Finished += SourceFinished;

        source.Source.Volume = volume;
        source.Source.Speed = speed;
        source.Source.SubmitBuffer(_buffer);
        source.Source.Play();

        return new SoundInstance(source, source.CurrentID);
    }

    private void SourceFinished(SourceInstance instance)
    {
        instance.Finished -= SourceFinished;
        _activeSources.Remove(instance);
        _sourcePool.Enqueue(instance);
    }

    public void Dispose()
    {
        foreach (SourceInstance source in _activeSources)
            source.Source.Dispose();
        foreach (SourceInstance source in _sourcePool)
            source.Source.Dispose();
        
        _buffer.Dispose();
    }

    internal class SourceInstance
    {
        public event OnFinished Finished = delegate { };

        public readonly AudioSource Source;

        public bool IsValid;
        public uint CurrentID;

        public SourceInstance(AudioSource source)
        {
            Source = source;
            source.StateChanged += SourceOnStateChanged;
        }

        private void SourceOnStateChanged(SourceState state)
        {
            if (state == SourceState.Stopped)
            {
                IsValid = false;
                Finished(this);
            }
        }

        public delegate void OnFinished(SourceInstance instance);
    }
}