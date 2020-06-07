using System;
using VidGifCreator.Core;

namespace VidGifCreator.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var creator = new GifCreator())
            {
                creator.LoadVideo(@"C:\Users\trick\Desktop\1.mp4");
                creator.SaveGif("test.gif", 50000, 60000, 100);
                creator.GetBitmapOfVid(60 * 15 * 1000).Save("test.png");
            }
        }
    }
}
