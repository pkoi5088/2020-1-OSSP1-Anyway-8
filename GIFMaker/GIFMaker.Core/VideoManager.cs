using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace GIFMaker.Core
{
    public class VideoManager : IDisposable
    {
        VidReader vidReader = null;

        public void LoadVideo(string path)
        {
            vidReader = new VidReader(path);
        }

        // TODO : videoreader 속에서 gif 저장을 수행하게 클래스를 수정하기
        // 데이터를 복사해서 한번에 받아오는 지금 방식은 낭비가 많다
        public void SaveGif(string gifPath, long start, long end, int delay)
        {
            if (vidReader == null)
                throw new VideoNotLoadedException();

            AnimatedGifCreator creator = new AnimatedGifCreator(gifPath, delay);
            var datas = vidReader.BytesOfPos(start, end, (long)delay);
            foreach (var data in datas)
            {
                unsafe
                {
                    fixed (byte* pData = data)
                    {
                        for (int i = 0; i < data.Length; i += 3)
                        {
                            var temp = pData[i + 2];
                            pData[i + 2] = pData[i];
                            pData[i] = temp;
                        }

                        Bitmap bitmap = new Bitmap(vidReader.width, vidReader.height, 3 * vidReader.width, System.Drawing.Imaging.PixelFormat.Format24bppRgb, (IntPtr)pData);
                        creator.AddFrame(bitmap);
                    }
                }
            }
            creator.Dispose();

        }

        public Bitmap GetBitmapOfVid(long pos)
        {
            if (vidReader == null)
                throw new VideoNotLoadedException();

            unsafe
            {
                var data = vidReader.ByteOfPos(pos);
                fixed (byte* pData = data)
                {
                    for (int i = 0; i < data.Length; i += 3)
                    {
                        var temp = pData[i + 2];
                        pData[i + 2] = pData[i];
                        pData[i] = temp;
                    }

                    Bitmap bitmap = new Bitmap(vidReader.width, vidReader.height, 3 * vidReader.width, System.Drawing.Imaging.PixelFormat.Format24bppRgb, (IntPtr)pData);
                    return bitmap;
                }
            }

        }

        ~VideoManager()
        {
            Dispose();
        }

        public void Dispose()
        {
            vidReader.Dispose();
            vidReader = null;
        }

    }

    public class VideoNotLoadedException : Exception
    { }
}
