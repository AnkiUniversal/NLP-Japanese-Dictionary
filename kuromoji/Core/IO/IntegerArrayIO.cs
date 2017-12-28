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

using NLPJDict.Kuromoji.Core.HelperClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.IO
{
    public class IntegerArrayIO
    {

        public static int[][] ReadArrays(BinaryReader reader, int arrayCount)
        {
            try
            {
                lock (reader)
                {
                    int[][] arrays = new int[arrayCount][];
                    for (int i = 0; i < arrayCount; i++)
                    {
                        arrays[i] = ReadArrayFromChannel(reader);
                    }
                    return arrays;
                }
            }
            catch (Exception ex)
            {
                throw new IOException("IntegerArrayIO.ReadArrays: " + ex.Message);
            }
        }

        public static void WriteArray(BinaryWriter dataOutput, int[] array)
        {
            try
            {
                lock (dataOutput)
                {
                    int length = array.Length;

                    dataOutput.WriteRawInt32(length);
                    dataOutput.WriteArrayInt32(array);
                }
            }
            catch (Exception ex)
            {
                throw new IOException("IntegerArrayIO.WriteArray: " + ex.Message);
            }
        }

        public static int[][] ReadArray2D(BinaryReader reader)
        {
            try
            {
                lock (reader)
                {
                    int arrayCount = reader.ReadRawInt32();
                    return ReadArrays(reader, arrayCount);
                }
            }
            catch (Exception ex)
            {
                throw new IOException("IntegerArrayIO.ReadArray2D: " + ex.Message);
            }
        }

        public static void WriteArray2D(BinaryWriter dataOutput, int[][] array)
        {
            try
            {
                lock (dataOutput)
                {
                    int length = array.Length;

                    dataOutput.WriteRawInt32(length);

                    for (int i = 0; i < length; i++)
                    {
                        WriteArray(dataOutput, array[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException("IntegerArrayIO.WriteArray2D: " + ex.Message);
            }
        }

        public static int[][] ReadSparseArray2D(BinaryReader reader)
        {
            try
            {
                lock (reader)
                {
                    int arrayCount = reader.ReadRawInt32();
                    int[][] arrays = new int[arrayCount][];

                    int index;
                    while ((index = reader.ReadRawInt32()) >= 0)
                    {
                        arrays[index] = ReadArrayFromChannel(reader);
                    }
                    return arrays;
                }
            }
            catch (Exception ex)
            {
                throw new IOException("IntegerArrayIO.ReadSparseArray2D: " + ex.Message);
            }
        }

        public static void WriteSparseArray2D(BinaryWriter dataOutput, int[][] array)
        {
            try
            {
                lock (dataOutput)
                {
                    int length = array.Length;
                    dataOutput.WriteRawInt32(length);

                    for (int i = 0; i < length; i++)
                    {
                        int[] inner = array[i];

                        if (inner != null)
                        {
                            dataOutput.WriteRawInt32(i);
                            WriteArray(dataOutput, inner);
                        }
                    }
                    // This negative index serves as an end-of-array marker
                    dataOutput.WriteRawInt32(-1);
                }
            }
            catch (Exception ex)
            {
                throw new IOException("IntegerArrayIO.WriteSparseArray2D: " + ex.Message);
            }
        }

        private static int[] ReadArrayFromChannel(BinaryReader reader)
        {
            try
            {
                int length = reader.ReadRawInt32();
                int[] array = new int[length];
                reader.ReadArrayInt32(array);
                return array;
            }
            catch (Exception ex)
            {
                throw new IOException("ReadArrayFromChannel.ReadIntFromByteChannel: " + ex.Message);
            }
        }
    }
}
