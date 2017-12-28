using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Dict
{
    public abstract class DictionaryEntryBase
    {
        protected readonly string surface;

        protected readonly short leftId;

        protected readonly short rightId;

        protected readonly short wordCost;

        public DictionaryEntryBase(string surface, short leftId, short rightId, int wordCost)
        {
            this.surface = surface;
            this.leftId = leftId;
            this.rightId = rightId;

            // TODO: Temporary work-around for UniDic NEologd to deal with costs outside the short value range
            if (wordCost < Int16.MinValue)
            {
                this.wordCost = Int16.MinValue;
            }
            else if (wordCost > Int16.MaxValue)
            {
                this.wordCost = Int16.MaxValue;
            }
            else
            {
                this.wordCost = (short)wordCost;
            }
        }

        public string GetSurface()
        {
            return surface;
        }

        public short GetLeftId()
        {
            return leftId;
        }

        public short GetRightId()
        {
            return rightId;
        }

        public short GetWordCost()
        {
            return wordCost;
        }
    }
}
