namespace Sprout.Graphics;

public struct Sampler : IEquatable<Sampler>
{
    public Filter MinFilter;

    public Filter MagFilter;

    public Filter MipFilter;

    public TextureAddress AddressU;

    public TextureAddress AddressV;

    public Sampler(Filter minFilter, Filter magFilter, Filter mipFilter, TextureAddress addressU, TextureAddress addressV)
    {
        MinFilter = minFilter;
        MagFilter = magFilter;
        MipFilter = mipFilter;
        AddressU = addressU;
        AddressV = addressV;
    }

    public Sampler(Filter filter, TextureAddress address) : this(filter, filter, filter, address, address) { }

    public static Sampler LinearWrap => new Sampler(Filter.Linear, TextureAddress.Repeat);

    public static Sampler LinearClamp => new Sampler(Filter.Linear, TextureAddress.ClampToEdge);

    public static Sampler PointWrap => new Sampler(Filter.Point, TextureAddress.Repeat);

    public static Sampler PointClamp => new Sampler(Filter.Point, TextureAddress.ClampToEdge);

    public bool Equals(Sampler other)
    {
        return MinFilter == other.MinFilter && MagFilter == other.MagFilter && MipFilter == other.MipFilter &&
               AddressU == other.AddressU && AddressV == other.AddressV;
    }

    public override bool Equals(object? obj)
    {
        return obj is Sampler other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int) MinFilter, (int) MagFilter, (int) MipFilter, (int) AddressU, (int) AddressV);
    }

    public static bool operator ==(Sampler left, Sampler right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Sampler left, Sampler right)
    {
        return !left.Equals(right);
    }
}