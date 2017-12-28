using System;
using System.Collections.Generic;
using System.Text;

namespace Jocr
{
    public struct MinOfSection
    {
        public uint Density { get; set; }
        public int Index { get; set; }
    }

    public static class HorizontalTextProjection
    {                
        private static float avgBlockWidth = 0;
        private static float avgBlockHeight = 0;
        public static List<float> AvgGapBlocks { get; set; } = new List<float>();
        private static float avgGapBlock = 0;
        private static float avgGapIntraChar = 0;
        private static float avgCharWidth = 0;        

        private static float sumBlockWidth = 0;
        private static float sumBlockHeight = 0;        

        private static int startIndex;
        private static int stopIndex;

        public const int MAX_INT = Int32.MaxValue;
        public const float MARK_HEIGH_POSITION_PERCENT = 0.3f;
        public const float MARK_SCALE = 0.3f;
        public const float HALF_SCALE = 0.7f;
        public const float MULTI_SCALE = 1.4f;        
        public const float MARK_GAP_MERGE_THRESHOLD_SCALE = 1.5f;
        private const float MARK_HEIGHT_MERGE_THRESHOLD_SCALE = 0.5f;
        private const float HALF_WIDTH_MERGE_THRESHOLD_SCALE = 1.4f;
        private const float HALF_WIDTH_GAP_DIFF_THRESHOLD_SCALE = 1.2f;        
        public static readonly float[] MODIFICATION_VECTORS = new float[] { 3, 2.5f, 2, 1.5f, 1, 1.5f, 2, 2.5f, 3 };        

        public static List<TextBlock> FindAllTextBlock(GrayImage image, uint[] density, SplitMultiLines.Line line, int lineIndex)
        {
            if (lineIndex == 0)
                AvgGapBlocks.Clear();

            startIndex = line.StartText * image.Width;
            stopIndex = line.StartSpace * image.Width;

            var blocks = InitTextBlocks(image, density, lineIndex);
            if (blocks.Count < 2)
                return blocks;

            GetAverageParams(blocks);
            PreProcessAllBlocksType(blocks, density, image);

            if(blocks.Count == 2)
            {
                ProcessFewBlocks(blocks);
                return blocks;
            }
            
            CalculateAverageGaps(blocks);
            MergeMarkBlocks(blocks);

            CalculateAverageGaps(blocks);
            MergeHalfCharBlocks(blocks);

            CalculateAverageGaps(blocks);

            GetTextBlockRatio(blocks);

            AvgGapBlocks.Add(avgGapBlock);

            return blocks;
        }

        private static void ProcessFewBlocks(List<TextBlock> blocks)
        {
            float scale = (float) blocks[0].Width / blocks[1].Width;
            if (scale <= HALF_SCALE || scale >= 1 / HALF_SCALE)
            {
                int gap = blocks[1].Left - blocks[0].Right;
                if (gap < 0.5 * avgBlockWidth)
                    MergeToNextBlocks(blocks, 0);
            }
        }

        private static List<TextBlock> InitTextBlocks(GrayImage image, uint[] density, int lineIndex)
        {
            sumBlockWidth = 0;
            sumBlockHeight = 0;

            List<TextBlock> blocks = new List<TextBlock>();
            bool isStartBlock = false;
            TextBlock block;
            int left = 0;
            for (int j = 1; j < density.Length; j++)
            {
                if (density[j] > 0)
                {
                    if (!isStartBlock)
                    {
                        isStartBlock = true;
                        left = j;
                    }
                }
                else
                {
                    if (isStartBlock)
                    {
                        block = GetBlock(image, left, j);
                        block.LineIndex = lineIndex;
                        blocks.Add(block);
                        sumBlockWidth += block.Width;
                        sumBlockHeight += block.Height;
                    }
                    isStartBlock = false;
                }
            }

            if (isStartBlock)
            {
                block = GetBlock(image, left, image.Width);
                block.LineIndex = lineIndex;
                blocks.Add(block);
            }
            return blocks;
        }

        private static TextBlock GetBlock(GrayImage image, int left, int j)
        {
            TextBlock block = new TextBlock();
            block.Left = left;
            block.Right = j - 1;
            block.Width = j - left;
            int bottom = 0;
            int top = MAX_INT;
            int width = image.Width;

            for (int colIndx = left; colIndx < j; colIndx++)
            {
                int start = stopIndex - width + colIndx;
                int stop = startIndex - 1 + colIndx;
                for (int i = start; i > stop; i-= width)
                {
                    if (image.Pixels[i] > 0)
                    {
                        if (i < top)
                            top = i;
                        if (i > bottom)
                            bottom = i;
                    }
                }
            }
            bottom = bottom / width;
            top = top / width;
            block.Height = bottom - top + 1;
            block.Top = top;
            block.Bottom = bottom;
            return block;
        }

        private static void GetAverageParams(List<TextBlock> blocks)
        {
            avgBlockWidth = sumBlockWidth / blocks.Count;
            avgBlockHeight = sumBlockHeight / blocks.Count;
            avgCharWidth = FindAverageCharWidth(blocks);
        }        

