using NLPJDict.Kuromoji.Core.FST;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Compile
{
    public class FSTCompiler : ICompiler
    {

        private readonly string[] surfaces;

        public FSTCompiler(List<string> surfaces)
        {
            this.surfaces = new HashSet<string>(surfaces).ToArray<string>();
        }

        public void Compile(Stream output)
        {
            try
            {
                Array.Sort(surfaces, StringHelper.SortLexicographically);

                using (Builder builder = new Builder())
                {
                    builder.Build(surfaces, null);

                    MemoryStream fst = new MemoryStream(builder.GetCompiler().GetBytes());
                    ByteBufferIO.Write(output, fst);
                }
            }
            catch (IOException ex)
            {
                throw new Exception("FSTCompiler.Compile: " + ex.Message);
            }
        }

    }
}
