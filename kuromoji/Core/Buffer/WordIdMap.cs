using NLPJDict.Kuromoji.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Buffer
{
    public class WordIdMap
    {

        private readonly int[] indices;

        private readonly int[] wordIds;

        private readonly int[] empty = new int[] { };

        public WordIdMap(Stream input)
        {
            try
            {
                lock (input)
                {
                    BinaryReader reader = new BinaryReader(input);
                    int[][] arrays = IntegerArrayIO.ReadArrays(reader, 2);
                    indices = arrays[0];
                    wordIds = arrays[1];
                }
            }
            catch (Exception ex)
            {
                throw new IOException("WordIdMap Contructor: " + ex.Message);
            }
        }

        public int[] LookUp(int sourceId)
        {
            int index = indices[sourceId];

            if (index == -1)
            {
                return empty;
            }
            int[] subArray = new int[wordIds[index]];
            Array.Copy(wordIds, index + 1, subArray, 0, wordIds[index]);
            return subArray;
        }
    }
}
