using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Jocr
{
    public static class Helper
    {
        public static void DrawTextBlockBox(GrayImage image, List<TextBlock> blocks)
        {
            foreach(var block in blocks)
            {
                int start = block.Top * image.Width + block.Left;
                for (int i = start; i < start + block.Width; i++)
                    image.Pixels[i] = 255;

                start = block.Bottom * image.Width + block.Left;
                for (int i = start; i < start + block.Width; i++)
                    image.Pixels[i] = 255;

                for (int i = block.Top * image.Width + block.Left; i < block.Bottom * image.Width; i+= image.Width)
                    image.Pixels[i] = 255;

                for (int i = block.Top * image.Width + block.Right; i < block.Bottom * image.Width; i += image.Width)
                    image.Pixels[i] = 255;
            }
        }

        public static GrayImage ConvertFloatToByteImage(float[] img, int height, int width)
        {
            GrayImage imageOut = new GrayImage(width, height);
            imageOut.Pixels = new byte[width * height];

            float max = float.MinValue;
            float min = float.MaxValue;
            for (int i = 0; i < img.Length; i++)
            {
                if (img[i] > max)
                    max = img[i];

                if (img[i] < min)
                    min = img[i];
            }
            
            for (int i = 0; i < img.Length; i++)
                img[i] += -min;

            float maxRange = max - min;
            float scale = 255 / maxRange;
            float value;
            for (int i = 0; i < img.Length; i++)
            {
                value = img[i] * scale;
                if (value > 255)
                    value = 255;
                else if (value < 0)
                    value = 0;

                imageOut.Pixels[i] = (byte)value;
            }

            return imageOut;
        }

        public static byte RoundToByte(float number)
        {
            return (byte)Round(number);
        }

        public static int Round(float number)
        {
            int result = (int)number;
            float diff = number - result;
            if (diff < 0.5)
                return result;
            else
                return result + 1;
        }  
    }
}