namespace Sprout.Graphics;

/// <summary>
/// A blend mode tells the GPU how to handle transparency.
/// </summary>
/// <param name="enabled">Whether blending should be enabled. Unless you need transparency, blending should be disabled
/// where possible as the GPU can process more pixels.</param>
/// <param name="src">The color source <see cref="BlendFactor"/>.</param>
/// <param name="dest">The color destination <see cref="BlendFactor"/>.</param>
/// <param name="blendOp">The color <see cref="Sprout.Graphics.BlendOperation"/>.</param>
/// <param name="srcAlpha">The alpha source <see cref="BlendFactor"/>.</param>
/// <param name="destAlpha">The alpha destination <see cref="BlendFactor"/>.</param>
/// <param name="blendOpAlpha">The alpha <see cref="Sprout.Graphics.BlendOperation"/>.</param>
public struct BlendMode(
    bool enabled,
    BlendFactor src,
    BlendFactor dest,
    BlendOperation blendOp,
    BlendFactor srcAlpha,
    BlendFactor destAlpha,
    BlendOperation blendOpAlpha) : IEquatable<BlendMode>
{
    /// <param name="enabled">Whether blending should be enabled. Unless you need transparency, blending should be
    /// disabled where possible as the GPU can process more pixels.</param>
    public bool Enabled = enabled;
    
    /// <param name="src">The color source <see cref="BlendFactor"/>.</param>
    public BlendFactor Src = src;
    
    /// <param name="dest">The color destination <see cref="BlendFactor"/>.</param>
    public BlendFactor Dest = dest;
    
    /// <param name="blendOp">The color <see cref="Sprout.Graphics.BlendOperation"/>.</param>
    public BlendOperation BlendOp = blendOp;
    
    /// <param name="srcAlpha">The alpha source <see cref="BlendFactor"/>.</param>
    public BlendFactor SrcAlpha = srcAlpha;
    
    /// <param name="destAlpha">The alpha destination <see cref="BlendFactor"/>.</param>
    public BlendFactor DestAlpha = destAlpha;
    
    /// <param name="blendOpAlpha">The alpha <see cref="Sprout.Graphics.BlendOperation"/>.</param>
    public BlendOperation BlendOpAlpha = blendOpAlpha;

    /// <summary>
    /// Create a simplified <see cref="BlendMode"/> with the same <see cref="BlendFactor"/> for both the color and
    /// alpha channels.
    /// </summary>
    /// <param name="enabled">Whether blending should be enabled.</param>
    /// <param name="src">The source <see cref="BlendFactor"/>.</param>
    /// <param name="dest">The destination <see cref="BlendFactor"/>.</param>
    public BlendMode(bool enabled, BlendFactor src, BlendFactor dest) : this(enabled, src, dest, BlendOperation.Add,
        src, dest, BlendOperation.Add) { }

    /// <summary>
    /// Disable blending & transparency.
    /// </summary>
    public static BlendMode Disabled => new BlendMode(false, BlendFactor.One, BlendFactor.One);

    /// <summary>
    /// Non-premultiplied alpha. This is the most "standard" method of transparency.
    /// </summary>
    public static BlendMode NonPremultiplied => new BlendMode(true, BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha);

    /// <inheritdoc />
    public bool Equals(BlendMode other)
    {
        return Enabled == other.Enabled && Src == other.Src && Dest == other.Dest && BlendOp == other.BlendOp &&
               SrcAlpha == other.SrcAlpha && DestAlpha == other.DestAlpha && BlendOpAlpha == other.BlendOpAlpha;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is BlendMode other && Equals(other);
    }

    /// <inheritdoc />
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