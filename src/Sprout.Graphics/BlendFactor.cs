namespace Sprout.Graphics;

/// <summary>
/// A blend factor describes the equation the GPU will use to calculate blending for the color/alpha channels. 
/// </summary>
public enum BlendFactor
{
    /// <summary>
    /// This channel will be 0.
    /// </summary>
    Zero,
    
    /// <summary>
    /// This channel will be 1.
    /// </summary>
    One,
    
    /// <summary>
    /// This channel will be the same as the source color.
    /// </summary>
    SrcColor,
    
    /// <summary>
    /// This channel will be the inverse of the source color.
    /// </summary>
    OneMinusSrcColor,
    
    /// <summary>
    /// This channel will be the same as the destination color.
    /// </summary>
    DestColor,
    
    /// <summary>
    /// This channel will be the same as the inverse of the destination color.
    /// </summary>
    OneMinusDestColor,
    
    /// <summary>
    /// This channel will be the same as the source alpha value.
    /// </summary>
    SrcAlpha,
    
    /// <summary>
    /// This channel will be the same as the inverse of the source alpha value.
    /// </summary>
    OneMinusSrcAlpha,
    
    
    /// <summary>
    /// This channel will be the same as the destination alpha value.
    /// </summary>
    DestAlpha,
    
    /// <summary>
    /// This channel will be the same as the inverse of the destination alpha value.
    /// </summary>
    OneMinusDestAlpha
}