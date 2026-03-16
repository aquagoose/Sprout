using MixrSharp;
using MixrSharp.Stream;

namespace Sprout.Audio;

public class StreamSound : IDisposable
{
    private const uint NumBuffers = 2;
    
    public event OnFinishedPlaying FinishedPlaying = delegate { };
    
    private readonly Context _context;
    private readonly AudioFormat _format;

    private readonly AudioStream _stream;
    private readonly AudioSource _source;

    private readonly AudioBuffer[] _audioBuffers;
    private readonly byte[] _buffer;
    private uint _currentBuffer;

    public bool Looping;

    public ulong LoopStart;

    public ulong LoopEnd;

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

    public bool IsPlaying => _source.State == SourceState.Playing;

    public bool IsPaused => _source.State == SourceState.Paused;

    public bool IsStopped => _source.State == SourceState.Stopped;

    internal StreamSound(Context context, string path, bool looping, uint loopStart, uint loopEnd)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(null, path);

        _context = context;
        Looping = looping;
        LoopStart = loopStart;
        LoopEnd = loopEnd;

        _stream = StreamUtils.CreateStream(path);
        _format = _stream.Format;
        
        SourceDescription description = new()
        {
            Format = _format,
            Type = SourceType.Pcm
        };
        _source = _context.CreateSource(description);
        _source.Looping = true;
        _source.BufferFinished += SourceOnBufferFinished;
        _source.StateChanged += SourceOnStateChanged;

        // Half-second buffer
        _buffer = new byte[_format.BytesPerSample * _format.Channels * 24000];
        
        _audioBuffers = new AudioBuffer[NumBuffers];
        Stop();
    }

    private void SourceOnBufferFinished()
    {
        ulong bytePos = _stream.PositionInSamples * _format.BytesPerSample * _format.Channels;
        ulong bytesReceived = _stream.GetBuffer(_buffer);
        
        byte[] buffer = _buffer;

        if (LoopEnd != 0)
        {
            ulong loopEnd = LoopEnd == 0 ? _stream.LengthInSamples : LoopEnd;
            ulong loopEndBytes = loopEnd * _format.BytesPerSample * _format.Channels;
            if (Looping && bytePos + bytesReceived > loopEndBytes)
            {
                ulong length = loopEndBytes - bytePos;
                buffer = _buffer[..(int) length];
                _stream.SeekToSample(LoopStart);
            }
        }
        
        if (bytesReceived < (ulong) _buffer.Length)
        {
            if (Looping)
            {
                _stream.SeekToSample(LoopStart);
                bytesReceived = _stream.GetBuffer(_buffer);
                if (bytesReceived == 0)
                    return;
            }
            else
            {
                _source.Looping = false;
                if (bytesReceived == 0)
                    return;

                buffer = _buffer[..(int) bytesReceived];
            }
        }
        
        _audioBuffers[_currentBuffer].Update(buffer);
        _source.SubmitBuffer(_audioBuffers[_currentBuffer]);

        _currentBuffer = (_currentBuffer + 1) % NumBuffers;
    }

    private void SourceOnStateChanged(SourceState state)
    {
        if (state == SourceState.Stopped)
            FinishedPlaying(this);
    }

    public void Play()
    {
        _source.Play();
    }

    public void Pause()
    {
        _source.Pause();
    }

    public void Stop()
    {
        _source.Stop();
        _stream.Restart();
        _currentBuffer = 0;
        for (int i = 0; i < _audioBuffers.Length; i++)
        {
            _stream.GetBuffer(_buffer);
            _audioBuffers[i] = _context.CreateBuffer(_buffer);
            _source.SubmitBuffer(_audioBuffers[i]);
        }
    }

    public void Restart()
    {
        Stop();
        Play();
    }

    public void Dispose()
    {
        FinishedPlaying = delegate { };
        
        _source.Dispose();
        
        foreach (AudioBuffer buffer in _audioBuffers)
            buffer.Dispose();
        
        _stream.Dispose();
    }

    public delegate void OnFinishedPlaying(StreamSound sound);
}