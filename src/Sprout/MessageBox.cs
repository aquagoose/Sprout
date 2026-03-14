using SDL3;

namespace Sprout;

public static class MessageBox
{
    public static void Show(Type type, string title, string message)
    {
        SDL.MessageBoxFlags flags = type switch
        {
            Type.Info => SDL.MessageBoxFlags.Information,
            Type.Warning => SDL.MessageBoxFlags.Warning,
            Type.Error => SDL.MessageBoxFlags.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        SDL.ShowSimpleMessageBox(flags, title, message, IntPtr.Zero);
    }

    public enum Type
    {
        Info,
        Warning,
        Error
    }
}