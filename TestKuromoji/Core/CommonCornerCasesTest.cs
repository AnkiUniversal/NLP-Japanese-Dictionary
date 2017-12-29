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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLPJapaneseDictionary.Kuromoji.Core.Compile;
using NLPJapaneseDictionary.Kuromoji.Core.IO;
using NLPJapaneseDictionary.Kuromoji.Core;
using NLPJapaneseDictionary.Kuromoji.Core.HelperClasses;
using NLPJapaneseDictionary.Kuromoji.Core.Buffer;

namespace NLPJDictTest.kuromoji.Core
{
    public static class CommonCornerCasesTest
    {
        public static void TestPunctuation<T>(TokenizerBase<T> tokenizer) where T : TokenBase
        {
            string gerryNoHanaNoHanashi = "僕の鼻はちょっと\r\n長いよ。";
            var expecter = new string[] { "僕", "の", "鼻", "は", "ちょっと", "\r", "\n", "長い", "よ", "。" };
            var actual = tokenizer.Tokenize(gerryNoHanaNoHanashi);
            TestUtils.AssertTokenSurfacesEquals(expecter, actual.ToArray());
        }
    }
}
