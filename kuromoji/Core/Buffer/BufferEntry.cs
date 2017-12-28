using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Buffer
{
    public class BufferEntry
    {
        public List<short> TokenInfo { get; set; } = new List<short>();
        public List<int> Features { get; set; } = new List<int>();
        public List<byte> PosInfo { get; set; } = new List<byte>();

        public short[] TokenInfos { get; set; } // left id, right id, word cost values
        public int[] FeatureInfos { get; set; } // references to string features
        public byte[] PosInfos { get; set; } // part-of-speech tag values

    }
}
