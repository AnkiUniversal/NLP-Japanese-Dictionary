using System;
using System.Collections.Generic;
using System.Text;

namespace Jocr
{
    public static class IntensityNormalization
    {
        private static float averagePixelIntensity;
        private static float standardPixelVariation;

        private static int originalImageWidth;
        private static int orginalImageHeight;

        public static int PadTop { get; private set; }
        public static int PadBottom { get; private set; }
        public static int PadLeft { get; private set; }
        public static int PadRight { get; private set; }

        public static float PadPixelIntensity { get; private set; }

        public static float[] ImageNorm { get; private set; } = new float[FeatureExtaction.SHAPE_NORM_TARGET_LENGTH * FeatureExtaction.SHAPE_NORM_TARGET_LENGTH];

        public static void Normalize(byte[] pixels, int width, int height, int padHeight, int padWidth)
        {
            int length = width * height;
            CalculatePads(width, height, padHeight, padWidth);
            FindAverageAndStandardIntensityOnePass(pixels, length);

            PadPixelIntensity = (-averagePixelIntensity / standardPixelVariation);
            
            for (int i = 0; i < length; i++)
                ImageNorm[i] = ((pixels[i] - averagePixelIntensity) / standardPixelVariation);
        }

        private static void CalculatePads(int width, int height, int padHeight, int padWidth)
        {
            PadTop = padHeight;
            PadBottom = padHeight;
            PadLeft = padWidth;
            PadRight = padWidth;
            originalImageWidth = width;
            orginalImageHeight = height;
            int length;
            if (orginalImageHeight > originalImageWidth)
            {
                length = height;
                PadLeft = padWidth + (length - originalImageWidth) / 2;
                PadRight = PadLeft + (length - originalImageWidth) % 2;
            }
            else
            {
                length = width;
                PadTop = padHeight + (length - orginalImageHeight) / 2;
                PadBottom = PadTop + (length - orginalImageHeight) % 2;
            }
        }

        private static void FindAverageAndStandardIntensityOnePass(byte[] pixels, int length)
        {
            double mean = 0;
            double meanAccumlator = 0;
            int totalWidth = (originalImageWidth + PadLeft + PadRight);
            int index = PadTop * (originalImageWidth + PadLeft + PadRight);
            double delta;
            double delta2;

            for (int i = 0; i < length;)
            {
                for(int j = 0; j < PadLeft; j++)
                {
                    index++;
                    delta = -mean;
                    mean += delta / index;
                    meanAccumlator += -delta * mean;
                }

                for (int j = 0; j < originalImageWidth; j++)
                {
                    byte intensity = pixels[i];
                    index++;
                    delta = intensity - mean;
                    mean += delta / index;
                    delta2 = intensity - mean;
                    meanAccumlator += delta * delta2;
                    i++;
                }

                for (int j = 0; j < PadRight; j++)
                {
                    index++;
                    delta = -mean;
                    mean += delta / index;
                    meanAccumlator += -delta * mean;
                }
            }

            for (int i = 0; i < totalWidth*PadBottom; i++)
            {
                index++;
                delta = -mean;
                mean += delta / index;
                meanAccumlator += -delta * mean;
            }

            averagePixelIntensity = (float)mean;
            standardPixelVariation = (float)Math.Sqrt((meanAccumlator / (index - 1)));
        }

        private static void FindAverageAndStandardIntensityTwoPass(byte[] pixels, int length)
        {
            double sum = 0;
            for (int index = 0; index < length; index++)            
                sum += pixels[index];

            int totalSize = (orginalImageHeight + PadTop + PadBottom) * (originalImageWidth + PadLeft + PadRight);
            averagePixelIntensity = (float)(sum / totalSize);

            double sumStd = 0;
            for (int index = 0; index < length; index++)
                sumStd += Math.Pow((pixels[index] - averagePixelIntensity),2);

            int leftOver = totalSize - originalImageWidth * orginalImageHeight;
            sumStd += averagePixelIntensity * averagePixelIntensity * leftOver;
            standardPixelVariation = (float)Math.Sqrt(sumStd/(totalSize - 1));
        }      
    }
}
