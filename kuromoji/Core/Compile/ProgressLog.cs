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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.Kuromoji.Core.Compile
{
    public class ProgressLog
    {
        private static int indent = 0;
        private static bool atEOL = false;
        private static Dictionary<int, long> startTimes = new Dictionary<int, long>();

        public static void Begin(string message)
        {
            NewLine();
            Debug.WriteLine(Leader() + message + "... ");
            atEOL = true;
            indent++;
            startTimes[indent] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public static void End()
        {
            NewLine();
            long start = startTimes[indent];
            indent = Math.Max(0, indent - 1);
            Debug.WriteLine(Leader() + "done" + " [" + ((DateTimeOffset.Now.ToUnixTimeMilliseconds() - start) / 1000) + "s]");
        }

        public static void Println(string message)
        {
            NewLine();
            Debug.WriteLine(Leader() + message);
        }

        private static void NewLine()
        {
            if (atEOL)
            {
                Debug.WriteLine("");
            }
            atEOL = false;
        }

        private static string Leader()
        {
            return "[KUROMOJI] " + DateTimeOffset.Now.ToLocalTime().ToString();
        }
    }

}
