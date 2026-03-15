using System.Drawing;
using System.Numerics;
using FreeTypeSharp;
using static FreeTypeSharp.FT_Error;
using static FreeTypeSharp.FT_LOAD;
using static FreeTypeSharp.FT;

namespace Sprout.Graphics;

public sealed unsafe class Font : IDisposable
{
    private const uint Spacing = 1;
    
    private readonly GraphicsDevice _device;
    private readonly uint _textureWidth;
    private readonly uint _textureHeight;
    
    private readonly FT_FaceRec_* _face;
    private readonly List<Texture> _atlases;
    private readonly Dictionary<(uint size, char c), Character> _characters;

    private uint _atlasX;
    private uint _atlasY;
    private uint _maxCharSize;
    
    public Font(GraphicsDevice device, ReadOnlySpan<byte> path, uint textureWidth = 1024, uint textureHeight = 1024)
    {
        _device = device;
        _textureWidth = textureWidth;
        _textureHeight = textureHeight;
        
        fixed (byte* pPath = path)
        fixed (FT_FaceRec_** face = &_face)
            CheckError(FT_New_Face(_library, pPath, 0, face), "New face");

        _atlases = [];
        _characters = [];
    }
    
    public void Dispose()
    {
        foreach (Texture texture in _atlases)
            texture.Dispose();
        
        CheckError(FT_Done_Face(_face), "Done face");
    }

    /// <summary>
    /// Draw a string of text.
    /// </summary>
    /// <param name="renderer">The <see cref="SpriteRenderer"/>. to draw using.</param>
    /// <param name="position">The position to draw the text at.</param>
    /// <param name="size">The font size, in pixels, that the text should be.</param>
    /// <param name="text">The text to draw.</param>
    /// <param name="color">The text color.</param>
    public void Draw(SpriteRenderer renderer, Vector2 position, uint size, string text, Color color)
    {
        Vector2 currentPos = position;
        
        foreach (char c in text)
        {
            switch (c)
            {
                case '\n':
                    currentPos.X = position.X;
                    currentPos.Y += size;
                    continue;
                case '\r':
                    continue;
            }
            
            Character character = GetCharacter(size, c);
            Texture texture = _atlases[character.TextureIndex];

            Vector2 pos = currentPos + new Vector2(character.Bearing.X, -character.Bearing.Y + character.Ascender);
            
            renderer.Draw(texture, pos, source: character.SourceRectangle, tint: color);
            currentPos.X += character.Advance;
        }
    }

    private Character GetCharacter(uint size, char c)
    {
        if (_characters.TryGetValue((size, c), out Character character))
            return character;

        _maxCharSize = uint.Max(_maxCharSize, size);

        CheckError(FT_Set_Pixel_Sizes(_face, 0, size), "Set pixel size");
        FT_Load_Char(_face, c, FT_LOAD_RENDER);

        FT_GlyphSlotRec_* slot = _face->glyph;
        FT_Bitmap_ bitmap = slot->bitmap;
        
        Console.WriteLine(c);

        if (_atlasX + bitmap.width >= _textureWidth)
        {
            Console.WriteLine("New line!");
            _atlasX = 0;
            _atlasY += _maxCharSize;
        }

        if (_atlasY + bitmap.rows >= _textureHeight || _atlases.Count == 0)
        {
            Console.WriteLine("Create new atlas!");
            _atlasX = 0;
            _atlasY = 0;
            Texture atlas =
                _device.CreateTexture(_textureWidth, _textureHeight, PixelFormat.RGBA8, TextureUsage.Shader);
            _atlases.Add(atlas);
        }
        
        int currentAtlas = _atlases.Count - 1;
        
        // Convert the single-channel text into a full bitmap + alpha channel.
        byte[] bitmapData = new byte[bitmap.width * bitmap.rows * 4];
        for (int y = 0; y < bitmap.rows; y++)
        {
            for (int x = 0; x < bitmap.width; x++)
            {
                byte b = bitmap.buffer[y * bitmap.width + x];
                bitmapData[(y * bitmap.width + x) * 4 + 0] = 255; // R
                bitmapData[(y * bitmap.width + x) * 4 + 1] = 255; // G
                bitmapData[(y * bitmap.width + x) * 4 + 2] = 255; // B
                bitmapData[(y * bitmap.width + x) * 4 + 3] = b;   // A
            }
        }

        Texture atlasTexture = _atlases[currentAtlas];
        Console.WriteLine($"Update X: {_atlasX}, Y: {_atlasY}");
        atlasTexture.Update(_atlasX, _atlasY, bitmap.width, bitmap.rows, 0, bitmapData);

        character = new Character(currentAtlas,
            new Rectangle((int) _atlasX, (int) _atlasY, (int) bitmap.width, (int) bitmap.rows),
            new Vector2(slot->bitmap_left, slot->bitmap_top), (int) (_face->size->metrics.ascender >> 6),
            (int) (slot->advance.x >> 6));
        _characters.Add((size, c), character);
        
        _atlasX += bitmap.width;

        return character;
    }

    private static FT_LibraryRec_* _library;

    static Font()
    {
        fixed (FT_LibraryRec_** library = &_library)
            CheckError(FT_Init_FreeType(library), "Initialize freetype");
    }
    
    private static void CheckError(FT_Error error, string operation)
    {
        if (error == FT_Err_Ok)
            return;

        throw new Exception($"FreeType operation '{operation}' failed: {error}");
    }

    private readonly struct Character
    {
        public readonly int TextureIndex;
        public readonly Rectangle SourceRectangle;
        public readonly Vector2 Bearing;
        public readonly int Ascender;
        public readonly int Advance;

        public Character(int textureIndex, Rectangle sourceRectangle, Vector2 bearing, int ascender, int advance)
        {
            TextureIndex = textureIndex;
            SourceRectangle = sourceRectangle;
            Bearing = bearing;
            Ascender = ascender;
            Advance = advance;
        }
    }
}