        private static float FindAverageCharWidth(List<TextBlock> blocks)
        {
            float sumCharWidth = 0;
            int nChar = 0;
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i].Width > avgBlockWidth)
                {
                    sumCharWidth += blocks[i].Width;
                    nChar++;
                }
            }
            if (nChar == 0)
                return avgBlockWidth;
            else
                return sumCharWidth / nChar;                
        }

        private static void PreProcessAllBlocksType(List<TextBlock> blocks, uint[] density, GrayImage image)
        {
            bool isSplitBlock = false;
            for (int i = 0; i < blocks.Count; i++)
            {
                FindBlockType(blocks[i]);
                if(blocks[i].Type == TextBlockType.Multi)
                {
                    int splitIndex = GetSplitIndexOfBlock(blocks[i], density);
                    if (splitIndex < blocks[i].Left + 2 || splitIndex > blocks[i].Right - 2)
                        continue;

                    TextBlock lastHalf = new TextBlock()
                    {
                        Left = splitIndex,                        
                        Right = blocks[i].Right,
                        Width = blocks[i].Right - splitIndex + 1,
                        Top = blocks[i].Top,
                        Bottom = blocks[i].Bottom,
                        Type = TextBlockType.Multi
                    };
                    CorrectBlockHeight(image, lastHalf);
                    blocks.Insert(i + 1, lastHalf);

                    blocks[i].Width = splitIndex - blocks[i].Left + 1;
                    blocks[i].Right = splitIndex;
                    CorrectBlockHeight(image, blocks[i]);

                    isSplitBlock = true;
                }
            }

            if(isSplitBlock)
            {
                FindAverageBlockParams(blocks);
                for(int i = 0; i< blocks.Count; i++)
                {
                    if (blocks[i].Type == TextBlockType.Multi)
                        continue;

                    FindBlockType(blocks[i]);
                }
            }
        }     

        private static void CorrectBlockHeight(GrayImage image, TextBlock block)
        {
            int bottom = 0;
            int top = MAX_INT;
            int width = image.Width;
            int startIndex = block.Top*width;
            int stopIndex = block.Bottom * width;

            for (int colIndx = block.Left; colIndx < block.Right + 1; colIndx++)
            {
                int start = stopIndex + colIndx;
                int stop = startIndex - 1 + colIndx;
                for (int i = start; i > stop; i -= width)
                {
                    if (image.Pixels[i] > 0)
                    {
                        if (i < top)
                            top = i;
                        if (i > bottom)
                            bottom = i;
                    }
                }
            }
            bottom = bottom / width;
            top = top / width;
            block.Height = bottom - top + 1;
            block.Top = top;
            block.Bottom = bottom;
        }

        private static void FindBlockType(TextBlock block)
        {
            if (block.Width < MARK_SCALE * avgCharWidth)
                block.Type = TextBlockType.Mark;
            else if (block.Width < HALF_SCALE * avgCharWidth)
                block.Type = TextBlockType.Half;
            else if (block.Width < MULTI_SCALE * avgCharWidth)
                block.Type = TextBlockType.Single;
            else
                block.Type = TextBlockType.Multi;
        }        

        private static int GetSplitIndexOfBlock(TextBlock block, uint[] density)
        {
            uint maxDensity = 0;            
            MinOfSection[] sectionMin = new MinOfSection[9];
            float leftDivide = avgCharWidth / 4;
            
            int section1 =  (int)(block.Left + leftDivide);
            int section2 = (int)(section1 + leftDivide);
            int section3 = (int)(section2 + leftDivide);
            int section4 = (int)(section3 + leftDivide);

            float rightDivide = (block.Left + block.Width - section4 ) / 5;
            int section5 = (int)(section4 + rightDivide);
            int section6 = (int)(section5 + rightDivide);
            int section7 = (int)(section6 + rightDivide);
            int section8 = (int)(section7 + rightDivide);

            sectionMin[0] = FindMinOfSection(density, block.Left, section1, ref maxDensity);
            sectionMin[1] = FindMinOfSection(density, section1, section2, ref maxDensity);
            sectionMin[2] = FindMinOfSection(density, section2, section3, ref maxDensity);
            sectionMin[3] = FindMinOfSection(density, section3, section4, ref maxDensity);
            uint minMiddleSectionDensity = sectionMin[3].Density;
            int minMiddleindex = sectionMin[3].Index;

            sectionMin[4] = FindMinOfSection(density, section4, section5, ref maxDensity);            
            if(sectionMin[4].Density < minMiddleSectionDensity)
            {
                minMiddleSectionDensity = sectionMin[4].Density;
                minMiddleindex = sectionMin[4].Index;
            }

            sectionMin[5] = FindMinOfSection(density, section5, section6, ref maxDensity);
            if (sectionMin[5].Density < minMiddleSectionDensity)
            {
                minMiddleSectionDensity = sectionMin[5].Density;
                minMiddleindex = sectionMin[5].Index;
            }

            sectionMin[6] = FindMinOfSection(density, section6, section7, ref maxDensity);
            sectionMin[7] = FindMinOfSection(density, section7, section8, ref maxDensity);
            sectionMin[8] = FindMinOfSection(density, section8, block.Right, ref maxDensity);

            int splitIndex = 0;
            if(minMiddleSectionDensity < (maxDensity / 4))
            {
                splitIndex = minMiddleindex;
            }
            else
            {
                float minDen = MAX_INT;
                for (int mod = 0; mod < MODIFICATION_VECTORS.Length; mod++)
                {
                    float temp = MODIFICATION_VECTORS[mod] * sectionMin[mod].Density;
                    if(minDen > temp)
                    {
                        minDen = temp;
                        splitIndex = sectionMin[mod].Index;
                    }
                }
            }
            return splitIndex;
        }

        public static MinOfSection FindMinOfSection(uint[] density, int start, int stop, ref uint maxDensity)
        {
            uint den;
            uint minDen = MAX_INT;
            int minIdx = 0;
            for (int i = start; i < stop; i++)
            {
                den = density[i];
                if (den > maxDensity)
                    maxDensity = den;

                if (minDen > den)
                {
                    minDen = den;
                    minIdx = i;
                }
            }
            return new MinOfSection() { Density = minDen, Index = minIdx };
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
            avgCharWidth = FindAverageCharWidth(blocks);
        }

        private static void CalculateAverageGaps(List<TextBlock> blocks)
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
                    sumGapBlock += blocks[i + 1].Left - blocks[i].Right;
                    nGapBlock++;
                }
            }
            if (blocks[blocks.Count - 2].Type == 0 && blocks[blocks.Count - 1].Type > 0)
            {
                sumGapBlock += blocks[blocks.Count - 1].Left - blocks[blocks.Count - 2].Right;
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
                    gap = blocks[i + 1].Left - blocks[i].Right;
                    if (gap < avgGapBlock)
                    {
                        sumGapBlock += gap;
                        nGapBlock++;
                    }
                }
            }

            if (blocks[blocks.Count - 2].Type == TextBlockType.Multi && blocks[blocks.Count - 1].Type < TextBlockType.Multi)
            {
                gap = blocks[blocks.Count - 1].Left - blocks[blocks.Count - 2].Right;
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
            float mergeHeigthThreshold = MARK_HEIGHT_MERGE_THRESHOLD_SCALE * avgBlockHeight;
            float mergeWidthThreshold = HALF_WIDTH_MERGE_THRESHOLD_SCALE * avgCharWidth;
            float mergeGapThreshold = MARK_GAP_MERGE_THRESHOLD_SCALE * avgGapBlock;
            while (blocks[0].Type == TextBlockType.Mark)
            {
                bool isMerged = TryMergeFirstMarkToNextBlock(blocks, mergeHeigthThreshold, mergeWidthThreshold);
                if (!isMerged)
                    break;
            }

            for (int j = 1; j < blocks.Count - 1; j++)
            {
                if (blocks[j].Type != TextBlockType.Mark)
                    continue;

                int gapIJ = blocks[j].Left - blocks[j - 1].Right;
                int widthIJ = blocks[j].Right - blocks[j - 1].Left;

                int gapJK = blocks[j + 1].Left - blocks[j].Right;
                int widthJK = blocks[j + 1].Right - blocks[j].Left;

                var percent = GetMaxHeightPostionPercentToNeightbor(blocks, j);
                float threshold = mergeGapThreshold;
                if (percent > MARK_HEIGH_POSITION_PERCENT)
                    threshold = 1.5f * mergeGapThreshold;
                if (gapJK > threshold)
                    continue;

                if (blocks[j - 1].Type == blocks[j + 1].Type)
                {
                    if (gapIJ < gapJK && gapIJ < avgGapIntraChar)
                    {
                        MergeToNextBlocks(blocks, j - 1);
                        j -= 2;
                        continue;
                    }
                    else if (gapJK < gapIJ && gapJK < avgGapIntraChar)
                    {
                        MergeToNextBlocks(blocks, j);
                        j--;
                        continue;
                    }
                }
                else if(blocks[j - 1].Type > TextBlockType.Mark && blocks[j + 1].Type > TextBlockType.Mark)
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

                if (blocks[j].Height > mergeHeigthThreshold || percent > MARK_HEIGH_POSITION_PERCENT)
                {
                    if (j - 2 >= 0 && blocks[j - 2].Type < TextBlockType.Single
                        && blocks[j - 1].Left - blocks[j - 2].Right < gapIJ)
                    {
                        blocks[j].IsNeedOCRProcess = true;
                    }
                    else if (j + 2 < blocks.Count && blocks[j + 2].Type < TextBlockType.Single
                           && blocks[j + 2].Left - blocks[j + 1].Right < gapJK)
                    {
                        blocks[j].IsNeedOCRProcess = true;
                    }
                    else if (blocks[j - 1].Type == blocks[j + 1].Type
                        || (blocks[j - 1].Type > TextBlockType.Half && blocks[j + 1].Type > TextBlockType.Half))
                    {
                        j = TryMergeMarkBlockToNeightbor(blocks, j, gapIJ, gapJK);
                    }
                    else if (blocks[j + 1].Type > TextBlockType.Half && blocks[j - 1].Type > TextBlockType.Mark && widthIJ < mergeWidthThreshold && gapIJ < gapJK)
                    {
                        MergeToNextBlocks(blocks, j - 1);
                        j -= 2;
                    }
                    else if (blocks[j - 1].Type > TextBlockType.Half && blocks[j + 1].Type > TextBlockType.Mark && widthJK < mergeWidthThreshold && gapJK < gapIJ)
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
                    int gap = blocks[finalIndex].Left - blocks[finalIndex - 1].Right;
                    var percent = GetMaxHeightPostionPercentToNeightbor(blocks, finalIndex);
                    if (gap < avgGapIntraChar)
                    {
                        if(percent > MARK_HEIGH_POSITION_PERCENT)
                            MergeToNextBlocks(blocks, finalIndex - 1);
                        else
                            blocks[finalIndex].IsNeedOCRProcess = true;
                    }
                    else if (blocks[finalIndex].Height > mergeHeigthThreshold && percent > MARK_HEIGH_POSITION_PERCENT)
                    {
                        if (blocks.Count < 3 || blocks[finalIndex - 2].Type > TextBlockType.Half || blocks[finalIndex - 1].Left - blocks[finalIndex - 2].Right > 1.5 * gap)
                        {
                            int widthAfterMerge = blocks[finalIndex].Right - blocks[finalIndex - 1].Left;
                            if (blocks[finalIndex - 1].Type < TextBlockType.Single && widthAfterMerge < mergeWidthThreshold)
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

        private static bool TryMergeFirstMarkToNextBlock(List<TextBlock> blocks, float mergeHeigthThreshold, float mergeWidthThreshold)
        {
            int gap = blocks[1].Left - blocks[0].Right;
            float threshold = avgGapBlock;
            var percent = GetMaxHeightPostionPercentToNeightbor(blocks, 0);
            if (percent > MARK_HEIGH_POSITION_PERCENT)
                threshold = MARK_GAP_MERGE_THRESHOLD_SCALE * avgGapBlock;
            if (gap <= threshold)
            {
                if (gap < avgGapIntraChar)
                {
                    MergeToNextBlocks(blocks, 0);
                    return true;
                }
                else if (blocks[0].Height > mergeHeigthThreshold || percent > MARK_HEIGH_POSITION_PERCENT)
                {
                    if (blocks.Count < 3 || blocks[2].Type > TextBlockType.Half || blocks[2].Left - blocks[1].Right > 1.5 * gap)
                    {
                        int widthAfterMerge = blocks[1].Right - blocks[0].Left;
                        if (blocks[1].Type < TextBlockType.Single && widthAfterMerge < mergeWidthThreshold)
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

        private static int TryMergeMarkBlockToNeightbor(List<TextBlock> blocks, int j, int gapIJ, int gapJK)
        {            
            if (gapIJ <= 0.5 * gapJK && blocks[j - 1].Type <= blocks[j + 1].Type)
            {
                MergeToNextBlocks(blocks, j - 1);
                j -= 2;
            }
            else if (gapJK <= 0.5 * gapIJ && blocks[j + 1].Type <= blocks[j - 1].Type)
            {
                MergeToNextBlocks(blocks, j);
                j--;
            }
            else
                blocks[j].IsNeedOCRProcess = true;
            return j;
        }

        public static float GetMaxHeightPostionPercentToNeightbor(List<TextBlock> blocks, int blockIndex)
        {
            float percentTopLeft = 0;
            if (blockIndex - 1 > 0 && blocks[blockIndex - 1].LineIndex == blocks[blockIndex].LineIndex)
            {
                int topLeft = blocks[blockIndex - 1].Bottom - blocks[blockIndex].Top + 1;
                percentTopLeft = (float)topLeft / blocks[blockIndex - 1].Height;
            }

            float percentTopRight = 0;
            if (blockIndex + 1 < blocks.Count && blocks[blockIndex + 1].LineIndex == blocks[blockIndex].LineIndex)
            {
                int topRight = blocks[blockIndex + 1].Bottom - blocks[blockIndex].Top + 1;
                percentTopRight = (float)topRight / blocks[blockIndex + 1].Height;
            }

            if (percentTopRight > percentTopLeft)
                return percentTopRight;
            else
                return percentTopLeft;
        }

        private static void MergeHalfCharBlocks(List<TextBlock> blocks)
        {
            if(blocks[0].Type == TextBlockType.Half)
            {
                int gap = blocks[1].Left - blocks[0].Right;
                if (blocks.Count < 3 || blocks[2].Type > TextBlockType.Half)
                    TryMergeHalfBlock(blocks, 0, 1, 0, gap);
                else
                    blocks[0].IsNeedOCRProcess = true;
            }

            for (int j = 1; j < blocks.Count - 1; j++)
            {
                if (blocks[j].Type != TextBlockType.Half)
                    continue;
                //TODO: Simplify comparison logic
                int gapIJ = blocks[j].Left - blocks[j - 1].Right;
                int gapJK = blocks[j + 1].Left - blocks[j].Right;
                var isWidthIJValid = blocks[j].Right - blocks[j - 1].Left <= HALF_WIDTH_MERGE_THRESHOLD_SCALE * avgCharWidth;
                var isWidthJKValid = blocks[j + 1].Right - blocks[j].Left <= HALF_WIDTH_MERGE_THRESHOLD_SCALE * avgCharWidth;

                if (j - 2 >= 0 && blocks[j - 2].Type < TextBlockType.Single && isWidthIJValid
                       && blocks[j - 1].Left - blocks[j - 2].Right < gapIJ)
                {
                    blocks[j].IsNeedOCRProcess = true;
                }
                else if (j + 2 < blocks.Count && blocks[j + 2].Type < TextBlockType.Single && isWidthJKValid
                       && blocks[j + 2].Left - blocks[j + 1].Right < gapJK)
                {
                    blocks[j].IsNeedOCRProcess = true;
                }                
                else if (blocks[j - 1].Type < blocks[j + 1].Type && blocks[j + 1].Type > TextBlockType.Half && isWidthIJValid)
                {
                    if (TryMergeHalfBlock(blocks, j, j - 1, j - 1, gapIJ))
                        j--;
                }
                else if (blocks[j - 1].Type > blocks[j + 1].Type && blocks[j - 1].Type > TextBlockType.Half && isWidthJKValid)
                    TryMergeHalfBlock(blocks, j, j + 1, j, gapJK);
                else if (gapIJ < gapJK && blocks[j + 1].Type > TextBlockType.Half && isWidthIJValid)
                {
                    if (TryMergeHalfBlock(blocks, j, j - 1, j - 1, gapIJ))
                        j--;
                }
                else if (gapJK < gapIJ && blocks[j - 1].Type > TextBlockType.Half && isWidthJKValid)
                    TryMergeHalfBlock(blocks, j, j + 1, j, gapJK);
                else if(blocks[j - 1].IsNeedOCRProcess == false)
                {                    
                    if (gapIJ <= 0.5 * gapJK && isWidthIJValid)
                    {
                        if (TryMergeHalfBlock(blocks, j, j - 1, j - 1, gapIJ))
                            j--;
                    }
                    else if (gapJK <= 0.5 * gapIJ && isWidthJKValid)
                        TryMergeHalfBlock(blocks, j, j + 1, j, gapJK);
                    else if (isWidthJKValid || isWidthIJValid)
                        blocks[j].IsNeedOCRProcess = true;
                }
                else
                    blocks[j].IsNeedOCRProcess = true;

            }

            int finalIndex = blocks.Count - 1;
            if(blocks[finalIndex].Type == TextBlockType.Half)
            {
                int gap = blocks[finalIndex].Left - blocks[finalIndex - 1].Right;
                var percent = GetMaxHeightPostionPercentToNeightbor(blocks, finalIndex);
                if (percent >= 0.5f)
                {                    
                    if (blocks.Count < 3 || blocks[finalIndex - 2].Type > TextBlockType.Half)
                        TryMergeHalfBlock(blocks, finalIndex, finalIndex - 1, finalIndex - 1, gap);
                    else
                        blocks[finalIndex].IsNeedOCRProcess = true;
                }                
            }                
        }

        private static bool TryMergeHalfBlock(List<TextBlock> blocks, int index, int comparedIndex, int startMergedIndex, int gap)
        {
            if ((blocks[comparedIndex].Type == TextBlockType.Single && gap < avgGapIntraChar)
                || (blocks[comparedIndex].Type == TextBlockType.Half && gap < 2 * avgGapIntraChar))
            {                
                MergeToNextBlocks(blocks, startMergedIndex);
                return true;                
            }

            blocks[index].IsNeedOCRProcess = true;
            return false;
        }

        private static void MergeToNextBlocks(List<TextBlock> blocks, int index)
        {
            int secondIndex = index + 1;
            blocks[index].Right = blocks[secondIndex].Right;
            blocks[index].Width = blocks[secondIndex].Right - blocks[index].Left + 1;
            blocks[index].Top = blocks[index].Top < blocks[secondIndex].Top ? blocks[index].Top : blocks[secondIndex].Top;
            blocks[index].Bottom = blocks[index].Bottom > blocks[secondIndex].Bottom ? blocks[index].Bottom : blocks[secondIndex].Bottom;
            blocks[index].Height = blocks[index].Bottom - blocks[index].Top + 1;
            FindBlockType(blocks[index]);
            blocks[index].IsNeedOCRProcess = false;
            blocks.RemoveAt(secondIndex);
        }

        public static void GetTextBlockRatio(List<TextBlock> blocks)
        {
            
            float avgBlockHeight = 0;
            int count = 0;
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i].Type > TextBlockType.Mark)
                {
                    avgBlockHeight += blocks[i].Height;
                    count++;
                }
            }
            if (count == 0)
                return;

            avgBlockHeight = avgBlockHeight / count;

            float avgChar = 0;
            float avgHeight = 0;            
            int countHeight = 0;
            count = 0;
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i].Height > avgBlockHeight)
                {
                    if (blocks[i].Type > TextBlockType.Half)
                    {
                        avgChar += blocks[i].Height * blocks[i].Width;
                        count++;
                    }                    
                    avgHeight += blocks[i].Height;
                    countHeight++;
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

        public static int KanjiRightPartsDetection(char part, char text)
        {
           if (part == 'り' || part == 'リ' || part == 'サ')
                return Array.BinarySearch<char>(BladeParts, text);
           else if(part == 'カ' || part == '力')
                return Array.BinarySearch<char>(ForceParts, text);
            else if (part == 'ト')
                return Array.BinarySearch<char>(ToParts, text);
            else if (part == '口' || part == 'ロ' || part == 'ｐ')
                return Array.BinarySearch<char>(QParts, text);
            else if (part == '寸')
                return Array.BinarySearch<char>(MeasureParts, text);
            else if (part == '巾' || part == '市')
                return Array.BinarySearch<char>(RightNushiParts, text);
            else if (part == '彡' || part == 'ミ')
                return Array.BinarySearch<char>(HairParts, text);
            else if (part == 'B')
                return Array.BinarySearch<char>(RightFlagParts, text);
            else if (part == '斗')
                return Array.BinarySearch<char>(FeeParts, text);
            else if (part == '虫')
                return Array.BinarySearch<char>(RightMushiParts, text);
            else if (part == 'テ')
                return Array.BinarySearch<char>(TeParts, text);
            else if (part == '頁')
                return Array.BinarySearch<char>(HeadParts, text);
            else if (part == '匕')
                return Array.BinarySearch<char>(SpoonParts, text);
            else if (part == '月')
                return Array.BinarySearch<char>(RightMoonParts, text);
            else 
                return Array.BinarySearch<char>(RightMistakenLetters, text);
        }

        public static int KanjiLeftPartsDetection(char part, char text)
        {
            if (Classifier.IsVerticalLineChar(part) || part == 'し' || part == '、' || part == 'ノ')
                return Array.BinarySearch<char>(LineParts, text);
            else if (part == '言' && text >= GON_PARTS_START && text <= GON_PARTS_END)
                return 0;
            else if (part == '矢' && text >= ARROW_PARTS_START && text <= ARROW_PARTS_END)
                return 0;
            else if (part == '口' || part == 'ロ')
                return Array.BinarySearch<char>(KuchiParts, text);
            else if (part == 'イ' || part == 'ィ')
                return Array.BinarySearch<char>(HitoParts, text);
            else if (part == '巾' || part == '市')
                return Array.BinarySearch<char>(NushiParts, text);
            else if (part == '氵' || part == '冫' || part == 'ソ' || part == 'ツ' || part == 'シ')
                return Array.BinarySearch<char>(WaterParts, text);
            else if (part == '女')
                return Array.BinarySearch<char>(OnnaParts, text);
            else if (part == '彳')
                return Array.BinarySearch<char>(GoParts, text);
            else if (part == '月')
                return Array.BinarySearch<char>(MoonParts, text);
            else if (part == '日' || part == '８')
                return Array.BinarySearch<char>(SunParts, text);
            else if (part == '目' || part == '耳')
                return Array.BinarySearch<char>(EyeAndEarParts, text);
            else if (part == '木')
                return Array.BinarySearch<char>(TreeParts, text);
            else if (part == '扌' || part == '手' || part == '犭' || part == '才')
                return Array.BinarySearch<char>(HandAndDogParts, text);
            else if (part == '王')
                return Array.BinarySearch<char>(OuParts, text);
            else if (part == '土' || part == '士')
                return Array.BinarySearch<char>(GroundParts, text);
            else if (part == '山')
                return Array.BinarySearch<char>(SanParts, text);
            else if (part == '弓' || part == '号')
                return Array.BinarySearch<char>(BowParts, text);
            else if (part == '火')
                return Array.BinarySearch<char>(FireParts, text);
            else if (part == '糸')
                return Array.BinarySearch<char>(ThreadParts, text);
            else if (part == '米')
                return Array.BinarySearch<char>(RiceParts, text);
            else if (part == '牛')
                return Array.BinarySearch<char>(CowParts, text);
            else if (part == '石')
                return Array.BinarySearch<char>(StoneParts, text);
            else if (part == '貝')
                return Array.BinarySearch<char>(ShellParts, text);
            else if (part == '足')
                return Array.BinarySearch<char>(AshiParts, text);
            else if (part == '車')
                return Array.BinarySearch<char>(ShaParts, text);
            else if (part == '酉')
                return Array.BinarySearch<char>(BottleParts, text);
            else if (part == '金')
                return Array.BinarySearch<char>(MetalParts, text);
            else if (part == '食')
                return Array.BinarySearch<char>(EatParts, text);
            else if (part == '馬')
                return Array.BinarySearch<char>(HorseParts, text);
            else if (part == 'B')
                return Array.BinarySearch<char>(FlagParts, text);
            else if(part == '田')
                return Array.BinarySearch<char>(FieldParts, text);
            else if (part == '舟')
                return Array.BinarySearch<char>(BoatParts, text);
            else if (part == '虫')
                return Array.BinarySearch<char>(MushiParts, text);
            else
                return Array.BinarySearch<char>(LeftMistakenLetters, text);
        }

        public static readonly char[] HighMistakenPriority = new char[]
        { 'い', 'か', 'が', 'は', 'ば', 'ぱ', 'ほ', 'ぼ', 'ぽ' };

        public static readonly char[] BladeParts = new char[]
        { '例', '倒', '側', '刈', '刊', '刎', '刑', '刖', '列', '判', '別', '利', '到', '制', '刷', '刹', '刺', '刻', '剃', '則', '削',
          '剔', '剕', '剖', '剛', '剣', '剤', '剥', '副', '剰', '割', '創', '劃', '劇', '劍', '劓', '捌', '渕', '測', '鯏', '鯯'};

        public static readonly char[] ForceParts = new char[]
        { '功', '助', '劫', '励', '効', '劾', '勁', '勃', '勅', '動', '勣', '勧' };

        public static readonly char[] ToParts = new char[]
        { '卦', '外', '掛', '朴' };

        public static readonly char[] QParts = new char[]
        { '卯', '印', '即', '却', '卸', '卿', '叩', '御', '抑', '柳', '聊', '脚' };

        public static readonly char[] MeasureParts = new char[]
        { '対', '封', '射', '尉', '村', '樹', '樹', '耐', '肘', '酎', '附', '鮒' };

        public static readonly char[] RightNushiParts = new char[]
        { '帥', '師', '鰤' };

        public static readonly char[] HairParts = new char[]
        { '形', '彩', '彫', '彬', '彭', '彰', '影', '杉' };

        public static readonly char[] RightFlagParts = new char[]
        { '娜', '揶', '梛', '耶', '那', '邦', '邨', '邪', '邱', '邸', '郁', '郊', '郎', '郡', '部', '郭', '郵', '郷', '都', '鄙', '鄭' };

        //public static readonly char[] MasterParts = new char[]
        //{ '孜', '撤', '攻', '放', '政', '故', '敏', '救', '敗', '教', '敢', '散', '敦', '敬', '数', '敵', '斂', '轍', '黴' };

        public static readonly char[] FeeParts = new char[]
        { '料', '斛', '斜' };

        public static readonly char[] RightMushiParts = new char[]
        { '蝕', '融', '触' };

        public static readonly char[] TeParts = new char[]
        { '術', '街', '衝', '衞', '衡', '衢', '裄', '銜', '鵆' };

        public static readonly char[] HeadParts = new char[]
        { '順', '頌', '預', '頑', '頒', '頓', '領', '頚', '頤', '頬', '頭', '頴', '頸', '頻', '頼', '顆', '顋', '額', '顎', '顏', '顔', '顕', '願', '類', '顧' };

        public static readonly char[] SpoonParts = new char[]
        { '北', '枇', '此', '比', '眦', '秕', '粃', '紕', '訛', '靴' };

        public static readonly char[] RightMoonParts = new char[]
        { '嘲', '朋', '朔', '朗', '朝', '期', '棚', '糊', '胡', '醐' };

        public static readonly char[] RightMistakenLetters = new char[] 
        { 'お', 'か', 'が', 'ぎ', 'ぐ', 'げ', 'ご', 'ざ', 'じ', 'ず', 'ぜ', 'ぞ', 'だ', 'ぢ', 'づ', 'で', 'ど', 'ば', 'び', 'ぴ', 'ふ', 'ぶ', 'ぷ', 'べ', 'ぼ',
          'む', 'り', 'ゔ', 'ガ', 'ギ', 'グ', 'ゲ', 'ゴ', 'ザ', 'ジ', 'ズ', 'ゼ', 'ゾ', 'ダ', 'ヂ', 'ヅ', 'デ', 'ド', 'ハ', 'バ', 'パ', 'ビ', 'ブ', 'プ', 'ベ',
          'リ', 'ヴ', 'ヷ', '乱', '乳', '伽', '俳', '働', '凱', '加', '卵', '和', '報', '孔', '孵', '小', '巛', '川', '州', '引', '弼', '心', '承', '挑', '排',
          '搬', '斡', '朧', '枷', '桃', '椚', '榊', '洲', '渊', '湘', '溺', '瀧', '珈', '琲', '畝', '眺', '粥', '緋', '緋', '群', '羽', '翔', '翔', '翻', '翻',
          '肌', '胤', '腓', '膨', '艶', '蜘', '蝦', '訓', '誹', '跳', '辯', '酬', '釧', '銚', '鍛', '門', '馴', '鯡', '鰕', '鰯', '鰰' };

        public const int GON_PARTS_START = 35330;
        public const int GON_PARTS_END = 35715;

        public const int ARROW_PARTS_START = 30693;
        public const int ARROW_PARTS_END = 30703;

        public static readonly char[] KuchiParts = new char[]
        { '叱', '叶', '吃', '吋', '吐', '吟', '吠', '吭', '吸', '吹', '吻', '呪', '味', '呵', '呼', '咄', '咋', '咤', '咲', '咳', '咽', '哈', '哨',
          '哺', '唄', '唆', '唖', '唯', '唱', '唾', '啄', '喃', '喉', '喙', '喚', '喝', '喧', '喩', '喫', '喰', '嗔', '嗜', '嗟', '嗣', '嗽', '嘆',
          '嘔', '嘘', '嘩', '嘲', '嘴', '嘸', '噂', '噌', '噛', '噪', '噫', '噯', '噴', '噸', '噺', '嚇', '嚊', '嚏', '嚔', '囁', '囃', '鳴' };        

        public static readonly char[] HitoParts = new char[]
        { '什', '仁', '仇', '仏', '仔', '仕', '他', '付', '仙', '仞', '仟', '代', '仭', '仮', '仰', '仲', '件', '任', '伊', '伍', '伎', '伏', '伐',
          '休', '伜', '伝', '伯', '伴', '伶', '伸', '伺', '似', '伽', '佃', '但', '位', '低', '住', '佐', '佑', '体', '何', '佗', '佚', '佛', '作',
          '佞', '佩', '佰', '佳', '併', '使', '侃', '例', '侍', '侏', '侑', '侘', '供', '依', '侠', '価', '侭', '侮', '侯', '侵', '侶', '便', '係',
          '促', '俄', '俊', '俑', '俗', '俘', '保', '信', '俣', '俤', '俥', '修', '俯', '俳', '俵', '俸', '俺', '倅', '個', '倍', '倒', '候', '借',
          '倣', '値', '倩', '倫', '倭', '倶', '倹', '偈', '偉', '偏', '偕', '停', '健', '偬', '偲', '側', '偵', '偶', '偽', '傀', '傅', '傍', '傑',
          '備', '催', '傭', '傲', '傳', '債', '傷', '傾', '働', '像', '僑', '僕', '僚', '僧', '價', '僻', '儀', '儁', '儂', '億', '儒', '儕', '儘',
          '償', '儡', '優', '儲', '儺', '儼', '化', '條', '脩'}; 

        public static readonly char[] NushiParts = new char[] 
        { '帆', '帖', '帙', '帳', '帷', '帽', '幅', '幌', '幟', '幡', '幢'};

        public static readonly char[] WaterParts = new char[]
        { '冰', '冴', '冶', '冷', '凄', '准', '凋', '凌', '凍', '凛', '凜', '凝', '次', '氾', '汀', '汁', '汎', '汐', '汗', '汚', '汝', '江', '池',
          '汪', '汰', '汲', '決', '汽', '沃', '沈', '沌', '沖', '沙', '没', '沢', '沫', '河', '沸', '油', '治', '沼', '沿', '況', '泄', '泊', '泌',
          '法', '泡', '波', '泣', '泥', '注', '泪', '泳', '洋', '洒', '洗', '洙', '洛', '洞', '洟', '津', '洩', '洪', '洲', '洸', '活', '洽', '派',
          '流', '浄', '浅', '浙', '浜', '浦', '浩', '浪', '浬', '浮', '浴', '海', '浸', '消', '涌', '涎', '涕', '涙', '涛', '涜', '涯', '液', '涼',
          '淀', '淋', '淑', '淘', '淡', '淦', '淫', '深', '淳', '混', '添', '清', '渇', '済', '渉', '渊', '渋', '渓', '渕', '渚', '減', '渡', '渥',
          '渦', '温', '測', '港', '游', '渾', '湊', '湍', '湖', '湘', '湛', '湧', '湯', '湾', '湿', '満', '源', '溜', '溝', '溢', '溥', '溪', '溶',
          '溺', '滅', '滉', '滋', '滑', '滓', '滝', '滞', '滴', '漁', '漂', '漆', '漉', '漏', '演', '漕', '漠', '漢', '漣', '漫', '漬', '漱', '漸',
          '潔', '潘', '潜', '潟', '潤', '潭', '潮', '潰', '澁', '澄', '澗', '澤', '澪', '澱', '澳', '激', '濁', '濃', '濠', '濡', '濤', '濯', '濱',
          '濶', '瀋', '瀕', '瀞', '瀧', '瀬', '灘', '酒', '鴻' };

        public static readonly char[] OnnaParts = new char[]
        { '奴', '奸', '好', '如', '妃', '妊', '妓', '妖', '妙', '妨', '妹', '姉', '始', '姐', '姑', '姓', '姥', '姪', '姫', '姶', '姻', '娘', '娠',
          '娩', '娯', '娼', '婚', '婢', '婦', '婬', '婿', '媒', '媚', '媛', '媳', '媼', '嫁', '嫂', '嫉', '嫌' ,'嫗', '嫡', '嬉', '嬬', '嬶' };

        public static readonly char[] GoParts = new char[]　
        { '彷', '役', '彼', '往', '征', '径', '待', '律', '後', '徐', '徒', '従', '得', '御', '復', '循', '微', '徳', '徴', '徹', '徽',
          '行', '術', '街', '衛', '衝', '衞', '衡', '衢', '銜', '鵆', '黴'  };

        public static readonly char[] MoonParts = new char[]
        { '朧', '肋', '肌', '肘', '肚', '肛', '肝', '股', '肢', '肥', '肪', '肱', '肺', '胆', '胎', '胙', '胚', '胛', '胝', '胞', '胯', '胱', '胴',
          '胸', '脂', '脆', '脇', '脈', '脉', '脚', '脛', '脱', '脳', '脾', '腋', '腑', '腓', '腔', '腕', '腟', '腥', '腫', '腭', '腮', '腰', '腱',
          '腴', '腸', '腹', '腺', '腿', '膀', '膈', '膕', '膜', '膝', '膠', '膣', '膨', '膩', '膰', '膳', '膾', '膿', '臆', '臈', '臍', '臑', '臓',
          '臘', '臚', '謄', '豚', '騰', '鵬' };
                   
        public static readonly char[] SunParts = new char[]
        { '旺', '映', '昨', '昭', '時', '晒', '晦', '晧', '晩', '晰', '晴', '暇', '暉', '暎', '暖', '暗', '曉', '曖', '曙', '曜', '曝', '的', '皓' };

        public static readonly char[] EyeAndEarParts = new char[]
        { '恥', '眇', '眠', '眦', '眸', '眺', '眼', '睡', '睦', '瞋', '瞑', '瞞', '瞠', '瞬', '瞰', '瞳', '聡', '聯', '聰', '聴', '職'};

        public static readonly char[] TreeParts = new char[] 
        { '札', '朴', '杉', '村', '杖', '杞', '杵', '杷', '枝', '枠', '枢', '枯', '柄', '柊', '柏', '柚', '柯', '柱', '柵', '柿', '栖', '根', '桁',
          '桂', '框', '桐', '桔', '桜', '桧', '桶', '梓', '梗', '梢', '梱', '椅', '植', '椙', '検', '楠', '楢', '楫', '極', '槇', '槽', '槿', '標',
          '権', '樫', '樹', '檻', '櫃', '櫛', '櫟', '欄', '相' };

        public static readonly char[] HandAndDogParts = new char[]
        { '打', '払', '扠', '扨', '扮', '扶', '批', '技', '抄', '把', '抑', '抔', '投', '抗', '折', '抜', '択', '披', '抱', '抵', '抹', '押', '抽',
          '担', '拉', '拍', '拐', '拒', '拓', '拗', '拘', '拙', '招', '拝', '拡', '括', '拭', '拮', '拶', '拷', '拾', '持', '指', '挑', '挟', '挨',
          '挫', '振', '挽', '挿', '捉', '捌', '捏', '捕', '捗', '捜', '捧', '捨', '捲', '捷', '捺', '捻', '掃', '授', '排', '掖', '掘', '掛', '掟',
          '採', '探', '接', '控', '推', '措', '揃', '揄', '揆', '描', '提', '揖', '換', '握', '揮', '援', '揺', '損', '搭', '携', '搾', '摂', '摑',
          '摘', '摺', '撒', '撓', '撤', '撥', '撫', '播', '撰', '撲', '擁', '擅', '操', '擒', '擦',   

          '犯', '状', '狂', '狄', '狆', '狐', '狗', '狙', '狛', '狡', '狢', '狩', '独', '狭', '狸', '狼', '猛', '猟', '猥', '猪', '猫', '献', '猴',
          '猶', '猿', '獄', '獅', '獏', '獨', '獲', '獺' };

        public static readonly char[] OuParts = new char[]
        { '玖', '玩', '玲', '珀', '珂', '珈', '珍', '珠', '班', '珮', '現', '球', '理', '琉', '琢', '琥', '琲', '琳', '琿', '瑕', '瑛', '瑞',
          '瑠', '璃', '璞', '環', '瓊' };

        public static readonly char[] GroundParts = new char[]
        { '地', '坂', '均', '坊', '坎', '坏', '坤', '坦', '坪', '垓', '垢', '垣', '埃', '埋', '城', '埒', '埓', '域', '埠', '埴', '執', '培', '埼',
          '堀', '堆', '堋', '堤', '堪', '堰', '場', '堵', '堺', '塀', '塊', '塒', '塔', '塙', '塚', '塩', '填', '境', '増', '墳', '墺', '壇', '壊',
          '壌', '壕', '壜'};

        public static readonly char[] SanParts = new char[]
        { '岐', '岬', '峙', '峠', '峡', '峰', '峻', '峽', '崎', '嵯', '嶋', '嶮' };

        public static readonly char[] BowParts = new char[]
        { '弘', '弥', '弦', '弧', '弭', '弱', '張', '強', '弼', '弾', '彌', '粥' };

        public static readonly char[] FireParts = new char[]
        { '灯', '炉', '炊', '炬', '炯', '炳', '烟', '焔', '焼', '煉', '煖', '煙', '煤', '煩', '煽', '熄', '熾', '燈', '燐', '燗', '燠', '燥' ,'燧',
          '燭', '燻', '爆', '爛', '畑'};

        public static readonly char[] ThreadParts = new char[]
        { '糺', '糾', '紀', '約', '紅', '紆', '紋', '納', '紐', '純', '紕', '紗', '紘', '紙', '級', '紛', '紡', '紬', '細', '紲', '紳', '紹', '終',
          '絃', '組', '絆', '経', '結', '絞', '絡', '絢', '絣', '給', '統', '絲', '絵', '絶', '絹', '絽', '經', '継', '続', '綛', '綜', '綟', '綬',
          '維', '綱', '網', '綴', '綺', '綻', '綾', '綿', '緋', '総', '緑', '緒', '線', '締', '編', '緩', '緬', '緯', '練', '縁', '縄', '縅', '縛',
          '縞', '縡', '縦', '縫', '縮', '縹', '總', '績', '繃', '繕', '繞', '繦', '繰', '纓', '纔'};

        public static readonly char[] RiceParts = new char[]
        { '籵', '籾', '粁', '粃', '粉', '粋', '粍', '粕', '粘', '粧', '粮', '粳', '粽', '精', '糀', '糊', '糎', '糟', '糧', '糯', };

        public static readonly char[] CowParts = new char[]
        { '牝', '牡', '牧', '物', '牲', '特', '犠', '犢' };

        public static readonly char[] StoneParts = new char[]
        { '砂', '砌', '研', '砕', '砥', '砧', '砲', '破', '砺', '砿', '硝', '硫', '硬', '硯', '硴', '碇', '碌', '碍', '碑', '碓', '碕', '碗', '碩',
          '確', '碼', '磁', '磋', '磔', '磧', '磯', '礁', '礎', '礒', '礫' };

        public static readonly char[] ShellParts = new char[]
        { '財', '販', '貯', '貼', '賂', '賄', '賊', '賜', '賠', '賤', '賦', '賭', '購', '贈' };

        public static readonly char[] AshiParts = new char[]
        { '趾', '跋', '跛', '距', '跡', '跣', '路', '跳', '践', '踊', '踏', '踝', '踵', '蹄', '蹊', '蹟', '蹲', '蹴', '蹼', '躇', '躊', '躍', };

        public static readonly char[] ShaParts = new char[]
        { '軌','軒','軟','転','軫','軸','軽','較','輌','輒','輔','輛','輪','輸','輻','轄','轅','轌','轍' };

        public static readonly char[] BottleParts = new char[]
        { '酌', '配', '酎', '酒', '酔', '酖', '酢', '酣', '酥', '酪', '酬', '酵', '酷', '酸', '醋', '醍', '醐', '醒', '醜', '醢', '醨', '醪', '醴', '釀' };

        public static readonly char[] MetalParts = new char[]
        { '釘', '針', '釣', '釧', '釵', '鈍', '鈎', '鈔', '鈕', '鈴', '鈹', '鉄', '鉈', '鉉', '鉋', '鉛', '鉞', '鉢', '鉤', '鉾', '銀', '銃', '銅', '銑', '銘',
          '銚', '銛', '銭', '鋒', '鋤', '鋩', '鋪', '鋭', '鋳', '鋼', '錆', '錢', '錦', '錨', '錫', '錬', '錯', '錻', '鍋', '鍔', '鍛', '鍰', '鍼', '鍾', '鎌',
          '鎔', '鎖', '鎗', '鎧', '鎬', '鎮', '鎹', '鏈', '鏑', '鏝', '鏡', '鐔', '鐘', '鐙', '鐶', '鐸', '鐺', '鑑', '鑓', '鑛', '鑞', '鑰', '鑵', '鑽' };

        public static readonly char[] EatParts = new char[]
        { '飢', '飯', '飲', '飴', '飼', '飽', '飾', '餅', '餌', '餓', '餞', '餠', '餡', '館' };

        public static readonly char[] HorseParts = new char[]
        { '馳', '馴', '駁', '駄', '駅', '駆', '駐', '駒', '駮', '駿', '騎', '騒', '験', '騨' };

        public static readonly char[] FlagParts = new char[]
        { '阪', '防', '阻', '阿', '陀', '附', '陌', '降', '限', '陛', '院', '陣', '除', '陥', '陪', '陰', '陳', '陵', '陶', '陸', '険', '陽', '隅', '隆', '隈', '隊',
          '隋', '階', '随', '隔', '隕', '隙', '際', '障', '隠', '隣' };

        public static readonly char[] FieldParts = new char[]
        { '町', '畔', '略', '畦', '畷' };

        public static readonly char[] BoatParts = new char[]
        { '舩', '航', '舫', '般', '舳', '舵', '舶', '舷', '舸', '船', '艇', '艘', '艦' };

        public static readonly char[] MushiParts = new char[] 
        { '虹', '虻', '蚊', '蚋', '蚌', '蚫', '蚶', '蛆', '蛇', '蛎', '蛙', '蛛', '蛟', '蛤', '蛭', '蛯', '蛸', '蛹', '蛻', '蛾', '蜂', '蜆', '蜘', '蜩', '蜷', '蝉',
          '蝋', '蝗', '蝟', '蝦', '蝮', '蝶', '蝿', '螭', '螺', '螻', '蟆', '蟎', '蟻' };

        public static readonly char[] LineParts = new char[]
        { 'け', 'げ', 'に', 'は', 'ば', 'ぱ', 'ふ', 'ぶ', 'ぷ', 'ほ', 'ぼ', 'ぽ', 'り', 'ソ', 'ゾ', 'ツ', 'ヅ', 'ハ', 'バ', 'パ', 'リ', 'ル', '小', '川', '州', '帰', '心',
          '必', '忙', '快', '怖', '怜', '性', '怪', '怯', '恒', '恨', '恬', '悌', '悔', '悟', '悦', '悩', '悴', '悼', '情', '惇', '惚', '惜', '惟', '惧', '惨', '愉', '愕',
          '慄', '慌', '慎', '慢', '慣', '慨', '憎', '憐', '憤', '憧', '憶', '憾', '懐', '懶', '懷', '旧', '胤', '順' };

        public static readonly char[] LeftMistakenLetters = new char[] 
        { 'か', 'が', 'け', 'げ', '剕', '博', '卿', '壮', '孵', '将', '小', '少', '巛', '川', '州', '帥', '帰', '心', '必', '忙', '快', '怖', '怜', '性', '怪', '怯', '恒', '恨',
          '恬', '悌', '悔', '悟', '悦', '悩', '悴', '悼', '情', '惇', '惚', '惜', '惟', '惧', '惨', '愉', '愕', '慄', '慌', '慎', '慢', '慣', '慨', '憎', '憐', '憤', '憧', '憶',
          '憾', '懐', '懶', '懷', '牆', '状', '的', '皓', '礼', '祖', '神', '祠', '禅', '禊', '禍', '福', '禧', '秤', '移', '稜', '種', '稲', '穎', '耗', '能', '蛇', '蟎', '袖',
          '覗', '解', '躬', '辞', '郷', '野', '雌', '非', '預', '鶸', '龍' };
    }
}
