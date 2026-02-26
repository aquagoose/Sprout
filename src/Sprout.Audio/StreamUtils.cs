using MixrSharp.Stream;

namespace Sprout.Audio;

internal static class StreamUtils
{
    public static AudioStream CreateStream(string path)
    {
        return Path.GetExtension(path) switch
        {
            ".wav" => new Wav(path),
            ".ogg" => new Vorbis(path),
            ".mp3" => new Mp3(path),
            ".flac" => new Flac(path),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}