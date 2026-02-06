using System.Diagnostics.CodeAnalysis;
using Sprout.Graphics;
using Sprout.Graphics.ShaderUtils;

namespace CompileShaders;

public static class CompileShaders
{
    public static void Main(string[] args)
    {
        string? inPath = null;
        string? vtxEntry = null;
        string? pxlEntry = null;

        int argIndex = 0;
        while (ReadArg(args, ref argIndex, out string? arg))
        {
            if (arg.StartsWith('-'))
            {
                switch (arg)
                {
                    case "--vertexEntry" or "-vE":
                        ReadArg(args, ref argIndex, out vtxEntry);
                        break;
                    case "--pixelEntry" or "-pE":
                        ReadArg(args, ref argIndex, out pxlEntry);
                        break;
                    default:
                        Console.WriteLine($"Unrecognized argument '{arg}'.");
                        PrintHelp();
                        return;
                }
            }
            else
            {
                if (inPath != null)
                {
                    Console.WriteLine("Cannot provide more than one infile.");
                    PrintHelp();
                    return;
                }

                ReadArg(args, ref argIndex, out inPath);
            }
        }

        if (inPath == null)
        {
            Console.WriteLine("No infile provided.");
            PrintHelp();
            return;
        }

        string source = File.ReadAllText(inPath);

        List<(Backend backend, ShaderStage stage, byte[] data)> compiledShaders = [];
        foreach (Backend backend in Enum.GetValues<Backend>())
        {
            if (backend == Backend.Unknown)
                continue;
    
            Console.WriteLine($"Compiling shader for {backend}");
            if (vtxEntry != null)
            {
                byte[] shader = Compiler.TranspileHLSL(backend, ShaderStage.Vertex, source, vtxEntry);
                compiledShaders.Add((backend, ShaderStage.Vertex, shader));
            }
        }
    }
    
    private static bool ReadArg(string[] args, ref int argIndex, [NotNullWhen(true)] out string? arg)
    {
        arg = null;
    
        if (argIndex >= args.Length)
            return false;

        arg = args[argIndex++];
        return true;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
                           Usage: CompileShaders [Options] <InFile>

                           Options:
                               --vertexEntry <EntryPoint>, -vE <EntryPoint>
                                   Set the vertex shader entry point name.
                               --pixelEntry <EntryPoint>, -pE <EntryPoint>
                                   Set the pixel shader entry point name.
                           """);
    }
}