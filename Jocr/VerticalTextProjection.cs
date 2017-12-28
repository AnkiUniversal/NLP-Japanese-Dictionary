using System;
using System.Collections.Generic;
using System.Text;

namespace Jocr
{
    public static class VerticalTextProjection
    {
        public static readonly char[] TopMistakenTextBlocks = new char[]
        { 'う', 'え', 'お', 'こ', 'ご', 'ニ', 'ミ', '三', '二', '元', '示', '言' };

        public static readonly char[] BottomMistakenTextBlocks = new char[]
        { 'ニ', 'ミ', '三', '二', '！' };

        public static readonly char[] HighPriorityMistakenList = new char[]
        { 'う', 'え', 'こ', 'ご', 'ニ', '三' };

        private static float avgBlockWidth = 0;
        private static float avgBlockHeight = 0;
        private static float avgGapBlock = 0;
        private static float avgGapIntraChar = 0;
        private static float avgCharHeight = 0;

        public static List<float> AvgGapBlocks { get; private set; } = new List<float>();
        public static List<float> AvgGapIntraChars { get; private set; } = new List<float>();

        private static float sumBlockWidth = 0;
        private static float sumBlockHeight = 0;        

        private static int startIndex;
        private static int stopIndex;

        private const float MARK_WIDTH_MERGE_THRESHOLD_SCALE = 0.5f;
        private const float HALF_HEIGHT_MERGE_THRESHOLD_SCALE = 1.4f;

        public const float MARK_WIDTH_POSITION_PERCENT = 0.35f;

        public static List<TextBlock> FindAllTextBlock(GrayImage image, uint[] density, SplitMultiLines.Line line, int lineIndex)
        {
            if(lineIndex == 0)
            {
                AvgGapBlocks.Clear();
                AvgGapIntraChars.Clear();
            }

            startIndex = line.StartText;
            stopIndex = line.StartSpace;

            var blocks = InitTextBlocks(image, density, lineIndex);
            if (blocks.Count < 2)
                return blocks;

            GetAverageParams(blocks);

            PreProcessAllBlocksType(blocks, density, image);

            CalculateAverageGaps(blocks);
            MergeMarkBlocks(blocks);

            CalculateAverageGaps(blocks);
            MergeHalfCharBlocks(blocks);

            CalculateAverageGaps(blocks);

            GetTextBlockRatio(blocks);

            AvgGapBlocks.Add(avgGapBlock);
            AvgGapIntraChars.Add(avgGapIntraChar);

            return blocks;
        }

        private static List<TextBlock> InitTextBlocks(GrayImage image, uint[] density, int lineIndex)
        {
            sumBlockWidth = 0;
            sumBlockHeight = 0;

            List<TextBlock> blocks = new List<TextBlock>();
            bool isStartBlock = false;
            TextBlock block;
            int top = 0;
            for (int j = 1; j < density.Length; j++)
            {
                if (density[j] > 0)
                {
                    if (!isStartBlock)
                    {
                        isStartBlock = true;
                        top = j;
                    }
                }
                else
                {
                    if (isStartBlock)
                    {
                        block = GetBlock(image, top, j);
                        block.LineIndex = lineIndex;
                        blocks.Add(block);
                        sumBlockHeight += block.Height;
                        sumBlockWidth += block.Width;                        
                    }
                    isStartBlock = false;
                }
            }

            if (isStartBlock)
            {
                block = GetBlock(image, top, image.Height);
                block.LineIndex = lineIndex;
                blocks.Add(block);
                sumBlockHeight += block.Height;
                sumBlockWidth += block.Width;
            }
            return blocks;
        }

        private static TextBlock GetBlock(GrayImage image, int topIndex, int bottomIndex)
        {
            TextBlock block = new TextBlock();
            block.Top = topIndex;
            block.Bottom = bottomIndex - 1;
            block.Height = bottomIndex - topIndex;            
            int right = 0;
            int left = HorizontalTextProjection.MAX_INT;
            int width = image.Width;

            for (int rowIndx = topIndex; rowIndx < bottomIndex; rowIndx++)
            {
                int index = rowIndx * width + startIndex;
                for (int j = startIndex; j < stopIndex; j++)
                {
                    if (image.Pixels[index] > 0)
                    {
                        if (j < left)
                            left = j;
                        if (j > right)
                            right = j;
                    }
                    index++;
                }
            }

            block.Width = right - left + 1;            
            block.Left = left;
            block.Right = right;
            return block;
        }

