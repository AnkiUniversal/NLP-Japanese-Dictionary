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
    public static class TextExtraction
    {
        public static bool IsHorizontalText { get; private set; }
        public static bool IsOcrParamentersInit { get; private set; } = false;

        public static void InitOcrParameters()
        {
            var test = QuadraticDiscriminant.EIG_VALUE[0];
            IsOcrParamentersInit = true;
        }

        public static List<TextBlock> FindTextBlocks(GrayImage image, bool? isHorizontal = null)
        {
            List<TextBlock> textBlocks;
            if (isHorizontal == null)
                textBlocks = ExtractTextBlocks(image);
            else
                textBlocks = ExtractTextBlocks(image, (bool)isHorizontal);

            RemoveNoiseTextBlocks(textBlocks);
            return textBlocks;
        }

        private static List<TextBlock> ExtractTextBlocks(GrayImage image)
        {
            var vertical = Projection.ProjectAndFindVarianceOnVerticalLine(image);
            var horizontal = Projection.ProjectAndFindVarianceOnHorizontalLine(image);
            IsHorizontalText = Projection.IsHorizontalTextLayout(vertical.Vtheta, horizontal.Vtheta);
            
            if (IsHorizontalText)
            {                
                return ProcessHorizontalLines(image, vertical.Density);
            }
            else
            {
                return ProcessVerticalLines(image, horizontal.Density);
            }            
        }

        private static List<TextBlock> ExtractTextBlocks(GrayImage image, bool isHorizontalText)
        {
            IsHorizontalText = isHorizontalText;
            if (isHorizontalText)
            {
                var vertical = Projection.ProjectOnVerticalLine(image);
                return ProcessHorizontalLines(image, vertical);
            }
            else
            {
                var horizontal = Projection.ProjectOnHorizontalLine(image);
                return ProcessVerticalLines(image, horizontal);
            }
        }

        private static List<TextBlock> ProcessVerticalLines(GrayImage image, uint[] horizontal)
        {
            List<TextBlock> textBlocks = new List<TextBlock>();
            List<SplitMultiLines.Line> textLines = SplitMultiLines.SplitProjectDensity(horizontal);
            textLines.Reverse(); //Japanese column is going from right to left
            for (int i = 0; i < textLines.Count; i++)
            {
                var density = Projection.ProjectSubImageOnVerticalLine(image, textLines[i]);
                textBlocks.AddRange(VerticalTextProjection.FindAllTextBlock(image, density, textLines[i], i));
            }

            return textBlocks;
        }

        private static List<TextBlock> ProcessHorizontalLines(GrayImage image, uint[] vertical)
        {
            List<TextBlock> textBlocks = new List<TextBlock>();
            List<SplitMultiLines.Line> textLines = SplitMultiLines.SplitProjectDensity(vertical);
            for (int i = 0; i < textLines.Count; i++)
            {
                var density = Projection.ProjectSubImageOnHorizontalLine(image, textLines[i]);
                textBlocks.AddRange(HorizontalTextProjection.FindAllTextBlock(image, density, textLines[i], i));
            }

            return textBlocks;
        }

        private static void RemoveNoiseTextBlocks(List<TextBlock> textBlocks)
        {
            for (int i = 0; i < textBlocks.Count; i++)
            {               
                if (textBlocks[i].Width * textBlocks[i].Height < 5)
                {
                    textBlocks.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
