using System;
using System.Collections.Generic;
using System.Text;

namespace Jocr
{
    public static class QuickSelect
    {
        private static float[] arrayValue;
        private static ushort[] arrayIndex;

        public static float Start(float[] value, ushort[] index, int left, int right, int rank)
        {
            arrayValue = value;
            arrayIndex = index;

            rank--; //Reduce one since counting from 0
            Random random = new Random();
            while (true)
            {
                if (left == right)
                    return arrayValue[left];

                int pivotIndex = random.Next(left, right);
                pivotIndex = Partition(left, right, pivotIndex);
                if (rank == pivotIndex)
                    return arrayValue[rank];
                else if (rank < pivotIndex)
                    right = pivotIndex - 1;
                else
                    left = pivotIndex + 1;
            }
        }

        private static int Partition(int left, int right, int pivotIndex)
        {
            float pivotValue = arrayValue[pivotIndex];
            Swap(pivotIndex, right);
            int storeIndex = left;
            for (int i = left; i < right; i++)
            {
                if (arrayValue[i] < pivotValue)
                {
                    Swap(storeIndex, i);
                    storeIndex++;
                }
            }
            Swap(storeIndex, right);
            return storeIndex;
        }

        private static void Swap(int left, int right)
        {
            float temp = arrayValue[left];
            arrayValue[left] = arrayValue[right];
            arrayValue[right] = temp;

            ushort tempIndex = arrayIndex[left];
            arrayIndex[left] = arrayIndex[right];
            arrayIndex[right] = tempIndex;
        }
    }
}
