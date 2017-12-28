/**
 * Copyright © 2010-2017 Atilika Inc. and contributors (see CONTRIBUTORS.md)
 * 
 * Modifications copyright (C) 2017 - 2018 Anki Universal Team <ankiuniversal@gmail.com>
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may
 * not use this file except in compliance with the License.  A copy of the
 * License is distributed with this work in the LICENSE.md file.  You may
 * also obtain a copy of the License from
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using NLPJDict.Kuromoji.Core.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace NLPJDict.Kuromoji.Core.Trie
{
    public class PatriciaTrie<V> : IDictionary<string, V>  
    {
        /** Root value is left -- right is unused */
        protected PatriciaNode<V> root;

        /** Number of entries in the trie */
        protected int entries = 0;

        /** Maps String keys to bits */
        private readonly KeyMapper<string> keyMapper = new StringKeyMapper();

        public ICollection<string> Keys
        {
            get
            {
                HashSet<string> keys = new HashSet<string>();
                KeysR(root.Left, -1, keys);
                return keys;
            }
        }

        public ICollection<V> Values
        {
            get
            {
                List<V> values = new List<V>();
                ValuesR(root.Left, -1, values);
                return values;
            }
        }

        public int Count
        {
            get
            {
               return entries;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public V this[string key]
        {
            get
            {
                return Get(key);
            }

            set
            {
                Put(key, value);
            }
        }

        /**
         * Constructs and empty trie
         */
        public PatriciaTrie()
        {
            Clear();
        }

        /**
         * Get value associated with specified key in this trie
         *
         * @param key  key to retrieve value for
         * @return value or null if non-existent
         */
        private V Get(string key)
        {
            // Keys can not be null
            if (key == null)
            {
                throw new ArgumentNullException("Key can not be null");
            }
            // Empty keys are stored in the root
            if (key.Equals(""))
            {
                if (root.Right == null)
                {
                    return default(V);
                }
                else
                {
                    return root.Right.Value;
                }
            }

            // Find nearest node
            PatriciaNode<V> nearest = FindNearestNode(key);

            // If the nearest node matches key, we have a match
            if (key.Equals(nearest.Key))
            {
                return nearest.Value;
            }
            else
            {
                return default(V);
            }
        }

        /**
         * Puts value into trie identifiable by key into this trie (key should be non-null)
         *
         * @param key  key to associate with value
         * @param value  value be inserted
         * @return value inserted
         * @throws NullPointerException in case key is null
         */
        private V Put(string key, V value)
        {
            // Keys can not be null
            if (key == null)
            {
                throw new ArgumentNullException("Key can not be null");
            }

            // Empty keys are stored in the root
            if (key.Equals(""))
            {
                PatriciaNode<V> nodeP = new PatriciaNode<V>(key, value, -1);
                nodeP.Value = value;
                root.Right = nodeP;
                entries++;
                return value;
            }

            // Find nearest node
            PatriciaNode<V> nearest = FindNearestNode(key);

            // Key already exist, replace value and return
            if (key.Equals(nearest.Key))
            {
                nearest.Value = value;
                return value;
            }

            // Find differing bit and create new node to insert
            int bit = FindFirstDifferingBit(key, nearest.Key);
            PatriciaNode<V> node = new PatriciaNode<V>(key, value, bit);

            // Insert new node
            InsertNode(node);

            entries++;

            return value;
        }

        /**
         * Inserts all key and value entries in a map into this trie
         *
         * @param map   map with entries to insert
         */
        public void PutAll(Dictionary<string, V> map)
        {
            foreach (KeyValuePair<string, V> entry in map)
            {
                Put(entry.Key, entry.Value);
            }
        }

        /**
         *  Test key prefix membership in this trie (prefix search using key)
         *
         * @param prefix  key prefix to search
         * @return true if trie contains key prefix
         */
        public bool ContainsKeyPrefix(string prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException("Prefix key can not be null");
            }

            // An empty string is a prefix of everything
            if (prefix.Equals(""))
            {
                return true;
            }

            // Find nearest node
            PatriciaNode<V> nearest = FindNearestNode(prefix);

            // If no nearest node exist, there isn't any prefix match either
            if (nearest == null)
            {
                return false;
            }

            // The nearest is the root, so no match
            if (nearest.Key == null)
            {
                return false;
            }

            // Test prefix match
            return nearest.Key.StartsWith(prefix);
        }

        public void Add(string key, V value)
        {
            Put(key, value);            
        }

        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("Key can not be null");
            }

            return Get(key) != null;
        }

        /**
         * Removes entry identified by key from this trie (currently unsupported)
         *
         * @param key to remove
         * @return value removed
         * @throws UnsupportedOperationException is always thrown since this operation is unimplemented
         */
        public bool Remove(string key)
        {
            throw new NotImplementedException("Remove is currently unsupported");
        }

        public bool TryGetValue(string key, out V value)
        {
            value = Get(key);
            if (value != null)
                return true;
            else
                return false;           
        }

        public void Add(KeyValuePair<string, V> item)
        {
            Put(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<string, V> item)
        {
            throw new NotImplementedException("Patriciarie.ContainsValue is not implemented");

            //var value = Get(item.Key);
            //if (value == null)
            //    return false;
            //else
            //    Wrong when V is an array:
            //    Java will compare each member of array while C# is reference
            //    return value.Equals(item.Value);
        }

        public void CopyTo(KeyValuePair<string, V>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, V> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, V>> GetEnumerator()
        {
            Dictionary<string, V> entries = new Dictionary<string, V>();
            EntriesR(root.Left, -1, entries);
            return entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool IsEmpty()
        {
            return entries == 0;
        }

        public void Clear()
        {
            root = new PatriciaNode<V>(null, default(V), -1);
            root.Left = root;
            entries = 0;
        }

        public bool ContainsValue(object value)
        {
            throw new NotImplementedException("Patriciarie.ContainsValue is not implemented");

            //foreach (V v in Values)
            //{
            //    Wrong when V is an array:
            //    Java will compare each member of array while C# is reference
            //    if (v.Equals(value)) 
            //    {
            //        return true;
            //    }
            //}
            //return false;
        }

        /**
         * Finds the closest node in the trie matching key
         *
         * @param key  key to look up
         * @return closest node, null null
         */
        private PatriciaNode<V> FindNearestNode(string key)
        {
            PatriciaNode<V> current = root.Left;
            PatriciaNode<V> parent = root;

            while (parent.Bit < current.Bit)
            {
                parent = current;
                if (!keyMapper.IsSet(current.Bit, key))
                {
                    current = current.Left;
                }
                else
                {
                    current = current.Right;
                }
            }
            return current;
        }

        /**
         * Returns the leftmost differing bit index when doing a bitwise comparison of key1 and key2
         *
         * @param key1  first key to compare
         * @param key2  second key to compare
         * @return bit index of first different bit
         */
        private int FindFirstDifferingBit(string key1, string key2)
        {
            int bit = 0;

            while (keyMapper.IsSet(bit, key1) == keyMapper.IsSet(bit, key2))
            {
                bit++;
            }
            return bit;
        }

        /**
         * Inserts a node into this trie
         *
         * @param node  node to insert
         */
        private void InsertNode(PatriciaNode<V> node)
        {
            PatriciaNode<V> current = root.Left;
            PatriciaNode<V> parent = root;

            while (parent.Bit < current.Bit && current.Bit < node.Bit)
            {
                parent = current;
                if (!keyMapper.IsSet(current.Bit, node.Key))
                {
                    current = current.Left;
                }
                else
                {
                    current = current.Right;
                }
            }

            if (!keyMapper.IsSet(node.Bit, node.Key))
            {
                node.Left = node;
                node.Right = current;
            }
            else
            {
                node.Left = current;
                node.Right = node;
            }

            if (!keyMapper.IsSet(parent.Bit, node.Key))
            {
                parent.Left = node;
            }
            else
            {
                parent.Right = node;
            }
        }

        /**
         * Should only be used by {@link PatriciaTrieFormatter}
         *
         * @return trie root, not null
         */
        public PatriciaNode<V> GetRoot()
        {
            return root;
        }

        /**
         * Should only be used by {@link PatriciaTrieFormatter}
         *
         * @return key mapper used to map key to bit strings
         */
        public KeyMapper<string> GetKeyMapper()
        {
            return keyMapper;
        }

        private void ValuesR(PatriciaNode<V> node, int bit, List<V> list)
        {
            if (node.Bit <= bit)
            {
                return;
            }
            else
            {
                ValuesR(node.Left, node.Bit, list);
                ValuesR(node.Right, node.Bit, list);
                list.Add(node.Value);
            }
        }

        private void KeysR(PatriciaNode<V> node, int bit, HashSet<string> keys)
        {
            if (node.Bit <= bit)
            {
                return;
            }
            else
            {
                KeysR(node.Left, node.Bit, keys);
                KeysR(node.Right, node.Bit, keys);
                keys.Add(node.Key);
            }
        }

        private void EntriesR(PatriciaNode<V> node, int bit, Dictionary<string, V> entries)
        {
            if (node.Bit <= bit)
            {
                return;
            }
            else
            {
                EntriesR(node.Left, node.Bit, entries);
                EntriesR(node.Right, node.Bit, entries);
                entries[node.Key] = node.Value;
            }
        }   

        /**
         * Generic interface to map a key to bits
         *
         * @param <K>  key type
         */
        public interface KeyMapper<K>
        {
            /** Tests a bit in a key
             *
             * @param bit  bit to test
             * @param key  key to use as a base for testing
             * @return true if the specified bit is set in the provided key
             */
            bool IsSet(int bit, K key);

            /** Formats a key as a String
             *
             * @param key  key to format to a String
             * @return key formatted as a String, not null
             */
            string ToBitString(K key);
        }

        /**
         * A {@link KeyMapper} mapping Strings to bits
         */
        public class StringKeyMapper : KeyMapper<string>
        {

            public bool IsSet(int bit, string key)
            {
                if (key == null)
                {
                    return false;
                }

                if (bit >= GetLength(key))
                {
                    return true;
                }

                int codePoint = key.CodePointAt(bit / Constant.CHAR_SIZE);
                int mask = 1 << (Constant.CHAR_SIZE - 1 - (bit % Constant.CHAR_SIZE));
                int result = codePoint & mask;

                if (result != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public string ToBitString(string key)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < GetLength(key); i++)
                {
                    if (IsSet(i, key))
                    {
                        builder.Append("1");
                    }
                    else
                    {
                        builder.Append("0");
                    }
                    if ((i + 1) % 4 == 0 && i < GetLength(key))
                    {
                        builder.Append(" ");
                    }
                }
                return builder.ToString();
            }

            private int GetLength(string key)
            {
                if (key == null)
                {
                    return 0;
                }
                else
                {
                    return key.Length * Constant.CHAR_SIZE;
                }
            }
        }

        /**
         * Nodes used in a {@link PatriciaTrie} containing a String key and associated value data
         *
         * @param <V>  value type
         */
        public class PatriciaNode<T>
        {

            /** This node's key */
            public string Key { get; private set; }

            /** This node's value */
            public T Value { get; set; }

            /** Critical bit */
            public int Bit { get; private set; }

            /** Left node */
            public PatriciaNode<T> Left { get; set; } = null;

            /** Right node */
            public PatriciaNode<T> Right { get; set; } = null;

            /**
             * Constructs a new node
             *
             * @param key  this node's key
             * @param value  this node's value
             * @param bit  this node's critical bit
             */
            public PatriciaNode(string key, T value, int bit)
            {
                this.Key = key;
                this.Value = value;
                this.Bit = bit;
            }

            /**
             * {@inheritDoc}
             */
            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("key: " + Key);
                builder.Append(", ");
                builder.Append("bit: " + Bit);
                builder.Append(", ");
                //		builder.append("bitString: " + StringKeyMapper.toBitString(key));
                //		builder.append(", ");
                builder.Append("value: " + Value);
                builder.Append(", ");
                if (Left != null)
                {
                    builder.Append("left: " + Left.Key);
                }
                else
                {
                    builder.Append("left: null");
                }
                builder.Append(", ");
                if (Right != null)
                {
                    builder.Append("right: " + Right.Key);
                }
                else
                {
                    builder.Append("right: null");
                }
                return builder.ToString();
            }
        }
    }
}
