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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.Kuromoji.Core.IO
{
    public class StringArrayIO
    {

        public static string[] ReadArray(BinaryReader dataInput)
        {
            try
            {
                int length = dataInput.ReadRawInt32();

                string[] array = new string[length];

                for (int i = 0; i < length; i++)
                {
                    array[i] = dataInput.ReadString();
                }

                return array;
            }
            catch (Exception ex)
            {
                throw new IOException("StringArrayIO.ReadArray: " + ex.Message);
            }
        }

        public static void WriteArray(BinaryWriter dataOutput, string[] array)
        {
            try
            {
                int length = array.Length;

                dataOutput.WriteRawInt32(length);

                for (int i = 0; i < array.Length; i++)
                {
                    dataOutput.Write(array[i]);
                }
            }
            catch (Exception ex)
            {
                throw new IOException("StringArrayIO.WriteArray: " + ex.Message);
            }
        }

        public static string[][] ReadArray2D(BinaryReader dataInput)
        {
            try
            {
                int length = dataInput.ReadRawInt32();

                string[][] array = new string[length][];

                for (int i = 0; i < length; i++)
                {
                    array[i] = ReadArray(dataInput);
                }

                return array;
            }
            catch (Exception ex)
            {
                throw new IOException("StringArrayIO.ReadArray2D: " + ex.Message);
            }
        }

        public static void WriteArray2D(BinaryWriter dataOutput, String[][] array)
        {
            try
            {
                int length = array.Length;

                dataOutput.WriteRawInt32(length);

                for (int i = 0; i < length; i++)
                {
                    WriteArray(dataOutput, array[i]);
                }
            }
            catch (Exception ex)
            {
                throw new IOException("StringArrayIO.WriteArray2D: " + ex.Message);
            }
        }

        public static string[][] ReadSparseArray2D(BinaryReader dataInput)
        {
            try
            {
                int length = dataInput.ReadRawInt32();

                string[][] array = new string[length][];

                int index;

                while ((index = dataInput.ReadRawInt32()) >= 0)
                {
                    array[index] = ReadArray(dataInput);
                }

                return array;
            }
            catch (Exception ex)
            {
                throw new IOException("StringArrayIO.WriteArray2D: " + ex.Message);
            }
        }

        public static void WriteSparseArray2D(BinaryWriter dataOutput, String[][] array)
        {
            try
            {
                int length = array.Length;

                dataOutput.WriteRawInt32(length);

                for (int i = 0; i < length; i++)
                {
                    string[] inner = array[i];

                    if (inner != null)
                    {
                        dataOutput.WriteRawInt32(i);
                        WriteArray(dataOutput, inner);
                    }
                }
                // This negative index serves as an end-of-array marker
                dataOutput.WriteRawInt32(-1);
            }
            catch (Exception ex)
            {
                throw new IOException("StringArrayIO.WriteArray2D: " + ex.Message);
            }
        }
    }
}
