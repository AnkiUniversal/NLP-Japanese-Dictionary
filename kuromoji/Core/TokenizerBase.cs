using NLPJDict.Kuromoji.Core.Dict;
using NLPJDict.Kuromoji.Core.Viterbi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core
{
    /**
     * <p>
     * The tokenization mode defines how Available modes are as follows:
     * <ul>
     * <li>{@link com.atilika.kuromoji.TokenizerBase.Mode#NORMAL} - The default mode
     * <li>{@link com.atilika.kuromoji.TokenizerBase.Mode#SEARCH} - Uses a heuristic to segment compound nouns (複合名詞) into their parts
     * <li>{@link com.atilika.kuromoji.TokenizerBase.Mode#EXTENDED} - Same as SEARCH, but emits unigram tokens for unknown terms
     * </ul>
     * See {@link #kanjiPenalty} and {@link #otherPenalty} for how to adjust costs used by SEARCH and EXTENDED mode
     *
     * @param mode  tokenization mode
     * @return this builder, not null
     */
    public enum Mode
    {
        NORMAL, SEARCH, EXTENDED
    }

    public abstract class TokenizerBase<T> : IDisposable where T : TokenBase
    {
        private ViterbiBuilder viterbiBuilder;

        private ViterbiSearcher viterbiSearcher;

        private ViterbiFormatter viterbiFormatter;

        private bool split;

        private TokenInfoDictionary tokenInfoDictionary;

        private UnknownDictionary unknownDictionary;

        private UserDictionary userDictionary;

        private InsertedDictionary insertedDictionary;

        protected Viterbi.TokenFactory<T> tokenFactory;

        protected Dictionary<ViterbiNode.NodeType, IDictionary> dictionaryMap = new Dictionary<ViterbiNode.NodeType, IDictionary>();

        public void Dispose()
        {
            if(viterbiBuilder != null)
                viterbiBuilder.Dispose();
            if(viterbiSearcher != null)
                viterbiSearcher.Dispose();
            if(viterbiFormatter != null)
                viterbiFormatter.Dispose();
            if(tokenInfoDictionary != null)
                tokenInfoDictionary.Dispose();

            viterbiBuilder = null;
            viterbiSearcher = null;
            viterbiFormatter = null;
            tokenInfoDictionary = null;
        }

        protected void Configure<V, K>(V builder) where V : Builder<K> where K : TokenizerBase<T>
        {
            builder.LoadDictionaries();

            this.tokenFactory = builder.TokenFactory;

            this.tokenInfoDictionary = builder.TokenInfoDictionary;
            this.unknownDictionary = builder.UnknownDictionary;
            this.userDictionary = builder.UserDictionary;
            this.insertedDictionary = builder.InsertedDictionary;

            this.viterbiBuilder = new ViterbiBuilder(
                builder.Fst,
                tokenInfoDictionary,
                unknownDictionary,
                userDictionary,
                builder.Mode
            );

            this.viterbiSearcher = new ViterbiSearcher(
                builder.Mode,
                builder.ConnectionCosts,
                unknownDictionary,
                builder.Penalties
            );

            this.viterbiFormatter = new ViterbiFormatter(builder.ConnectionCosts);
            this.split = builder.Split;

            InitDictionaryMap();
        }

        private void InitDictionaryMap()
        {
            dictionaryMap[ViterbiNode.NodeType.KNOWN] = tokenInfoDictionary;
            dictionaryMap[ViterbiNode.NodeType.UNKNOWN] = unknownDictionary;
            dictionaryMap[ViterbiNode.NodeType.USER] = userDictionary;
            dictionaryMap[ViterbiNode.NodeType.INSERTED] = insertedDictionary;
        }

        public virtual List<T> Tokenize(string text)
        {
            return CreateTokenList(text);
        }

        public List<LinkedList<T>> MultiTokenize(string text, int maxCount, int costSlack)
        {
            return CreateMultiTokenList(text, maxCount, costSlack);
        }

        public List<LinkedList<T>> MultiTokenizeNBest(string text, int n)
        {
            return MultiTokenize(text, n, int.MaxValue);
        }

        public List<LinkedList<T>> MultiTokenizeBySlack(string text, int costSlack)
        {
            return MultiTokenize(text, int.MaxValue, costSlack);
        }

        /**
         * Tokenizes the provided text and returns a list of tokens with various feature information
         * <p>
         * This method is thread safe
         *
         * @param text  text to tokenize
         * @param <T>  token type
         * @return list of Token, not null
         */
        protected List<T> CreateTokenList(string text)
        {
            if (!split)
            {
                return CreateTokenList(0, text);
            }

            List<int> splitPositions = GetSplitPositions(text);

            if (splitPositions.Count == 0)
            {
                return CreateTokenList(0, text);
            }

            List<T> result = new List<T>();

            int offset = 0;

            foreach (int position in splitPositions)
            {
                result.AddRange(this.CreateTokenList(offset, text.Substring(offset, position + 1 - offset)));
                offset = position + 1;
            }

            if (offset < text.Length)
            {
                result.AddRange(this.CreateTokenList(offset, text.Substring(offset)));
            }

            return result;
        }

        /**
         * Tokenizes the provided text and returns up to maxCount lists of tokens with various feature information.
         * Each list corresponds to a possible tokenization with cost at most OPT + costSlack, where OPT is the optimal solution.
         * <p>
         * This method is thread safe
         *
         * @param text  text to tokenize
         * @param maxCount  maximum number of different tokenizations
         * @param costSlack  maximum cost slack of a tokenization
         * @param <T>  token type
         * @return list of Token, not null
         */
        protected List<LinkedList<T>> CreateMultiTokenList(string text, int maxCount, int costSlack)
        {
            if (!split)
            {
                return ConvertMultiSearchResultToList(CreateMultiSearchResult(text, maxCount, costSlack));
            }

            List<int> splitPositions = GetSplitPositions(text);

            if (splitPositions.Count == 0)
            {
                return ConvertMultiSearchResultToList(CreateMultiSearchResult(text, maxCount, costSlack));
            }

            List<MultiSearchResult> results = new List<MultiSearchResult>();
            int offset = 0;

            foreach (int position in splitPositions)
            {
                results.Add(CreateMultiSearchResult(text.Substring(offset, position + 1 - offset), maxCount, costSlack));
                offset = position + 1;
            }

            if (offset < text.Length)
            {
                results.Add(CreateMultiSearchResult(text.Substring(offset), maxCount, costSlack));
            }

            MultiSearchMerger merger = new MultiSearchMerger(maxCount, costSlack);
            MultiSearchResult mergedResult = merger.Merge(results);

            return ConvertMultiSearchResultToList(mergedResult);
        }

        private List<LinkedList<T>> ConvertMultiSearchResultToList(MultiSearchResult multiSearchResult)
        {
            List<LinkedList<T>> result = new List<LinkedList<T>>();

            List<LinkedList<ViterbiNode>> paths = multiSearchResult.GetTokenizedResultsList();

            foreach (LinkedList<ViterbiNode> path in paths)
            {
                var tokens = new LinkedList<T>();
                foreach (ViterbiNode node in path)
                {
                    int wordId = node.WordId;
                    if (node.Type == ViterbiNode.NodeType.KNOWN && wordId == -1)
                    { // Do not include BOS/EOS
                        continue;
                    }
                    T token = tokenFactory.CreateToken(
                                   wordId,
                                   node.Surface,
                                   node.Type,
                                   node.StartIndex,
                                   dictionaryMap[node.Type]
                                   );
                    tokens.AddLast(token);
                }
                result.Add(tokens);
            }

            return result;
        }

        /**
         * Tokenizes the provided text and outputs the corresponding Viterbi lattice and the Viterbi path to the provided output stream
         * <p>
         * The output is written in <a href="https://en.wikipedia.org/wiki/DOT_(graph_description_language)">DOT</a> format.
         * <p>
         * This method is not thread safe
         *
         * @param outputStream  output stream to write to
         * @param text  text to tokenize
         * @throws java.io.IOException if an error occurs when writing the lattice and path
         */
        public void DebugTokenize(Stream outputStream, string text)
        {
            ViterbiLattice lattice = viterbiBuilder.Build(text);
            var bestPath = viterbiSearcher.Search(lattice);
            using (var writer = new StreamWriter(outputStream, Encoding.UTF8))
            {
                var bytes = Encoding.UTF8.GetBytes(viterbiFormatter.Format(lattice, bestPath));
                outputStream.Write(bytes, 0, bytes.Length);
            }
        }

        /**
         * Writes the Viterbi lattice for the provided text to an output stream
         * <p>
         * The output is written in <a href="https://en.wikipedia.org/wiki/DOT_(graph_description_language)">DOT</a> format.
         * <p>
         * This method is not thread safe
         *
         * @param outputStream  output stream to write to
         * @param text  text to create lattice for
         * @throws java.io.IOException if an error occurs when writing the lattice
         */
        public void DebugLattice(Stream outputStream, string text)
        {
            ViterbiLattice lattice = viterbiBuilder.Build(text);
            using (var writer = new StreamWriter(outputStream, Encoding.UTF8))
            {
                var bytes = Encoding.UTF8.GetBytes(viterbiFormatter.Format(lattice));
                outputStream.Write(bytes, 0, bytes.Length);
            }
        }

        /**
         * Split input text at 句読点, which is 。 and 、
         *
         * @param text
         * @return list of split position
         */
        private List<int> GetSplitPositions(string text)
        {
            var splitPositions = new List<int>();
            int position;
            int currentPosition = 0;

            while (true)
            {
                int indexOfMaru = text.IndexOf("。", currentPosition);
                int indexOfTen = text.IndexOf("、", currentPosition);

                if (indexOfMaru < 0 || indexOfTen < 0)
                {
                    position = Math.Max(indexOfMaru, indexOfTen);
                }
                else
                {
                    position = Math.Min(indexOfMaru, indexOfTen);
                }

                if (position >= 0)
                {
                    splitPositions.Add(position);
                    currentPosition = position + 1;
                }
                else
                {
                    break;
                }
            }

            return splitPositions;
        }

        /**
         * Tokenize input sentence.
         *
         * @param offset   offset of sentence in original input text
         * @param text sentence to tokenize
         * @return list of Token
         */
        private List<T> CreateTokenList(int offset, string text)
        {
            List<T> result = new List<T>();

            ViterbiLattice lattice = viterbiBuilder.Build(text);
            List<ViterbiNode> bestPath = viterbiSearcher.Search(lattice);

            foreach (ViterbiNode node in bestPath)
            {
                int wordId = node.WordId;
                if (node.Type == ViterbiNode.NodeType.KNOWN && wordId == -1)
                { // Do not include BOS/EOS
                    continue;
                }
                T token = (T)tokenFactory.CreateToken(
                        wordId,
                        node.Surface,
                        node.Type,
                        offset + node.StartIndex,
                        dictionaryMap[node.Type]
                    );
                result.Add(token);
            }
            return result;
        }

        /**
         * Tokenize input sentence. Up to maxCount different paths of cost at most OPT + costSlack are returned ordered in ascending order by cost, where OPT is the optimal solution.
         *
         * @param text sentence to tokenize
         * @param maxCount  maximum number of paths
         * @param costSlack  maximum cost slack of a path
         * @return  instance of MultiSearchResult containing the tokenizations
         */
        private MultiSearchResult CreateMultiSearchResult(string text, int maxCount, int costSlack)
        {
            ViterbiLattice lattice = viterbiBuilder.Build(text);
            MultiSearchResult multiSearchResult = viterbiSearcher.SearchMultiple(lattice, maxCount, costSlack);
            return multiSearchResult;
        }

        /**
         * Abstract Builder shared by all tokenizers
         */
        public abstract class Builder<K> : IDisposable where K : TokenizerBase<T>
        {
            protected int totalFeatures = -1;
            protected int readingFeature = -1;
            protected int pronunFeature = -1;
            protected int partOfSpeechFeature = -1;

            public ConnectionCosts ConnectionCosts { get; protected set; }
            public TokenInfoDictionary TokenInfoDictionary { get; protected set; }
            public UnknownDictionary UnknownDictionary { get; protected set; }
            public CharacterDefinitions CharacterDefinitions { get; protected set; }
            public InsertedDictionary InsertedDictionary { get; protected set; }
            public UserDictionary UserDictionary { get; protected set; } = null;
            public string AbsoluteFolderPath { get; protected set; } = null;

            public TokenFactory<T> TokenFactory { get; protected set; }

            public FST.FST Fst { get; protected set; }

            public Mode Mode { get; set; } = Mode.NORMAL;
            public List<int> Penalties { get; protected set; } = new List<int>();
            public bool Split { get; protected set; } = true;

            public virtual void LoadDictionaries()
            {
                try
                {
                    Fst = FST.FST.NewInstance(AbsoluteFolderPath);
                    ConnectionCosts = ConnectionCosts.NewInstance(AbsoluteFolderPath);
                    TokenInfoDictionary = TokenInfoDictionary.NewInstance(AbsoluteFolderPath);
                    CharacterDefinitions = CharacterDefinitions.NewInstance(AbsoluteFolderPath);
                    UnknownDictionary = UnknownDictionary.NewInstance(AbsoluteFolderPath, CharacterDefinitions, totalFeatures);
                    InsertedDictionary = new InsertedDictionary(totalFeatures);
                }
                catch (Exception ouch)
                {
                    throw new Exception("Could not load dictionaries.", ouch);
                }
            }

            /**
             * Creates a Tokenizer instance defined by this Builder
             *
             * @param <T> token type
             * @return Tokenizer instance
             */
            public abstract K Build();

            /**
             * Sets an optional user dictionary as an input stream
             * <p>
             * The inpuut stream provided is not closed by this method
             *
             * @param input  user dictionary as an input stream
             * @return this builder
             * @throws java.io.IOException if an error occurs when reading the user dictionary
             */
            public Builder<K> LoadUserDictionary(Stream input)
            {
                this.UserDictionary = new UserDictionary(input, totalFeatures, readingFeature, partOfSpeechFeature);
                return this;
            }

            /**
             * Sets an optional user dictionary filename
             *
             * @param filename  user dictionary filename
             * @return this builder
             * @throws java.io.IOException if an error occurs when reading the user dictionary
             */
            public Builder<K> LoadUserDictionary(string absoluteFilePath)
            {
                using (var input = File.OpenRead(absoluteFilePath))
                {
                    this.LoadUserDictionary(input);
                    return this;
                }
            }

            public void Dispose()
            {
                if(ConnectionCosts != null)
                    ConnectionCosts.Dispose();
                if(TokenInfoDictionary != null)
                    TokenInfoDictionary.Dispose();
            }
        }
    }
}
