using System;

namespace GIFMaker.Core.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var creator = new VideoManager())
            {
                creator.LoadVideo(@"C:\Users\trick\Desktop\1.mp4");
                creator.SaveGif("test.gif", 50000, 60000, 100);
                creator.GetBitmapOfVid(60 * 15 * 1000).Save("test.png");
            }
        }
    }
}
