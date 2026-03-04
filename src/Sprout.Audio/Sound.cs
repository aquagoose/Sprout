using MixrSharp;
using MixrSharp.Stream;

namespace Sprout.Audio;

public class Sound : IDisposable
{
    private readonly Context _context;
    private readonly AudioFormat _format;
    private readonly AudioBuffer _buffer;
    private readonly List<SoundInstance> _instances;
    
    internal Sound(Context context, string path)
    {
        _context = context;
        
        using AudioStream stream = StreamUtils.CreateStream(path);
        _format = stream.Format;
        
        byte[] buffer = stream.GetPcm();
        _buffer = _context.CreateBuffer(buffer);
        
        _instances = [];
    }

    public SoundInstance Play(float volume = 1.0f, double speed = 1.0)
    {
        SourceDescription description = new()
        {
            Format = _format,
            Type = SourceType.Pcm
        };
        AudioSource source = _context.CreateSource(description);
        source.Volume = volume;
        source.Speed = speed;
        source.SubmitBuffer(_buffer);
        SoundInstance instance = new SoundInstance(source);
        _instances.Add(instance);
        // TODO: This implementation is absolutely god awful!! I hate it
        instance.FinishedPlaying += InstanceOnFinishedPlaying;
        instance.Play();

        return instance;
    }

    private void InstanceOnFinishedPlaying(SoundInstance instance)
    {
        Console.WriteLine("Finished");
        _instances.Remove(instance);
        instance.Dispose();
    }

    public void Dispose()
    {
        foreach (SoundInstance instance in _instances)
            instance.Dispose();
        
        _buffer.Dispose();
    }
}