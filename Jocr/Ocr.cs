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

namespace Jocr
{
    public static class Ocr
    {
        public const int MAX_IMAGE_WIDTH = 1920;
        public const int MAX_IMAGE_HEIGHT = 1080;

        public delegate List<TextBlock> RunOcrHandler(GrayImage image);
        public delegate void ShowImageHandle(GrayImage image);

        public static List<TextBlock> RecognizeOneWord(GrayImage image)
        {
            ColorConverter.StartBinarized(image);            
            var textBlock = Classifier.ClassifyOneText(image);                         
            var blocks = new List<TextBlock>(1);
            blocks.Add(textBlock);

            return blocks;
        }

        public static List<TextBlock> RecognizeSentences(GrayImage image)
        {
            ColorConverter.StartBinarized(image);
            var textBlocks = TextExtraction.FindTextBlocks(image);

            if (textBlocks.Count > 0)
                Classifier.FindTextOfTextBlocks(image, textBlocks, TextExtraction.IsHorizontalText);

            return textBlocks;
        }        

        /// <summary>
        /// This function is for debugging purposes only
        /// </summary>
        /// <param name="image"></param>
        /// <param name="showImageFunction"></param>
        /// <returns></returns>
        public static List<TextBlock> DebugRecognizeSentences(GrayImage image, ShowImageHandle showImageFunction)
        {
            ColorConverter.StartBinarized(image);
            var textBlocks = TextExtraction.FindTextBlocks(image);

            DebugTextExtract(image, textBlocks, showImageFunction);
            //DebugShapNormilization(image, textBlocks, showImageFunction);

            if (textBlocks.Count > 0)
                Classifier.FindTextOfTextBlocks(image, textBlocks, TextExtraction.IsHorizontalText);

            return textBlocks;
        }

        private static void DebugTextExtract(GrayImage image, List<TextBlock> textBlocks, ShowImageHandle showImageFunction)
        {
            var displayImage = image.Clone();
            Helper.DrawTextBlockBox(displayImage, textBlocks);
            showImageFunction(displayImage);
        }

        private static void DebugShapNormilization(GrayImage image, List<TextBlock> textBlocks, ShowImageHandle showImageFunction)
        {
            foreach (var b in textBlocks)
            {
                ShapeNormalization.LinearNormalization(image, b);
                GrayImage bImag = new GrayImage(ShapeNormalization.NormWidth, ShapeNormalization.NormHeigth);
                bImag.Pixels = ShapeNormalization.Pixels;
                showImageFunction(bImag);
            }
        }
    }
}
