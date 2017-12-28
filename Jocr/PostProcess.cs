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
    public static class PostProcess
    {
        private const float SMALL_BLOCK_RATIO_THRESHOLD = 0.8f;
        private const float TSU_HEIGHT_SCALE = 0.75f;
        private const float TSU_RATIO_SCALE = 0.8f;

        private const int UNICODE_ALPHABET_START = 62581;
        private const int UNICODE_HIRAGANA_START = 12354;
        private const int UNICODE_HIRAGANA_END = 12435;
        private const int UNICODE_KATAKANA_START = 12449;
        private const int UNICODE_KATAKANA_END = 12532;

        public const int MAX_POST_PROCESSING = 5;

        private static List<TextBlock> blocks;
        private static bool isHorizontalText;

        private static readonly char[] HiraganaBeforeSmallChars = new char[]
        { 'き', 'ぎ', 'し', 'じ', 'ち', 'ぢ', 'に', 'ひ', 'び', 'ぴ', 'み', 'り' };

        private static readonly char[] KatakanaBeforeSmallChars = new char[]
        { 'キ', 'ギ', 'シ', 'ジ', 'チ', 'ヂ', 'ニ', 'ヒ', 'ビ', 'ピ', 'ミ', 'リ' };

        private static readonly char[] SpecialKatakanaBeforeSmallChars = new char[]
        { 'イ', 'ウ', 'ク', 'シ', 'ジ', 'チ', 'ツ', 'テ', 'デ', 'ト', 'フ', 'ヴ' };

        private static readonly char[] PotentialKatakana = new char[]
        { 'ヘ', 'ベ', 'ペ', '一', '二', '力', '口', '夕', '工' };

        private static bool isPreviousHiraValid;
        private static bool isPreviousKataValid;
        private static bool isPreviousKataSpecialValid;

        private static float avgArea = 0;
        private static float avgHeight = 0;
        private static float count = 0;

        private static float heightKanaRatio;
        private static float areaKanaRatio;

        public static void Start(List<TextBlock> textBlocks, bool isHorizontalTextLayout)
        {
            blocks = textBlocks;
            isHorizontalText = isHorizontalTextLayout;
            PostProcessPotentialMisClassifiedChars();
            PostProcessSmallChars();
        }

        private static void PostProcessPotentialMisClassifiedChars()
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                for (int j = 0; j < MAX_POST_PROCESSING; j++)
                {
                    if (blocks[i].Text[j] == '力') //chikara
                    {
                        if (IsLeftOrRightKatakana(i))
                            SwapToNextClassifiedChars(blocks[i], j, 'カ');
                        break;
                    }
                    else if (blocks[i].Text[j] == '夕') //yuu
                    {
                        if (IsLeftOrRightKatakana(i))
                            SwapToNextClassifiedChars(blocks[i], j, 'タ');
                        break;
                    }
                    else if (blocks[i].Text[j] == '口') //kuchi
                    {
                        if (IsLeftOrRightKatakana(i))
                            SwapToNextClassifiedChars(blocks[i], j, 'ロ');
                        break;
                    }
                    else if (blocks[i].Text[j] == '二') //nichi
                    {
                        if (IsLeftOrRightKatakana(i))
                            SwapToNextClassifiedChars(blocks[i], j, 'ニ');
                        break;
                    }
                    else if (blocks[i].Text[j] == 'へ') //he
                    {
                        if (IsLeftOrRightKatakana(i))
                            SwapToNextClassifiedChars(blocks[i], j, 'ヘ');
                        break;
                    }
                    else if (blocks[i].Text[j] == 'べ') //he
                    {
                        if (IsLeftOrRightKatakana(i))
                            SwapToNextClassifiedChars(blocks[i], j, 'ベ');
                        break;
                    }
                    else if (blocks[i].Text[j] == 'ぺ') //he
                    {
                        if (IsLeftOrRightKatakana(i))
                            SwapToNextClassifiedChars(blocks[i], j, 'ペ');
                        break;
                    }
                    else if (blocks[i].Text[j] == '工') //kou リ
                    {
                        if (IsLeftOrRightKatakana(i))
                            SwapToNextClassifiedChars(blocks[i], j, 'エ');
                        break;
                    }
                    else if (blocks[i].Text[0] == 'り') //ri
                    {
                        if (IsLeftOrRightKatakana(i))
                            SwapToNextClassifiedChars(blocks[i], j, 'リ');
                        break;
                    }
                    else if (IsKatakana(blocks[i].Text[j]))
                    {
                        if (blocks[i].Text[j] == 'カ') //ka
                        {
                            if (!IsLeftOrRightKatakana(i))
                                SwapToNextClassifiedChars(blocks[i], j, '力');
                            break;
                        }
                        else if (blocks[i].Text[j] == 'タ') //ta
                        {
                            if (!IsLeftOrRightKatakana(i))
                                SwapToNextClassifiedChars(blocks[i], j, '夕');
                            break;
                        }
                        else if (blocks[i].Text[j] == 'ロ') //ro
                        {
                            if (!IsLeftOrRightKatakana(i))
                                SwapToNextClassifiedChars(blocks[i], j, '口');
                            break;
                        }
                        else if (blocks[i].Text[j] == 'コ') //ko
                        {
                            if (!IsLeftOrRightKatakana(i))
                                SwapToNextClassifiedChars(blocks[i], j, 'つ');
                            break;
                        }
                        else if (blocks[i].Text[j] == 'ニ') //ni
                        {
                            if (!IsLeftOrRightKatakana(i))
                                SwapToNextClassifiedChars(blocks[i], j, '二');
                            break;
                        }
                        else if (blocks[i].Text[j] == 'ヘ') //he
                        {
                            if (!IsLeftOrRightKatakana(i))
                                SwapToNextClassifiedChars(blocks[i], j, 'へ');
                            break;
                        }
                        else if (blocks[i].Text[j] == 'ベ') //he
                        {
                            if (!IsLeftOrRightKatakana(i))
                                SwapToNextClassifiedChars(blocks[i], j, 'べ');
                            break;
                        }
                        else if (blocks[i].Text[j] == 'ペ') //he
                        {
                            if (!IsLeftOrRightKatakana(i))
                                SwapToNextClassifiedChars(blocks[i], j, 'ぺ');
                            break;
                        }
                        else if (blocks[i].Text[j] == 'エ') //e
                        {
                            if (!IsLeftOrRightKatakana(i))
                                SwapToNextClassifiedChars(blocks[i], j, '工');
                            break;
                        }
                        else if (blocks[i].Text[0] == 'リ') //ri
                        {
                            if (!IsLeftOrRightKatakana(i))
                                SwapToNextClassifiedChars(blocks[i], j, 'り');
                            break;
                        }
                        else if (blocks[i].Text[j] == 'ク' && !IsLeftOrRightKatakana(i)) //ku
                        {
                            for (int subIndex = j + 1; subIndex < MAX_POST_PROCESSING; subIndex++)
                            {
                                if (blocks[i].Text[subIndex] == 'つ')
                                {
                                    blocks[i].Text[j] = blocks[i].Text[subIndex];
                                    blocks[i].Text[subIndex] = 'ク';
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    else if (blocks[i].Text[j] == '一')
                    {
                        if (isHorizontalText && i > 0 && IsKatakana(blocks[i - 1].Text[0]))
                        {
                            blocks[i].Text.Insert(j, 'ー');
                            break;
                        }
                    }
                    else if (Classifier.IsVerticalLineChar(blocks[i].Text[j]) && !isHorizontalText && i > 0 && IsKatakana(blocks[i - 1].Text[0]))
                    {
                        blocks[i].Text.Insert(j, 'ー');
                        break;
                    }
                    else if (blocks[i].Text[j] == '氵')
                    {
                        blocks[i].Text[j] = 'シ';
                        break;
                    }
                    else if (blocks[i].Text[j] == '冫')
                    {
                        blocks[i].Text[j] = 'ソ';
                        break;
                    }
                    else if (blocks[i].Text[j] == '扌')
                    {
                        blocks[i].Text[j] = '才';
                        break;
                    }
                }
            }
        }

        private static bool IsLeftOrRightKatakana(int i)
        {
            bool isLeftOrRightKatakana = i > 0
                                         && (IsKatakana(blocks[i - 1].Text[0])
                                            || (IsPotentialKatakana(blocks[i - 1].Text[0]) && blocks[i - 1].Text[0] != '一'));
            isLeftOrRightKatakana = isLeftOrRightKatakana
                                    || (i < blocks.Count - 1
                                            && (IsKatakana(blocks[i + 1].Text[0])
                                                || IsPotentialKatakana(blocks[i + 1].Text[0])));
            return isLeftOrRightKatakana;
        }

        private static bool IsKatakana(char letter)
        {
            if (letter >= UNICODE_KATAKANA_START && letter <= UNICODE_KATAKANA_END)
                return true;
            else if (letter == 'ー')
                return true;

            return false;
        }

        private static bool IsPotentialKatakana(char letter)
        {
            int index = Array.BinarySearch(PotentialKatakana, letter);
            if (index > -1)
                return true;
            else
            {
                if (!isHorizontalText && Classifier.IsVerticalLineChar(letter))
                    return true;

                return false;
            }
        }

        private static void SwapToNextClassifiedChars(TextBlock block, int index, params char[] charArray)
        {
            if (charArray.Length == 1)
            {
                var c = charArray[0];
                for (int i = index + 1; i < MAX_POST_PROCESSING; i++)
                {
                    if (block.Text[i] == c)
                    {
                        block.Text[i] = block.Text[index];
                        block.Text[index] = c;
                        return;
                    }
                }
            }
            else
            {
                for (int i = index + 1; i < MAX_POST_PROCESSING; i++)
                {
                    for (int j = 0; j < charArray.Length; j++)
                    {
                        if (block.Text[i] == charArray[j])
                        {
                            block.Text[i] = block.Text[index];
                            block.Text[index] = charArray[j];
                            return;
                        }
                    }
                }
            }
        }

        private static void PostProcessSmallChars()
        {
            GetKanaAverageBlockSizes();

            ProcessFirstSmallBlock();

            for (int blockIndex = 1; blockIndex < blocks.Count; blockIndex++)
            {
                var block = blocks[blockIndex];
                if (count > 1 && block.Ratio <= SMALL_BLOCK_RATIO_THRESHOLD)
                {
                    heightKanaRatio = block.Height / avgHeight;
                    areaKanaRatio = (block.Width * block.Height) / avgArea;
                  
                    isPreviousHiraValid = Array.BinarySearch(HiraganaBeforeSmallChars, blocks[blockIndex - 1].Text[0]) > -1;
                    isPreviousKataValid = Array.BinarySearch(KatakanaBeforeSmallChars, blocks[blockIndex - 1].Text[0]) > -1;
                    isPreviousKataSpecialValid = Array.BinarySearch(SpecialKatakanaBeforeSmallChars, blocks[blockIndex - 1].Text[0]) > -1;

                    for (int charIndex = 0; charIndex < MAX_POST_PROCESSING; charIndex++)
                    {
                        if (IsInHiraganaRange(block.Text[charIndex]))
                        {
                            charIndex = ProcessHiragana(blockIndex, charIndex);
                        }
                        else if (IsInKatakanaRange(block.Text[charIndex]))
                        {
                            charIndex = ProcessKatakana(blockIndex, charIndex);
                        }
                    }
                }

                if (block.Ratio < 0.4f)
                {
                    for (int i = 0; i < MAX_POST_PROCESSING; i++)
                    {
                        if (block.Text[i] == '0' || block.Text[i] == 'o' || block.Text[i] == 'O' || block.Text[i] == 'Q')
                        {
                            block.Text.Insert(i, '。');
                            break;
                        }
                    }
                }
            }
        }

        private static void ProcessFirstSmallBlock()
        {
            if (blocks[0].Ratio < 0.4f)
            {
                for (int i = 0; i < MAX_POST_PROCESSING; i++)
                {
                    if (blocks[0].Text[i] == '0' || blocks[0].Text[i] == 'o' || blocks[0].Text[i] == 'O' || blocks[0].Text[i] == 'Q')
                    {
                        blocks[0].Text.Insert(i, '。');
                        return;
                    }
                }
            }

            if (count > 1 && blocks[0].Ratio <= SMALL_BLOCK_RATIO_THRESHOLD)
            {
                heightKanaRatio = blocks[0].Height / avgHeight;
                areaKanaRatio = (blocks[0].Width * blocks[0].Height) / avgArea;
                for (int i = 0; i < MAX_POST_PROCESSING; i++)
                {
                    if (blocks[0].Text[i] == 'つ')
                    {
                        if (areaKanaRatio < TSU_RATIO_SCALE && (heightKanaRatio < TSU_HEIGHT_SCALE || isHorizontalText))
                            blocks[i].Text.Insert(i, 'っ');
                        else
                            blocks[i].Text.Insert(i + 1, 'っ');
                        break;
                    }
                }
            }
        }

        private static void GetKanaAverageBlockSizes()
        {
            avgArea = 0;
            avgHeight = 0;
            count = 0;

            for (int i = 0; i < blocks.Count; i++)
            {
                if (IsInKanaRange(blocks[i].Text[0]))
                {
                    avgHeight += blocks[i].Height;
                    avgArea += blocks[i].Width * blocks[i].Height;
                    count++;
                }
            }
            if (count > 1)
            {
                avgArea = avgArea / count;
                avgHeight = avgHeight / count;
            }
        }

        private static bool IsInKanaRange(char text)
        {
            return IsInHiraganaRange(text) || IsInKatakanaRange(text);
        }

        private static bool IsInHiraganaRange(char text)
        {
            return (text <= UNICODE_HIRAGANA_END && text >= UNICODE_HIRAGANA_START);
        }

        private static bool IsInKatakanaRange(char text)
        {
            return text <= UNICODE_KATAKANA_END && text >= UNICODE_KATAKANA_START;
        }

        private static int ProcessHiragana(int blockIndex, int charIndex)
        {
            if (blocks[blockIndex].Text[charIndex] == 'つ')
            {
                if (areaKanaRatio < TSU_RATIO_SCALE && (heightKanaRatio < TSU_HEIGHT_SCALE || isHorizontalText))
                    blocks[blockIndex].Text.Insert(charIndex, 'っ');
                else
                    blocks[blockIndex].Text.Insert(charIndex + 1, 'っ');
                return charIndex + 1;
            }
            if (blockIndex == 0)
                return charIndex;

            switch (blocks[blockIndex].Text[charIndex])
            {
                case 'や':
                    if (isPreviousHiraValid && areaKanaRatio < SMALL_BLOCK_RATIO_THRESHOLD)
                        blocks[blockIndex].Text.Insert(charIndex, 'ゃ');
                    else
                        blocks[blockIndex].Text.Insert(charIndex + 1, 'ゃ');
                    return charIndex + 1;
                case 'ゆ':
                    if (isPreviousHiraValid && areaKanaRatio < SMALL_BLOCK_RATIO_THRESHOLD)
                        blocks[blockIndex].Text.Insert(charIndex, 'ゅ');
                    else
                        blocks[blockIndex].Text.Insert(charIndex + 1, 'ゅ');
                    return charIndex + 1;
                case 'よ':
                    if (isPreviousHiraValid && areaKanaRatio < SMALL_BLOCK_RATIO_THRESHOLD)
                        blocks[blockIndex].Text.Insert(charIndex, 'ょ');
                    else
                        blocks[blockIndex].Text.Insert(charIndex + 1, 'ょ');
                    return charIndex + 1;
                default:
                    return charIndex;
            }
        }

        private static int ProcessKatakana(int blockIndex, int charIndex)
        {
            if (blocks[blockIndex].Text[charIndex] == 'ツ')
            {
                if (areaKanaRatio < SMALL_BLOCK_RATIO_THRESHOLD)
                    blocks[blockIndex].Text.Insert(charIndex, 'ッ');
                else
                    blocks[blockIndex].Text.Insert(charIndex + 1, 'ッ');

                return charIndex + 1;
            }

            if (blockIndex == 0)
                return charIndex;

            switch (blocks[blockIndex].Text[charIndex])
            {
                case 'ア':
                    if (isPreviousKataSpecialValid && areaKanaRatio < SMALL_BLOCK_RATIO_THRESHOLD)
                        blocks[blockIndex].Text.Insert(charIndex, 'ァ');
                    else
                        blocks[blockIndex].Text.Insert(charIndex + 1, 'ァ');
                    break;
                case 'イ':
                    if (isPreviousKataSpecialValid && areaKanaRatio < SMALL_BLOCK_RATIO_THRESHOLD)
                        blocks[blockIndex].Text.Insert(charIndex, 'ィ');
                    else
                        blocks[blockIndex].Text.Insert(charIndex + 1, 'ィ');
                    break;
                case 'ウ':
                    if (isPreviousKataSpecialValid && areaKanaRatio < SMALL_BLOCK_RATIO_THRESHOLD)
                        blocks[blockIndex].Text.Insert(charIndex, 'ゥ');
                    else
                        blocks[blockIndex].Text.Insert(charIndex + 1, 'ゥ');
                    break;
                case 'エ':
                    if (isPreviousKataSpecialValid && areaKanaRatio < SMALL_BLOCK_RATIO_THRESHOLD)
                        blocks[blockIndex].Text.Insert(charIndex, 'ェ');
                    else
                        blocks[blockIndex].Text.Insert(charIndex + 1, 'ェ');
                    break;
                case 'オ':
                    if (isPreviousKataSpecialValid && areaKanaRatio < SMALL_BLOCK_RATIO_THRESHOLD)
                        blocks[blockIndex].Text.Insert(charIndex, 'ォ');
                    else
                        blocks[blockIndex].Text.Insert(charIndex + 1, 'ォ');
                    break;
                case 'ヤ':
                    if (isPreviousKataValid && areaKanaRatio < SMALL_BLOCK_RATIO_THRESHOLD)
                        blocks[blockIndex].Text.Insert(charIndex, 'ャ');
                    else
                        blocks[blockIndex].Text.Insert(charIndex + 1, 'ャ');
                    break;
                case 'ユ': //yu belong to both
                    if ((isPreviousKataValid || isPreviousKataSpecialValid)
                        && areaKanaRatio < SMALL_BLOCK_RATIO_THRESHOLD)
                        blocks[blockIndex].Text.Insert(charIndex, 'ュ');
                    else
                        blocks[blockIndex].Text.Insert(charIndex + 1, 'ュ');
                    break;
                case 'ヨ':
                    if (isPreviousKataValid && areaKanaRatio < SMALL_BLOCK_RATIO_THRESHOLD)
                        blocks[blockIndex].Text.Insert(charIndex, 'ョ');
                    else
                        blocks[blockIndex].Text.Insert(charIndex + 1, 'ョ');
                    break;

                default:
                    return charIndex;
            }
            return charIndex + 1;
        }
    }
}
