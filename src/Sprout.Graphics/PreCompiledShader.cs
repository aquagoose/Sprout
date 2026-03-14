using System.Reflection;
using Sprout.Content;

namespace Sprout.Graphics;

/// <summary>
/// A Pre-Compiled Shader is a container for various GPU shader formats that have been compiled to their optimal form,
/// to prevent the need for runtime shader compilation and transpilation, and the large libraries that come with doing
/// so.
/// </summary>
public class PreCompiledShader
{
    private const uint Version = 1;
    private const uint Magic = 0x48534350;

    private readonly Dictionary<(Backend backend, ShaderStage stage), (string entryPoint, byte[] source)> _sourceList = [];

    /// <summary>
    /// Fetch a compiled shader source.
    /// </summary>
    /// <param name="backend">The graphics <see cref="Backend"/> of the shader to fetch.</param>
    /// <param name="stage">The <see cref="ShaderStage"/> to fetch.</param>
    /// <param name="entryPoint">The entry point of the shader.</param>
    /// <returns>The shader source, as bytes.</returns>
    public byte[] GetSource(Backend backend, ShaderStage stage, out string entryPoint)
    {
        if (!_sourceList.TryGetValue((backend, stage), out (string entry, byte[] source) source))
        {
            throw new PlatformNotSupportedException(
                $"There is no compiled shader available for the {backend} backend. Perhaps this shader needs recompiling?");
        }

        entryPoint = source.entry;
        return source.source;
    }

    /// <summary>
    /// Add a compiled shader source for the given backend and shader stage.
    /// </summary>
    /// <param name="backend">The <see cref="Backend"/> to associate this source with.</param>
    /// <param name="stage">The source's <see cref="ShaderStage"/>.</param>
    /// <param name="entryPoint">The entry point into the shader.</param>
    /// <param name="source">The shader source itself.</param>
    public void AddSource(Backend backend, ShaderStage stage, string entryPoint, byte[] source)
    {
        _sourceList.Add((backend, stage), (entryPoint, source));
    }

    /// <summary>
    /// Save this <see cref="PreCompiledShader"/> to a file.
    /// </summary>
    /// <param name="path">The path to save to.</param>
    public void Save(string path)
    {
        using FileStream stream = File.OpenWrite(PathUtils.GetFullPath(path));
        using BinaryWriter writer = new BinaryWriter(stream);
        
        writer.Write(Magic);
        writer.Write(Version);
        
        writer.Write(_sourceList.Count);

        foreach (((Backend backend, ShaderStage stage), (string entryPoint, byte[] data)) in _sourceList)
        {
            byte backendStageFlag = (byte) (((byte) backend << 4) | (byte) stage);
            writer.Write(backendStageFlag);
            writer.Write(entryPoint);
            writer.Write(data.Length);
            writer.Write(data);
        }
    }

    /// <summary>
    /// Load a <see cref="PreCompiledShader"/> from a stream.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to load from.</param>
    /// <returns>The loaded <see cref="PreCompiledShader"/>.</returns>
    /// <exception cref="Exception">Thrown if the file is not a valid Pre-Compiled Shader file.</exception>
    public static PreCompiledShader FromStream(Stream stream)
    {
        using BinaryReader reader = new BinaryReader(stream);

        if (reader.ReadUInt32() != Magic)
            throw new Exception("File is not a valid PCSH file.");

        reader.ReadUInt32(); // Version, not used yet.

        uint numSources = reader.ReadUInt32();
        PreCompiledShader pcsh = new();
        
        for (int i = 0; i < numSources; i++)
        {
            byte backendStageFlag = reader.ReadByte();
            Backend backend = (Backend) (backendStageFlag >> 4);
            ShaderStage stage = (ShaderStage) (backendStageFlag & 0xF);

            string entryPoint = reader.ReadString();
            
            int length = reader.ReadInt32();
            byte[] data = reader.ReadBytes(length);
            
            pcsh.AddSource(backend, stage, entryPoint, data);
        }

        return pcsh;
    }

    /// <summary>
    /// Load a <see cref="PreCompiledShader"/> from a file.
    /// </summary>
    /// <param name="path">The file path to load from..</param>
    /// <returns>The loaded <see cref="PreCompiledShader"/>.</returns>
    /// <exception cref="Exception">Thrown if the file is not a valid Pre-Compiled Shader file.</exception>
    public static PreCompiledShader FromFile(string path)
    {
        using FileStream stream = File.OpenRead(PathUtils.GetFullPath(path));
        return FromStream(stream);
    }

    /// <summary>
    /// Load a <see cref="PreCompiledShader"/> from an embedded resource.
    /// </summary>
    /// <param name="resourceName">The resource name of the shader file.</param>
    /// <param name="assembly">The assembly to load from. If none is provided, the currently executing assembly will be
    /// used.</param>
    /// <returns>The loaded <see cref="PreCompiledShader"/>.</returns>
    /// <exception cref="Exception">Thrown if the file is not a valid Pre-Compiled Shader file.</exception>
    public static PreCompiledShader FromEmbeddedResource(string resourceName, Assembly? assembly = null)
    {
        Assembly loadAssembly = assembly ?? Assembly.GetExecutingAssembly();
        using Stream? stream = loadAssembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new Exception($"Failed to find embedded resource with name '{resourceName}' in assembly {assembly}");

        return FromStream(stream);
    }
}