#nullable enable
using SkiaSharp;
using UnityEngine;

namespace Score
{
    /// <summary>A score's sheet segment, which needs to be loaded on the main Unity thread.</summary>
    public sealed class SheetSegment
    {
        private readonly SKImage _bitmap;
        private Sprite? _sprite;

        /// <summary>
        /// The sprite associated with this segment.
        /// <para>This value is loaded in a call by need fashion, and since the first evaluation needs to be performed
        /// on the main Unity thread, DO NOT read this attribute from other threads.</para><para>Since getting this value may incur
        /// in blocking loading operations, ensure that the value is loaded/accessed in such a way not to impact the
        /// user experience.</para>
        /// </summary>
        public Sprite Sprite
        {
            get
            {
                if (_sprite != null) return _sprite; // If already loaded return the sprite.

                // If not loaded proceed to create a new texture from the raw image data.
                var texture = new Texture2D(_bitmap.Info.Width, _bitmap.Info.Height, TextureFormat.RGBA32, false,
                    true);
                using (var pixmap = _bitmap.PeekPixels())
                    texture.LoadRawTextureData(pixmap.GetPixels(), pixmap.BytesSize);
                _bitmap.Dispose(); // Dispose, since it is no longer needed.
                texture.Apply();
                
                // Create the sprite with a pivot aligned horizontally to the left and vertically in the middle.  
                _sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0, texture.height / 2.0f));
                return _sprite;
            }
        }

        /// <summary>Evaluates the Sprite property value. Refer to the Sprite property for more information on the
        /// implications of such action.</summary>
        public void Load() => _ = Sprite;

        /// <summary>Instantiates a new sheet segment with raw image data.</summary>
        public SheetSegment(SKImage bitmap) => _bitmap = bitmap;
    }
}