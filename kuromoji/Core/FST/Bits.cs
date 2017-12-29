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

using NLPJapaneseDictionary.Kuromoji.Core.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.Kuromoji.Core.FST
{
    public static class Bits
    {
        public static byte GetByte(byte[] array, int index)
        {
            return array[index];
        }

        public static int GetShort(byte[] bytes, int index)
        {
            var result = (bytes[index - 1] & 0xff) << 8 | (bytes[index] & 0xff);
            return result;
        }

        public static int GetInt(byte[] bytes, int index)
        {
            return (bytes[index - 3] & 0xff) << 24 
                | (bytes[index - 2] & 0xff) << 16 
                | (bytes[index - 1] & 0xff) << 8 
                | (bytes[index] & 0xff);
        }

        public static int GetInt(byte[] bytes, int index, int intBytes)
        {
            switch (intBytes)
            {
                case 0:
                    return 0;

                case 1:
                    return bytes[index] & 0xff;

                case 2:
                    return (bytes[index - 1] & 0xff) << 8 
                        | (bytes[index] & 0xff);

                case 3:
                    return (bytes[index - 2]  & 0xff) << 16 
                        | (bytes[index - 1]  & 0xff) << 8 | 
                        (bytes[index] & 0xff);

                case 4:
                    return (bytes[index - 3] & 0xff) << 24 
                        | (bytes[index - 2] & 0xff) << 16 
                        | (bytes[index - 1] & 0xff) << 8 
                        | (bytes[index] & 0xff);

                default:
                    throw new Exception("Illegal int byte size: " + intBytes);
            }
        }

        public static void PutInt(byte[] bytes, int index, int value, int intBytes)
        {
            switch (intBytes)
            {
                case 1:
                    bytes[index] = (byte)(value & 0xff);
                    break;

                case 2:
                    bytes[index - 1] = (byte)(value >> 8 & 0xff);
                    bytes[index] = (byte)(value & 0xff);
                    break;

                case 3:
                    bytes[index - 2] = (byte)(value >> 16 & 0xff);
                    bytes[index - 1] = (byte)(value >> 8 & 0xff);
                    bytes[index] = (byte)(value & 0xff);
                    break;

                case 4:
                    bytes[index - 3] = (byte)(value >> 24 & 0xff);
                    bytes[index - 2] = (byte)(value >> 16 & 0xff);
                    bytes[index - 1] = (byte)(value >> 8 & 0xff);
                    bytes[index] = (byte)(value & 0xff);
                    break;
                default:
                    throw new Exception("Illegal int byte size: " + intBytes);
            }
        }
    }
}
