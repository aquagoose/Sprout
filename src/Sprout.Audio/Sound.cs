using MixrSharp;
using MixrSharp.Stream;

namespace Sprout.Audio;

public class Sound : IDisposable
{
    private readonly Context _context;
    private readonly AudioBuffer _buffer;
    private readonly AudioFormat _format;

    public Sound(Context context, string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(null, path);

        using Wav wav = new Wav(path);
        
        _context = context;
        _buffer = _context.CreateBuffer(wav.GetPcm());
        _format = wav.Format;
    }

    public void Play()
    {
        SourceDescription description = new()
        {
            Format = _format,
            Type = SourceType.Pcm
        };
        AudioSource source = _context.CreateSource(description);
        source.SubmitBuffer(_buffer);
        source.Play();
    }

    public void Dispose()
    {
        _buffer.Dispose();
    }
}