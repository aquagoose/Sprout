using Sprout.Graphics;
using Sprout.Graphics.Utils;

string path = args[0];
string outPath = args[1];

string source = File.ReadAllText(path);

foreach (Backend backend in Enum.GetValues<Backend>())
{
    if (backend == Backend.Unknown)
        continue;
    
    Console.WriteLine($"Compiling shader for {backend}");
    byte[] shader = ShaderUtils.TranspileHLSL(backend, ShaderStage.Vertex);
}