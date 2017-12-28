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
    public static class Filter
    {
        private static float[] ONE_SIGMA_COEF = new float[] { 0.004f, 0.054f, 0.242f, 0.399f, 0.242f, 0.054f, 0.004f };
        private static byte[] filteredPixels = new byte[FeatureExtaction.SHAPE_NORM_TARGET_LENGTH * FeatureExtaction.SHAPE_NORM_TARGET_LENGTH];

        public static void GaussianFilter(byte[] pixels, int imageWidth, int imageHeight)
        {
            int runWidth = imageWidth - 3;
            Parallel.For(0, imageHeight, (index) =>
            {
                HorizontalFilter(pixels, imageWidth, index, filteredPixels, runWidth);
            });

            int row3 = 3 * imageWidth;
            int row2 = 2 * imageWidth;
            int length = imageWidth * imageHeight - row3;
            Parallel.For(0, imageWidth, (j) =>
            {
                VerticalFilter(pixels, imageWidth, j, filteredPixels, row3, row2, length);
            });
        }

        private static void HorizontalFilter(byte[] pixels, int imageWidth, int index, byte[] filteredPixels, int runWidth)
        {
            int i = index * imageWidth;
            filteredPixels[i] = Helper.RoundToByte(ONE_SIGMA_COEF[3] * pixels[i] + ONE_SIGMA_COEF[4] * pixels[i + 1] + ONE_SIGMA_COEF[5] * pixels[i + 2] + ONE_SIGMA_COEF[6] * pixels[i + 3]);
            i++;
            filteredPixels[i] = Helper.RoundToByte(ONE_SIGMA_COEF[2] * pixels[i - 1] + ONE_SIGMA_COEF[3] * pixels[i] + ONE_SIGMA_COEF[4] * pixels[i + 1] + ONE_SIGMA_COEF[5] * pixels[i + 2] + ONE_SIGMA_COEF[6] * pixels[i + 3]);
            i++;
            filteredPixels[i] = Helper.RoundToByte(ONE_SIGMA_COEF[1] * pixels[i - 2] + ONE_SIGMA_COEF[2] * pixels[i - 1] + ONE_SIGMA_COEF[3] * pixels[i] + ONE_SIGMA_COEF[4] * pixels[i + 1] + ONE_SIGMA_COEF[5] * pixels[i + 2] + ONE_SIGMA_COEF[6] * pixels[i + 3]);
            for (int j = 3; j < runWidth; j++)
            {
                i++;
                filteredPixels[i] = Helper.RoundToByte(ONE_SIGMA_COEF[0] * pixels[i - 3] + ONE_SIGMA_COEF[1] * pixels[i - 2] + ONE_SIGMA_COEF[2] * pixels[i - 1] + ONE_SIGMA_COEF[3] * pixels[i] + ONE_SIGMA_COEF[4] * pixels[i + 1] + ONE_SIGMA_COEF[5] * pixels[i + 2] + ONE_SIGMA_COEF[6] * pixels[i + 3]);
            }
            i++;
            filteredPixels[i] = Helper.RoundToByte(ONE_SIGMA_COEF[0] * pixels[i - 3] + ONE_SIGMA_COEF[1] * pixels[i - 2] + ONE_SIGMA_COEF[2] * pixels[i - 1] + ONE_SIGMA_COEF[3] * pixels[i] + ONE_SIGMA_COEF[4] * pixels[i + 1] + ONE_SIGMA_COEF[5] * pixels[i + 2]);
            i++;
            filteredPixels[i] = Helper.RoundToByte(ONE_SIGMA_COEF[0] * pixels[i - 3] + ONE_SIGMA_COEF[1] * pixels[i - 2] + ONE_SIGMA_COEF[2] * pixels[i - 1] + ONE_SIGMA_COEF[3] * pixels[i] + ONE_SIGMA_COEF[4] * pixels[i + 1]);
            i++;
            filteredPixels[i] = Helper.RoundToByte(ONE_SIGMA_COEF[0] * pixels[i - 3] + ONE_SIGMA_COEF[1] * pixels[i - 2] + ONE_SIGMA_COEF[2] * pixels[i - 1] + ONE_SIGMA_COEF[3] * pixels[i]);
        }

        private static void VerticalFilter(byte[] pixels, int imageWidth, int j, byte[] filteredPixels, int row3, int row2, int length)
        {
            int index = j;
            pixels[index] = Helper.RoundToByte(ONE_SIGMA_COEF[3] * filteredPixels[index] + ONE_SIGMA_COEF[4] * filteredPixels[index + imageWidth] + ONE_SIGMA_COEF[5] * filteredPixels[index + row2] + ONE_SIGMA_COEF[6] * filteredPixels[index + row3]);
            index += imageWidth;
            pixels[index] = Helper.RoundToByte(ONE_SIGMA_COEF[2] * filteredPixels[index - imageWidth] + ONE_SIGMA_COEF[3] * filteredPixels[index] + ONE_SIGMA_COEF[4] * filteredPixels[index + imageWidth] + ONE_SIGMA_COEF[5] * filteredPixels[index + row2] + ONE_SIGMA_COEF[6] * filteredPixels[index + row3]);
            index += imageWidth;
            pixels[index] = Helper.RoundToByte(ONE_SIGMA_COEF[1] * filteredPixels[index - row2] + ONE_SIGMA_COEF[2] * filteredPixels[index - imageWidth] + ONE_SIGMA_COEF[3] * filteredPixels[index] + ONE_SIGMA_COEF[4] * filteredPixels[index + imageWidth] + ONE_SIGMA_COEF[5] * filteredPixels[index + row2] + ONE_SIGMA_COEF[6] * filteredPixels[index + row3]);
            index += imageWidth;
            for (; index < length; index += imageWidth)
            {
                pixels[index] = Helper.RoundToByte(ONE_SIGMA_COEF[0] * filteredPixels[index - row3] + ONE_SIGMA_COEF[1] * filteredPixels[index - row2] + ONE_SIGMA_COEF[2] * filteredPixels[index - imageWidth] + ONE_SIGMA_COEF[3] * filteredPixels[index] + ONE_SIGMA_COEF[4] * filteredPixels[index + imageWidth] + ONE_SIGMA_COEF[5] * filteredPixels[index + row2] + ONE_SIGMA_COEF[6] * filteredPixels[index + row3]);
            }
            pixels[index] = Helper.RoundToByte(ONE_SIGMA_COEF[0] * filteredPixels[index - row3] + ONE_SIGMA_COEF[1] * filteredPixels[index - row2] + ONE_SIGMA_COEF[2] * filteredPixels[index - imageWidth] + ONE_SIGMA_COEF[3] * filteredPixels[index] + ONE_SIGMA_COEF[4] * filteredPixels[index + imageWidth] + ONE_SIGMA_COEF[5] * filteredPixels[index + row2]);
            index += imageWidth;
            pixels[index] = Helper.RoundToByte(ONE_SIGMA_COEF[0] * filteredPixels[index - row3] + ONE_SIGMA_COEF[1] * filteredPixels[index - row2] + ONE_SIGMA_COEF[2] * filteredPixels[index - imageWidth] + ONE_SIGMA_COEF[3] * filteredPixels[index] + ONE_SIGMA_COEF[4] * filteredPixels[index + imageWidth]);
            index += imageWidth;
            pixels[index] = Helper.RoundToByte(ONE_SIGMA_COEF[0] * filteredPixels[index - row3] + ONE_SIGMA_COEF[1] * filteredPixels[index - row2] + ONE_SIGMA_COEF[2] * filteredPixels[index - imageWidth] + ONE_SIGMA_COEF[3] * filteredPixels[index]);
        } 
    }
}