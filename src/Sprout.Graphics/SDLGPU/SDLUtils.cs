using SDL3;

namespace Sprout.Graphics.SDLGPU;

internal static class SDLUtils
{
    public static void Check(this bool b, string operation)
    {
        if (!b)
            throw new Exception($"SDL operation '{operation}' failed: {SDL.GetError()}");
    }
    
    public static IntPtr Check(this IntPtr ptr, string operation)
    {
        Check(ptr != IntPtr.Zero, operation);
        return ptr;
    }
}