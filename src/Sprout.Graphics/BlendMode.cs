namespace Sprout.Graphics;

public struct BlendMode(
    bool enabled,
    BlendFactor src,
    BlendFactor dest,
    BlendOperation blendOp,
    BlendFactor srcAlpha,
    BlendFactor destAlpha,
    BlendOperation blendOpAlpha) : IEquatable<BlendMode>
{
    public bool Enabled = enabled;
    public BlendFactor Src = src;
    public BlendFactor Dest = dest;
    public BlendOperation BlendOp = blendOp;
    public BlendFactor SrcAlpha = srcAlpha;
    public BlendFactor DestAlpha = destAlpha;
    public BlendOperation BlendOpAlpha = blendOpAlpha;

    public BlendMode(bool enabled, BlendFactor src, BlendFactor dest) : this(enabled, src, dest, BlendOperation.Add,
        src, dest, BlendOperation.Add) { }

    public static BlendMode Disabled => new BlendMode(false, BlendFactor.One, BlendFactor.One);

    public static BlendMode NonPremultiplied => new BlendMode(true, BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha);

    public bool Equals(BlendMode other)
    {
        return Enabled == other.Enabled && Src == other.Src && Dest == other.Dest && BlendOp == other.BlendOp &&
               SrcAlpha == other.SrcAlpha && DestAlpha == other.DestAlpha && BlendOpAlpha == other.BlendOpAlpha;
    }

    public override bool Equals(object? obj)
    {
        return obj is BlendMode other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Enabled, (int) Src, (int) Dest, (int) BlendOp, (int) SrcAlpha, (int) DestAlpha,
            (int) BlendOpAlpha);
    }

    public static bool operator ==(BlendMode left, BlendMode right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BlendMode left, BlendMode right)
    {
        return !left.Equals(right);
    }
}