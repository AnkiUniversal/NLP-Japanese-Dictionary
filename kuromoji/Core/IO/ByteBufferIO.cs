using NLPJDict.Kuromoji.Core.HelperClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.IO
{
    public class ByteBufferIO
    {
        public const int BUFFER_CAPACITY = 1024*1000;

        public static MemoryStream Read(Stream input)
        {
            try
            {
                lock (input)
                {
                    BinaryReader dataInput = new BinaryReader(input);
                    return Read(dataInput);
                }
            }
            catch (Exception e)
            {
                throw new IOException("ByteBufferIO.Read: " + e.Message);
            }
        }

        public static MemoryStream Read(BinaryReader dataInput)
        {
            lock (dataInput)
            {
                int size = dataInput.ReadRawInt32();
                MemoryStream outStream = new MemoryStream(size);
                long remaining = dataInput.BaseStream.Length - dataInput.BaseStream.Position;
     
                while (remaining > 0)
                {
                    if (remaining > BUFFER_CAPACITY)
                        outStream.Write(dataInput.ReadBytes(BUFFER_CAPACITY), 0, BUFFER_CAPACITY);
                    else
                        outStream.Write(dataInput.ReadBytes((int)remaining), 0, (int)remaining);

                    remaining = dataInput.BaseStream.Length - dataInput.BaseStream.Position;
                }

                outStream.Position = 0;
                return outStream;
            }
        }

        public static void Write(Stream output, MemoryStream inputStream)
        {
            try
            {
                lock (output)
                {
                    BinaryWriter dataOutput = new BinaryWriter(output);

                    var oldPosition = inputStream.Position;
                    inputStream.Position = 0;
                    dataOutput.WriteRawInt32((int)inputStream.Length);
                    byte[] buffer = new byte[BUFFER_CAPACITY];
                    int readLength;

                    while (inputStream.HasRemaining())
                    {
                        readLength = inputStream.Read(buffer, 0, buffer.Length);
                        dataOutput.Write(buffer, 0, readLength);
                    }
                }
            }
            catch (Exception e)
            {
                throw new IOException("ByteBufferIO.Write: " + e.Message);
            }
        }
    }
}
