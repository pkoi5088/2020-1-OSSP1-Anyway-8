using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace GIFMaker.Core
{
    public class VideoManager : IDisposable
    {
        public int width { get { return vidReader.width; } }
        public int height { get { return vidReader.height; } }
        public int duration { get { return vidReader.duration; } }

        private VidReader vidReader = null;

        public void LoadVideo(string path)
        {
            vidReader = new VidReader(path);
        }

        public void SaveGif(GifOption option, string gifPath)
        {
            if (vidReader == null)
                throw new VideoNotLoadedException();

            vidReader.SaveGif(option, gifPath);
        }

        public void Seek(long pos)
        {
            vidReader.Seek(pos);
        }

        public BitmapFrame NextBitmapFrame()
        {
            return vidReader.NextBitmapFrame();
        }

        ~VideoManager()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (vidReader != null)
            {
                vidReader.Dispose();
                vidReader = null;
            }
        }

    }

    public class VideoNotLoadedException : Exception
    { }
}
