namespace Sprout.Graphics;

/// <summary>
/// Represents various graphics APIs that can be used to create a <see cref="GraphicsDevice"/>.
/// </summary>
public enum Backend
{
    /// <summary>
    /// Unknown backend. This may be an API that isn't publicly available. It can also mean "any" backend.
    /// </summary>
    Unknown = 0,
    
    /// <summary>
    /// Vulkan 1.3. Runs on most platforms.
    /// </summary>
    Vulkan = 1,
    
    /*/// <summary>
    /// DirectX 12. Runs on Windows.
    /// </summary>
    D3D12 = 2,
    
    /// <summary>
    /// Metal. Runs on Apple devices.
    /// </summary>
    Metal = 3,
    
    /// <summary>
    /// DirectX 11. Runs on Windows. Has more stable performance than <see cref="D3D12"/>.
    /// </summary>
    D3D11 = 4,*/
    
    /// <summary>
    /// OpenGL 3.3. Runs on all desktop platforms.
    /// </summary>
    OpenGL = 5,
    
    /*/// <summary>
    /// OpenGL ES 2.0. Runs on mobile devices and older desktops. 
    /// </summary>
    OpenGLES = 6*/
}