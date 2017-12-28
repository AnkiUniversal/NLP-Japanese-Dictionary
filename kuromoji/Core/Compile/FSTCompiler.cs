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

using NLPJDict.Kuromoji.Core.FST;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Compile
{
    public class FSTCompiler : ICompiler
    {

        private readonly string[] surfaces;

        public FSTCompiler(List<string> surfaces)
        {
            this.surfaces = new HashSet<string>(surfaces).ToArray<string>();
        }

        public void Compile(Stream output)
        {
            try
            {
                Array.Sort(surfaces, StringHelper.SortLexicographically);

                using (Builder builder = new Builder())
                {
                    builder.Build(surfaces, null);

                    MemoryStream fst = new MemoryStream(builder.GetCompiler().GetBytes());
                    ByteBufferIO.Write(output, fst);
                }
            }
            catch (IOException ex)
            {
                throw new Exception("FSTCompiler.Compile: " + ex.Message);
            }
        }

    }
}
