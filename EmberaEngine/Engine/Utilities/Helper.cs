using EmberaEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Utilities
{
    public class Helper
    {
        public static System.Numerics.Vector2 ToNumerics2(OpenTK.Mathematics.Vector2 value)
        {
            return Unsafe.As<OpenTK.Mathematics.Vector2, System.Numerics.Vector2>(ref value);
        }

        public static OpenTK.Mathematics.Vector4 ToVector4(OpenTK.Mathematics.Color4 value)
        {
            return new OpenTK.Mathematics.Vector4(value.R, value.G, value.B, value.A);
        }

        public static Texture loadImageAsTex(string file, TextureMagFilter tmf = TextureMagFilter.Linear)
        {
            Image image = new EmberaEngine.Engine.Utilities.Image();
            image.LoadPNG(file);

            Texture texture = new Texture(EmberaEngine.Engine.Core.TextureTarget2d.Texture2D);
            texture.SetFilter((TextureMinFilter)tmf, tmf);
            texture.SetWrapMode(EmberaEngine.Engine.Core.TextureWrapMode.ClampToEdge, EmberaEngine.Engine.Core.TextureWrapMode.ClampToEdge);
            texture.TexImage2D<byte>(image.Width, image.Height, EmberaEngine.Engine.Core.PixelInternalFormat.Rgba16f, EmberaEngine.Engine.Core.PixelFormat.Rgba, EmberaEngine.Engine.Core.PixelType.UnsignedByte, image.Pixels);
            texture.GenerateMipmap();

            return texture;

        }

    }
}
