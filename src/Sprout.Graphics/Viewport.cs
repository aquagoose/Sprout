namespace Sprout.Graphics;

public struct Viewport
{
    public int X;

    public int Y;

    public uint Width;

    public uint Height;

    public Viewport(int x, int y, uint width, uint height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}