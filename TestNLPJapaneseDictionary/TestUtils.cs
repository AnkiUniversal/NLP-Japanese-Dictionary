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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLPJDict.Kuromoji.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDictTest
{
    public static class TestUtils
    {
        public static bool IsArrayEqual<T>(T[] expected, T[] actual)
        {
            if (expected.Length != actual.Length)
                return false;

            for (int i = 0; i < expected.Length; i++)
            {
                if (!expected[i].Equals(actual[i]))
                    return false;
            }

            return true;
        }

        public static string Array2String<T>(this IEnumerable<T> list)
        {
            return "[" + string.Join(", ", list) + "]";
        }

        public static bool ContainsAll<T>(this IEnumerable<T> list, IEnumerable<T> subList)
        {
            foreach (var item in subList)
            {
                if (!list.Contains(item))
                    return false;
            }
            return true;
        }

        public static void AssertTokenSurfacesEquals<T>(string[] expectedSurfaces, T[] actualTokens) where T : TokenBase
        {
            List<string> actualSurfaces = new List<string>();

            foreach (var token in actualTokens)
            {
                actualSurfaces.Add(token.Surface);
            }

           Assert.IsTrue(IsArrayEqual<string>(expectedSurfaces, actualSurfaces.ToArray()));
        }

        public static void AssertCanTokenizeStream<T>(Stream untokenizedInput, TokenizerBase<T> tokenizer) where T : TokenBase
        {
            untokenizedInput.Position = 0;
            using (var untokenizedInputReader = new StreamReader(untokenizedInput, Encoding.UTF8))
            {
                while (!untokenizedInputReader.EndOfStream)
                {
                    AssertCanTokenizeString(untokenizedInputReader.ReadLine(), tokenizer);
                }

                Assert.IsTrue(true);
            }
        }


        public static void AssertCanTokenizeString<T>(string input, TokenizerBase<T> tokenizer) where T : TokenBase
        {
            List<T> tokens = tokenizer.Tokenize(input);

            if (input.Length > 0)
            {
                Assert.IsTrue(tokens.Count > 0);
            }
            else
            {
                Assert.AreEqual(0, tokens.Count);
            }
        }

        public static void AssertCanMultiTokenizeString<T>(string input, int maxCount, int costSlack,
            TokenizerBase<T> tokenizer) where T : TokenBase
        {
            var tokens = tokenizer.MultiTokenize(input, maxCount, costSlack);

            if (input.Length > 0)
            {
                Assert.IsTrue(tokens.Count > 0);
            }
            else
            {
                Assert.AreEqual(0, tokens.Count);
            }
        }

        public static void AssertTokenizedStreamEquals<T>(Stream tokenizedInput,
                                                       Stream untokenizedInput,
                                                       TokenizerBase<T> tokenizer) where T : TokenBase
        {
            tokenizedInput.Position = 0;
            untokenizedInput.Position = 0;
            using (var untokenizedInputReader = new StreamReader(untokenizedInput, Encoding.UTF8))
            using (var tokenizedInputReader = new StreamReader(tokenizedInput, Encoding.UTF8))
            {                
                while (!untokenizedInputReader.EndOfStream)
                {
                    var tokens = tokenizer.Tokenize(untokenizedInputReader.ReadLine());

                    foreach (TokenBase token in tokens)
                    {
                        string tokenLine = tokenizedInputReader.ReadLine();

                        Assert.IsNotNull(tokenLine);
                        //if (tokenLine.StartsWith("――")) continue;

                        string[] parts = tokenLine.Split(new string[] { "\t" }, 2, StringSplitOptions.RemoveEmptyEntries);
                        string surface = parts[0];
                        string features = parts[1];

                        Assert.IsTrue(surface.Equals(token.Surface, StringComparison.Ordinal), 
                                        OutFailMessage(surface, token.Surface));
                        var allFeatures = token.GetAllFeatures();
                        Assert.IsTrue(features.Equals(allFeatures, StringComparison.Ordinal),
                                        OutFailMessage(features, allFeatures));
                    }
                }
            }
        }

        private static string OutFailMessage(object expected, object actual)
        {
            return String.Format("Expected: {0}\nActual: {1}", expected, actual);
        }

        public static void AssertMultiThreadedTokenizedStreamEquals<T>(int numThreads,
                                                                    int perThreadRuns,
                                                                    string tokenizedInputResource,
                                                                    string untokenizedInputResource,
                                                                    TokenizerBase<T> tokenizer) where T : TokenBase
        {
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < numThreads; i++)
            {
                var task = new Task(() =>
                {
                    TestTask(perThreadRuns, tokenizedInputResource, untokenizedInputResource, tokenizer);
                });
                tasks.Add(task);
                task.Start();
            }

            foreach (var task in tasks)
            {
                task.Wait();
            }
            Assert.IsTrue(true);
        }

        private static void TestTask<T>(int perThreadRuns, string tokenizedInputResource, string untokenizedInputResource, TokenizerBase<T> tokenizer) where T : TokenBase
        {
            for (int run = 0; run < perThreadRuns; run++)
            {
                try
                {
                    using (Stream tokenizedInput = File.OpenRead(tokenizedInputResource))
                    using (Stream untokenizedInput = File.OpenRead(untokenizedInputResource))
                    {
                        AssertTokenizedStreamEquals(tokenizedInput, untokenizedInput, tokenizer);
                    }
                }
                catch (IOException e)
                {
                    Assert.Fail(e.Message);
                }
            }
        }

        public static void AssertEqualTokenFeatureLengths<T>(string text, TokenizerBase<T> tokenizer) where T : TokenBase
        {
            var tokens = tokenizer.Tokenize(text);
            HashSet<int> lengths = new HashSet<int>();

            foreach (TokenBase token in tokens)
            {
                lengths.Add(token.GetAllFeaturesArray().Length);
            }

            Assert.AreEqual(1, lengths.Count);
        }
    }
}
