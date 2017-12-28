using NLPJDict.Kuromoji.Core.Dict;
using NLPJDict.Kuromoji.Core.Viterbi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core
{
    public abstract class TokenBase
    {
        private const int META_DATA_SIZE = 4;

        private readonly IDictionary dictionary;
        private readonly int wordId;
        private readonly string surface;
        private readonly int position;
        protected readonly ViterbiNode.NodeType type;

        public string Surface { get { return surface; } }

        public int Position { get { return position; } }

        public TokenBase(int wordId, string surface, ViterbiNode.NodeType type, int position, IDictionary dictionary)
        {
            this.wordId = wordId;
            this.surface = surface;
            this.type = type;
            this.position = position;
            this.dictionary = dictionary;
        }

        /**
         * Predicate indicating whether this token is known (contained in the standard dictionary)
         *
         * @return true if the token is known, otherwise false
         */
        public bool IsKnown { get { return type == ViterbiNode.NodeType.KNOWN; } }

        /**
         * Predicate indicating whether this token is included is from the user dictionary
         * <p>
         * If a token is contained both in the user dictionary and standard dictionary, this method will return true
         *
         * @return true if this token is in user dictionary. false if not.
         */
        public bool IsUser { get { return type == ViterbiNode.NodeType.USER; } }

        /**
         * Gets all features for this token as a comma-separated String
         *
         * @return token features, not null
         */
        public string GetAllFeatures()
        {
            return dictionary.GetAllFeatures(wordId);
        }

        /**
         * Gets all features for this token as a String array
         *
         * @return token feature array, not null
         */
        public string[] GetAllFeaturesArray()
        {
            return dictionary.GetAllFeaturesArray(wordId);
        }

        public override string ToString()
        {
            return "Token{" +
                "surface='" + surface + '\'' +
                ", position=" + position +
                ", type=" + type +
                ", dictionary=" + dictionary +
                ", wordId=" + wordId +
                '}';
        }

        /**
         * Gets a numbered feature for this token
         *
         * @param feature  feature number
         * @return token feature, not null
         */
        protected string GetFeature(int feature)
        {
            return dictionary.GetFeature(wordId, feature - META_DATA_SIZE);
        }

    }
}
