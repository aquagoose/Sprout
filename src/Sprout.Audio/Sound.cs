using MixrSharp;
using MixrSharp.Stream;

namespace Sprout.Audio;

public class Sound : IDisposable
{
    private readonly Context _context;
    private readonly AudioFormat _format;
    private readonly AudioBuffer _buffer;
    
    internal Sound(Context context, string path)
    {
        _context = context;
        
        using AudioStream stream = StreamUtils.CreateStream(path);
        _format = stream.Format;
        
        // TODO: GetPCM is broken
        ulong bufferSize = stream.LengthInSamples * _format.BytesPerSample * _format.Channels;
        byte[] buffer = new byte[bufferSize];
        stream.GetBuffer(buffer);

        _buffer = _context.CreateBuffer(buffer);
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
        // TODO: This implementation is absolutely god awful!! I hate it
        instance.FinishedPlaying += InstanceOnFinishedPlaying;
        instance.Play();

        return instance;
    }

    private void InstanceOnFinishedPlaying(SoundInstance instance)
    {
        Console.WriteLine("Finished");
        instance.Dispose();
    }

    public void Dispose()
    {
        _buffer.Dispose();
    }
}