        private static void CorrectBlockWidth(GrayImage image, TextBlock block)
        {                        
            int right = 0;
            int left = HorizontalTextProjection.MAX_INT;
            int width = image.Width;
            int topIndex = block.Top;
            int bottomIndex = block.Bottom + 1;
            int startIndex = block.Left;
            int stopIndex = block.Right + 1;

            for (int rowIndx = topIndex; rowIndx < bottomIndex; rowIndx++)
            {
                int index = rowIndx * width + startIndex;
                for (int j = startIndex; j < stopIndex; j++)
                {
                    if (image.Pixels[index] > 0)
                    {
                        if (j < left)
                            left = j;
                        if (j > right)
                            right = j;
                    }
                    index++;
                }
            }

            block.Width = right - left + 1;
            block.Left = left;
            block.Right = right;
        }

        private static void GetAverageParams(List<TextBlock> blocks)
        {
            avgBlockWidth = sumBlockWidth / blocks.Count;
            avgBlockHeight = sumBlockHeight / blocks.Count;
            avgCharHeight = FindAverageCharHeight(blocks);
        }

        private static float FindAverageCharHeight(List<TextBlock> blocks)
        {
            float sumCharHeight = 0;
            int nChar = 0;
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i].Height > avgBlockHeight)
                {
                    sumCharHeight += blocks[i].Height;
                    nChar++;
                }
            }
            if (nChar == 0)
                return avgBlockHeight;
            else
                return sumCharHeight / nChar;
        }

        private static void PreProcessAllBlocksType(List<TextBlock> blocks, uint[] density, GrayImage image)
        {
            bool isSplitBlock = false;
            for(int i = 0; i < blocks.Count; i++)
            {
                FindBlockType(blocks[i]);
                if(blocks[i].Type == TextBlockType.Multi)                
                    isSplitBlock = SplitMultiBlock(blocks, density, image, i);                
            }

            if(isSplitBlock)
            {
                FindAverageBlockParams(blocks);
                for (int i = 0; i < blocks.Count; i++)
                {
                    if (blocks[i].Type == TextBlockType.Multi)
                        continue;

                    FindBlockType(blocks[i]);
                }
            }
        }

        private static bool SplitMultiBlock(List<TextBlock> blocks, uint[] density, GrayImage image, int i)
        {
            int splitIndex = GetSplitIndexOfBlock(blocks[i], density);
            if (splitIndex < blocks[i].Top + 2 || splitIndex > blocks[i].Bottom - 2)
                return false;

            TextBlock bottomHalf = new TextBlock()
            {
                Left = blocks[i].Left,
                Right = blocks[i].Right,            
                Top = splitIndex,
                Bottom = blocks[i].Bottom,
                Height = blocks[i].Bottom - splitIndex + 1,
                Type = TextBlockType.Multi
            };            
            CorrectBlockWidth(image, bottomHalf);
            blocks.Insert(i + 1, bottomHalf);

            blocks[i].Bottom = splitIndex;
            blocks[i].Height = blocks[i].Bottom - blocks[i].Top + 1;
            CorrectBlockWidth(image, blocks[i]);
            return true;
        }

        public static void FindBlockType(TextBlock block)
        {
            if (block.Height < HorizontalTextProjection.MARK_SCALE * avgCharHeight)
                block.Type = TextBlockType.Mark;
            else if (block.Height < HorizontalTextProjection.HALF_SCALE * avgCharHeight)
                block.Type = TextBlockType.Half;
            else if (block.Height < HorizontalTextProjection.MULTI_SCALE * avgCharHeight)
                block.Type = TextBlockType.Single;
            else
                block.Type = TextBlockType.Multi;
        }        

        private static int GetSplitIndexOfBlock(TextBlock block, uint[] density)
        {
            uint maxDensity = 0;            
            MinOfSection[] sectionMin = new MinOfSection[9];
            float topDivide = avgCharHeight / 4;
        
            int section1 =  (int)(block.Top + topDivide);
            int section2 = (int)(section1 + topDivide);
            int section3 = (int)(section2 + topDivide);
            int section4 = (int)(section3 + topDivide);                       

            float bottomDivide = (block.Top  + block.Height - section4) / 5;
            int section5 = (int)(section4 + bottomDivide);
            int section6 = (int)(section5 + bottomDivide);
            int section7 = (int)(section6 + bottomDivide);
            int section8 = (int)(section7 + bottomDivide);

            sectionMin[0] = HorizontalTextProjection.FindMinOfSection(density, block.Top, section1, ref maxDensity);
            sectionMin[1] = HorizontalTextProjection.FindMinOfSection(density, section1, section2, ref maxDensity);
            sectionMin[2] = HorizontalTextProjection.FindMinOfSection(density, section2, section3, ref maxDensity);
            sectionMin[3] = HorizontalTextProjection.FindMinOfSection(density, section3, section4, ref maxDensity);
            uint minMiddleSectionDensity = sectionMin[3].Density;
            int minMiddleindex = sectionMin[3].Index;

            sectionMin[4] = HorizontalTextProjection.FindMinOfSection(density, section4, section5, ref maxDensity);            
            if(sectionMin[4].Density < minMiddleSectionDensity)
            {
                minMiddleSectionDensity = sectionMin[4].Density;
                minMiddleindex = sectionMin[4].Index;
            }

            sectionMin[5] = HorizontalTextProjection.FindMinOfSection(density, section5, section6, ref maxDensity);
            if (sectionMin[5].Density < minMiddleSectionDensity)
            {
                minMiddleSectionDensity = sectionMin[5].Density;
                minMiddleindex = sectionMin[5].Index;
            }

            sectionMin[6] = HorizontalTextProjection.FindMinOfSection(density, section6, section7, ref maxDensity);
            sectionMin[7] = HorizontalTextProjection.FindMinOfSection(density, section7, section8, ref maxDensity);
            sectionMin[8] = HorizontalTextProjection.FindMinOfSection(density, section8, block.Bottom, ref maxDensity);

            int splitIndex = 0;
            if(minMiddleSectionDensity < (maxDensity / 4))
            {
                splitIndex = minMiddleindex;
            }
            else
            {
                float minDen = HorizontalTextProjection.MAX_INT;
                for (int mod = 0; mod < HorizontalTextProjection.MODIFICATION_VECTORS.Length; mod++)
                {
                    float temp = HorizontalTextProjection.MODIFICATION_VECTORS[mod] * sectionMin[mod].Density;
                    if(minDen > temp)
                    {
                        minDen = temp;
                        splitIndex = sectionMin[mod].Index;
                    }
                }
            }
            return splitIndex;
        }

        private static void FindAverageBlockParams(List<TextBlock> blocks)
        {
            int sumBlockWidth = 0;
            int sumBlockHeight = 0;
            for (int i = 0; i < blocks.Count; i++)
            {
                sumBlockWidth += blocks[i].Width;
                sumBlockHeight += blocks[i].Height;
            }
            avgBlockWidth = (float)sumBlockWidth / blocks.Count;
            avgBlockHeight = (float)sumBlockHeight / blocks.Count;
            avgCharHeight = FindAverageCharHeight(blocks);
        }

        public static void CalculateAverageGaps(List<TextBlock> blocks)
        {
            CalculateAverageCharGapBlocks(blocks);
            CalculateAverageIntraCharGapBlocks(blocks);
        }

        private static void CalculateAverageCharGapBlocks(List<TextBlock> blocks)
        {
            float sumGapBlock = 0;
            int nGapBlock = 0;
            for (int i = 0; i < blocks.Count - 1; i++)
            {
                if (blocks[i].Type > 0)
                {
                    sumGapBlock += blocks[i + 1].Top - blocks[i].Bottom;
                    nGapBlock++;
                }
            }
            if (blocks[blocks.Count - 2].Type == 0 && blocks[blocks.Count - 1].Type > 0)
            {
                sumGapBlock += blocks[blocks.Count - 1].Top - blocks[blocks.Count - 2].Bottom;
                nGapBlock++;
            }

            if (nGapBlock > 0)
                avgGapBlock = sumGapBlock / nGapBlock;
            else
                avgGapBlock = 0;
        }

        private static void CalculateAverageIntraCharGapBlocks(List<TextBlock> blocks)
        {
            float sumGapBlock = 0;
            int nGapBlock = 0;
            int gap;
            for (int i = 0; i < blocks.Count - 1; i++)
            {
                if (blocks[i].Type < TextBlockType.Multi)
                {
                    gap = blocks[i + 1].Top - blocks[i].Bottom;
                    if (gap < avgGapBlock)
                    {
                        sumGapBlock += gap;
                        nGapBlock++;
                    }
                }
            }

            if (blocks[blocks.Count - 2].Type == TextBlockType.Multi && blocks[blocks.Count - 1].Type < TextBlockType.Multi)
            {
                gap = blocks[blocks.Count - 1].Top - blocks[blocks.Count - 2].Bottom;
                if (gap < avgGapBlock)
                {
                    sumGapBlock += gap;
                    nGapBlock++;
                }
            }
            if (nGapBlock > 0)
                avgGapIntraChar = sumGapBlock / nGapBlock;
            else
                avgGapIntraChar = avgGapBlock;
        }

        private static void MergeMarkBlocks(List<TextBlock> blocks)
        {
            float mergeWidthThreshold = MARK_WIDTH_MERGE_THRESHOLD_SCALE * avgBlockWidth;
            float mergeHeightThreshold = HALF_HEIGHT_MERGE_THRESHOLD_SCALE * avgCharHeight;
            float mergeGapThreshold = HorizontalTextProjection.MARK_GAP_MERGE_THRESHOLD_SCALE * avgGapBlock;

            while (blocks[0].Type == TextBlockType.Mark)
            {
                bool isMerged = TryMergeFirstMarkToNextBlock(blocks, mergeWidthThreshold, mergeHeightThreshold);
                if (!isMerged)
                    break;
            }      

            for (int j = 1; j < blocks.Count - 1; j++)
            {
                if (blocks[j].Type != TextBlockType.Mark)
                    continue;

                int gapIJ = blocks[j].Top - blocks[j - 1].Bottom;
                int heightIJ = blocks[j].Top - blocks[j - 1].Bottom;

                int gapJK = blocks[j + 1].Top - blocks[j].Bottom;
                int heightJK = blocks[j + 1].Top - blocks[j].Bottom;
                var percent = GetMaxWidthPostionPercentToNeightbor(blocks, j);
                float theshold = mergeWidthThreshold;
                if (percent > MARK_WIDTH_POSITION_PERCENT)
                    theshold = 2f * mergeWidthThreshold;
                if (gapJK > theshold && blocks[j].Width < mergeWidthThreshold)
                    continue;

                if (blocks[j - 1].Type == blocks[j + 1].Type)
                {
                    if (gapIJ < 0.5f * gapJK && gapIJ < avgGapIntraChar)
                    {
                        MergeToNextBlocks(blocks, j - 1);
                        j -= 2;
                        continue;
                    }
                    else if (gapJK <= gapIJ && gapJK < avgGapIntraChar)
                    {
                        MergeToNextBlocks(blocks, j);
                        j--;
                        continue;
                    }
                }
                else if (blocks[j - 1].Type > TextBlockType.Mark && blocks[j + 1].Type > TextBlockType.Mark)
                {
                    if (blocks[j - 1].Type < blocks[j + 1].Type)
                    {
                        if (gapIJ < avgGapIntraChar && gapIJ < gapJK)
                        {
                            MergeToNextBlocks(blocks, j - 1);
                            j -= 2;
                            continue;
                        }
                    }
                    else if (blocks[j - 1].Type > blocks[j + 1].Type && gapJK < avgGapIntraChar && gapJK < gapIJ)
                    {
                        MergeToNextBlocks(blocks, j);
                        j--;
                        continue;
                    }
                }

                if (blocks[j].Width > mergeWidthThreshold || percent > MARK_WIDTH_POSITION_PERCENT)
                {
                    if (j - 2 >= 0 && blocks[j - 2].Type < TextBlockType.Single
                        && blocks[j - 1].Top - blocks[j - 2].Bottom < gapIJ)
                    {
                        blocks[j].IsNeedOCRProcess = true;
                    }
                    else if (j + 2 < blocks.Count && blocks[j + 2].Type < TextBlockType.Single
                           && blocks[j + 2].Top - blocks[j + 1].Bottom < gapJK)
                    {
                        blocks[j].IsNeedOCRProcess = true;
                    }
                    else if (blocks[j - 1].Type == blocks[j + 1].Type
                        || (blocks[j - 1].Type > TextBlockType.Half && blocks[j + 1].Type > TextBlockType.Half))
                    {
                        j = TryMergeMarkBlockToNeightbor(blocks, j, gapIJ, gapJK);
                    }
                    else if (blocks[j + 1].Type > TextBlockType.Half && blocks[j - 1].Type > TextBlockType.Mark && heightIJ < mergeWidthThreshold && gapIJ < gapJK)
                    {
                        MergeToNextBlocks(blocks, j - 1);
                        j -= 2;
                    }
                    else if (blocks[j - 1].Type > TextBlockType.Half && heightJK < mergeWidthThreshold && gapJK < gapIJ)
                    {
                        MergeToNextBlocks(blocks, j);
                        j--;
                    }
                    else
                    {
                        j = TryMergeMarkBlockToNeightbor(blocks, j, gapIJ, gapJK);
                    }
                }
            }

            int finalIndex = blocks.Count - 1;
            if (blocks[finalIndex].Type == TextBlockType.Mark)
            {
                if (blocks[finalIndex - 1].IsNeedOCRProcess)
                    blocks[finalIndex].IsNeedOCRProcess = true;
                else
                {
                    int gap = blocks[finalIndex].Top - blocks[finalIndex - 1].Bottom;
                    var isAroundMiddle = GetMaxWidthPostionPercentToNeightbor(blocks, finalIndex) >= MARK_WIDTH_POSITION_PERCENT;
                    if (gap < avgGapIntraChar && isAroundMiddle)
                        MergeToNextBlocks(blocks, finalIndex - 1);
                    else if (blocks[finalIndex].Width > mergeWidthThreshold || isAroundMiddle)
                    {
                        if (blocks.Count < 3 || blocks[finalIndex - 2].Type > TextBlockType.Half || blocks[finalIndex - 1].Top - blocks[finalIndex - 2].Bottom > 1.5 * gap)
                        {
                            int heightAfterMerge = blocks[finalIndex].Bottom - blocks[finalIndex - 1].Top;
                            if (blocks[finalIndex - 1].Type < TextBlockType.Single && heightAfterMerge < mergeHeightThreshold)
                                MergeToNextBlocks(blocks, finalIndex - 1);
                            else
                                blocks[finalIndex].IsNeedOCRProcess = true;
                        }
                        else
                            blocks[finalIndex].IsNeedOCRProcess = true;
                    }
                }
            }
        }

        private static bool TryMergeFirstMarkToNextBlock(List<TextBlock> blocks, float mergeWidthThreshold, float mergeHeightThreshold)
        {
            int gap = blocks[1].Top - blocks[0].Bottom;
            if (gap <= avgGapBlock || blocks[0].Width >= mergeWidthThreshold)
            {
                if (gap < avgGapIntraChar)
                {
                    MergeToNextBlocks(blocks, 0);
                    return true;
                }
                else if (blocks[0].Width > mergeWidthThreshold)
                {
                    if (blocks.Count < 3 || blocks[2].Type > TextBlockType.Half || blocks[2].Top - blocks[1].Bottom > 1.5f * gap)
                    {
                        int heightAfterMerge = blocks[1].Bottom - blocks[0].Top;
                        if (blocks[1].Type < TextBlockType.Single && heightAfterMerge < mergeHeightThreshold)
                        {
                            MergeToNextBlocks(blocks, 0);
                            return true;
                        }
                        else
                            blocks[0].IsNeedOCRProcess = true;
                    }
                    else
                        blocks[0].IsNeedOCRProcess = true;
                }
            }
            return false;
        }

        public static float GetMaxWidthPostionPercentToNeightbor(List<TextBlock> blocks, int j)
        {
            float percentPreviousRight = 0;
            if (j - 1 > 0 && blocks[j - 1].LineIndex == blocks[j].LineIndex)
            {
                int previousRightDistance = blocks[j - 1].Right - blocks[j].Left + 1;
                percentPreviousRight = (float)previousRightDistance / blocks[j - 1].Width;
            }

            float percentNextRight = 0;
            if (j + 1 < blocks.Count && blocks[j + 1].LineIndex == blocks[j].LineIndex)
            {
                int nextRightDistance = blocks[j + 1].Right - blocks[j].Left + 1;
                percentNextRight = (float)nextRightDistance / blocks[j + 1].Width;
            }

            if (percentPreviousRight > percentNextRight)
                return percentPreviousRight;
            else
                return percentNextRight;
        }

        private static int TryMergeMarkBlockToNeightbor(List<TextBlock> blocks, int j, int gapIJ, int gapJK)
        {
            if (gapIJ <= 0.4 * gapJK)
            {
                MergeToNextBlocks(blocks, j - 1);
                j -= 2;
            }
            else if (gapJK <= 0.4 * gapIJ)
            {
                MergeToNextBlocks(blocks, j);
                j--;
            }
            else
                blocks[j].IsNeedOCRProcess = true;
            return j;
        }

        private static void MergeHalfCharBlocks(List<TextBlock> blocks)
        {
            if(blocks[0].Type == TextBlockType.Half)
            {
                int gap = blocks[1].Top - blocks[0].Bottom;
                TryMergeHalfBlock(blocks, 0, 1, 0, gap);                
            }

            for(int j = 1; j < blocks.Count - 1; j++)
            {
                if (blocks[j].Type != TextBlockType.Half)
                    continue;

                int gapIJ = blocks[j].Top - blocks[j - 1].Bottom;
                int gapJK = blocks[j + 1].Top - blocks[j].Bottom;                
                if( blocks[j - 1].Type < blocks[j + 1].Type)
                    TryMergeHalfBlock(blocks, j, j - 1, j - 1, gapIJ);
                else if(blocks[j - 1].Type > blocks[j + 1].Type)
                    TryMergeHalfBlock(blocks, j, j + 1, j, gapJK);
                else
                {
                    if (gapIJ < gapJK)
                        TryMergeHalfBlock(blocks, j, j - 1, j - 1, gapIJ);
                    else
                        TryMergeHalfBlock(blocks, j, j + 1, j, gapJK);
                }
            }

            int finalIndex = blocks.Count - 1;
            if(blocks[finalIndex].Type == TextBlockType.Half)
            {
                int gap = blocks[finalIndex].Top - blocks[finalIndex - 1].Bottom;                          
                var isInMiddle = GetMaxWidthPostionPercentToNeightbor(blocks, finalIndex) > MARK_WIDTH_POSITION_PERCENT;
                if (isInMiddle)
                {
                    if (blocks.Count < 3 || blocks[finalIndex - 2].Type > TextBlockType.Half)
                        TryMergeHalfBlock(blocks, finalIndex, finalIndex - 1, finalIndex - 1, gap);
                    else
                        blocks[finalIndex].IsNeedOCRProcess = true;
                }
            }
        }

        private static void TryMergeHalfBlock(List<TextBlock> blocks, int index, int comparedIndex, int startMergedIndex, int gap)
        {
            if ((blocks[comparedIndex].Type == TextBlockType.Single && gap < avgGapIntraChar)
                || (blocks[comparedIndex].Type == TextBlockType.Half && gap < 2 * avgGapIntraChar))
            {
                if (blocks[comparedIndex].Height + gap + blocks[index].Height <= HALF_HEIGHT_MERGE_THRESHOLD_SCALE * avgCharHeight)
                    MergeToNextBlocks(blocks, startMergedIndex);
            }
        }

        private static void MergeToNextBlocks(List<TextBlock> blocks, int index)
        {
            int secondIndex = index + 1;
            blocks[index].Bottom = blocks[secondIndex].Bottom;
            blocks[index].Height = blocks[secondIndex].Bottom - blocks[index].Top + 1;
            blocks[index].Left = blocks[index].Left < blocks[secondIndex].Left ? blocks[index].Left : blocks[secondIndex].Left;
            blocks[index].Right = blocks[index].Right > blocks[secondIndex].Right ? blocks[index].Right : blocks[secondIndex].Right;
            blocks[index].Width = blocks[index].Right - blocks[index].Left + 1;
            FindBlockType(blocks[index]);
            blocks[index].IsNeedOCRProcess = false;
            blocks.RemoveAt(secondIndex);
        }

        public static void GetTextBlockRatio(List<TextBlock> blocks)
        {
            float avgChar = 0;
            float avgHeight = 0;
            int count = 0;
            int countHeight = 0;
            var widthThreshold = 0.7f * avgBlockWidth;
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i].Width > widthThreshold)
                {
                    if (blocks[i].Height > avgBlockHeight)
                    {
                        avgHeight += blocks[i].Height;
                        countHeight++;
                    }
                    avgChar += blocks[i].Height * blocks[i].Width;
                    count++;
                }
            }
            if (count == 0 || countHeight == 0)
                return;

            avgChar = avgChar / count;
            avgHeight = avgHeight / countHeight;

            foreach (var block in blocks)
            {
                block.HeighRatio = block.Height / avgHeight;
                block.Ratio = (block.Height * block.Width) / avgChar;
            }
        }
    }
}
