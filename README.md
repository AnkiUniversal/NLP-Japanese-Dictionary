# NLP Japanese Dictionary
A Japanese - English dictionary software with OCR for Windows 7, 8, 10. (Require .Net Framework 4.6.1)

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
This application uses various open source projects (binary or source version). Please see `CREDITS.md` for details. The most important ones are:
- Japanese sentence segmentation: Kuromoji under Apache License 2.0 and Mecab-Ipadic.
- Japanese - English dictionary: Japanese-Multilingual Dictionary under Creative Commons Attribution-ShareAlike Licence 3.0.
- Kanji databases: KANJIDIC2, KanjiVG under Creative Commons Attribution-ShareAlike Licence 3.0.
- Example sentences: Tanaka Corpus and Tatoeba Project under Creative Commons Attribution Licence 2.0.

# Project Structure
## 1. Jocr & Mqdf (.Net Standard 1.4)
**Required nuget packets** `System.Numerics.Vectors` and `System.Threading.Tasks.Parallel`.

These projects are used for Japanese OCR. They are written from scratch by us and do not rely on any other source code. Thus, you can easily build or use them in other projects. Please note that they are only trained for printing text.

 Please see `TestOCR` project for general uses. To convert your images to `Jocr.GrayImage` format before processing, see `NLPJapaneseDictionary.OCR.JorcImageConvert.BitmapToGrayImageJocr`.

## 2. Kuromoji (.Net Standard 2.0)
This is a port from java to C# by us. For more details, please view the original project `https://github.com/atilika/kuromoji`.

## 3. NLPJapaneseDictionary WPF
**Required nuget packets** `sqlite-net-pcl`, `Newtonsoft.Json`, and `System.Text.Encoding.CodePages`.

The main project provides the user interface written in WPF (and WinForms). 

# [Wiki](https://github.com/AnkiUniversal/NLP-Japanese-Dictionary/wiki)
For normal users, please read our [FAQ](https://github.com/AnkiUniversal/NLP-Japanese-Dictionary/wiki/Users'-FAQ).

# Contributing
We welcome any contributions from submitting bug reports to new feature recommendations. 

For pull requests, please license your contributions under Apache License V2.0. We will retain your copyright and include your name in the CONTRIBUTORS.md file.

We plan to add a few white papers on how the OCR engine works so that you can help us improve it or write a new engine for other languages. Though we don't expect to finish these papers anytime soon. At the moment, the engine does not work well with uneven lighting images, text with noisy background, or skew text. Thus, preprocessing steps are really needed to be improved.

# License
NLP Japanese Dictionary is licensed under the Apache License, Version 2.0. Please see `LICENSE` for details.

