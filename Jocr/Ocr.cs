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
        public static List<TextBlock> Start(GrayImage image, ShowImageHandle showImageFunction)
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
