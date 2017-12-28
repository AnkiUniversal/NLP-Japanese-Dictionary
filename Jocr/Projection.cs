using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jocr
{
    public static class Projection
    {
        public static ImageProjection ProjectAndFindVarianceOnVerticalLine(GrayImage image)
        {
            int lineIdx = 0;
            float meanAccumlator = 0;
            float mean = 0;
            int lineCrossingPoints = 0;
            int width = image.Width;
            ImageProjection vertical = new ImageProjection { Vtheta = 0, Density = new uint[image.Height] };            
            for(int index = 1; index < image.Pixels.Length; index++)
            {
                lineCrossingPoints = 0;
                uint lineProject = 0;                
                for (int j = 1; j < width; j++)
                {                    
                    if (image.Pixels[index] > 0)
                        lineProject++;

                    if (image.Pixels[index - 1] < image.Pixels[index])
                        lineCrossingPoints++;
                    index++;
                }
                lineIdx++;
                float delta = lineCrossingPoints - mean;
                mean += delta / lineIdx;
                float delta2 = lineCrossingPoints - mean;
                meanAccumlator += delta * delta2;
                vertical.Density[lineIdx - 1] = lineProject;
            }
            vertical.Vtheta = meanAccumlator / lineIdx;
            return vertical;
        }

        public static ImageProjection ProjectAndFindVarianceOnHorizontalLine(GrayImage image)
        {
            int lineIdx = 0;
            float meanAccumlator = 0;
            float mean = 0;
            int lineCrossingPoints = 0;
            int width = image.Width;
            ImageProjection horizontal = new ImageProjection { Vtheta = 0, Density = new uint[width] };
            for (int j = 0; j < width; j++)
            {
                lineCrossingPoints = 0;
                uint lineProject = 0;
                for (int index = j + width; index < image.Pixels.Length; index += width)
                {
                    if (image.Pixels[index] > 0)
                        lineProject++;

                    if (image.Pixels[index - width] < image.Pixels[index])
                        lineCrossingPoints++;                    
                }
                lineIdx++;
                float delta = lineCrossingPoints - mean;
                mean += delta / lineIdx;
                float delta2 = lineCrossingPoints - mean;
                meanAccumlator += delta * delta2;
                horizontal.Density[lineIdx - 1] = lineProject;
            }
            horizontal.Vtheta = meanAccumlator / lineIdx;
            return horizontal;
        }

        public static uint[] ProjectOnHorizontalLine(GrayImage image)
        {
            int width = image.Width;
            var density = new uint[width];

            Parallel.For(0, density.Length, (j) =>
            {
               uint lineProject = 0;
               for (int index = j; index < image.Pixels.Length; index += width)
               {
                   if (image.Pixels[index] > 0)
                       lineProject++;
               }
               density[j] = lineProject;
            });
            return density;
        }

        public static uint[] ProjectSubImageOnHorizontalLine(GrayImage image, SplitMultiLines.Line line)
        {
            int width = image.Width;
            var density = new uint[width];            
            int stop = line.StartSpace * width;

            Parallel.For(0, width, (j) =>
            {
               int start = width * line.StartText + j;
               uint lineProject = 0;
               for (int index = start; index < stop; index += width)
               {
                   if (image.Pixels[index] > 0)
                       lineProject++;
               }
               density[j] = lineProject;               
            });
            return density;
        }

        public static uint[] ProjectOnVerticalLine(GrayImage image)
        {
            int width = image.Width;
            var density = new uint[image.Height];
            Parallel.For(0, density.Length, (i) =>
            {
               int index = i * width;
               uint lineProject = 0;
               for (int j = 0; j < width; j++)
               {
                   if (image.Pixels[index] > 0)
                       lineProject++;
                   index++;
               }
               density[i] = lineProject;
            });
            return density;
        }

        public static uint[] ProjectSubImageOnVerticalLine(GrayImage image, SplitMultiLines.Line line)
        {
            int width = image.Width;
            var density = new uint[image.Height];
            int lineWidth = line.StartSpace - line.StartText;            
            Parallel.For(0, density.Length, (i) =>
            {
               int index = i * width + line.StartText;
               uint lineProject = 0;
               for (int j = 0; j < lineWidth; j++)
               {
                   if (image.Pixels[index] > 0)
                       lineProject++;
                   index++;
               }
               density[i] = lineProject;
            });
            return density;
        }

        public static bool IsHorizontalTextLayout(double verticalVtheta, double horizontalVtheta)
        {
            if (verticalVtheta > horizontalVtheta)
                return true;
            else
                return false;            
        }
    }

    public struct ImageProjection : IEquatable<ImageProjection>
    {
        public float Vtheta { get; set; }
        public uint[] Density { get; set; }

        public override int GetHashCode()
        {
            var result = 0;
            result = (result * 397) ^ (int)(Vtheta * 1000);
            result = (result * 397) ^ Density.Length;            
            return result;
        }


        public bool Equals(ImageProjection other)
        {
            if (Vtheta != other.Vtheta)
                return false;

            if (Density.Length != other.Density.Length)
                return false;

            for(int i = 0; i < Density.Length; i++)
            {
                if (Density[i] != other.Density[i])
                    return false;
            }

            return true;
        }
    }
}