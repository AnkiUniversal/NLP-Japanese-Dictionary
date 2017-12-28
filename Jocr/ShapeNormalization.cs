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
using System.Text;
using System.Threading.Tasks;

namespace Jocr
{
    public static class ShapeNormalization
    {
        public static int NormWidth { get; private set; }
        public static int NormHeigth { get; private set; }
        public static int Length { get; private set; }
        public static byte[] Pixels { get; private set; }

        static ShapeNormalization()
        {
            Pixels = new byte[FeatureExtaction.SHAPE_NORM_TARGET_LENGTH * FeatureExtaction.SHAPE_NORM_TARGET_LENGTH];
        }

        [System.Obsolete("Not tested and very slow, should only be used for handwriting recognition")]
        public static void MomentNormalization(GrayImage img, TextBlock block)
        {
            var M10 = GetRawMoment10(img, block);
            var M01 = GetRawMoment01(img, block);
            var M00 = GetRawMoment00(img, block);
            if (M00 == 0)
                LinearNormalization(img, block);

            var cX = M10 / M00;
            var cY = M01 / M00;
            var u20 = GetRawMoment20(img, block) - cX * M10;
            var u02 = GetRawMoment02(img, block) - cY * M01;
            if (u20 <= 0 || u02 <= 0)
                LinearNormalization(img, block);

            double deltaX2 = u20 * 16.0;
            double deltaY2 = u02 * 16.0;
            double deltaX = Math.Sqrt(deltaX2);
            double deltaY = Math.Sqrt(deltaY2);            

            FindAspectRatioAdaptiveNormalization(deltaY, deltaX, FeatureExtaction.SHAPE_NORM_TARGET_LENGTH);
            double halfWidth = NormWidth / 2;
            double halfHeight = NormHeigth / 2;
            int shiftTop = block.Top;
            int shiftLeft = block.Left;
            int imgWidth = img.Width;
            int textWidth = block.Width;
            int textHeight = block.Height;
            int length = NormWidth * NormHeigth;
            Parallel.For(0, length, (index) =>
            {
                int i = index / NormWidth;
                int j = index % NormWidth;
                double xs = deltaX * (j - halfWidth) / NormWidth + cX;
                double ys = deltaY * (i - halfHeight) / NormHeigth + cY;
                int x0 = GetOriginalPixelCoordinate(xs, cX, deltaX, deltaX2, textWidth);
                int y0 = GetOriginalPixelCoordinate(ys, cY, deltaY, deltaY2, textHeight);
                int originalIndex = (y0 + shiftTop) * imgWidth + x0 + shiftLeft;
                Pixels[index] = img.Pixels[originalIndex];
            });            
        }

        private static long GetRawMoment00(GrayImage img, TextBlock block)
        {
            int shiftTop = block.Top;
            int shiftLeft = block.Left;
            int width = img.Width;
            long moment = 0;
            int start = shiftTop * width + shiftLeft;
            int textBlockWidth = block.Width;
            int textBlockHeight = block.Height;
            int skipWidth = width - textBlockWidth;
            for (int i = 0; i < textBlockHeight; i++)
            {
                for (int j = 0; j < textBlockWidth; j++)
                {                    
                    if (img.Pixels[start] > 0)
                        moment++;

                    start++;
                }                
                start += skipWidth;                
            }
            return moment;
        }

        private static long GetRawMoment01(GrayImage img, TextBlock block)
        {
            int shiftTop = block.Top;
            int shiftLeft = block.Left;
            int width = img.Width;
            long moment = 0;
            int start = shiftTop * width + shiftLeft;
            int textBlockWidth = block.Width;
            int textBlockHeight = block.Height;
            int skipWidth = width - textBlockWidth;
            for (int i = 0; i < textBlockHeight; i++)
            {
                for (int j = 0; j < textBlockWidth; j++)
                {
                    if (img.Pixels[start] > 0)
                        moment += i;

                    start++;
                }
                start += skipWidth;
            }
            return moment;
        }

        private static long GetRawMoment10(GrayImage img, TextBlock block)
        {
            int shiftTop = block.Top;
            int shiftLeft = block.Left;
            int width = img.Width;
            long moment = 0;
            int start = shiftTop * width + shiftLeft;
            int textBlockWidth = block.Width;
            int textBlockHeight = block.Height;
            int skipWidth = width - textBlockWidth;
            for (int i = 0; i < textBlockHeight; i++)
            {
                for (int j = 0; j < textBlockWidth; j++)
                {
                    if (img.Pixels[start] > 0)
                        moment += j;

                    start++;
                }
                start += skipWidth;
            }
            return moment;
        }

