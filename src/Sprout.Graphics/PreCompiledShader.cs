namespace Sprout.Graphics;

public static class PreCompiledShader
{
    private const uint Version = 1;
    private const uint Magic = 0x48534350;

    public static byte[] Load(Backend backend, Stream fileData)
    {
        using BinaryReader reader = new BinaryReader(fileData);
        if (reader.ReadUInt32() != Magic)
            throw new Exception("File is not a valid PCSH file.");

        reader.ReadUInt32(); // Version, not used currently.

        byte numBackends = reader.ReadByte();

        for (int i = 0; i < numBackends; i++)
        {
            Backend thisBackend = (Backend) reader.ReadByte();
            uint offset = reader.ReadUInt32();
            
            if (thisBackend != backend)
                continue;

            reader.BaseStream.Position = offset;

            int length = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(length);
            return bytes;
        }

        throw new PlatformNotSupportedException(
            $"The backend {backend} isn't present in the PCSH file. It might need to be recompiled.");
    }

    public static byte[] Load(Backend backend, string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Load(backend, stream);
    }

    public static void Save((Backend backend, byte[] data)[] datas, string path)
    {
        using FileStream stream = File.OpenWrite(path);
        using BinaryWriter writer = new BinaryWriter(stream);
        
        writer.Write(Magic);
        writer.Write(Version);
        
        // Write the backend table.
        // The point of the backend table is so that the loader can immediately jump to the shader source it needs,
        // instead of needing to iterate through the entire file.
        writer.Write(datas.Length);

        uint totalOffset = (uint) (writer.BaseStream.Position + (datas.Length * 2 * sizeof(uint)));
        foreach ((Backend backend, byte[] data) in datas)
        {
            writer.Write((byte) backend);
            writer.Write(totalOffset);
            totalOffset += (uint) (data.Length + sizeof(uint));
        }

        foreach ((_, byte[] data) in datas)
        {
            writer.Write(data.Length);
            writer.Write(data);
        }
    }
}