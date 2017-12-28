using NLPJDict.Kuromoji.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Compile
{
    public class WordIdMapCompiler : ICompiler
    {

        private int[][] wordIds = new int[1][];

        private int[] indices;

        private GrowableIntArray wordIdArray = new GrowableIntArray();

        public void AddMapping(int sourceId, int wordId)
        {
            if (wordIds.Length <= sourceId)
            {
                int[][] newArray = new int[sourceId + 1][];
                Array.Copy(wordIds, 0, newArray, 0, wordIds.Length);
                wordIds = newArray;
            }

            // Prepare array -- extend the length of array by one
            int[] current = wordIds[sourceId];
            if (current == null)
            {
                current = new int[1];
            }
            else
            {
                int[] newArray = new int[current.Length + 1];
                Array.Copy(current, 0, newArray, 0, current.Length);
                current = newArray;
            }
            wordIds[sourceId] = current;

            int[] targets = wordIds[sourceId];
            targets[targets.Length - 1] = wordId;
        }

        public void Write(Stream output)
        {
            try
            {
                lock (output)
                {
                    BinaryWriter writer = new BinaryWriter(output);
                    Compile(null);
                    IntegerArrayIO.WriteArray(writer, indices);
                    IntegerArrayIO.WriteArray(writer, wordIdArray.GetArray());
                }
                
            }
            catch (IOException ex)
            {
                throw new Exception("WordIdMapCompiler.Write: " + ex.Message);
            }
        }

        public void Compile(Stream output)
        {
            this.indices = new int[wordIds.Length];
            int wordIdIndex = 0;

            for (int i = 0; i < wordIds.Length; i++)
            {
                int[] inner = wordIds[i];

                if (inner == null)
                {
                    indices[i] = -1;
                }
                else
                {
                    indices[i] = wordIdIndex;
                    wordIdArray.Set(wordIdIndex++, inner.Length);

                    for (int j = 0; j < inner.Length; j++)
                    {
                        wordIdArray.Set(wordIdIndex++, inner[j]);
                    }
                }
            }
        }

        public class GrowableIntArray
        {

            private const float ARRAY_GROWTH_RATE = 1.25f;

            private const int ARRAY_INITIAL_SIZE = 1024;

            private int maxIndex;

            private int[] array;

            public GrowableIntArray(int size)
            {
                this.array = new int[size];
                this.maxIndex = 0;
            }

            public GrowableIntArray() : this(ARRAY_INITIAL_SIZE)
            {
            }

            public int[] GetArray()
            {
                int length = maxIndex + 1;
                int[] a = new int[length];
                Array.Copy(array, 0, a, 0, length);
                return a;
            }

            public void Set(int index, int value)
            {
                if (index >= array.Length)
                {
                    Grow(GetNewLength(index));
                }

                if (index > maxIndex)
                {
                    maxIndex = index;
                }

                array[index] = value;
            }

            private void Grow(int newLength)
            {
                int[] tmp = new int[newLength];
                Array.Copy(array, 0, tmp, 0, maxIndex + 1);
                array = tmp;
            }

            private int GetNewLength(int index)
            {
                return (int)Math.Max(
                    index + 1,
                    array.Length * ARRAY_GROWTH_RATE
                );
            }
        }
    }
}