        private static long GetRawMoment02(GrayImage img, TextBlock block)
        {
            int shiftTop = block.Top;
            int shiftLeft = block.Left;
            int width = img.Width;
            long moment = 0;
            int start = shiftTop * width + shiftLeft;
            int textBlockWidth = block.Width;
            int textBlockHeight = block.Height;
            int skipWidth = width - textBlockWidth;
            for (int i = 0; i < textBlockHeight; i++)
            {
                for (int j = 0; j < textBlockWidth; j++)
                {
                    if (img.Pixels[start] > 0)
                        moment += i*i;

                    start++;
                }
                start += skipWidth;
            }
            return moment;
        }

        private static long GetRawMoment20(GrayImage img, TextBlock block)
        {
            int shiftTop = block.Top;
            int shiftLeft = block.Left;
            int width = img.Width;
            long moment = 0;
            int start = shiftTop * width + shiftLeft;
            int textBlockWidth = block.Width;
            int textBlockHeight = block.Height;
            int skipWidth = width - textBlockWidth;
            for (int i = 0; i < textBlockHeight; i++)
            {
                for (int j = 0; j < textBlockWidth; j++)
                {
                    if (img.Pixels[start] > 0)
                        moment += j*j;

                    start++;
                }
                start += skipWidth;
            }
            return moment;
        }

        private static int GetOriginalPixelCoordinate(double shift, long center, double delta, double delta2, int originalDimension)
        {
            double shift2 = shift * shift;
            long center2 = center * center;
            double shiftCenter2 = 2 * shift * center;
            double right = 2 * originalDimension * (shift2 - shiftCenter2 + center2 + delta * (shift - center) / 2) / delta2;
            double left = 4 * center * (shift2 + center2 - delta2 / 4 - shiftCenter2) / delta2;
            int original = (int)Math.Round(right - left);
            if (original < 0)
                return 0;
            else if (original >= originalDimension)
                return originalDimension - 1;
            else
                return original;
        }

        public static void LinearNormalization(GrayImage img, TextBlock block)
        {
            FindAspectRatioAdaptiveNormalization(block.Height, block.Width, FeatureExtaction.SHAPE_NORM_TARGET_LENGTH);            
            float width = (float)block.Width / NormWidth;
            float height = (float)block.Height / NormHeigth;
            int maxColumnIndex = block.Right;
            int maxRowIndex = block.Bottom;
            int shiftLeft = block.Left;
            int shiftTop = block.Top;
            int imgWidth = img.Width;
            int length = NormWidth * NormHeigth;
            Parallel.For(0, length, (index) =>
            {
                int j = index % NormWidth;
                int i = index / NormWidth;
                int x0 = (int)Math.Floor(width * j) + shiftLeft;
                int y0 = (int)Math.Floor(height * i) + shiftTop;

                int imgIndex = imgWidth * y0 + x0;
                if (imgIndex < img.Pixels.Length)
                    Pixels[index] = img.Pixels[imgIndex];
                else
                    Pixels[index] = 0;
            });            
        }      

        private static void FindAspectRatioAdaptiveNormalization(double height, double width, int length)
        {
            double R1;
            bool isWidth = false;
            if (height < width)
                R1 = height / width;
            else
            {
                R1 = width / height;
                isWidth = true;
            }

            var R2 = Math.Sqrt(Math.Sin(Math.PI * R1 / 2 ));

            if(!isWidth)
            {
                NormWidth = length;
                NormHeigth = (int)Math.Ceiling(length * R2);
            }
            else
            {
                NormHeigth = length;
                NormWidth = (int)Math.Ceiling(length * R2);
            }
        }

        public static TextBlock GetTextBlock(GrayImage img)
        {
            int left = 0;
            for (int j = 0; j < img.Width; j++)
            {
                for (int i = 0; i < img.Height; i++)
                {
                    int index = i * img.Width + j;
                    if (img.Pixels[index] > 0)
                    {
                        left = j;
                        j = img.Width;
                        break;
                    }
                }                
            }

            int right = 0;
            for (int j = img.Width - 1; j >= 0; j--)
            {
                for (int i = 0; i < img.Height; i++)
                {
                    int index = i * img.Width + j;
                    if (img.Pixels[index] > 0)
                    {
                        right = j;
                        j = -1;
                        break;
                    }
                }
            }

            int top = 0;
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    int index = i * img.Width + j;
                    if (img.Pixels[index] > 0)
                    {
                        top = i;
                        i = img.Height;
                        break;
                    }
                }
            }

            int bottom = 0;
            for (int i = img.Height - 1; i >= 0; i--)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    int index = i * img.Width + j;
                    if (img.Pixels[index] > 0)
                    {
                        bottom = i;
                        i = -1;
                        break;
                    }
                }
            }

            TextBlock block = new TextBlock()
            {
                Left = left,
                Right = right,
                Top = top,
                Bottom = bottom,
                Width = right - left + 1,
                Height = bottom - top + 1
            };
            return block;
        }
    }
}
