using System;
using System.Collections.Generic;
using System.Text;

namespace Jocr
{
    public static class SplitMultiLines
    {
        private const double LINE_WIDTH_THRESHOLD = 0.7;
        static int startSpace = 0;
        static bool isStartSpace = false;
        static int startTextIndex;     

        public static List<Line> SplitProjectDensity(uint[] density)
        {                   
            List<Line> spaces = new List<Line>();
            
            for (startTextIndex = 1; startTextIndex < density.Length; startTextIndex++)
            {
                if (density[startTextIndex] > 0)
                    break;
            }

            isStartSpace = false;
            startSpace = 0;

            double avgSpan = 0;
            double avgLineWidth = 0;
            int end = 0;
            int span = 0;
            for (int i = startTextIndex; i < density.Length; i++)
            {
                if (density[i] == 0 && density[i - 1] > 0)
                {
                    isStartSpace = true;
                    startSpace = i;
                }
                else if (isStartSpace && density[i] > 0)
                {
                    isStartSpace = false;
                    end = i;
                    span = end - startSpace + 1;
                    avgSpan += span;
                    avgLineWidth += startSpace - startTextIndex;
                    Line space = new Line()
                    {
                        StartText = startTextIndex,
                        StartSpace = startSpace,
                        SpaceSpan = span
                    };
                    spaces.Add(space);
                    startTextIndex = i;
                }
            }            
            ProcessUnclosedFinalBlock(density, spaces);
            avgLineWidth += spaces[spaces.Count - 1].StartSpace - spaces[spaces.Count - 1].StartText;

            if (spaces.Count > 1)
            {
                avgSpan = avgSpan / (spaces.Count - 1);
                avgLineWidth = avgLineWidth / spaces.Count;
            }
            else
                return spaces;

            for (int i = 0; i < spaces.Count;)
            {
                if ((i < spaces.Count - 1) && spaces[i].SpaceSpan < 0.5 * avgSpan)
                {
                    int lineWidthI = spaces[i].StartSpace - spaces[i].StartText;
                    int lineWidthJ = spaces[i + 1].StartSpace - spaces[i + 1].StartText;
                    if (lineWidthI < lineWidthJ)
                        spaces.RemoveAt(i);
                    else if (lineWidthI > lineWidthJ)
                    {
                        spaces.RemoveAt(i + 1);
                        i++;
                    }
                    else
                        i++;
                }
                else if ((spaces[i].StartSpace - spaces[i].StartText) < LINE_WIDTH_THRESHOLD * avgLineWidth)
                    spaces.RemoveAt(i);
                else
                    i++;
            }

            return spaces;
        }

        private static void ProcessUnclosedFinalBlock(uint[] density, List<Line> spaces)
        {
            if (isStartSpace)
            {                
                Line space = new Line()
                {
                    StartText = startTextIndex,
                    StartSpace = startSpace,
                    SpaceSpan = 0
                };
                spaces.Add(space);
            }
            else
            {
                Line space = new Line()
                {
                    StartText = startTextIndex,
                    StartSpace = density.Length,
                    SpaceSpan = 0
                };
                spaces.Add(space);
            }
        }

        /// <summary>
        /// Get sub images from the results returned by SplitProjectDensity()
        /// </summary>
        /// <param name="image">Image that has been analyzed and split by Split(uint[] density)</param>
        /// <param name="space">A split block</param>
        /// <param name="isHorizontal">True if text layout is horizontal</param>
        /// <returns></returns>
        public static GrayImage GetSubImage(GrayImage image, Line block, bool isHorizontal)
        {
            GrayImage subImage;
            if (isHorizontal)
            {
                subImage = GetHorizontalSplitImage(image, block);
            }
            else
            {
                subImage = GetVerticalSplitImage(image, block);
            }
            return subImage;
        }

        private static GrayImage GetHorizontalSplitImage(GrayImage image, Line block)
        {
            GrayImage subImage;
            int width = image.Width;
            int height = block.StartSpace - block.StartText;
            subImage = new GrayImage(width, height);
            subImage.Pixels = new byte[width * height];

            int index = 0;
            int start = block.StartText * width;
            int stop = start + height * width;
            for (int i = start; i < stop; i++)
            {
                subImage.Pixels[index] = image.Pixels[i];
                index++;
            }

            return subImage;
        }

        private static GrayImage GetVerticalSplitImage(GrayImage image, Line block)
        {
            GrayImage subImage;
            int width = block.StartSpace - block.StartText;
            int height = image.Height;
            subImage = new GrayImage(width, height);
            subImage.Pixels = new byte[width * height];

            int index = 0;
            int start = block.StartText;
            int stop = block.StartSpace;
            for (int i = 0; i < image.Pixels.Length; i += image.Width)
            {
                for (int j = start; j < stop; j++)
                {
                    subImage.Pixels[index] = image.Pixels[i + j];
                    index++;
                }
            }

            return subImage;
        }

        public struct Line : IEquatable<Line>
        {
            public int StartText { get; set; }
            public int StartSpace { get; set; }
            public int SpaceSpan { get; set; }

            public override int GetHashCode()
            {
                var result = 0;
                result = (result * 397) ^ StartText;
                result = (result * 397) ^ StartSpace;
                result = (result * 397) ^ SpaceSpan;                
                return result;

            }

            public bool Equals(Line other)
            {
                if (StartText == other.StartText && StartSpace == other.StartSpace && SpaceSpan == other.SpaceSpan)
                    return true;

                return false;
            }
        }
    }
}
