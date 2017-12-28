using NLPJDict.Kuromoji.Core.HelperClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.IO
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
