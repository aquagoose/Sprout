namespace Sprout.Graphics;

public struct Sampler
{
    public Filter MinFilter;

    public Filter MagFilter;

    public Filter MipFilter;

    public Sampler(Filter minFilter, Filter magFilter, Filter mipFilter)
    {
        MinFilter = minFilter;
        MagFilter = magFilter;
        MipFilter = mipFilter;
    }

    public static Sampler Linear => new Sampler(Filter.Linear, Filter.Linear, Filter.Linear);

    public static Sampler Point => new Sampler(Filter.Point, Filter.Point, Filter.Point);
}