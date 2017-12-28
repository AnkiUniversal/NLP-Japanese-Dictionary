using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.HelperClasses
{
    public static class ConversionHelper
    {
        public static sbyte ToSignedByte(this byte value)
        {
            return unchecked((sbyte)value);        
        }
    }
}
