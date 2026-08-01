using System.Drawing;

namespace Sprout.Framework;

public struct WindowInfo(string title, Size size, bool resizable = false, bool fullscreen = false)
{
    public string Title = title;

    public Size Size = size;

    public bool Resizable = resizable;

    public bool Fullscreen = fullscreen;
}