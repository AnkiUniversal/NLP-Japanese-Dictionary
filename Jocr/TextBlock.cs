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
    public class TextBlock : IEquatable<TextBlock>
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public int Bottom { get; set; }
        public int Right { get; set; }
        public int LineIndex { get; set; }
        public TextBlockType Type { get; set; }
        public float Ratio { get; set; } = 1;
        public float HeighRatio { get; set; } = 1;
        public bool IsNeedOCRProcess { get; set; }
        public List<char> Text { get; set; }

        public TextBlock()
        {

        }

        public static TextBlock CreateMergeTextBlock(TextBlock firstHalf, TextBlock lastHalf, bool isHorizontal)
        {
            if (isHorizontal)
                return CreateMergeHorizontalTextBlock(firstHalf, lastHalf);
            else
                return CreateMergeVerticalTextBlock(firstHalf, lastHalf);
        }

        public static TextBlock CreateMergeHorizontalTextBlock(TextBlock firstHalf,TextBlock lastHalf)
        {
            TextBlock newBlock = new TextBlock();
            newBlock.Top = firstHalf.Top < lastHalf.Top ? firstHalf.Top : lastHalf.Top;
            newBlock.Bottom = firstHalf.Bottom > lastHalf.Bottom ? firstHalf.Bottom : lastHalf.Bottom;
            newBlock.Height = newBlock.Bottom - newBlock.Top + 1;
            newBlock.Left = firstHalf.Left;
            newBlock.Right = lastHalf.Right;
            newBlock.Width = newBlock.Right - newBlock.Left + 1;
            newBlock.Type = TextBlockType.Single;
            return newBlock;
        }

        public static TextBlock CreateMergeVerticalTextBlock(TextBlock firstHalf, TextBlock lastHalf)
        {
            TextBlock newBlock = new TextBlock();
            newBlock.Left = firstHalf.Left < lastHalf.Left ? firstHalf.Left : lastHalf.Left;
            newBlock.Right = firstHalf.Right > lastHalf.Right ? firstHalf.Right : lastHalf.Right;
            newBlock.Width = newBlock.Right - newBlock.Left + 1;
            newBlock.Top = firstHalf.Top;
            newBlock.Bottom = lastHalf.Bottom;
            newBlock.Height = newBlock.Bottom - newBlock.Top + 1;
            newBlock.Type = TextBlockType.Single;
            return newBlock;
        }

        public override int GetHashCode()
        {
            var result = 0;
            result = (result * 397) ^ Top;
            result = (result * 397) ^ Left;
            result = (result * 397) ^ Bottom;
            result = (result * 397) ^ Right;
            return result;
        }

        public bool Equals(TextBlock other)
        {
            if (Top == other.Top && Bottom == other.Bottom && Left == other.Left && Right == other.Right)
                return true;

            return false;
        }
    }

    public enum TextBlockType
    {
        Mark = 0,
        Half = 1,
        Single = 2,
        Multi = 3
    }
}
