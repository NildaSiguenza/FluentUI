using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentControls
{
    public interface INotifyCollectionChanged
    {
        event NotifyCollectionChangedEventHandler CollectionChangedDetailed;
    }

    public delegate void NotifyCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e);


    public class NotifyCollectionChangedEventArgs : EventArgs
    {
        public NotifyCollectionChangedAction Action { get; }

        public IList NewItems { get; }

        public IList OldItems { get; }

        public int NewStartingIndex { get; }

        public int OldStartingIndex { get; }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action)
        {
            Action = action;
            NewStartingIndex = -1;
            OldStartingIndex = -1;
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index)
        {
            Action = action;
            NewItems = new[] { changedItem };
            NewStartingIndex = index;
            OldStartingIndex = -1;
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
        {
            Action = action;
            NewItems = changedItems;
            NewStartingIndex = startingIndex;
            OldStartingIndex = -1;
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem, int index)
        {
            Action = action;
            NewItems = new[] { newItem };
            OldItems = new[] { oldItem };
            NewStartingIndex = index;
            OldStartingIndex = index;
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex)
        {
            Action = action;
            NewItems = new[] { changedItem };
            NewStartingIndex = index;
            OldStartingIndex = oldIndex;
        }
    }

    public enum NotifyCollectionChangedAction
    {
        Add,
        Remove,
        Replace,
        Move,
        Reset
    }
}
