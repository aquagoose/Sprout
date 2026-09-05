using MixrSharp;
using piko.SDL3;
using Sprout.Content;

namespace Sprout.Audio;

public class AudioDevice : IDisposable
{
    private readonly SDL.AudioStreamCallback _callback;
    private readonly SDL.AudioStream _audioDevice;
    private readonly Context _context;

    private List<StreamSound> _singleFireSounds;

    public unsafe AudioDevice()
    {
        if (!SDL.Init(SDL.InitFlags.Audio))
            throw new Exception($"Failed to initialize SDL: {SDL.GetError()}");

        _callback = AudioCallback;
        _context = new Context(48000);
        
        SDL.AudioSpec spec = new()
        {
            Format = SDL.AudioFormat.F32le,
            Freq = 48000,
            Channels = 2
        };

        _audioDevice = SDL.OpenAudioDeviceStream(SDL.AudioDeviceDefaultPlayback, &spec, _callback, 0);
        if (_audioDevice.IsNull)
            throw new Exception($"Failed to open audio device: {SDL.GetError()}");
        SDL.ResumeAudioStreamDevice(_audioDevice);

        _singleFireSounds = [];
    }

    public void Dispose()
    {
        SDL.DestroyAudioStream(_audioDevice);
        _context.Dispose();
    }

    public Sound CreateSound(string path)
    {
        return new Sound(_context, PathUtils.GetFullPath(path));
    }

    public StreamSound CreateStreamSound(string path, bool looping = false, uint loopStart = 0, uint loopEnd = 0)
    {
        return new StreamSound(_context, PathUtils.GetFullPath(path), looping, loopStart, loopEnd);
    }

    public void PlaySoundOneShot(string path)
    {
        StreamSound sound = new StreamSound(_context, PathUtils.GetFullPath(path), false, 0, 0);
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

    private unsafe void AudioCallback(nint userdata, SDL.AudioStream stream, int additionalAmount, int totalAmount)
    {
        const int bufferSize = 512;
        float* buffer = stackalloc float[bufferSize];
        while (additionalAmount > 0)
        {
            int total = int.Min(additionalAmount, bufferSize);
            Span<float> bufferSlice = new Span<float>(buffer, total / 4);
            _context.MixToStereoF32Buffer(bufferSlice);
            SDL.PutAudioStreamData(stream, (nint) buffer, total);
            additionalAmount -= total;
        }
    }
}