/**
 * Copyright © 2010-2017 Atilika Inc. and contributors (see CONTRIBUTORS.md)
 * 
 * Modifications copyright (C) 2017 - 2018 Anki Universal Team <ankiuniversal@gmail.com>
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

using NLPJapaneseDictionary.Kuromoji.Core.Dict;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.Kuromoji.Core.Compile
{
    public abstract class DictionaryCompilerBase<T> where T : DictionaryEntryBase
    {

        public void Build(string inputDirAbsolutePath, string outputDirRelativePath, string encoding, EncodingProvider provider)
        {
            try
            {
                string outputtDirAbsolutePath = Constant.RESOURCE_PATH + Path.DirectorySeparatorChar + outputDirRelativePath;

                BuildTokenInfoDictionary(inputDirAbsolutePath, outputtDirAbsolutePath, encoding, provider);
                BuildUnknownWordDictionary(inputDirAbsolutePath, outputtDirAbsolutePath, encoding, provider);
                BuildConnectionCosts(inputDirAbsolutePath, outputtDirAbsolutePath);
            }
            catch (Exception ex)
            {
                throw new Exception("DictionaryCompilerBase.Build: " + ex.Message);
            }
        }

        private void BuildTokenInfoDictionary(string inputDirAbsolutePath, string outputDirAbsolutePath, string encoding, EncodingProvider provider)
        {
            try
            {
                ProgressLog.Begin("compiling tokeninfo dict");
                var tokenInfoCompiler = GetTokenInfoDictionaryCompiler(encoding, provider);

                ProgressLog.Println("analyzing dictionary features");                

                using (var stream = tokenInfoCompiler.CombinedSequentialFileInputStream(inputDirAbsolutePath))
                {
                    tokenInfoCompiler.AnalyzeTokenInfo(stream);
                    ProgressLog.Println("reading tokeninfo");
                    tokenInfoCompiler.ReadTokenInfo(stream);
                    tokenInfoCompiler.Compile(stream);
                }

                List<string> surfaces = tokenInfoCompiler.GetSurfaces();

                ProgressLog.Begin("compiling fst");

                FSTCompiler fstCompiler = new FSTCompiler(surfaces);
                using (var stream = File.Open(outputDirAbsolutePath + Path.DirectorySeparatorChar + FST.FST.FST_FILENAME, FileMode.OpenOrCreate))
                {
                    fstCompiler.Compile(stream);
                }

                ProgressLog.Println("validating saved fst");

                FST.FST fst;
                using (var stream = File.OpenRead(outputDirAbsolutePath + Path.DirectorySeparatorChar + FST.FST.FST_FILENAME))
                {
                    fst = new FST.FST(stream);
                }

                foreach (string surface in surfaces)
                {
                    if (fst.Lookup(surface) < 0)
                    {
                        ProgressLog.Println("failed to look up [" + surface + "]");
                    }
                }

                ProgressLog.End();

                ProgressLog.Begin("processing target map");

                for (int i = 0; i < surfaces.Count; i++)
                {
                    int id = fst.Lookup(surfaces[i]);
                    tokenInfoCompiler.AddMapping(id, i);
                }

                tokenInfoCompiler.Write(outputDirAbsolutePath); // TODO: Should be refactored -Christian
                ProgressLog.End();
            }
            catch (Exception ex)
            {
                throw new Exception("DictionaryCompilerBase.BuildTokenInfoDictionary: " + ex.Message);
            }
        }

        abstract protected TokenInfoDictionaryCompilerBase<T> GetTokenInfoDictionaryCompiler(string encoding, EncodingProvider provider);

        protected void BuildUnknownWordDictionary(string inputDirAbsolutePath, string outputDirAbsolutePath, string encoding, EncodingProvider provider)
        {
            try
            {
                ProgressLog.Begin("compiling unknown word dict");

                CharacterDefinitionsCompiler charDefCompiler = new CharacterDefinitionsCompiler(provider);
                string outputFilePath = outputDirAbsolutePath + Path.DirectorySeparatorChar + CharacterDefinitions.CHARACTER_DEFINITIONS_FILENAME;
                using (var stream = File.OpenRead(inputDirAbsolutePath + Path.DirectorySeparatorChar + "char.def"))
                using (var outputStream = File.Open(outputFilePath, FileMode.OpenOrCreate))
                {
                    charDefCompiler.ReadCharacterDefinition(stream, encoding);
                    charDefCompiler.Compile(outputStream);
                }

                UnknownDictionaryCompiler unkDefCompiler = new UnknownDictionaryCompiler(charDefCompiler.MakeCharacterCategoryMap());
                outputFilePath = outputDirAbsolutePath + Path.DirectorySeparatorChar + UnknownDictionary.UNKNOWN_DICTIONARY_FILENAME;
                using (var stream = File.OpenRead(inputDirAbsolutePath + Path.DirectorySeparatorChar + "unk.def"))
                using (var outputStream = File.Open(outputFilePath, FileMode.OpenOrCreate))
                {
                    unkDefCompiler.ReadUnknownDefinition(stream, encoding);
                    unkDefCompiler.Compile(outputStream);
                }

                ProgressLog.End();

            }
            catch (Exception ex)
            {
                throw new Exception("DictionaryCompilerBase.BuildUnknownWordDictionary: " + ex.Message);
            }
        }


        private void BuildConnectionCosts(string inputDirAbsolutePath, string outputDirAbsolutePath)
        {
            try
            {
                ProgressLog.Begin("compiling connection costs");
                string outputFilePath = outputDirAbsolutePath + Path.DirectorySeparatorChar + ConnectionCosts.CONNECTION_COSTS_FILENAME;
                using(var inputSream = File.OpenRead(inputDirAbsolutePath + Path.DirectorySeparatorChar + "matrix.def"))
                using (var outputStream = File.Open(outputFilePath, FileMode.OpenOrCreate))
                {
                    using (ConnectionCostsCompiler connectionCostsCompiler = new ConnectionCostsCompiler())
                    {
                        connectionCostsCompiler.ReadCosts(inputSream);
                        connectionCostsCompiler.Compile(outputStream);
                    }
                    ProgressLog.End();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("DictionaryCompilerBase.BuildConnectionCosts: " + ex.Message);
            }
        }

        protected void Build(string[] args, EncodingProvider provider)
        {
            try
            {
                string inputDirname = args[0];
                string outputDirname = args[1];
                string inputEncoding = args[2];

                ProgressLog.Println("dictionary compiler");
                ProgressLog.Println("");
                ProgressLog.Println("input directory: " + inputDirname);
                ProgressLog.Println("output directory: " + outputDirname);
                ProgressLog.Println("input encoding: " + inputEncoding);
                ProgressLog.Println("");

                Build(inputDirname, outputDirname, inputEncoding, provider);
            }

            catch (Exception ex)
            {
                throw new Exception("DictionaryCompilerBase.Build: " + ex.Message);
            }
        }
    }
}
