/**
 * Copyright © 2017-2018 Anki Universal Team.
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
