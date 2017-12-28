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
