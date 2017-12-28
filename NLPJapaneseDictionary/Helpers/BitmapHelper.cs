/**
 * Copyright © 2017-2018 Anki Universal Team.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may
 * not use this file except in compliance with the License.  A copy of the
 * License is distributed with this work in the LICENSE.md file.  You may
 * also obtain a copy of the License from
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDictCompanion
{
    public class BitmapHelper
    {
        public static Bitmap EnlargeBitmapBoudary(Bitmap image, int size)
        {            
            int outputImageWidth = image.Width + size;
            int outputImageHeight = image.Height + size;

            var boundaryFillColor = CalculateAverageBoundariesColor(image);
            Bitmap outputImage = FillBitmap(outputImageWidth, outputImageHeight, boundaryFillColor);

            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                var coordinate = new Point(size/2, size/2);
                graphics.DrawImage(image, new Rectangle(coordinate, image.Size), 
                    new Rectangle(new Point(), image.Size), GraphicsUnit.Pixel);
            }

            return outputImage;
        }

        public static Bitmap FillBitmap(int width, int height, Color color)
        {
            Bitmap Bmp = new Bitmap(width, height);
            using (Graphics gfx = Graphics.FromImage(Bmp))
            using (SolidBrush brush = new SolidBrush(color))
            {
                gfx.FillRectangle(brush, 0, 0, width, height);
            }
            return Bmp;
        }

        private static System.Drawing.Color CalculateAverageBoundariesColor(Bitmap bm)
        {
            int width = bm.Width;
            int height = bm.Height;

            // cutting corners, will fail on anything else but 32 and 24 bit images
            int bppModifier = bm.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? 4 : 3; 

            BitmapData srcData = bm.LockBits(new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, bm.PixelFormat);

            try
            {
                PixelColors avgColorNew;
                PixelColors avgColorOld = GetAverageColor(width, height, srcData, bppModifier);
                while (true)
                {
                    avgColorNew = GetAverageColor(width, height, srcData, bppModifier, avgColorOld);

                    if (!avgColorNew.IsDiffer(avgColorOld, 5))
                        break;
                    avgColorOld = avgColorNew;
                }

                return System.Drawing.Color.FromArgb(avgColorNew.Red, avgColorNew.Green, avgColorNew.Blue);
            }
            finally
            {
                bm.UnlockBits(srcData);
            }
        }

        private static PixelColors GetAverageColor(int width, int height, BitmapData srcData, int bppModifier, PixelColors oldAvgColor = null)
        {
            PixelColors avgColor = new PixelColors();
            double[] totals = new double[] { 0, 0, 0 };

            double red = 0;
            double green = 0;
            double blue = 0;

            int stride = srcData.Stride;
            IntPtr Scan0 = srcData.Scan0;            

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int heightIndex = 0; heightIndex < height; heightIndex++)
                {
                    for (int widthIndex = 0; widthIndex < width;)
                    {
                        int idx = (heightIndex * stride) + widthIndex * bppModifier;

                        if (oldAvgColor != null)
                        {
                            if(oldAvgColor.Red != 0)
                                if (p[idx + 2] / oldAvgColor.Red < 0.5 || p[idx + 2] / oldAvgColor.Red > 2)
                                    p[idx + 2] = (byte)oldAvgColor.Red;

                            if (oldAvgColor.Green != 0)
                                if (p[idx + 1] / oldAvgColor.Green < 0.5 || p[idx + 1] / oldAvgColor.Green > 2)
                                p[idx + 1] = (byte)oldAvgColor.Green;

                            if (oldAvgColor.Blue != 0)
                                if (p[idx] / oldAvgColor.Blue < 0.5 || p[idx] / oldAvgColor.Blue > 2)
                                p[idx] = (byte)oldAvgColor.Blue;
                        }

                        red = p[idx + 2];
                        green = p[idx + 1];
                        blue = p[idx];


                        totals[2] += red;
                        totals[1] += green;
                        totals[0] += blue;

                        if ((heightIndex == 0) || (heightIndex == height - 1))
                            widthIndex++;
                        else if (widthIndex == 0)
                            widthIndex  = width - 1;                                                
                        else
                            break;
                    }
                }

                int count = 2 * width + 2 * (height - 2);
                avgColor.Red = (int)(totals[2] / count);
                avgColor.Green = (int)(totals[1] / count);
                avgColor.Blue = (int)(totals[0] / count);
                return avgColor;
            }
        }

        private class PixelColors
        {
            public int Red { get; set; }
            public int Green { get; set; }
            public int Blue { get; set; }

            public bool IsDiffer(PixelColors color, int diff)
            {
                if (Math.Abs(this.Red - color.Red) > diff)
                    return true;
                if (Math.Abs(this.Green - color.Green) > diff)
                    return true;
                if (Math.Abs(this.Blue - color.Blue) > diff)
                    return true;

                return false;
            }
        }

        private static int GetFinalValue(int color)
        {
            if (color < 15)
                color = 0;
            else if (color > 240)
                color = 255;
            return color;
        }

        public static System.Windows.Media.Imaging.BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                System.Windows.Media.Imaging.BitmapImage bitmapimage = new System.Windows.Media.Imaging.BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();                                
                return bitmapimage;
            }
        }
    }
}
