using System;
using System.Collections.Generic;
using System.Text;

namespace Jocr
{
    public sealed class GrayImage
    {
        public byte[] Pixels { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public GrayImage() { }

        public GrayImage(int width, int height)
        {
            Width = width;
            Height = height;            
        }

        public GrayImage Clone()
        {
            GrayImage cloneImg = new GrayImage(Width, Height);
            cloneImg.Pixels = new byte[Pixels.Length];
            Pixels.CopyTo(cloneImg.Pixels, 0);
            return cloneImg;
        }
    }   
}
