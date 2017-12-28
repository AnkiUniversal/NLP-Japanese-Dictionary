namespace Jocr
{
    using System;
    using System.Numerics;
    using System.IO;
    using System.Threading.Tasks;

    public static class ColorConverter
    {
        private const int NUMBER_OF_GRAY_LEVELS = 256;
        private const byte MAX_BYTE = 255;
        private const byte MIN_BYTE = 0;
        private const byte MAX_BINARY = 255;
        private const byte MIN_BINARY = 0;

        public static void StartBinarized(GrayImage image)
        {
            var otsu = GetOtsuBinarizedParams(image);
            bool isWhiteOnBlack = IsWhiteOnBlack(image, otsu.Threshold);
            OtsuBinarizedImage(image, otsu, isWhiteOnBlack);
        }

        public static bool IsWhiteOnBlack(GrayImage image, int threshold)
        {            
            int width = image.Width;
            int height = image.Height;

            int temp = 0;
            int lastRow = (height - 1) * width;
            for (int j = 0; j < width; j++)
            {
                temp += image.Pixels[j];
                temp += image.Pixels[lastRow + j];
            }

            int lastCol = width - 1;
            for (int i = 0; i < height; i++)
            {
                temp += image.Pixels[i * width];
                temp += image.Pixels[i * width + lastCol];
            }

            double avgDen = (double)(temp) / ((width + height) * 2);
            if (avgDen < threshold)
                return true;
            else
            {
                return false;
            }
        }

        public static void OtsuBinarizedImage(GrayImage image, Otsu otsu, bool isWhiteOnBlack)
        {            
            if (isWhiteOnBlack)
            {
                Parallel.For(0, image.Pixels.Length, (i) =>
                {
                    image.Pixels[i] = image.Pixels[i] > otsu.Threshold ? image.Pixels[i] : MIN_BINARY;
                });
            }
            else
            {
                Parallel.For(0, image.Pixels.Length, (i) =>
                {
                    image.Pixels[i] = image.Pixels[i] > otsu.Threshold ? MIN_BINARY : (byte)(MAX_BINARY - image.Pixels[i]);
                });
            }
        }

        public static void ModifiedBinarizedImage(GrayImage image, Otsu otsu)
        {
            double delta = 0.3 * otsu.Sigma;
            double lowThreshold = otsu.Threshold - delta;
            double highThreshold = otsu.Threshold + delta;
            int width = image.Width;
            int height = image.Height;
            byte[] pixels = new byte[image.Pixels.Length];

            for (int i = 0; i < image.Width; i++)
            {
                if (image.Pixels[i] < otsu.Threshold)
                    pixels[i] = MIN_BINARY;
                else
                    pixels[i] = MAX_BINARY;
            }

            for (int i = width * (height - 1); i < pixels.Length; i++)
            {
                if (image.Pixels[i] < otsu.Threshold)
                    pixels[i] = MIN_BINARY;
                else
                    pixels[i] = MAX_BINARY;
            }
            
            for (int index = width; index < image.Pixels.Length - width; index++)
            {                
                if (image.Pixels[index] < otsu.Threshold)
                    pixels[index] = MIN_BINARY;
                else
                    pixels[index] = MAX_BINARY;

                for (int j = 1; j < width - 1; j++)
                {
                    index++;
                    if (image.Pixels[index] < lowThreshold)
                        pixels[index] = MIN_BINARY;
                    else if (image.Pixels[index] >= highThreshold)
                        pixels[index] = MAX_BINARY;
                    else
                    {                        
                        if ((image.Pixels[index - width] > 0 && image.Pixels[index] >= image.Pixels[index - width])
                            || (image.Pixels[index + width] > 0 && image.Pixels[index] >= image.Pixels[index + width])
                            || (image.Pixels[index - 1] > 0 && image.Pixels[index] >= image.Pixels[index - 1])
                            || (image.Pixels[index + 1] > 0 && image.Pixels[index] >= image.Pixels[index + 1])
                        )
                            pixels[index] = MAX_BINARY;
                        else
                            pixels[index] = MIN_BINARY;                        
                    }
                }

                index++;
                if (image.Pixels[index] < otsu.Threshold)
                    pixels[index] = MIN_BINARY;
                else
                    pixels[index] = MAX_BINARY;
            }
            image.Pixels = pixels;
        }

        public static Otsu GetOtsuBinarizedParams(GrayImage image)
        {
            var imgHist = GetImageHistogram(image);
            double maxSigma = 0;
            int threshold = 0;

            float sumWhite = 0;
            float sumBlack = 0;
            float mulWhite = 0;
            float mulBlack = 0;

            int i;
            for (i = 1; i < imgHist.Length - 1; i++)
            {
                if (imgHist[i] == 0)
                    continue;
                
                for (int j = 0; j <= i; j++)
                {
                    int histLevel = imgHist[j];
                    sumWhite = sumWhite + imgHist[j];
                    mulWhite = mulWhite + (j + 1) * histLevel;
                }

                for (int j = i + 1; j < imgHist.Length; j++)
                {
                    int histLevel = imgHist[j];
                    sumBlack = sumBlack + histLevel;
                    mulBlack = mulBlack + (j + 1) * histLevel;
                }
                if (sumBlack == 0)
                    break;

                double sigma = FindOtsuSigma(image.Pixels.Length, sumWhite, mulWhite, sumBlack, mulBlack);
                if (sigma > maxSigma)
                {
                    maxSigma = sigma;
                    threshold = i;
                }                
                break;
            }

            i++;
            for (; i < imgHist.Length - 1; i++)
            {
                if (imgHist[i] == 0)
                    continue;

                int histLevel = imgHist[i];
                int mulHistLevel = (i + 1) * histLevel;
                sumWhite = sumWhite + histLevel;
                mulWhite = mulWhite + mulHistLevel;
                                
                sumBlack = sumBlack - histLevel;
                mulBlack = mulBlack - mulHistLevel;
                if (sumBlack == 0)
                    break;

                double sigma = FindOtsuSigma(image.Pixels.Length, sumWhite, mulWhite, sumBlack, mulBlack);
                if (sigma > maxSigma)
                {
                    maxSigma = sigma;
                    threshold = i;
                }
            }
            Otsu param = new Otsu() { Threshold = threshold, Sigma = Math.Sqrt(maxSigma) };
            return param;
        }

        private static double FindOtsuSigma(int length, float sumWhite, float mulWhite, float sumBlack, float mulBlack)
        {
            double w0 = sumWhite / length;
            double w1 = sumBlack / length;
            double u0 = mulWhite / sumWhite;
            double u1 = mulBlack / sumBlack;
            double sigma = w0 * w1 * (Math.Pow((u1 - u0), 2));
            return sigma;
        }

        private static int[] GetImageHistogram(GrayImage image)
        {
            int[] hist = new int[NUMBER_OF_GRAY_LEVELS];            
            for (int i = 0; i < image.Pixels.Length; i++)
            {                
                hist[image.Pixels[i]] = hist[image.Pixels[i]] + 1;
            }
            return hist;
        }

    }

    public struct Otsu : IEquatable<Otsu>
    {
        public int Threshold { get; set; }
        public double Sigma { get; set; }

        public override int GetHashCode()
        {
            return Threshold;

        }

        public bool Equals(Otsu other)
        {
            if (Threshold == other.Threshold)
                return true;

            return false;
        }
    }
}