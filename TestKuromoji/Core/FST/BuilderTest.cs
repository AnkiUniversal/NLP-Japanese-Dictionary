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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLPJDict.Kuromoji.Core.FST;

namespace NLPJDictTest.kuromoji.Core.FST
{
    [TestClass]
    public class BuilderTest
    {
        [TestMethod]
        public void TestCreateDictionary()
        {
            string[] inputValues = { "cat", "cats", "dog", "dogs", "friday", "friend", "pydata" };
            int[] outputValues = { 1, 2, 3, 4, 20, 42, 43 };

            using (Builder builder = new Builder())
            {
                builder.Build(inputValues, outputValues);

                for (int i = 0; i < inputValues.Length; i++)
                {
                    Assert.AreEqual(
                        outputValues[i],
                        builder.Transduce(inputValues[i])
                    );
                }
            }
        }
    }
}
