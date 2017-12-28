using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.HelperClasses
{
    public static class ArrayHelper
    {
        public static void Fill<T>(this T[] array, T value)
        {
            for(int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        public static int GetJavaListHashCode<T>(this List<T> values)
        {
            int hashCode = 1;
            for (int i = 0; i < values.Count; i++)
                hashCode = 31 * hashCode + (values[i] == null ? 0 : values[i].GetHashCode());
            return hashCode;
        }

        public static bool AreEqual<T>(List<T> compared, List<T> comparer)
        {
            if (compared.Count != comparer.Count)
                return false;

            for(int i = 0; i < compared.Count; i++)
            {
                if (!compared[i].Equals(comparer[i]))
                    return false;
            }
            return true;
        }
    }
}
