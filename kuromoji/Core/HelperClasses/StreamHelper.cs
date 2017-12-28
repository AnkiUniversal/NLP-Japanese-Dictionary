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
using System.IO;


namespace NLPJDict.Kuromoji.Core.HelperClasses
{
    public static class StreamHelper
    {
        /// <summary>
        /// WARNING: This method is not thread safe
        /// Use this function to avoid endian conversion of default ReadInt32
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static int ReadRawInt32(this BinaryReader stream)
        {
            byte[] buffer = stream.ReadBytes(4);
            return BitConverter.ToInt32(buffer, 0);
        }

        /// <summary>
        /// WARNING: This method is not thread safe
        /// Use this function to avoid endian conversion of default WriteInt32
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        public static void WriteRawInt32(this BinaryWriter stream, int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, 4);
        }

        public static bool IsNotFull(this MemoryStream buffer)
        {
            return buffer.Capacity - buffer.Position > 0;
        }

        public static bool HasRemaining(this MemoryStream buffer)
        {
            return buffer.Length - buffer.Position > 0;
        }

        public static MemoryStream SequenceInputStream(string[] files)
        {
            MemoryStream finalStream = new MemoryStream();
            foreach (var file in files)
            {
                var stream = File.ReadAllBytes(file);
                finalStream.Write(stream, 0, stream.Length);
            }
            finalStream.Position = 0;
            return finalStream;
        }


        public static void ReadArrayInt32(this BinaryReader stream, int[] value)
        {
            lock (stream)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    value[i] = stream.ReadRawInt32();
                }
            }
        }

        public static void WriteArrayInt32(this BinaryWriter stream, int[] value)
        {
            lock (stream)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    stream.WriteRawInt32(value[i]);
                }
            }
        }
    }
}
