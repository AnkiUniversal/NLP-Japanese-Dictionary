using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLPJDict.Kuromoji.Core.Compile;
using NLPJDict.Kuromoji.Core.IO;
using NLPJDict.Kuromoji.Core;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Core.Buffer;
using NLPJDict.Kuromoji.Core.FST;

namespace NLPJDictTest.kuromoji.Core.FST
{
    [TestClass]
    public class StateTest
    {
        [TestMethod]
        public void TestBinarySearchArc()
        {
            State state = new State();
            State destState = new State();

            Arc ArcA = state.SetArc('a', 1, destState);
            Arc ArcB = state.SetArc('b', 1, destState);
            Arc ArcC = state.SetArc('c', 1, destState);

            Assert.AreEqual(ArcA, state.BinarySearchArc('a', 0, state.Arcs.Count));
            Assert.AreEqual(ArcB, state.BinarySearchArc('b', 0, state.Arcs.Count));
            Assert.AreEqual(ArcC, state.BinarySearchArc('c', 0, state.Arcs.Count));
            Assert.IsNull(state.BinarySearchArc('d', 0, state.Arcs.Count));
        }

        [TestMethod]
        public void TestFindArcWithFourStates()
        {
            State state = new State();
            State destState = new State();
            Arc ArcA = state.SetArc('a', 1, destState);
            Arc ArcB = state.SetArc('b', 1, destState);
            Arc ArcC = state.SetArc('c', 1, destState);
            Arc ArcD = state.SetArc('d', 1, destState);

            Assert.AreEqual(ArcA, state.FindArc('a'));
            Assert.AreEqual(ArcB, state.FindArc('b'));
            Assert.AreEqual(ArcC, state.FindArc('c'));
            Assert.AreEqual(ArcD, state.FindArc('d'));
        }

        [TestMethod]
        public void TestFindArcWithSurrogatePairs()
        {
            State state = new State();
            State destState = new State();
            Arc ArcA = state.SetArc('a', 1, destState);
            Arc ArcB = state.SetArc('b', 1, destState);
            Arc ArcC = state.SetArc('c', 1, destState);

            string surrogateOne = "𥝱"; // U+25771
            Arc ArcD = state.SetArc((surrogateOne[0]), 1, destState); // surrogate pair
            Arc ArcE = state.SetArc(surrogateOne[1], 1, destState); // surrogate pair

            Assert.AreEqual(ArcA, state.FindArc('a'));
            Assert.AreEqual(ArcB, state.FindArc('b'));
            Assert.AreEqual(ArcD, state.FindArc(surrogateOne[0]));
            Assert.AreEqual(ArcE, state.FindArc(surrogateOne[1]));
        }
    }
}
