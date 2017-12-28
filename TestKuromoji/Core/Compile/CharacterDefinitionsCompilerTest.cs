using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLPJDict.Kuromoji.Core.Compile;
using NLPJDict.Kuromoji.Core.IO;
using NLPJDict.Kuromoji.Core;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Core.Buffer;
using System;
using System.Collections.Generic;
using NLPJDict.Kuromoji.Core.Trie;
using System.Text;
using System.Threading.Tasks;
using NLPJDict.KuromojiIpadic.Ipadic;
using System.IO;
using System.Linq;
using NLPJDict.Kuromoji.Core.Dict;

namespace NLPJDictTest.kuromoji.Core.Compile
{
    [TestClass]
    public class CharacterDefinitionsCompilerTest
    {        
        private static string charDef = TestUtils.CompiledPath + Path.DirectorySeparatorChar + "kuromoji-chardef-.bin";

        private SortedDictionary<int, string> categoryIdMap;

        private CharacterDefinitions characterDefinition;

        [TestInitialize]
        public void SetUp()
        {
            if (Directory.Exists(TestUtils.CompiledPath))
                Directory.Delete(TestUtils.CompiledPath, true);

            Directory.CreateDirectory(TestUtils.CompiledPath);

            using (var outputStream = File.Create(charDef))
            {
                CharacterDefinitionsCompiler compiler = new CharacterDefinitionsCompiler(CodePagesEncodingProvider.Instance);
                string assetFileName = @"./Core/Resource/char.def";                
                using (var defStream = File.OpenRead(assetFileName))
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    compiler.ReadCharacterDefinition(defStream, "euc-jp");
                    categoryIdMap = Invert(compiler.MakeCharacterCategoryMap());
                    compiler.Compile(outputStream);
                }
            }

            using (var input = File.OpenRead(charDef))
            using (BinaryReader reader = new BinaryReader(input))
            {
                int[][] definitions = IntegerArrayIO.ReadSparseArray2D(reader);
                int[][] mappings = IntegerArrayIO.ReadSparseArray2D(reader);

                string[] symbols = StringArrayIO.ReadArray(reader);
                characterDefinition = new CharacterDefinitions(definitions, mappings, symbols);
            }
        }

        [TestMethod]
        public void TestCharacterCategories()
        {
            // Non-defined characters get the default definition
            AssertCharacterCategories(characterDefinition, '\u0000', "DEFAULT");
            AssertCharacterCategories(characterDefinition, '〇', "SYMBOL", "KANJI", "KANJINUMERIC");
            AssertCharacterCategories(characterDefinition, ' ', "SPACE");
            AssertCharacterCategories(characterDefinition, '。', "SYMBOL");
            AssertCharacterCategories(characterDefinition, 'A', "ALPHA");
            AssertCharacterCategories(characterDefinition, 'Ａ', "ALPHA");
        }

        [TestMethod]
        public void TestAddCategoryDefinitions()
        {
            AssertCharacterCategories(characterDefinition, '・', "KATAKANA");

            characterDefinition.SetCategories('・', new string[] { "SYMBOL", "KATAKANA" });
            AssertCharacterCategories(characterDefinition, '・', "KATAKANA", "SYMBOL");
            AssertCharacterCategories(characterDefinition, '・', "SYMBOL", "KATAKANA");
        }

        public void AssertCharacterCategories(CharacterDefinitions characterDefinition, char c, params string[] categories)
        {
            int[] categoryIds = characterDefinition.LookupCategories(c);

            if (categoryIds == null)
            {
                Assert.IsNull(categories);
                return;
            }

            Assert.AreEqual(categories.Length, categoryIds.Length);

            List<string> categoryList = new List<string>(categories);

            foreach (int categoryId in categoryIds)
            {
                string category = categoryIdMap[categoryId];
                Assert.IsTrue(categoryList.Contains(category));
            }
        }

        private static SortedDictionary<int, string> Invert(SortedDictionary<string, int> map)
        {
            SortedDictionary<int, string> inverted = new SortedDictionary<int, string>();

            foreach (string key in map.Keys)
            {
                inverted[map[key]] = key;
            }

            return inverted;
        }
    }
}
