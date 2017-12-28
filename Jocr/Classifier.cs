using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jocr
{
    public static class Classifier
    {        
        private const int FIRST_STAGE_DIMENSION = 30;
        private const int FIRST_STAGE_NCLASS = 300;
        private const int FIRST_STAGE_FINAL_INDEX = QuadraticDiscriminant.NUMBER_OF_CLASS - 1;

        private const int SECOND_STAGE_K_DIMENSION = 10;
        private const int SECOND_STAGE_NCLASS = 20;
        private const int SECOND_STAGE_FINAL_INDEX = FIRST_STAGE_NCLASS - 1;
        private const int RESULT_INIT_CAPACITY = SECOND_STAGE_NCLASS + PostProcess.MAX_POST_PROCESSING;

        private const int THIRD_STAGE_FINAL_INDEX = SECOND_STAGE_NCLASS - 1;
        
        private static ushort[] ClassIndex = new ushort[QuadraticDiscriminant.NUMBER_OF_CLASS];

        private static float[] FirstStageDistance = new float[QuadraticDiscriminant.NUMBER_OF_CLASS];        
        private static float[] SecondStageDistance = new float[FIRST_STAGE_NCLASS];
        private static float[] ThirdStageDistance = new float[SECOND_STAGE_NCLASS];

        private static bool isPreviousMerge = false;
        private static bool isHorizontalText = false;
        private static List<TextBlock> blocks;
        private static GrayImage image;             

        private static readonly char[] Symbols = new char[]
        { '。', '々', '？', '～', '｢', '｣' };

        public static TextBlock ClassifyOneText(GrayImage grayImage)
        {
            isHorizontalText = true;
            image = grayImage;
            TextBlock block = InitOneTextBlock();
            block.Type = TextBlockType.Single;
            block.IsNeedOCRProcess = false;
            block.Ratio = 1;
            block.HeighRatio = 1;            

            ClassifyTextBlock(block);
            return block;
        }

        private static TextBlock InitOneTextBlock()
        {
            int left = image.Width - 1;
            int right = 0;
            int top = image.Height - 1;
            int bottom = 0;

            int index = 0;
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    if (image.Pixels[index] > 0)
                    {
                        if (j < left)
                            left = j;
                        if (j > right)
                            right = j;
                        if (i < top)
                            top = i;
                        if (i > bottom)
                            bottom = i;
                    }
                    index++;
                }
            }

            GetOutLinePixelsPosition(image, ref left, ref right, ref top, ref bottom);

            TextBlock block = new TextBlock();
            block.Top = top;
            block.Bottom = bottom;
            block.Left = left;
            block.Right = right;
            block.Height = bottom - top + 1;
            block.Width = right - left + 1;
            return block;
        }

        private static void GetOutLinePixelsPosition(GrayImage image, ref int left, ref int right, ref int top, ref int bottom)
        {
            if (left > 0)
                left--;
            if (right < image.Width - 1)
                right++;
            if (top > 0)
                top--;
            if (bottom < image.Height - 1)
                bottom++;
        }

        public static void FindTextOfTextBlocks(GrayImage grayImage, List<TextBlock> textBlocks, bool isHorizontalTextLayout)
        {
            blocks = textBlocks;
            image = grayImage;
            isHorizontalText = isHorizontalTextLayout;
            ProcessFirstBlock();

            int i;
            for (i = 1; i < blocks.Count - 1; i++)
            {
                if (blocks[i].IsNeedOCRProcess)
                    ProcessPotentialMergeBlock(ref i);
                else
                {
                    ClassifyTextBlock(blocks[i]);
                    isPreviousMerge = false;
                }
            }

            if(i < blocks.Count)
                ProcessFinalBlock();

           PostProcess.Start(blocks, isHorizontalText);
        }

        private static void ProcessFirstBlock()
        {
            bool isSymbol = TryClassifyAsSymbol(blocks[0]);
            if (!isSymbol && blocks[0].IsNeedOCRProcess && blocks.Count > 1)
            {
                TextBlock mergeNextBlock = null;
                bool isPreviousMergeValid = false;
                bool isMergeValid = TryMergeToNextBlock(0, ref mergeNextBlock, ref isPreviousMergeValid);                
                if (isMergeValid)
                {
                    blocks.RemoveAt(0);
                    blocks[0] = mergeNextBlock;                    
                }
            }
        }

        private static void ProcessFinalBlock()
        {
            int lastIndex = blocks.Count - 1;
            bool isSymbol = TryClassifyAsSymbol( blocks[lastIndex]);         
            bool isPreviousBlockNotProcess = blocks.Count > 1 && blocks[lastIndex - 1].Type > TextBlockType.Half;            
            if (!isSymbol && blocks[lastIndex].IsNeedOCRProcess && isPreviousBlockNotProcess)
            {
                TextBlock mergePreviousBlock = null;
                bool isMergeValid = TryMergeToPreviousBlock(lastIndex, ref mergePreviousBlock);
                if (isMergeValid)
                {
                    blocks.RemoveAt(lastIndex);
                    blocks[lastIndex - 1] = mergePreviousBlock;
                }
            }
        }

        private static void ProcessPotentialMergeBlock(ref int i)
        {
            if (TryClassifyAsSymbol(blocks[i]))
                return;

            TextBlock mergePreviousBlock = null;
            bool isPreviousMergeValid = TryMergeToPreviousBlock( i, ref mergePreviousBlock);

            TextBlock mergeNextBlock = null;            
            bool isNextMergeValid = TryMergeToNextBlock(i, ref mergeNextBlock, ref isPreviousMergeValid);

            if (isPreviousMergeValid && isNextMergeValid)
            {
                DetermineMergeDirection(i, mergePreviousBlock, ref isPreviousMergeValid, mergeNextBlock, ref isNextMergeValid);
            }

            if (isPreviousMergeValid)
            {
                JoinPreviousMergeBlockToListBlock(ref i, mergePreviousBlock);
                return;
            }

            if (isNextMergeValid)
            {
                JoinNextMergeBlockToListBlock(i, mergeNextBlock);
                return;
            }

            if(!isHorizontalText)
            {
                if (VerticalTextProjection.GetMaxWidthPostionPercentToNeightbor(blocks, i) > VerticalTextProjection.MARK_WIDTH_POSITION_PERCENT)
                {
                    var lineIndex = blocks[i].LineIndex;
                    var avgGapIntra = VerticalTextProjection.AvgGapIntraChars[lineIndex];
                    if (lineIndex == blocks[i - 1].LineIndex && blocks[i].Top - blocks[i - 1].Bottom <= avgGapIntra && mergePreviousBlock != null)
                    {
                        JoinPreviousMergeBlockToListBlock(ref i, mergePreviousBlock);
                        return;
                    }
                    else if (lineIndex == blocks[i + 1].LineIndex && blocks[i + 1].Top - blocks[i].Bottom <= avgGapIntra && mergeNextBlock != null)
                    {
                        JoinNextMergeBlockToListBlock(i, mergeNextBlock);
                        return;
                    }
                }
            }

            isPreviousMerge = false;
        }

        private static void DetermineMergeDirection(int i, TextBlock mergePreviousBlock, ref bool isPreviousMergeValid, TextBlock mergeNextBlock, ref bool isNextMergeValid)
        {
            if (isHorizontalText)
            {
                DetermineHorizontalDirection(i, mergePreviousBlock, ref isPreviousMergeValid, mergeNextBlock, ref isNextMergeValid);
            }
            else
            {
                DetermineVerticalDirection(i, mergePreviousBlock, ref isPreviousMergeValid, mergeNextBlock, ref isNextMergeValid);
            }
        }       

        private static void DetermineHorizontalDirection(int i, TextBlock mergePreviousBlock, ref bool isPreviousMergeValid, TextBlock mergeNextBlock, ref bool isNextMergeValid)
        {
            bool isLeftInHigh = Array.BinarySearch(HorizontalTextProjection.HighMistakenPriority, mergePreviousBlock.Text[0]) > -1;
            bool isRightInHigh = Array.BinarySearch(HorizontalTextProjection.HighMistakenPriority, mergeNextBlock.Text[0]) > -1;
            if (isLeftInHigh != isRightInHigh)
            {
                if (isLeftInHigh)
                    isNextMergeValid = false;
                else
                    isPreviousMergeValid = false;
            }
            else
            { //Always prriority previous merge because we process from left to right or top to bottom

                if (blocks[i - 1].Type < blocks[i + 1].Type)
                    isNextMergeValid = false;
                else
                {
                    int gapIJ = blocks[i].Left - blocks[i - 1].Right;
                    int gapJK = blocks[i + 1].Left - blocks[i].Right;

                    if (blocks[i - 1].Type > blocks[i + 1].Type &&
                            (blocks[i + 1].Type == TextBlockType.Half || blocks[i].Type == TextBlockType.Half))
                        isPreviousMergeValid = false;
                    else if (gapIJ < gapJK)
                        isNextMergeValid = false;
                    else if (gapIJ > gapJK)
                        isPreviousMergeValid = false;
                    else if (blocks[i - 1].Width <= blocks[i + 1].Width)
                        isNextMergeValid = false;
                    else
                        isPreviousMergeValid = false;
                }

            }
        }

        private static void DetermineVerticalDirection(int i, TextBlock mergePreviousBlock, ref bool isPreviousMergeValid, TextBlock mergeNextBlock, ref bool isNextMergeValid)
        {
            if (mergePreviousBlock.Text[0] != '？' && blocks[i].Width / blocks[i - 1].Width < 0.6)
            {
                isPreviousMergeValid = false;
            }
            else if (mergePreviousBlock.Text[0] == '？' && blocks[i].Width / blocks[i - 1].Width >= 0.6)
            {
                isPreviousMergeValid = false;
            }
            else
            {
                bool isPreviousInHigh = Array.BinarySearch(VerticalTextProjection.HighPriorityMistakenList, mergePreviousBlock.Text[0]) > -1;
                bool isNextInHigh = Array.BinarySearch(VerticalTextProjection.HighPriorityMistakenList, mergeNextBlock.Text[0]) > -1;
                if (isPreviousInHigh && !isNextInHigh)
                {
                    isNextMergeValid = false;
                }
                else if (!isPreviousInHigh && isNextInHigh)
                {
                    isPreviousMergeValid = false;
                }
                else
                {
                    if (blocks[i - 1].Type >= blocks[i + 1].Type) //priority next merge
                        isPreviousMergeValid = false;
                    else
                        isNextMergeValid = false;
                }
            }
        }

        private static void JoinPreviousMergeBlockToListBlock(ref int i, TextBlock mergePreviousBlock)
        {
            InsertRange(mergePreviousBlock.Text, PostProcess.MAX_POST_PROCESSING, blocks[i - 1].Text, PostProcess.MAX_POST_PROCESSING);
            blocks.RemoveAt(i);
            blocks[i - 1] = mergePreviousBlock;
            i--;
            isPreviousMerge = true;
            return;
        }

        private static void JoinNextMergeBlockToListBlock(int i, TextBlock mergeNextBlock)
        {
            blocks.RemoveAt(i);
            if (blocks[i].Text == null)
                ClassifyTextBlock(blocks[i]);
            InsertRange(mergeNextBlock.Text, PostProcess.MAX_POST_PROCESSING, blocks[i].Text, PostProcess.MAX_POST_PROCESSING);
            blocks[i] = mergeNextBlock;
            isPreviousMerge = true;
            return;
        }

        private static bool TryMergeToPreviousBlock(int i, ref TextBlock mergePreviousBlock)
        {
            bool isCanMerge = IsCanMergeBlock(i, blocks[i].Text[0]);
            bool isPreviousMergeValid = false;
            if (!isPreviousMerge && blocks[i - 1].LineIndex == blocks[i].LineIndex && isCanMerge)
            {
                mergePreviousBlock = ClassifyMergeBlock(i - 1);
                isPreviousMergeValid = IsCanCombineToPrevious(mergePreviousBlock, blocks[i].Text[0]);
            }

            return isPreviousMergeValid;
        }

        private static bool TryMergeToNextBlock(int i, ref TextBlock mergeNextBlock, ref bool isPreviousMergeValid)
        { 
            if(blocks[i + 1].LineIndex != blocks[i].LineIndex)
                return false;

            mergeNextBlock = null;
            bool isNextMergeValid = false;
            bool isCanMerge = IsCanMergeBlock(i + 1, blocks[i].Text[0]);
            if (isCanMerge)
            {
                mergeNextBlock = ClassifyMergeBlock(i);

                if (isHorizontalText)
                {
                    ClassifyTextBlock(blocks[i + 1]);
                    if (blocks[i].Text[0] == 'し')
                    {
                        if (mergeNextBlock.Text[0] == 'い' && (blocks[i + 1].Text[0] == '、' || IsVerticalLineChar(blocks[i + 1].Text[0])))
                        {
                            isPreviousMergeValid = false;
                            isNextMergeValid = true;
                        }
                    }
                    else if (IsVerticalLineChar(blocks[i].Text[0]) && IsVerticalLineChar(blocks[i + 1].Text[0])
                            && blocks[i + 1].IsNeedOCRProcess && i + 2 < blocks.Count)
                    {
                        var temp = mergeNextBlock;
                        mergeNextBlock = TextBlock.CreateMergeHorizontalTextBlock(mergeNextBlock, blocks[i + 2]);
                        ClassifyTextBlock(mergeNextBlock);
                        isNextMergeValid = Array.BinarySearch<char>(HorizontalTextProjection.LeftMistakenLetters, mergeNextBlock.Text[0]) > -1;
                        if (isNextMergeValid)
                        {
                            blocks.RemoveAt(i);
                            blocks[i] = temp;
                        }
                        else
                            mergeNextBlock = temp;
                    }
                    else
                        isNextMergeValid = IsCanCombineToNext(mergeNextBlock, blocks[i].Text[0]);
                }
                else
                {
                    isNextMergeValid = IsCanCombineToNext(mergeNextBlock, blocks[i].Text[0]);
                    if(isNextMergeValid 
                        && (mergeNextBlock.Text[0] == 'こ'                             
                            || mergeNextBlock.Text[0] == 'ご' 
                            || mergeNextBlock.Text[0] == 'ニ'
                            || mergeNextBlock.Text[0] == '二' ))
                    {
                        if (blocks[i].Width < 0.5f * blocks[i + 1].Width)
                            return false;

                        VerticalTextProjection.FindBlockType(mergeNextBlock);                    
                        var index = i + 2;
                        if (mergeNextBlock.Type < TextBlockType.Single 
                            && index < blocks.Count 
                            && blocks[index].Type == TextBlockType.Mark 
                            && blocks[index].Width > 0.8f * mergeNextBlock.Width)
                        {
                            var temp = mergeNextBlock;
                            mergeNextBlock = TextBlock.CreateMergeVerticalTextBlock(mergeNextBlock, blocks[i + 2]);
                            ClassifyTextBlock(mergeNextBlock);
                            if (mergeNextBlock.Text[0] == '三' || mergeNextBlock.Text[0] == 'ミ')
                            {
                                blocks.RemoveAt(i);
                                blocks[i] = temp;
                            }
                            else
                                mergeNextBlock = temp;
                        }
                    }
                }
            }
            else if(!isHorizontalText)
            {
                ProcessSpecialCasesInVerticalLayout(i, ref mergeNextBlock, ref isNextMergeValid);
            }

            return isNextMergeValid;
        }

        private static void ProcessSpecialCasesInVerticalLayout(int i, ref TextBlock mergeNextBlock, ref bool isNextMergeValid)
        {
            if (blocks[i].Text[0] == '一' && (blocks[i + 1].Top - blocks[i].Bottom < 3 * VerticalTextProjection.AvgGapBlocks[blocks[i].LineIndex]))
            {
                ClassifyTextBlock( blocks[i + 1]);
                if (blocks[i + 1].Text[0] == '一')
                {
                    mergeNextBlock = ClassifyMergeBlock(i);
                    if (mergeNextBlock.Text[0] == '二' || mergeNextBlock.Text[0] == 'ニ')
                    {
                        isNextMergeValid = true;
                    }
                }
            }
        }

        private static bool TryClassifyAsSymbol(TextBlock block)
        {
            ClassifyTextBlock(block);
            if (Array.BinarySearch<char>(Symbols, block.Text[0]) > -1)
                return true;
            else
                return false;            
        }

        private static void InsertRange(List<char> texts, int startIndex, List<char> textsToInsert, int length)
        {
            texts.InsertRange(startIndex, textsToInsert.GetRange(0, length));
        }

        private static bool IsCanMergeBlock(int i, char text)
        {
            int gap;
            var validGap = Math.Ceiling(CaluculateValidGap(i, text));
            if (isHorizontalText)
            {
                gap = blocks[i].Left - blocks[i - 1].Right;
                if (gap <= validGap)
                    return true;                
            }
            else
            {
                gap = blocks[i].Top - blocks[i - 1].Bottom;
                if (gap <= validGap)
                    return true;
            }
            return false;
        }

        private static float CaluculateValidGap(int blockIndex, char text)
        {            
            if (isHorizontalText)
            {
                float avgGap = HorizontalTextProjection.AvgGapBlocks[blocks[blockIndex].LineIndex];
                float percent = HorizontalTextProjection.GetMaxHeightPostionPercentToNeightbor(blocks, blockIndex);
                if (text == '、' || text == 'ｌ' || text == 'ノ')
                {
                    if (percent > HorizontalTextProjection.MARK_HEIGH_POSITION_PERCENT)
                        return 2f * avgGap;
                }
                else if (text == 'し')
                {
                    return (float)Math.Ceiling(avgGap * 2f);
                }
                else if (percent > HorizontalTextProjection.MARK_HEIGH_POSITION_PERCENT)
                    return 1.5f * avgGap;

                return avgGap;
            }
            else
            {
                float percent = VerticalTextProjection.GetMaxWidthPostionPercentToNeightbor(blocks, blockIndex);
                if (percent > VerticalTextProjection.MARK_WIDTH_POSITION_PERCENT)
                {
                    return VerticalTextProjection.AvgGapBlocks[blocks[blockIndex].LineIndex] * 1.5f;
                }
                else
                    return VerticalTextProjection.AvgGapBlocks[blocks[blockIndex].LineIndex];
            }
            
        }

        private static void ChangeToMergeBlock(int i, TextBlock mergeBlock)
        {
            blocks.RemoveAt(i);
            blocks[i] = mergeBlock;
            isPreviousMerge = true;
        }

        private static TextBlock ClassifyMergeBlock(int firstIndex)
        {
            var mergeBlock = TextBlock.CreateMergeTextBlock(blocks[firstIndex], blocks[firstIndex + 1], isHorizontalText);
            ClassifyTextBlock(mergeBlock);
            return mergeBlock;
        }

        private static bool IsCanCombineToNext(TextBlock block, char part)
        {
            int index = -1;
            if (isHorizontalText)            
                index = HorizontalTextProjection.KanjiLeftPartsDetection(part, block.Text[0]);            
            else
                index = Array.BinarySearch<char>(VerticalTextProjection.TopMistakenTextBlocks, block.Text[0]);

            if (index < 0)
                return false;
            else
                return true;
        }      

        private static bool IsCanCombineToPrevious(TextBlock block, char part)
        {
            int index = -1;
            if (isHorizontalText)
                index = HorizontalTextProjection.KanjiRightPartsDetection(part, block.Text[0]);
            else
            {
                if (block.Text[0] == '？')
                    return true;
                else
                    index = Array.BinarySearch<char>(VerticalTextProjection.BottomMistakenTextBlocks, block.Text[0]);
            }

            if (index < 0)
                return false;
            else
                return true;
        }

        private static void ClassifyTextBlock(TextBlock block)
        {            
            ShapeNormalization.LinearNormalization(image, block);
            Filter.GaussianFilter(ShapeNormalization.Pixels, ShapeNormalization.NormWidth, ShapeNormalization.NormHeigth);
            FeatureExtaction.HistogramOrientedGradient(ShapeNormalization.Pixels, ShapeNormalization.NormWidth, ShapeNormalization.NormHeigth);
            LdaVector.ReduceDimension();

            CalculateFirstStage();
            CalculateSecondStage();
            CalculateThirdStage();
            GetClassifiedResults(block);
        }

        private static void CalculateFirstStage()
        {
            Parallel.For(0, FirstStageDistance.Length, (i) =>
            {
                double temp = 0;
                int index = i * FeatureExtaction.FEATURE_REDUCE_SIZE;
                for (int j = 0; j < FIRST_STAGE_DIMENSION; j++)
                {
                    var diff = (FeatureExtaction.Features[j] - QuadraticDiscriminant.MEAN_VECTOR[index]);
                    temp += diff * diff;
                    index++;
                }
                FirstStageDistance[i] = (float)temp;
                ClassIndex[i] = (ushort)i;
            });
            QuickSelect.Start(FirstStageDistance, ClassIndex, 0, FIRST_STAGE_FINAL_INDEX, FIRST_STAGE_NCLASS);
        }

        private static void CalculateSecondStage()
        {
            CalculateMDQF(SecondStageDistance, SECOND_STAGE_K_DIMENSION);
            QuickSelect.Start(SecondStageDistance, ClassIndex, 0, SECOND_STAGE_FINAL_INDEX, SECOND_STAGE_NCLASS);
        }

        private static void CalculateThirdStage()
        {
            CalculateMDQF(ThirdStageDistance, QuadraticDiscriminant.K_DIMENSION);         
        }

        private static void CalculateMDQF(float[] distanceOut, int kDimension)
        {
            Parallel.For(0, distanceOut.Length, (i) =>
            {
                int qdfIndex = ClassIndex[i];
                int vectorIndex = qdfIndex * FeatureExtaction.FEATURE_REDUCE_SIZE;
                int eigvectorIndex = qdfIndex * QuadraticDiscriminant.CLASS_EIG_VECTOR_SIZE;
                double firstSum = 0;
                double[] eigDiff = new double[kDimension];
                double diff = 0;
                for (int j = 0; j < FeatureExtaction.Features.Length; j++)
                {
                    diff = FeatureExtaction.Features[j] - QuadraticDiscriminant.MEAN_VECTOR[vectorIndex];
                    firstSum += diff * diff;

                    int eigVectorColumnIndex = eigvectorIndex + j;
                    for (int k = 0; k < eigDiff.Length; k++)
                    {
                        eigDiff[k] += QuadraticDiscriminant.EIG_VECTOR[eigVectorColumnIndex] * diff / QuadraticDiscriminant.EIG_VECTOR_SCALE;
                        eigVectorColumnIndex += FeatureExtaction.FEATURE_REDUCE_SIZE;
                    }

                    vectorIndex++;
                }

                int eigValueIndex = qdfIndex * QuadraticDiscriminant.K_DIMENSION;
                double secondSum = 0;
                for (int k = 0; k < kDimension; k++, eigValueIndex++)
                {
                    secondSum += (1 - QuadraticDiscriminant.MINOR_EIG_VALUE / QuadraticDiscriminant.EIG_VALUE[eigValueIndex]) * (eigDiff[k] * eigDiff[k]);
                }

                distanceOut[i] = (float)((firstSum - secondSum) / QuadraticDiscriminant.MINOR_EIG_VALUE + QuadraticDiscriminant.SUM_LOG[qdfIndex]);
            });
        }

        private static void GetClassifiedResults(TextBlock block)
        {                        
            Array.Sort(ThirdStageDistance, ClassIndex);

            block.Text = new List<char>(RESULT_INIT_CAPACITY);
            for (int i = 0; i < SECOND_STAGE_NCLASS; i++)            
                block.Text.Add(CharMap.Unicode[ClassIndex[i]]);            
        }       

        public static bool IsVerticalLineChar(char text)
        {
            if (text == 'ｌ' || text == 'ｉ' || text == '！')
                return true;
            else
                return false;
        }  

    }
}
