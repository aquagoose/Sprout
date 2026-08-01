using piko.SDL3;

namespace Sprout.Framework;

public static class MessageBox
{
    public static void Show(Type type, string title, string message, Window? window = null)
    {
        SDL.MessageBoxFlags flags = type switch
        {
            Type.Info => SDL.MessageBoxFlags.Information,
            Type.Warning => SDL.MessageBoxFlags.Warning,
            Type.Error => SDL.MessageBoxFlags.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        SDL.ShowSimpleMessageBox(flags, title, message, window?.Handle ?? new SDL.Window());
    }

    public enum Type
    {
        Info,
        Warning,
        Error
    }
}