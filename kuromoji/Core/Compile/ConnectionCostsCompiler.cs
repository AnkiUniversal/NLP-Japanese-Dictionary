using NLPJDict.Kuromoji.Core.HelperClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Compile
{
    public class ConnectionCostsCompiler : ICompiler, IDisposable
    {
        private int cardinality;
        public int Cardinality
        {
            get { return cardinality; }
        }

        private int bufferSize;

        public MemoryStreamWrapper costs;
        public MemoryStreamWrapper Costs
        {
            get { return costs; }
        }

        public void ReadCosts(Stream input)
        {
            using (StreamReader reader = new StreamReader(input))
            {
                string line = reader.ReadLine();
                string[] cardinalities = line.SplitSpace();

                int forwardSize = Int32.Parse(cardinalities[0]);
                int backwardSize = Int32.Parse(cardinalities[1]);

                cardinality = backwardSize;
                bufferSize = forwardSize * backwardSize * Constant.SHORT_BYTES;

                if (costs != null)
                    costs.Dispose();

                costs = new MemoryStreamWrapper(new MemoryStream(bufferSize));

                while (!reader.EndOfStream)
                {
                    string[] fields = reader.ReadLine().SplitSpace();

                    short forwardId = Int16.Parse(fields[0]);
                    short backwardId = Int16.Parse(fields[1]);
                    short cost = Int16.Parse(fields[2]);

                    PutCost(forwardId, backwardId, cost);
                }
            }
        }

        public void PutCost(short forwardId, short backwardId, short cost)
        {
            this.costs.WriteInt16(cost, (backwardId + forwardId * cardinality)*Constant.SHORT_BYTES);
        }

        public void Compile(Stream outputStream)
        {
            try
            {
                lock (outputStream)
                {
                    using (BinaryWriter dataOut = new BinaryWriter(outputStream))
                    {
                        dataOut.WriteRawInt32(cardinality);
                        dataOut.WriteRawInt32(bufferSize);
                        var buffer = costs.Stream.ToArray();
                        dataOut.Write(buffer);
                    }
                }
            }
            catch (IOException ex)
            {
                throw new IOException("ConnectionCostsCompiler.Compile: " + ex.Message);
            }
        }

        public void Dispose()
        {
            if(costs != null)
                costs.Dispose();
        }
    }
}
