using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.NLPJDictCore.Collections
{
    public class ObservableCollectionAutoResize<T> : ObservableCollection<T>
    {
        public bool IsRemoveAtZero { get; set; } = false;

        public int MaxCollectionSize { get; set; }

        public ObservableCollectionAutoResize(int maxCollectionSize = 0) : base()
        {
            MaxCollectionSize = maxCollectionSize;
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            if (MaxCollectionSize > 0 && MaxCollectionSize < Count)
            {
                int trimCount = Count - MaxCollectionSize;
                for (int i = 0; i < trimCount; i++)
                {
                    if (IsRemoveAtZero)
                        RemoveAt(0);
                    else
                        RemoveAt(Count - i - 1);
                }
            }            
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
        }
    }
}
