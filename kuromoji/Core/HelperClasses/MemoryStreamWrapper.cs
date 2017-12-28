using System;
using System.IO;

namespace NLPJDict.Kuromoji.Core.HelperClasses
{
    public class MemoryStreamWrapper : IDisposable
    {
        private MemoryStream stream;

        public MemoryStream Stream { get { return stream; } }

        public MemoryStreamWrapper(MemoryStream stream)
        {
            this.stream = stream;
        }

        public void Dispose()
        {
            stream.Dispose();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }

        public int ReadInt32(int offset)
        {
            var buffer = ReadStreamAt(offset, Constant.INTEGER_BYTES);
            return BitConverter.ToInt32(buffer, 0);
        }

        public int ReadInt32()
        {
            byte[] buffer = new byte[Constant.INTEGER_BYTES];
            lock (stream)
            {
                stream.Read(buffer, 0, Constant.INTEGER_BYTES);
            }
            return BitConverter.ToInt32(buffer, 0);
        }

        public short ReadInt16(int offset)
        {
            var buffer = ReadStreamAt(offset, Constant.SHORT_BYTES);
            return BitConverter.ToInt16(buffer, 0);
        }

        public short ReadInt16()
        {
            byte[] buffer = new byte[Constant.SHORT_BYTES];
            lock (stream)
            {
                stream.Read(buffer, 0, Constant.SHORT_BYTES);
            }          
            return BitConverter.ToInt16(buffer, 0);
        }

        public byte ReadByteAt(int offset)
        {
            var buffer = ReadStreamAt(offset, 1);
            return buffer[0];
        }

        private byte[] ReadStreamAt(int offset, int numberOfBytes)
        {
            lock (stream)
            {
                byte[] buffer = new byte[numberOfBytes];
                var oldPos = stream.Position;
                stream.Position = offset;
                stream.Read(buffer, 0, numberOfBytes);
                stream.Position = oldPos;
                return buffer;
            }
        }

        /// <summary>
        /// Write 4 bytes to MemoryStream and auto reverse if IsLittleEndian
        /// Stream position is not changed
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        public void WriteInt32(int value, int offset)
        {
            var buffer = BitConverter.GetBytes(value);
            WriteStreamAt(buffer, offset, Constant.INTEGER_BYTES);
        }

        /// <summary>
        /// At the current posiion, write 4 bytes to MemoryStream and auto reverse if IsLittleEndian
        /// Stream position is then advanced
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        public void WriteInt32(int value)
        {
            var buffer = BitConverter.GetBytes(value);
            lock (stream)
            {
                stream.Write(buffer, 0, Constant.INTEGER_BYTES);
            }
        }

        /// <summary>
        /// Write 2 bytes to MemoryStream and auto reverse if IsLittleEndian
        /// Stream position is not changed
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        public void WriteInt16(short value, int offset)
        {
            var buffer = BitConverter.GetBytes(value);
            WriteStreamAt(buffer, offset, Constant.SHORT_BYTES);
        }

        /// <summary>
        /// At the current posiion, write 2 bytes to MemoryStream and auto reverse if IsLittleEndian
        /// Stream position is then advanced
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        public void WriteInt16(short value)
        {
            var buffer = BitConverter.GetBytes(value);
            lock (stream)
            {
                stream.Write(buffer, 0, Constant.SHORT_BYTES);
            }
        }

        private void WriteStreamAt(byte[] buffer, int offset, int numberOfBytes)
        {
            lock (stream)
            {
                var oldPos = stream.Position;
                stream.Position = offset;
                stream.Write(buffer, 0, numberOfBytes);
                stream.Position = oldPos;
            }
        }
    }
}
