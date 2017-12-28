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
using System.IO;
using System.Threading.Tasks;

namespace Jocr
{
    public static class LdaVector
    {
        public const int LDA_VECTOR_LENGTH = 1296;
        public const int LDA_VECTOR_DIMENSION = 160;
        public static readonly float[] LDA_EIG_VECTOR;
        
        static LdaVector()
        {            
            int count = 0;
            using (Stream resource = Mqdf.MqdfParams.GetLdaVectorStream())
            using (BinaryReader stream = new BinaryReader(resource))
            {
                LDA_EIG_VECTOR = new float[LDA_VECTOR_DIMENSION * LDA_VECTOR_LENGTH];
                while (stream.BaseStream.Position < stream.BaseStream.Length)
                {
                    LDA_EIG_VECTOR[count] = stream.ReadSingle();
                    count++;
                }
            }
            if (count != LDA_EIG_VECTOR.Length)
                throw new Exception("Invalid LDA Vector file!");
        }

        public static void ReduceDimension()
        {                     
            Parallel.For(0, FeatureExtaction.Features.Length, (i) =>
           {
               double temp = 0;
               int index = i * FeatureExtaction.FEATURE_SIZE;
               for (int j = 0; j < FeatureExtaction.FeaturesFull.Length; j++)
               {
                   temp += FeatureExtaction.FeaturesFull[j] * LDA_EIG_VECTOR[index];
                   index++;
               }
               FeatureExtaction.Features[i] = (float)temp;
           });
        }
    }
}
