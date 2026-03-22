using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using FreeTypeSharp;
using Sprout.Content;
using static FreeTypeSharp.FT_Error;
using static FreeTypeSharp.FT_LOAD;
using static FreeTypeSharp.FT;

namespace Sprout.Graphics;

public sealed unsafe class Font : IDisposable
{
    private const uint Spacing = 1;
    
    private readonly GraphicsDevice _device;
    private readonly bool _antialias;
    private readonly uint _textureWidth;
    private readonly uint _textureHeight;
    
    private readonly FaceRec _face;
    private readonly List<Texture> _atlases;
    private readonly Dictionary<(uint size, uint c), Character> _characters;

    private uint _atlasX;
    private uint _atlasY;
    private uint _maxCharSize;
    
    public Font(GraphicsDevice device, string path, bool antialias = true, uint textureWidth = 1024,
        uint textureHeight = 1024)
    {
        _device = device;
        _antialias = antialias;
        _textureWidth = textureWidth;
        _textureHeight = textureHeight;

        _atlases = [];
        _characters = [];
        
        _face = CreateFace(path);
    }
    
    public void Dispose()
    {
        foreach (Texture texture in _atlases)
            texture.Dispose();

        FaceRec? face = _face;
        do
        {
            CheckError(FT_Done_Face(face.Face), "Done face");
            face = face.Next;
        } while (face != null);
    }

    public void AddFont(string path)
    {
        FaceRec faceRec = CreateFace(path);
        
        // TODO: Doubly linked list
        FaceRec face = _face;
        while (face.Next != null)
            face = face.Next;

        face.Next = faceRec;
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

        int i = 0;
        while (TryGetNextCharacter(text, ref i, out uint c))
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

    public Size MeasureText(uint size, string text)
    {
        Vector2 currentPos = Vector2.Zero;
        Size textSize = new Size();

        int i = 0;
        while (TryGetNextCharacter(text, ref i, out uint c))
        {
            switch (c)
            {
                case '\n':
                    currentPos.Y += size;
                    currentPos.X = 0;
                    continue;
            }
            
            Character character = GetCharacter(size, c);
            currentPos.X += character.Advance;
            textSize.Width = int.Max(textSize.Width, (int) currentPos.X);
            textSize.Height = int.Max(textSize.Height, (int) (currentPos.Y + character.Ascender));
        }

        return textSize;
    }

    private Character GetCharacter(uint size, uint c)
    {
        if (_characters.TryGetValue((size, c), out Character character))
            return character;

        _maxCharSize = uint.Max(_maxCharSize, size);

        // Start at the first face, which is assumed to be the "primary" face.
        // If it contains the codepoint, great, use that. If not, continue to the next face and try again.
        FaceRec faceRec = _face;
        while (FT_Get_Char_Index(faceRec.Face, c) == 0 && faceRec.Next != null)
            faceRec = faceRec.Next;
        
        FT_FaceRec_* face = faceRec.Face;

        CheckError(FT_Set_Pixel_Sizes(face, 0, size), "Set pixel size");
        FT_LOAD flags = FT_LOAD_RENDER;
        if (!_antialias)
            flags |= FT_LOAD_MONOCHROME;

        FT_Load_Char(face, c, flags);

        FT_GlyphSlotRec_* slot = face->glyph;
        FT_Bitmap_ bitmap = slot->bitmap;

        if (_atlasX + bitmap.width >= _textureWidth)
        {
            _atlasX = 0;
            _atlasY += _maxCharSize;
        }

        // Create a new atlas if the bitmap goes outside the texture.
        if (_atlasY + bitmap.rows >= _textureHeight || _atlases.Count == 0)
        {
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
                byte b;
                if (_antialias)
                    b = bitmap.buffer[y * bitmap.width + x];
                else
                {
                    byte* row = &bitmap.buffer[bitmap.pitch * y];
                    // Copied directly from Pie cause as the comment there said... wtf???
                    b = (byte) ((row[x >> 3] & (128 >> (x & 7))) != 0 ? 255 : 0);
                }

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
            new Vector2(slot->bitmap_left, slot->bitmap_top), (int) (face->size->metrics.ascender >> 6),
            (int) (slot->advance.x >> 6));
        _characters.Add((size, c), character);
        
        _atlasX += bitmap.width;

        return character;
    }
    
    private FaceRec CreateFace(string path)
    {
        string fullPath = PathUtils.GetFullPath(path);
        Span<byte> fullPathBytes = stackalloc byte[fullPath.Length + 1];
        Encoding.UTF8.GetBytes(fullPath, fullPathBytes);
        
        FT_FaceRec_* faceRec;
        fixed (byte* pPath = fullPathBytes)
            CheckError(FT_New_Face(_library, pPath, 0, &faceRec), "New face");
        return new FaceRec(faceRec);
    }

    private bool TryGetNextCharacter(string text, ref int i, out uint codePoint)
    {
        if (i >= text.Length)
        {
            codePoint = 0;
            return false;
        }

        char c = text[i];
        // https://learn.microsoft.com/en-us/dotnet/standard/base-types/character-encoding-introduction#surrogate-pairs
        // Handle surrogate pairs
        // If the character is within a certain character range, and the next character is as well, combine them into
        // a single character. Used for emojis etc.
        if (c >= '\uD800' && c < '\uDBFF' && i < text.Length && text[i + 1] is >= '\uDC00' and <= '\uDFFF')
        {
            codePoint = (uint) (0x10000 + (((uint) c - 0xD800) * 0x0400) + (text[i + 1] - 0xDC00));
            i++;
        }
        else
            codePoint = c;

        i++;
        return true;
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

    // TODO: Struct
    // Face linked list
    private class FaceRec
    {
        public FT_FaceRec_* Face;
        public FaceRec? Next;

        public FaceRec(FT_FaceRec_* face)
        {
            Face = face;
        }
    }
}