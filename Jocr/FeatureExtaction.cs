using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Jocr
{
    public static class FeatureExtaction
    {
        public const int SHAPE_NORM_TARGET_LENGTH = 64;

        private const int PAD_PIXELS = 8;
        private const int STEP_BLOCK = 8;        
        private const int LENGTH = SHAPE_NORM_TARGET_LENGTH + 2 * PAD_PIXELS;
        private const int TOTAL_SIZE = LENGTH * LENGTH;
        private const int NUMBER_OF_BLOCK_PER_ROW = (LENGTH - STEP_BLOCK) / STEP_BLOCK;        
        private const int RAW_FEATURE_BLOCK_SIZE = 32;
        private const int FEATURE_BLOCK_SIZE = 16;

        public const int FEATURE_SIZE = NUMBER_OF_BLOCK_PER_ROW * NUMBER_OF_BLOCK_PER_ROW * FEATURE_BLOCK_SIZE;
        public const int FEATURE_REDUCE_SIZE = 160;        

        public static float[] GradientStrength { get; private set; } = new float[TOTAL_SIZE];
        public static float[] GradientDirection { get; private set; } = new float[TOTAL_SIZE];
        
        public static float[] FeaturesFull { get; private set; } = new float[FEATURE_SIZE];
        public static float[] Features { get; private set; } = new float[FEATURE_REDUCE_SIZE];

        public static void HistogramOrientedGradient(byte[] image, int imageWidth, int imageHeight)
        {
            IntensityNormalization.Normalize(image, imageWidth, imageHeight, PAD_PIXELS, PAD_PIXELS);

            Array.Clear(GradientStrength, 0, GradientStrength.Length);
            Array.Clear(GradientDirection, 0, GradientDirection.Length);

            FindGradientValuesPreRow(imageWidth, IntensityNormalization.ImageNorm);
            Parallel.For(0, imageHeight - 1, (index) =>
            {
                FindGradientValues(imageWidth, index, IntensityNormalization.ImageNorm);
            });
            FindGradientValuesFinalRow(imageWidth, imageHeight, IntensityNormalization.ImageNorm);

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = Environment.ProcessorCount / 2;
            Parallel.For(0, LENGTH / STEP_BLOCK - 1, (i) =>
            {
                for (int j = 0; j < LENGTH - STEP_BLOCK; j += STEP_BLOCK)
                    ExtractBlockFeatures(i, j);
            });

            NormalizeFeatureValue();
        }

        private static void FindGradientValuesPreRow(int imageWidth, float[] imgNorm)
        {
            int nextRowIndex = 0;
            int gradientIndex = (IntensityNormalization.PadTop - 1) * LENGTH + IntensityNormalization.PadLeft;
            float G1;
            float G2;

            G1 = IntensityNormalization.PadPixelIntensity - imgNorm[nextRowIndex];            
            GradientStrength[gradientIndex - 1] = (float)Math.Sqrt(G1 * G1);
            GradientDirection[gradientIndex - 1] = (float)Math.Atan2(0, G1);

            for (int j = 0; j < imageWidth - 1; j++)
            {
                G1 = IntensityNormalization.PadPixelIntensity - imgNorm[nextRowIndex + 1];
                G2 = IntensityNormalization.PadPixelIntensity - imgNorm[nextRowIndex];
                GradientStrength[gradientIndex] = (float)Math.Sqrt(G1 * G1 + G2 * G2);
                GradientDirection[gradientIndex] = (float)Math.Atan2(G2, G1);
                nextRowIndex++;
                gradientIndex++;
            }
            
            G2 = IntensityNormalization.PadPixelIntensity - imgNorm[imageWidth - 1];
            GradientStrength[gradientIndex] = (float)Math.Sqrt(G2 * G2);
            GradientDirection[gradientIndex] = (float)Math.Atan2(G2, 0);
        }

        private static void FindGradientValues(int imageWidth, int index, float[] imgNorm)
        {
            int i = index * imageWidth;
            int nextRowIndex = i + imageWidth;
            int gradientIndex = (IntensityNormalization.PadTop + index) * LENGTH + IntensityNormalization.PadLeft;
            float G1;
            float G2;

            G1 = IntensityNormalization.PadPixelIntensity - imgNorm[nextRowIndex];
            G2 = imgNorm[i] - IntensityNormalization.PadPixelIntensity;
            GradientStrength[gradientIndex - 1] = (float)Math.Sqrt(G1 * G1 + G2 * G2);
            GradientDirection[gradientIndex - 1] = (float)Math.Atan2(G2, G1);

            for (int j = 0; j < imageWidth - 1; j++)
            {                
                G1 = imgNorm[i] - imgNorm[nextRowIndex + 1];
                G2 = imgNorm[i + 1] - imgNorm[nextRowIndex];
                GradientStrength[gradientIndex] = (float)Math.Sqrt(G1 * G1 + G2 * G2);
                GradientDirection[gradientIndex] = (float)Math.Atan2(G2, G1);
                i++;
                nextRowIndex++;
                gradientIndex++;
            }
                        
            G1 = imgNorm[i] - IntensityNormalization.PadPixelIntensity;
            G2 = IntensityNormalization.PadPixelIntensity - imgNorm[nextRowIndex];
            GradientStrength[gradientIndex] = (float)Math.Sqrt(G1 * G1 + G2 * G2);
            GradientDirection[gradientIndex] = (float)Math.Atan2(G2, G1);            
        }

        private static void FindGradientValuesFinalRow(int imageWidth, int imageHeight, float[] imgNorm)
        {
            int index = (imageHeight - 1);
            int i = index * imageWidth;
            int gradientIndex = (IntensityNormalization.PadTop + index) * LENGTH + IntensityNormalization.PadLeft;
            float G1;
            float G2;

            G2 = imgNorm[i] - IntensityNormalization.PadPixelIntensity;
            GradientStrength[gradientIndex - 1] = (float)Math.Sqrt(G2 * G2);
            GradientDirection[gradientIndex - 1] = (float)Math.Atan2(G2, 0);

            for (int j = 0; j < imageWidth - 1; j++)
            {
                G1 = imgNorm[i] - IntensityNormalization.PadPixelIntensity;
                G2 = imgNorm[i + 1] - IntensityNormalization.PadPixelIntensity;
                GradientStrength[gradientIndex] = (float)Math.Sqrt(G1 * G1 + G2 * G2);
                GradientDirection[gradientIndex] = (float)Math.Atan2(G2, G1);
                i++;
                gradientIndex++;
            }

            G1 = imgNorm[i] - IntensityNormalization.PadPixelIntensity;            
            GradientStrength[gradientIndex] = (float)Math.Sqrt(G1 * G1);
            GradientDirection[gradientIndex] = (float)Math.Atan2(0, G1);
        }

        private static void ExtractBlockFeatures(int i, int j)
        {
            i = i * STEP_BLOCK;
            float aS1 = FindGradientSumS1(i, j);
            float aS2 = FindGradientSumS2(i, j);
            float aS3 = FindGradientSumS3(i, j);
            float aS4 = FindGradientSumS4(i, j);
            float strength = 4 * aS1 + 3 * aS2 + 2 * aS3 + aS4;

            int stopRow = i + 16;
            int stopCol = j + 16;
            var twoPi = 2 * Math.PI;
            float[] kArray = new float[RAW_FEATURE_BLOCK_SIZE];
            for (int iRow = i; iRow < stopRow; iRow++)
            {
                for (int jCol = j; jCol < stopCol; jCol++)
                {
                    double k = (GradientDirection[iRow * LENGTH + jCol] + Math.PI) * 32 / twoPi;
                    var kInt = (int)Math.Floor(k);
                    float kRem = (float)(k - kInt);
                    if (kInt > 31)
                        kInt -= 32;
                    kArray[kInt] += strength * (1 - kRem);
                    kInt++;
                    if (kInt > 31)
                        kInt -= 32;
                    kArray[kInt] += strength * kRem;
                }
            }
            int block = (i * NUMBER_OF_BLOCK_PER_ROW + j) * FEATURE_BLOCK_SIZE / STEP_BLOCK;
            var temp = 6 * kArray[0] + 4 * kArray[1] + kArray[2];
            FeaturesFull[block] = (float)Math.Pow(temp, 0.4);
            block++;

            for (int k = 2; k < 29; k += 2)
            {
                temp = kArray[k - 2] + 4 * kArray[k - 1] + 6 * kArray[k] + 4 * kArray[k + 1] + kArray[k + 2];
                FeaturesFull[block] = (float)Math.Pow(temp, 0.4);
                block++;
            }

            temp = kArray[28] + 4 * kArray[29] + 6 * kArray[30] + 4 * kArray[31];
            FeaturesFull[block] = (float)Math.Pow(temp, 0.4);
        }

        private static float FindGradientSumS1(int rowIndex, int columnIndex)
        {
            float sum = 0;
            int row = (rowIndex + 6) * LENGTH + 6 + columnIndex;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 2] + GradientStrength[row + 3];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 2] + GradientStrength[row + 3];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 2] + GradientStrength[row + 3];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 2] + GradientStrength[row + 3];
            return sum;
        }

        private static float FindGradientSumS2(int index, int columnIndex)
        {
            float sum = 0;
            int row = (index + 4) * LENGTH + 4 + columnIndex;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 2] + GradientStrength[row + 3] + GradientStrength[row + 4] + GradientStrength[row + 5] + GradientStrength[row + 6] + GradientStrength[row + 7];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 2] + GradientStrength[row + 3] + GradientStrength[row + 4] + GradientStrength[row + 5] + GradientStrength[row + 6] + GradientStrength[row + 7];

            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 6] + GradientStrength[row + 7];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 6] + GradientStrength[row + 7];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 6] + GradientStrength[row + 7];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 6] + GradientStrength[row + 7];

            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 2] + GradientStrength[row + 3] + GradientStrength[row + 4] + GradientStrength[row + 5] + GradientStrength[row + 6] + GradientStrength[row + 7];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 2] + GradientStrength[row + 3] + GradientStrength[row + 4] + GradientStrength[row + 5] + GradientStrength[row + 6] + GradientStrength[row + 7];

            return sum;
        }

        private static float FindGradientSumS3(int index, int columnIndex)
        {
            float sum = 0;
            int row = (index + 2) * LENGTH + 2 + columnIndex;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 2] + GradientStrength[row + 3] + GradientStrength[row + 4] + GradientStrength[row + 5] + GradientStrength[row + 6] + GradientStrength[row + 7] + GradientStrength[row + 8] + GradientStrength[row + 9] + GradientStrength[row + 10] + GradientStrength[row + 11];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 2] + GradientStrength[row + 3] + GradientStrength[row + 4] + GradientStrength[row + 5] + GradientStrength[row + 6] + GradientStrength[row + 7] + GradientStrength[row + 8] + GradientStrength[row + 9] + GradientStrength[row + 10] + GradientStrength[row + 11];

            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 10] + GradientStrength[row + 11];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 10] + GradientStrength[row + 11];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 10] + GradientStrength[row + 11];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 10] + GradientStrength[row + 11];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 10] + GradientStrength[row + 11];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 10] + GradientStrength[row + 11];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 10] + GradientStrength[row + 11];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 10] + GradientStrength[row + 11];

            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 2] + GradientStrength[row + 3] + GradientStrength[row + 4] + GradientStrength[row + 5] + GradientStrength[row + 6] + GradientStrength[row + 7] + GradientStrength[row + 8] + GradientStrength[row + 9] + GradientStrength[row + 10] + GradientStrength[row + 11];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 2] + GradientStrength[row + 3] + GradientStrength[row + 4] + GradientStrength[row + 5] + GradientStrength[row + 6] + GradientStrength[row + 7] + GradientStrength[row + 8] + GradientStrength[row + 9] + GradientStrength[row + 10] + GradientStrength[row + 11];

            return sum;
        }

        private static float FindGradientSumS4(int index, int columnIndex)
        {
            float sum = 0;
            int row = index * LENGTH + columnIndex;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 2] + GradientStrength[row + 3] + GradientStrength[row + 4] + GradientStrength[row + 5] + GradientStrength[row + 6] + GradientStrength[row + 7] + GradientStrength[row + 8] + GradientStrength[row + 9] + GradientStrength[row + 10] + GradientStrength[row + 11] + GradientStrength[row + 12] + GradientStrength[row + 13] + GradientStrength[row + 14] + GradientStrength[row + 15];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 2] + GradientStrength[row + 3] + GradientStrength[row + 4] + GradientStrength[row + 5] + GradientStrength[row + 6] + GradientStrength[row + 7] + GradientStrength[row + 8] + GradientStrength[row + 9] + GradientStrength[row + 10] + GradientStrength[row + 11] + GradientStrength[row + 12] + GradientStrength[row + 13] + GradientStrength[row + 14] + GradientStrength[row + 15];

            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 14] + GradientStrength[row + 15];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 14] + GradientStrength[row + 15];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 14] + GradientStrength[row + 15];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 14] + GradientStrength[row + 15];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 14] + GradientStrength[row + 15];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 14] + GradientStrength[row + 15];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 14] + GradientStrength[row + 15];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 14] + GradientStrength[row + 15];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 14] + GradientStrength[row + 15];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 14] + GradientStrength[row + 15];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 14] + GradientStrength[row + 15];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 14] + GradientStrength[row + 15];

            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 2] + GradientStrength[row + 3] + GradientStrength[row + 4] + GradientStrength[row + 5] + GradientStrength[row + 6] + GradientStrength[row + 7] + GradientStrength[row + 8] + GradientStrength[row + 9] + GradientStrength[row + 10] + GradientStrength[row + 11] + GradientStrength[row + 12] + GradientStrength[row + 13] + GradientStrength[row + 14] + GradientStrength[row + 15];
            row += LENGTH;
            sum += GradientStrength[row] + GradientStrength[row + 1] + GradientStrength[row + 2] + GradientStrength[row + 3] + GradientStrength[row + 4] + GradientStrength[row + 5] + GradientStrength[row + 6] + GradientStrength[row + 7] + GradientStrength[row + 8] + GradientStrength[row + 9] + GradientStrength[row + 10] + GradientStrength[row + 11] + GradientStrength[row + 12] + GradientStrength[row + 13] + GradientStrength[row + 14] + GradientStrength[row + 15];

            return sum;
        }

        private static void NormalizeFeatureValue()
        {
            float maxValue = FindMaxFeatureValue();
            Parallel.For(0, FeaturesFull.Length, (i) =>
            {
                FeaturesFull[i] = FeaturesFull[i] / maxValue;
            });
        }

        private static float FindMaxFeatureValue()
        {
            float max = float.MinValue;
            for(int i = 0; i < FeaturesFull.Length; i++)
            {
                if (FeaturesFull[i] > max)
                    max = FeaturesFull[i];
            }
            return max;
        }
    
    }
}
