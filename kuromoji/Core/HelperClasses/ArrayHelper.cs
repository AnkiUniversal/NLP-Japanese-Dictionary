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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.HelperClasses
{
    public static class ArrayHelper
    {
        public static void Fill<T>(this T[] array, T value)
        {
            for(int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        public static int GetJavaListHashCode<T>(this List<T> values)
        {
            int hashCode = 1;
            for (int i = 0; i < values.Count; i++)
                hashCode = 31 * hashCode + (values[i] == null ? 0 : values[i].GetHashCode());
            return hashCode;
        }

        public static bool AreEqual<T>(List<T> compared, List<T> comparer)
        {
            if (compared.Count != comparer.Count)
                return false;

            for(int i = 0; i < compared.Count; i++)
            {
                if (!compared[i].Equals(comparer[i]))
                    return false;
            }
            return true;
        }
    }
}
