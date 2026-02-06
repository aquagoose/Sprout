namespace Sprout.Graphics;

public class PreCompiledShader
{
    private const uint Version = 1;
    private const uint Magic = 0x48534350;

    private Dictionary<(Backend backend, ShaderStage stage), (string entryPoint, byte[] source)> _sourceList;

    public PreCompiledShader()
    {
        _sourceList = [];
    }

    public byte[] GetSource(Backend backend, ShaderStage stage, out string entryPoint)
    {
        (entryPoint, byte[] source) = _sourceList[(backend, stage)];
        return source;
    }

    public void AddSource(Backend backend, ShaderStage stage, string entryPoint, byte[] source)
    {
        _sourceList.Add((backend, stage), (entryPoint, source));
    }

    public void Save(string path)
    {
        using FileStream stream = File.OpenWrite(path);
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

    public static PreCompiledShader FromFile(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return FromStream(stream);
    }
}