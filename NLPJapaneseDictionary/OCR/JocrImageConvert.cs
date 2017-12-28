using Jocr;
using NLPJapaneseDictionary.Windows;
using NLPJDictCompanion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NLPJapaneseDictionary.OCR
{

    public static class JorcImageConvert
    {
        private const float BLUE_COEF = 0.0722f;
        private const float GREEN_COEF = 0.7152f;
        private const float RED_COEF = 0.2126f;

        public static void ShowJocrGrayImage(GrayImage image)
        {
            ImageWindow imgWindow = new ImageWindow();
            using (var bm = JocrGrayImageToBitmap(image))
            {
                imgWindow.image.Source = BitmapHelper.BitmapToImageSource(bm);
                imgWindow.ShowDialog();                
            }
        }

        public static Bitmap JocrGrayImageToBitmap(GrayImage image)
        {
            Bitmap bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
            BitmapData srcData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            int stride = srcData.Stride;
            IntPtr Scan0 = srcData.Scan0;
            int size = image.Width * image.Height;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int index = 0;
                for (int i = 0; i < size; i++)
                {
                    p[index] = image.Pixels[i];
                    index++;
                    p[index] = image.Pixels[i];
                    index++;
                    p[index] = image.Pixels[i];
                    index++;
                    p[index] = 255;
                    index++;
                }
            }
            return bitmap;
        }

        public static GrayImage BitmapToGrayImageJocr(Bitmap bitmap)
        {
            BitmapData srcData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            try
            {                
                Jocr.GrayImage image = new Jocr.GrayImage(srcData.Width, srcData.Height);
                uint size = (uint)(srcData.Width * srcData.Height);
                switch (srcData.PixelFormat)
                {
                    case PixelFormat.Format32bppArgb:
                        image.Pixels = Argb32ToGray(srcData, size);
                        break;
                    default:
                        throw new FormatException("Wrong image format!");
                }
                return image;
            }            
            finally
            {
                bitmap.UnlockBits(srcData);
            }
        }

        private static byte[] Argb32ToGray(BitmapData srcData, uint size)
        {
            float temp = 0;
            byte[] image = new byte[size];            
            int stride = srcData.Stride;
            IntPtr Scan0 = srcData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int i = 0;
                uint totalLength = size * 4;
                for (int index = 0; index < totalLength; i++, index++)
                {
                    temp = BLUE_COEF * p[index];
                    index++;
                    temp = temp + GREEN_COEF * p[index];
                    index++;
                    temp = temp + RED_COEF * p[index];
                    index++;

                    image[i] = (byte) temp;                    
                }
            }

            return image;
        }
    }
}
