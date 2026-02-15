namespace Sprout.Graphics;

public class SpriteRenderer : IDisposable
{
    /// <summary>
    /// The maximum number of sprites in a single batch. If any more sprites are added, the batch will automatically
    /// be flushed and a new batch will begin.
    /// </summary>
    public const uint MaxSpritesPerBatch = 1 << 16;

    private const uint NumVertices = 4;
    private const uint NumIndices = 6;
    
    private readonly Renderable _renderable;

    public SpriteRenderer(GraphicsDevice device)
    {
        RenderableInfo info = new()
            { };
    }
    
    public void Dispose()
    {
        
    }
}