using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Compile
{
    public interface ICompiler
    {
        /// <summary>
        /// Different with the original java ver. We need to pass an output stream into this mehod
        /// </summary>
        void Compile(Stream output);
    }
}
