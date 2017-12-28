using NLPJDict.Kuromoji.Core.Compile;
using System.Text;

namespace NLPJDict.KuromojiIpadic.Compile
{
    public class DictionaryCompiler : DictionaryCompilerBase<DictionaryEntry>
    {
        public static void StartCompile(string[] args, EncodingProvider provider)
        {
            DictionaryCompiler dictionaryBuilder = new DictionaryCompiler();
            dictionaryBuilder.Build(args, provider);
        }

        protected override TokenInfoDictionaryCompilerBase<DictionaryEntry> GetTokenInfoDictionaryCompiler(string encoding, EncodingProvider provider)
        {
            return new TokenInfoDictionaryCompiler(encoding, provider);
        }

    }
}
