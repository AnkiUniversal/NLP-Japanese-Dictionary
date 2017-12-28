# NLP Japanese Dictionary
A Japanse - English dictionary software with OCR for Windows 7, 8, 10.
More than just a simple dictionary, NLP Japanese Dictionary integrates advanced natural language processing algorithms to help you read Japanese as effortlessly as possible.

**Main Features:**
- Breakdown complicated sentences into simple words then give their reading, pronunciation, and definitions.
- Sort definitions of a word based on its context in a sentence.
- Include 170000+ Japanese - English entries from JMdict.
- Allow searching by Kana, Kanji, Romaji, and English. 
- Provide Kanji information and their writing.
- Provide sentence examples.
- Extract text from images using OCR.
- Support text-to-speech for Japanese.
- Allow switching between day and night mode instantly.

# 3rd Party Open Source Projects
This application uses various open source projects. Please see `CREDITS.md` for details. The most important ones are:
- Japanese sentence segmentation: Kuromoji under Apache License 2.0 and Mecab-Ipadic.
- Japanese - English dictionary: Japanese-Multilingual Dictionary under Creative Commons Attribution-ShareAlike Licence 3.0.
- Kanji databases: KANJIDIC2, KanjiVG under Creative Commons Attribution-ShareAlike Licence 3.0.
- Example sentences: Tanaka Corpus and Tatoeba Project under Creative Commons Attribution Licence 2.0.

# Project Structure
## 1. Jocr & Mqdf (.Net Standard 1.4)
**Required nuget packets** `System.Numerics.Vectors` and `System.Threading.Tasks.Parallel`.

These projects are used for Japanese OCR. They are written from scratch by us and do not rely on any other source code. Thus, you can easily build or use them in yor projects. Please note that they are only trained for printing text.

 Please see TestOCR project for their general uses. To convert your images to `Jocr.GrayImage` format for further processing, see `NLPJapaneseDictionary.OCR.JorcImageConvert.BitmapToGrayImageJocr`.

## 2. Kuromoji (.Net Standard 2.0)
No special actions needed.

This is a port from java to C# by us. For more details, please view the original project `https://github.com/atilika/kuromoji`.

## 3. NLPJapaneseDictionary WPF
**Required nuget packets** `sqlite-net-pcl`, `Newtonsoft.Json`, and `System.Text.Encoding.CodePages`.

The main project provides user interface written in WPF (and WinForms). 

# 
