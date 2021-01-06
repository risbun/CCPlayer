using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using Fleck;
using System.Windows.Forms;

namespace CCPlayer
{
    class ScreenshotConvert
    {
        private Dictionary<Color, char> colors = new Dictionary<Color, char>()
        {
            { Color.FromArgb(240, 240, 240), '0' },
            { Color.FromArgb(242, 178, 51), '1' },
            { Color.FromArgb(229, 127, 216), '2' },
            { Color.FromArgb(153, 178, 242), '3' },
            { Color.FromArgb(222, 222, 108), '4' },
            { Color.FromArgb(127, 204, 25), '5' },
            { Color.FromArgb(242, 178, 204), '6' },
            { Color.FromArgb(76, 76, 76), '7' },
            { Color.FromArgb(153, 153, 153), '8' },
            { Color.FromArgb(76, 153, 178), '9' },
            { Color.FromArgb(178, 102, 229), 'a' },
            { Color.FromArgb(51, 102, 204), 'b' },
            { Color.FromArgb(127, 102, 76), 'c' },
            { Color.FromArgb(87, 166, 78), 'd' },
            { Color.FromArgb(204, 76, 76), 'e' },
            { Color.FromArgb(25, 25, 25), 'f' },
        };
        private Color GetClosestColor(Color[] colorArray, Color baseColor)
        {
            var colors = colorArray.Select(x => new { Value = x, Diff = GetDiff(x, baseColor) }).ToList();
            var min = colors.Min(x => x.Diff);
            return colors.Find(x => x.Diff == min).Value;
        }
        private int GetDiff(Color color, Color baseColor)
        {
            int a = color.A - baseColor.A,
                r = color.R - baseColor.R,
                g = color.G - baseColor.G,
                b = color.B - baseColor.B;
            return a * a + r * r + g * g + b * b;
        }
        private Bitmap makeScreenshot()
        {
            Bitmap screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
            Graphics gfxScreenshot = Graphics.FromImage(screenshot);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            gfxScreenshot.Dispose();
            return screenshot;
        }
        public ScreenshotConvert(string[] split, IWebSocketConnection socket)
        {
            var compress = new Func<byte[], byte[]>(a => {
                using (var ms = new MemoryStream())
                {
                    using (var compressor = new Ionic.Zlib.ZlibStream(ms, 0, (Ionic.Zlib.CompressionLevel) 1))
                    {
                        compressor.Write(a, 0, a.Length);
                    }
                    return ms.ToArray();
                }
            });

            int width = int.Parse(split[1]);
            int height = int.Parse(split[2]);

            int i = 0;
            while (i < 4000)
            {
                if (!socket.IsAvailable) break;
                i++;
                Mat image = BitmapConverter.ToMat(makeScreenshot());

                var arr = new List<byte>();
                var resized = image.Resize(new OpenCvSharp.Size(width, height), 0, 0, InterpolationFlags.Nearest);
                var indexer = new Mat<Vec3b>(resized).GetIndexer();

                for (int x = 0; x < height; x += 1)
                {
                    for (int y = 0; y < width; y += 1)
                    {
                        Vec3b color = indexer[x, y]; // BGR
                        arr.Add(Convert.ToByte(colors[GetClosestColor(colors.Keys.ToArray(), Color.FromArgb(color.Item2, color.Item1, color.Item0))]));
                    }
                }

                byte[] compressed = compress(arr.ToArray());
                Console.WriteLine(arr.Count + " -> " + compressed.Length);
                socket.Send(compressed);
                Cv2.WaitKey(1);
            }
        }
    }
    class VideoConvert
    {
        private Dictionary<Color, string> colors = new Dictionary<Color, string>()
        {
            { Color.FromArgb(240, 240, 240), "0" },
            { Color.FromArgb(242, 178, 51), "1" },
            { Color.FromArgb(229, 127, 216), "2" },
            { Color.FromArgb(153, 178, 242), "3" },
            { Color.FromArgb(222, 222, 108), "4" },
            { Color.FromArgb(127, 204, 25), "5" },
            { Color.FromArgb(242, 178, 204), "6" },
            { Color.FromArgb(76, 76, 76), "7" },
            { Color.FromArgb(153, 153, 153), "8" },
            { Color.FromArgb(76, 153, 178), "9" },
            { Color.FromArgb(178, 102, 229), "a" },
            { Color.FromArgb(51, 102, 204), "b" },
            { Color.FromArgb(127, 102, 76), "c" },
            { Color.FromArgb(87, 166, 78), "d" },
            { Color.FromArgb(204, 76, 76), "e" },
            { Color.FromArgb(25, 25, 25), "f" },
        };
        private Color GetClosestColor(Color[] colorArray, Color baseColor)
        {
            var colors = colorArray.Select(x => new { Value = x, Diff = GetDiff(x, baseColor) }).ToList();
            var min = colors.Min(x => x.Diff);
            return colors.Find(x => x.Diff == min).Value;
        }

        private int GetDiff(Color color, Color baseColor)
        {
            int a = color.A - baseColor.A,
                r = color.R - baseColor.R,
                g = color.G - baseColor.G,
                b = color.B - baseColor.B;
            return a * a + r * r + g * g + b * b;
        }
        public VideoConvert(string[] split, IWebSocketConnection socket)
        {
            var compress = new Func<byte[], byte[]>(a => {
                using (var ms = new MemoryStream())
                {
                    using (var compressor = new Ionic.Zlib.ZlibStream(ms, 0, (Ionic.Zlib.CompressionLevel)1))
                    {
                        compressor.Write(a, 0, a.Length);
                    }
                    return ms.ToArray();
                }
            });

            VideoCapture capture = new VideoCapture("./vidoe.mp4");
            int width = int.Parse(split[1]);
            int height = int.Parse(split[2]);

            int i = 0;
            using (Mat image = new Mat())
            {
                while (true)
                {
                    if (!socket.IsAvailable) break;
                    capture.Read(image);
                    if (image.Empty())
                    {
                        socket.Close();
                        break;
                    }

                    Console.WriteLine((i + 1) + " / " + capture.FrameCount);
                    var arr = new List<byte>();
                    var indexer = new Mat<Vec3b>(image.Resize(new OpenCvSharp.Size(width, height), 0, 0, InterpolationFlags.Nearest)).GetIndexer();

                    for (byte x = 0; x < height; x += 1)
                    {
                        for (byte y = 0; y < width; y += 1)
                        {
                            Vec3b color = indexer[x, y]; // BGR
                            arr.Add(Convert.ToByte(colors[GetClosestColor(colors.Keys.ToArray(), Color.FromArgb(color.Item2, color.Item1, color.Item0))]));
                        }
                    }

                    byte[] compressed = compress(arr.ToArray());
                    Console.WriteLine(arr.Count + " -> " + compressed.Length);
                    socket.Send(compressed);
                    i++;
                }
            }
        }
    }
}