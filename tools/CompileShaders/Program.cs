using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Sprout.Graphics;
using Sprout.Graphics.ShaderUtils;

namespace CompileShaders;

public static class CompileShaders
{
    public static int Main(string[] args)
    {
        string? inFile = null;
        string? outFile = null;
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
                    case "--output" or "-o":
                        ReadArg(args, ref argIndex, out outFile);
                        break;
                    default:
                        Console.WriteLine($"Unrecognized argument '{arg}'.");
                        PrintHelp();
                        return 1;
                }
            }
            else
            {
                if (inFile != null)
                {
                    Console.WriteLine("Cannot provide more than one infile.");
                    PrintHelp();
                    return 1;
                }

                inFile = arg;
                outFile ??= Path.GetFileNameWithoutExtension(inFile) + ".pcsh";
            }
        }

        if (inFile == null)
        {
            Console.WriteLine("No infile provided.");
            PrintHelp();
            return 1;
        }
        
        Debug.Assert(outFile != null);

        if (vtxEntry == null && pxlEntry == null)
        {
            Console.WriteLine("Must contain at least one entry point.");
            PrintHelp();
            return 1;
        }

        string source = File.ReadAllText(inFile);

        PreCompiledShader pcsh = new();
        foreach (Backend backend in Enum.GetValues<Backend>())
        {
            if (backend == Backend.Unknown)
                continue;
    
            Console.WriteLine($"Compiling shader for {backend}");
            if (vtxEntry != null)
            {
                byte[] shader = Compiler.TranspileHLSL(backend, ShaderStage.Vertex, source, vtxEntry);
                pcsh.AddSource(backend, ShaderStage.Vertex, vtxEntry, shader);
            }

            if (pxlEntry != null)
            {
                byte[] shader = Compiler.TranspileHLSL(backend, ShaderStage.Pixel, source, pxlEntry);
                pcsh.AddSource(backend, ShaderStage.Pixel, pxlEntry, shader);
            }
        }
        
        pcsh.Save(outFile);

        return 0;
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