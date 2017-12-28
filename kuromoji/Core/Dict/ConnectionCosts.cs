using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Dict
{
    public class ConnectionCosts : IDisposable
    {

        public const string CONNECTION_COSTS_FILENAME = "connectionCosts.bin";

        private int size;

        private MemoryStreamWrapper costs;

        public ConnectionCosts(int size, MemoryStreamWrapper costs)
        {
            this.size = size;
            this.costs = costs;
        }

        public int Get(int forwardId, int backwardId)
        {
            return costs.ReadInt16((backwardId + forwardId * size)*Constant.SHORT_BYTES);
        }

        public static ConnectionCosts NewInstance(string resourceAbsolutePath)
        {
            try
            {
                using (Stream stream = File.OpenRead(resourceAbsolutePath + Path.DirectorySeparatorChar + CONNECTION_COSTS_FILENAME))
                {
                    return Read(stream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ConnectionCosts.NewInstance: " + ex.Message);
            }
        }

        private static ConnectionCosts Read(Stream input)
        {
            try
            {
                lock (input)
                {
                    using (BinaryReader reader = new BinaryReader(input))
                    {
                        int size = reader.ReadRawInt32();                        
                        var costs = new MemoryStreamWrapper(ByteBufferIO.Read(reader));
                        return new ConnectionCosts(size, costs);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ConnectionCosts.Read: " + ex.Message);
            }
        }

        public void Dispose()
        {
            if(costs != null)
                costs.Dispose();
        }
    }
}
