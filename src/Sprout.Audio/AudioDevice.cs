using MixrSharp;
using SDL3;

namespace Sprout.Audio;

public class AudioDevice : IDisposable
{
    private readonly SDL.AudioStreamCallback _callback;
    private readonly IntPtr _audioDevice;
    private readonly Context _context;

    private List<StreamSound> _singleFireSounds;

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

        _singleFireSounds = [];
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public StreamSound CreateStreamSound(string path)
    {
        return new StreamSound(_context, path);
    }

    public void PlaySoundOneShot(string path)
    {
        StreamSound sound = new StreamSound(_context, path);
        sound.FinishedPlaying += SoundOnFinishedPlaying;
        _singleFireSounds.Add(sound);
        sound.Play();
    }

    private void SoundOnFinishedPlaying(StreamSound sound)
    {
        sound.Dispose();
        _singleFireSounds.Remove(sound);
        Console.WriteLine(_singleFireSounds.Count);
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