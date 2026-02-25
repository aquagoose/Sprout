using MixrSharp;
using SDL3;

namespace Sprout.Audio;

public class AudioDevice : IDisposable
{
    private readonly SDL.AudioStreamCallback _callback;
    private readonly IntPtr _audioDevice;
    private readonly Context _context;

    public AudioDevice()
    {
        if (!SDL.Init(SDL.InitFlags.Audio))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");

        _callback = AudioCallback;
        
        SDL.AudioSpec spec = new()
        {
            Format = SDL.AudioFormat.AudioF32LE,
            Freq = 48000,
            Channels = 2
        };

        _audioDevice = SDL.OpenAudioDeviceStream(SDL.AudioDeviceDefaultPlayback, in spec, _callback, 0);
        SDL.ResumeAudioStreamDevice(_audioDevice);
        
        _context = new Context(48000);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public Sound CreateSound(string path)
    {
        return new Sound(_context, path);
    }
    
    private unsafe void AudioCallback(IntPtr userdata, IntPtr stream, int additionalAmount, int totalAmount)
    {
        const int bufferSize = 512;
        float* buffer = stackalloc float[bufferSize];
        while (additionalAmount > 0)
        {
            int total = int.Min(additionalAmount, bufferSize);
            Span<float> bufferSlice = new Span<float>(buffer, total / 4);
            _context.MixToStereoF32Buffer(bufferSlice);
            SDL.PutAudioStreamData(stream, (IntPtr) buffer, total);
            additionalAmount -= total;
        }
    }
}