using Jocr;
using NLPJapaneseDictionary.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.OCR
{
    public static class JocrWrapper
    {
        public const string OCR_FILE_TOKEN = "OCR_IMG";
        public static readonly string[] SupportedImages = new string[] { ".jpg", ".jpeg", ".png" };

        public static bool IsOcrParametersInit { get { return Jocr.TextExtraction.IsOcrParamentersInit; } }       

        public static void InitOcrParameters()
        {
            Jocr.TextExtraction.InitOcrParameters();
        }

        public static string GetFirstString(List<TextBlock> textBlocks)
        {
            if (textBlocks == null || textBlocks.Count == 0)
                return "";

            StringBuilder builder = new StringBuilder();
            foreach (var text in textBlocks)
                builder.Append(text.Text[0]);

            return builder.ToString();
        }

        public static List<TextBlock> ExtractTextFromLocalImage(string filePath)
        {
            using (Bitmap bm = new Bitmap(filePath))
            {
                return ExtractTextFromBitmap(bm);
            }
        }

        public static List<TextBlock> ExtractTextFromBitmap(Bitmap bitmap)
        {            
            var image = JorcImageConvert.BitmapToGrayImageJocr(bitmap);
            return RunOcr(image);
        }

        public static List<TextBlock> RunOcr(GrayImage image, Ocr.RunOcrHandler runOcrHandler = null)
        {
            try
            {
                bool isValidSize = CheckImageSize(image.Width, image.Height);
                if (!isValidSize)
                    return null;
                if (runOcrHandler == null)
                    return Jocr.Ocr.RecognizeSentences(image);
                else
                    return runOcrHandler(image);

                //Debuging purpose only
                //return Jocr.Ocr.Start(image, JorcImageConvert.ShowJocrGrayImage);
            }
            catch (Exception e)
            {             
                UIUtilities.ShowErrorDialog("RunOcr: " +  e.Message + "\n" + e.StackTrace);
                return null;
            }
        }

        private static bool CheckImageSize(int PixelWidth, int PixelHeight)
        {
            if (PixelWidth > Jocr.Ocr.MAX_IMAGE_WIDTH || PixelHeight > Jocr.Ocr.MAX_IMAGE_HEIGHT)
            {
                string message = String.Format("Bitmap dimensions ({0}x{1}) are too big for OCR.", PixelWidth, PixelHeight)
                                              + "\nMax image dimension is " + Ocr.MAX_IMAGE_WIDTH + "x" + Ocr.MAX_IMAGE_HEIGHT + ".";
                UIUtilities.ShowMessageDialog(message);
                return false;
            }
            return true;
        }

    }
}
