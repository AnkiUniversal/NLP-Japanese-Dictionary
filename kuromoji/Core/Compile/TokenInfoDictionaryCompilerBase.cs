using NLPJDict.Kuromoji.Core.Buffer;
using NLPJDict.Kuromoji.Core.Dict;
using NLPJDict.Kuromoji.Core.HelperClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NLPJDict.Kuromoji.Core.Compile
{
    public abstract class TokenInfoDictionaryCompilerBase<T> : ICompiler where T : DictionaryEntryBase
    {
        protected List<BufferEntry> bufferEntries = new List<BufferEntry>();
        protected FeatureInfoMap posInfo = new FeatureInfoMap();
        protected FeatureInfoMap otherInfo = new FeatureInfoMap();
        protected WordIdMapCompiler wordIdsCompiler = new WordIdMapCompiler();

        // optional list to collect the generic dictionary entries
        protected List<GenericDictionaryEntry> dictionaryEntries = null;

        private string encoding;
        private List<string> surfaces = new List<string>();

        /// <summary>
        /// Pass CodePagesEncodingProvider.Instance for provider        
        /// </summary>
        /// <param name="encoding"></param>
        /// <param name="provider"></param>
        public TokenInfoDictionaryCompilerBase(string encoding, EncodingProvider provider)
        {
            this.encoding = encoding;            
            Encoding.RegisterProvider(provider);
        }

        public void AnalyzeTokenInfo(Stream input)
        {
            try
            {
                input.Position = 0;
                var reader = new StreamReader(input, Encoding.GetEncoding(encoding));                
                string line;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine().RemapCharIfNeeded();                    
                    T entry = Parse(line);

                    GenericDictionaryEntry dictionaryEntry = MakeGenericDictionaryEntry(entry);
                    posInfo.MapFeatures(dictionaryEntry.GetPartOfSpeechFeatures());
                }
            }
            catch (IOException ex)
            {
                throw new IOException("TokenInfoDictionaryCompilerBase.AnalyzeTokenInfo: " + ex.Message);
            }
        }

        public void ReadTokenInfo(Stream input)
        {
            try
            {
                input.Position = 0;
                var reader = new StreamReader(input, Encoding.GetEncoding(encoding));
                int entryCount = posInfo.GetEntryCount();

                while (!reader.EndOfStream)
                {
                    T entry = Parse(reader.ReadLine().RemapCharIfNeeded());

                    GenericDictionaryEntry dictionaryEntry = MakeGenericDictionaryEntry(entry);

                    short leftId = dictionaryEntry.GetLeftId();
                    short rightId = dictionaryEntry.GetRightId();
                    short wordCost = dictionaryEntry.GetWordCost();

                    string[] allPosFeatures = dictionaryEntry.GetPartOfSpeechFeatures();

                    List<int> posFeatureIds = posInfo.MapFeatures(allPosFeatures);

                    string[] featureList = dictionaryEntry.GetOtherFeatures();
                    List<int> otherFeatureIds = otherInfo.MapFeatures(featureList);

                    BufferEntry bufferEntry = new BufferEntry();
                    bufferEntry.TokenInfo.Add(leftId);
                    bufferEntry.TokenInfo.Add(rightId);
                    bufferEntry.TokenInfo.Add(wordCost);

                    if (EntriesFitInAByte(entryCount))
                    {
                        List<Byte> posFeatureIdBytes = CreatePosFeatureIds(posFeatureIds);
                        bufferEntry.PosInfo.AddRange(posFeatureIdBytes);
                    }
                    else
                    {
                        foreach (int posFeatureId in posFeatureIds)
                        {
                            bufferEntry.TokenInfo.Add((short)posFeatureId);
                        }
                    }

                    bufferEntry.Features.AddRange(otherFeatureIds);

                    bufferEntries.Add(bufferEntry);
                    surfaces.Add(dictionaryEntry.GetSurface());

                    if (dictionaryEntries != null)
                    {
                        dictionaryEntries.Add(dictionaryEntry);
                    }
                }

            }
            catch (IOException ex)
            {
                throw new IOException("TokenInfoDictionaryCompilerBase.AnalyzeTokenInfo: " + ex.Message);
            }
        }

        protected abstract GenericDictionaryEntry MakeGenericDictionaryEntry(T entry);

        protected abstract T Parse(string line);

        public void Compile(Stream output)
        {
            // TODO: Should call this method instead of write()
        }

        private bool EntriesFitInAByte(int entryCount)
        {
            return entryCount <= 0xff;
        }

        private List<byte> CreatePosFeatureIds(List<int> posFeatureIds)
        {
            List<byte> posFeatureIdBytes = new List<byte>();
            foreach (int posFeatureId in posFeatureIds)
            {
                posFeatureIdBytes.Add((byte)posFeatureId);
            }
            return posFeatureIdBytes;
        }


        public Stream CombinedSequentialFileInputStream(string absolutePath)
        {
            List<Stream> fileInputStreams = new List<Stream>();
            var files = GetCsvFiles(absolutePath);
            return StreamHelper.SequenceInputStream(files);
        }

        public string[] GetCsvFiles(string absolutePath)
        {
            var files = Directory.GetFiles(absolutePath, "*.csv", SearchOption.AllDirectories);            
            Array.Sort(files, StringHelper.SortLexicographically);
            return files;
        }

        public void AddMapping(int sourceId, int wordId)
        {
            wordIdsCompiler.AddMapping(sourceId, wordId);
        }

        public List<string> GetSurfaces()
        {
            return surfaces;
        }

        public void Write(string directoryName)
        {
            string path = "";
            if (!String.IsNullOrWhiteSpace(directoryName))
                path = directoryName + Path.DirectorySeparatorChar;
            WriteDictionary(path + TokenInfoDictionary.TOKEN_INFO_DICTIONARY_FILENAME);
            WriteMap(path + TokenInfoDictionary.POS_MAP_FILENAME, posInfo);
            WriteMap(path + TokenInfoDictionary.FEATURE_MAP_FILENAME, otherInfo);
            WriteWordIds(path + TokenInfoDictionary.TARGETMAP_FILENAME);
        }


        protected void WriteMap(string absoluteFilePath, FeatureInfoMap map)
        {
            SortedDictionary<int, string> features = map.Invert();

            using(StringValueMapBuffer mapBuffer = new StringValueMapBuffer(features))
            using(var fos = File.Open(absoluteFilePath, FileMode.OpenOrCreate))
            {
                mapBuffer.Write(fos);
            }
        }

        protected void WriteDictionary(string absoluteFilePath)
        {
            using (TokenInfoBufferCompiler tokenInfoBufferCompiler = new TokenInfoBufferCompiler(bufferEntries))
            using (var fos = File.Open(absoluteFilePath, FileMode.OpenOrCreate))
            {
                tokenInfoBufferCompiler.Compile(fos);
            }
        }

        protected void WriteWordIds(string absoluteFilePath)
        {
            using (var fos = File.Open(absoluteFilePath, FileMode.OpenOrCreate))
            {
                wordIdsCompiler.Write(fos);                
            }            
        }
    }
}
