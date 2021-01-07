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
using Ionic.Zlib;
using NAudio.Wave;
//using java.io;
using Console = System.Console;

namespace CCPlayer
{
    class ComputerCraftStuff
    {
        public Dictionary<Color, char> colors = new Dictionary<Color, char>()
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

        public Color GetClosestColor(Color[] colorArray, Color baseColor)
        {
            var colors = colorArray.Select(x => new { Value = x, Diff = GetDiff(x, baseColor) }).ToList();
            var min = colors.Min(x => x.Diff);
            return colors.Find(x => x.Diff == min).Value;
        }

        int GetDiff(Color color, Color baseColor)
        {
            int a = color.A - baseColor.A,
                r = color.R - baseColor.R,
                g = color.G - baseColor.G,
                b = color.B - baseColor.B;
            return a * a + r * r + g * g + b * b;
        }

        public byte[] compress(byte[] a)
        {
            using (var ms = new MemoryStream())
            {
                using (var compressor = new ZlibStream(ms, 0, CompressionLevel.BestSpeed))
                {
                    compressor.Write(a, 0, a.Length);
                }
                return ms.ToArray();
            }
        }

        private int RESP_INC = 1;
        private int RESP_DEC = 1;
        private int RESP_PREC = 10;

        private int response = 0;
        private int level = 0;
        private bool lastbit = false;

        internal void AudioCompress(byte[] dest, byte[] src, int destoffs, int srcoffs, int len)
        {
            for (int i = 0; i < len; i++)
            {
                int d = 0;
                for (int j = 0; j < 8; j++)
                {
                    int inlevel = src[srcoffs++];
                    bool curbit = (inlevel > level || (inlevel == level && level == 127));
                    d = (curbit ? (d >> 1) + 128 : d >> 1);
                    ctx_update(curbit);
                }
                dest[destoffs++] = (byte)d;
            }
        }

        private void ctx_update(bool curbit)
        {
            int target = (curbit ? 127 : -128);
            int nlevel = (level + ((response * (target - level)
                + (1 << (RESP_PREC - 1))) >> RESP_PREC));
            if (nlevel == level && level != target)
                nlevel += (curbit ? 1 : -1);

            int rtarget, rdelta;
            if (curbit == lastbit)
            {
                rtarget = (1 << RESP_PREC) - 1;
                rdelta = RESP_INC;
            }
            else
            {
                rtarget = 0;
                rdelta = RESP_DEC;
            }

            int nresponse = response + (false ? ((rdelta * (rtarget - response) + 128) >> 8) : 0);
            if (nresponse == response && response != rtarget)
                nresponse += (curbit == lastbit ? 1 : -1);

            if (RESP_PREC > 8)
            {
                if (nresponse < (2 << (RESP_PREC - 8)))
                    nresponse = (2 << (RESP_PREC - 8));
            }

            response = nresponse;
            lastbit = curbit;
            level = nlevel;
        }
    }

    class ScreenshotConvert
    {
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

            int width = int.Parse(split[1]);
            int height = int.Parse(split[2]);

            ComputerCraftStuff CCS = new ComputerCraftStuff();
            Dictionary<Color, char> colors = CCS.colors;

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
                        arr.Add(Convert.ToByte(colors[CCS.GetClosestColor(colors.Keys.ToArray(), Color.FromArgb(color.Item2, color.Item1, color.Item0))]));
                    }
                }

                byte[] compressed = CCS.compress(arr.ToArray());
                Console.WriteLine(arr.Count + " -> " + compressed.Length);
                socket.Send(compressed);
                Cv2.WaitKey(1);
            }
        }
    }
    class VideoConvert
    {
        public VideoConvert(string[] split, IWebSocketConnection socket)
        {
            VideoCapture capture = new VideoCapture("./vidoe.mp4");
            int width = int.Parse(split[1]);
            int height = int.Parse(split[2]);

            ComputerCraftStuff CCS = new ComputerCraftStuff();
            Dictionary<Color, char> colors = CCS.colors;

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
                            arr.Add(Convert.ToByte(colors[CCS.GetClosestColor(colors.Keys.ToArray(), Color.FromArgb(color.Item2, color.Item1, color.Item0))]));
                        }
                    }

                    byte[] compressed = CCS.compress(arr.ToArray());
                    Console.WriteLine(arr.Count + " -> " + compressed.Length);
                    socket.Send(compressed);
                    i++;
                }
            }
        }
    }

    class AudioConvert
    {
        public AudioConvert()
        {
            ComputerCraftStuff CCS = new ComputerCraftStuff();

            string file = "./data/banana.wav";
            string outfile = "./data/pang.dfpwm";

            var raw = new MediaFoundationReader(file);
            var outFormat = new WaveFormat(48000, 8, 1);
            var reader = new MediaFoundationResampler(raw, outFormat);

            List<byte> outFile = new List<byte>();

            byte[] readBuffer = new byte[1024];
            byte[] outBuffer = new byte[readBuffer.Length / 8];

            int read;
            do
            {
                for (read = 0; read < readBuffer.Length;)
                {
                    int amt = reader.Read(readBuffer, read, readBuffer.Length - read);

                    if (amt == 0) break;
                    read += amt;
                }
                read &= ~0x07;
                CCS.AudioCompress(outBuffer, readBuffer, 0, 0, read / 8);

                outFile.AddRange(outBuffer);
            } while (read == readBuffer.Length);

            File.WriteAllBytes(outfile, outFile.ToArray());
        }
    }
}