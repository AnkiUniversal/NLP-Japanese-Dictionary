using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jocr
{
    public static class QuadraticDiscriminant
    {
        public const int NUMBER_OF_CLASS = 3615;
        public const int K_DIMENSION = 30;
        public const float MINOR_EIG_VALUE = 0.0002f;
        public const int CLASS_EIG_VECTOR_SIZE = K_DIMENSION * FeatureExtaction.FEATURE_REDUCE_SIZE;
        public const int EIG_VECTOR_SCALE = 10000;

        public static readonly Int16[] EIG_VECTOR;
        public static readonly float[] EIG_VALUE;
        public static readonly float[] MEAN_VECTOR;
        public static readonly float[] SUM_LOG;

        static QuadraticDiscriminant()
        {
            EIG_VECTOR = new Int16[CLASS_EIG_VECTOR_SIZE * NUMBER_OF_CLASS];
            MEAN_VECTOR = new float[FeatureExtaction.FEATURE_REDUCE_SIZE * NUMBER_OF_CLASS];
            EIG_VALUE = new float[K_DIMENSION * NUMBER_OF_CLASS];
            SUM_LOG = new float[NUMBER_OF_CLASS];
            using (var file = Mqdf.MqdfParams.GetMqdfStream())
            using(var reader = new BinaryReader(file))
            {
                while(reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    for (int index = 0; index < NUMBER_OF_CLASS; index++)
                    {
                        int j = index * K_DIMENSION;
                        for (int i = 0; i < K_DIMENSION; i++, j++)                        
                            EIG_VALUE[j] = reader.ReadSingle();

                        j = index * CLASS_EIG_VECTOR_SIZE;
                        for (int i = 0; i < CLASS_EIG_VECTOR_SIZE; i++, j++)                        
                            EIG_VECTOR[j] = reader.ReadInt16();

                        j = index * FeatureExtaction.FEATURE_REDUCE_SIZE;
                        for (int i = 0; i < FeatureExtaction.FEATURE_REDUCE_SIZE; i++, j++)                        
                            MEAN_VECTOR[j] = reader.ReadSingle();                        

                        SUM_LOG[index] = reader.ReadSingle();
                    }
                }
            }

        }
    }
}
