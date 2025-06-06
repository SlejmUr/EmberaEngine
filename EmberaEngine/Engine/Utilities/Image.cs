using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Utilities
{
    public class Image
    {
        public int Width, Height;
        public byte[] Pixels;
        public float[] PixelHP;
        public bool IsHDR;

        public Image()
        {

        }

        public static (int width, int height, int channels) GetImageDimensions(Stream stream)
        {
            IImageFormat format;
            var info = SixLabors.ImageSharp.Image.Identify(stream); // Reads metadata only

            if (info != null)
                return (info.Width, info.Height, info.PixelType.BitsPerPixel / 8);
            else
                throw new Exception("Unable to identify image format.");
        }

        public void LoadHDRI(string path, bool directPath = false)
        {
            using (var stream = File.OpenRead(path))
            {
                var imageResult = ImageResultFloat.FromStream(stream, ColorComponents.RedGreenBlue);

                this.Width = imageResult.Width;
                this.Height = imageResult.Height;
                this.PixelHP = imageResult.Data;
                this.IsHDR = true;

                this.Pixels = null;
            }
        }

        public void Load(byte[] data)
        {
            SixLabors.ImageSharp.Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(data);
            this.Width = image.Width;
            this.Height = image.Height;
            List<byte> pixels = new List<byte>(4 * image.Width * image.Height);
            Pixels = new byte[4 * image.Width * image.Height];
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);

                    for (int x = 0; x < image.Width; x++)
                    {
                        pixels.Add(row[x].R);
                        pixels.Add(row[x].G);
                        pixels.Add(row[x].B);
                        pixels.Add(row[x].A);
                    }
                }
            });

            Pixels = pixels.ToArray();
        }

        public void LoadPNG(string path, bool directPath = false)
        {
            SixLabors.ImageSharp.Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);
            this.Width = image.Width;
            this.Height = image.Height;
            List<byte> pixels = new List<byte>(4 * image.Width * image.Height);
            Pixels = new byte[4 * image.Width * image.Height];
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);

                    for (int x = 0; x < image.Width; x++)
                    {
                        pixels.Add(row[x].R);
                        pixels.Add(row[x].G);
                        pixels.Add(row[x].B);
                        pixels.Add(row[x].A);
                    }
                }
            });

            Pixels = pixels.ToArray();
        }


    }
}
