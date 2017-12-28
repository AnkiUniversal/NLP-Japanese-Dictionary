using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLPJDict.Kuromoji.Core.Compile;
using NLPJDict.Kuromoji.Core.IO;
using NLPJDict.Kuromoji.Core.Dict;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLPJDict.Kuromoji.Core;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Core.Buffer;

namespace NLPJDictTest.kuromoji.Core.Compile
{
    [TestClass]
    public class UnknownDictionaryCompilerTest
    {
        private string charDef = TestUtils.CompiledPath + Path.DirectorySeparatorChar + "kuromoji-tokeinfo-buffer-.bin";
        private static UnknownDictionary unknownDictionary;

        private static CharacterDefinitions characterDefinitions;

        private static int[][] costs;

        private static int[][] references;

        private static string[][] features;

        [TestInitialize]
        public void SetUp()
        {
            SortedDictionary<string, int> categoryMap;
            using (var outputStream = File.Create(charDef))
            {
                CharacterDefinitionsCompiler charDefCompiler = new CharacterDefinitionsCompiler(CodePagesEncodingProvider.Instance);
                string assetFileName = @"./Core/Resource/char.def";                
                using (var defStream = File.OpenRead(assetFileName))
                {
                    charDefCompiler.ReadCharacterDefinition(defStream, "euc-jp");
                    charDefCompiler.Compile(outputStream);
                }

                categoryMap = charDefCompiler.MakeCharacterCategoryMap();
            }

            var unkDefFile = TestUtils.CompiledPath + Path.DirectorySeparatorChar + "kuromoji-unkdef-.bin";
            using (var outputStream = File.Create(unkDefFile))
            {
                UnknownDictionaryCompiler unkDefCompiler = new UnknownDictionaryCompiler(categoryMap);
                string assetFileName = @"./Core/Resource/unk.def";                
                using (var defStream = File.OpenRead(assetFileName))
                {
                    unkDefCompiler.ReadUnknownDefinition(defStream, "euc-jp");
                    unkDefCompiler.Compile(outputStream);
                }
            }

            using (var charDefInput = File.OpenRead(charDef))
            using (var reader = new BinaryReader(charDefInput))
            {
                int[][] definitions = IntegerArrayIO.ReadSparseArray2D(reader);
                int[][] mappings = IntegerArrayIO.ReadSparseArray2D(reader);
                string[] symbols = StringArrayIO.ReadArray(reader);

                characterDefinitions = new CharacterDefinitions(definitions, mappings, symbols);
            }

            using (var unkDefInput = File.OpenRead(unkDefFile))
            using(var reader = new BinaryReader(unkDefInput))
            {
                costs = IntegerArrayIO.ReadArray2D(reader);
                references = IntegerArrayIO.ReadArray2D(reader);
                features = StringArrayIO.ReadArray2D(reader);

                unknownDictionary = new UnknownDictionary(characterDefinitions, references, costs, features);
            }
        }

        [TestMethod]
        public void TestCostsAndFeatures()
        {
            int[] categories = characterDefinitions.LookupCategories('一');

            // KANJI & KANJINUMERIC
            Assert.AreEqual(2, categories.Length);

            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(new int[] { 5, 6 }, categories));

            // KANJI entries
            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new int[] { 2, 3, 4, 5, 6, 7 },
                unknownDictionary.LookupWordIds(categories[0])
            ));

            // KANJI feature variety
            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new string[] { "名詞", "一般", "*", "*", "*", "*", "*" },
                unknownDictionary.GetAllFeaturesArray(2)
            ));

            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new string[] { "名詞", "サ変接続", "*", "*", "*", "*", "*" },
                unknownDictionary.GetAllFeaturesArray(3)
            ));

            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new string[] { "名詞", "固有名詞", "地域", "一般", "*", "*", "*" },
                unknownDictionary.GetAllFeaturesArray(4)
            ));

            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new string[] { "名詞", "固有名詞", "組織", "*", "*", "*", "*" },
                unknownDictionary.GetAllFeaturesArray(5)
            ));

            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new string[] { "名詞", "固有名詞", "人名", "一般", "*", "*", "*" },
                unknownDictionary.GetAllFeaturesArray(6)
            ));

            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new string[] { "名詞", "固有名詞", "人名", "一般", "*", "*", "*" },
                unknownDictionary.GetAllFeaturesArray(6)
            ));

            // KANJINUMERIC entry
            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new int[] { 29 },
                unknownDictionary.LookupWordIds(categories[1])
            ));

            // KANJINUMERIC costs
            Assert.AreEqual(1295, unknownDictionary.GetLeftId(29));
            Assert.AreEqual(1295, unknownDictionary.GetRightId(29));
            Assert.AreEqual(27473, unknownDictionary.GetWordCost(29));

            // KANJINUMERIC features
            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new string[] { "名詞", "数", "*", "*", "*", "*", "*" },
                unknownDictionary.GetAllFeaturesArray(29)
            ));
        }
    }
}
