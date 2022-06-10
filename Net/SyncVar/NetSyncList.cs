//using System;
//using System.Collections;
//using System.Collections.Generic;

//namespace Poly.NetManager
//{
//    public abstract class ANetSyncList : ANetSyncBase
//    {
//        public event SyncListChangedDelegate ValueChangedEvent;
//        public abstract IList ObjectList { get; }

//        public ANetSyncList(ENetSyncVarPermission permission = ENetSyncVarPermission.OwnerOnly) : base(permission)
//        {
//        }
//        internal abstract void SetValue(ESyncListOperation op, int itemIndex, object obj);
//        public void OnValueChanged(ESyncListOperation op, int itemIndex, object obj)
//        {
//            if (NetBehaviour == null || !NetBehaviour.IsSpawned)
//                return;
//            ValueChangedEvent?.Invoke(op, itemIndex, obj);
//        }
//    }
//    //public interface INetSyncList
//    //{
//    //    event SyncListChangedDelegate ValueChangedEvent;
//    //    IList ObjectList { get; }

//    //    void SetValue(ESyncListOperation op, int itemIndex, object obj);

//    //    //bool IsDirty { get; set; }
//    //    //object Value { get; set; }
//    //}
//    public delegate void SyncListChangedDelegate(ESyncListOperation op, int index, object obj);
//    public enum ESyncListOperation
//    {
//        /// <summary>
//        /// Item was added to the list.
//        /// </summary>
//        OP_ADD,
//        /// <summary>
//        /// The list was cleared.
//        /// </summary>
//        OP_CLEAR,
//        /// <summary>
//        /// An item was inserted into the list.
//        /// </summary>
//        OP_INSERT,
//        /// <summary>
//        /// An item was removed from the list.
//        /// </summary>
//        OP_REMOVE,
//        /// <summary>
//        /// An item was removed at an index from the list.
//        /// </summary>
//        OP_REMOVEAT,
//        /// <summary>
//        /// An item was set to a new value in the list.
//        /// </summary>
//        OP_SET,
//        /// <summary>
//        /// An item in the list was manually marked dirty.
//        /// </summary>
//        OP_DIRTY
//    };

//    [Serializable]
//    public class NetSyncList<T> : ANetSyncList, IList<T>, IList
//    {
//        private List<T> objectList = new List<T>();

//        public int Count { get { return objectList.Count; } }
//        public bool IsReadOnly { get { return false; } }
//        public override IList ObjectList => objectList;
//        public List<T> Value { get => objectList; set => objectList = value; }

//        public NetSyncList(ENetSyncVarPermission permission = ENetSyncVarPermission.OwnerOnly) : base(permission)
//        {
//            //this.netObject = netObject;
//        }

//        internal override void SetValue(ESyncListOperation op, int index, object obj)
//        {
//            var value = (T)obj;
//            switch ((ESyncListOperation)op)
//            {
//                case ESyncListOperation.OP_ADD:
//                    //objectList.Add(item);
//                    DoAdd(value);
//                    break;

//                case ESyncListOperation.OP_CLEAR:
//                    //objectList.Clear();
//                    DoClear();
//                    break;

//                case ESyncListOperation.OP_INSERT:
//                    //objectList.Insert(itemIndex, item);
//                    DoInsert(index, value);
//                    break;

//                case ESyncListOperation.OP_REMOVE:
//                    //objectList.Remove(item);
//                    DoRemove(value);
//                    break;

//                case ESyncListOperation.OP_REMOVEAT:
//                    //objectList.RemoveAt(itemIndex);
//                    DoRemoveAt(index);
//                    break;

//                case ESyncListOperation.OP_SET:
//                case ESyncListOperation.OP_DIRTY:
//                    //objectList[itemIndex] = item;
//                    DoSetValue(index, value);
//                    break;
//            }
//        }

//        public void Init(IEnumerable<T> items)
//        {
//            objectList.Clear();
//            objectList.AddRange(items);
//        }
//        public void Add(T item)
//        {
//            if (!CanWrite())
//                return;
//            DoAdd(item);
//        }

//        private void DoAdd(T item)
//        {
//            objectList.Add(item);
//            OnValueChanged(ESyncListOperation.OP_ADD, objectList.Count - 1, item);
//        }

//        public void Clear()
//        {
//            if (!CanWrite())
//                return;
//            DoClear();
//        }

//        private void DoClear()
//        {
//            objectList.Clear();
//            OnValueChanged(ESyncListOperation.OP_CLEAR, 0, default(T));
//        }

//        public bool Contains(T item)
//        {
//            return objectList.Contains(item);
//        }
//        public void CopyTo(T[] array, int index)
//        {
//            objectList.CopyTo(array, index);
//        }
//        public int IndexOf(T item)
//        {
//            return objectList.IndexOf(item);
//        }
//        public void Insert(int index, T item)
//        {
//            if (!CanWrite())
//                return;
//            DoInsert(index, item);
//        }

//        private void DoInsert(int index, T item)
//        {
//            objectList.Insert(index, item);
//            OnValueChanged(ESyncListOperation.OP_INSERT, index, item);
//        }

//        public bool Remove(T item)
//        {
//            if (!CanWrite())
//                return false;
//            return DoRemove(item);
//        }

//        private bool DoRemove(T item)
//        {
//            var result = objectList.Remove(item);
//            if (result)
//            {
//                OnValueChanged(ESyncListOperation.OP_REMOVE, 0, item);
//            }
//            return result;
//        }

//        public void RemoveAt(int index)
//        {
//            if (!CanWrite())
//                return;
//            DoRemoveAt(index);
//        }

//        private void DoRemoveAt(int index)
//        {
//            objectList.RemoveAt(index);
//            OnValueChanged(ESyncListOperation.OP_REMOVEAT, index, default(T));
//        }

//        public void Dirty(int index)
//        {
//            if (!CanWrite())
//                return;
//            OnValueChanged(ESyncListOperation.OP_DIRTY, index, objectList[index]);
//        }
//        public T this[int i]
//        {
//            get { return objectList[i]; }
//            set
//            {
//                if (!CanWrite())
//                    return;
//                if (objectList[i] == null)
//                {
//                    if (value == null)
//                        return;
//                }
//                else
//                {
//                    if (objectList[i].Equals(value))
//                        return;
//                }
//                DoSetValue(i, value);
//            }
//        }

//        private void DoSetValue(int i, T value)
//        {
//            var count = objectList.Count;
//            if (i < 0 || i > count)
//            {
//                //NetBehaviour.NetManager.Logger.LogError(GetType().Name, $"can't set value {i},{value}");
//                return;
//            }
//            if (i < count)
//                objectList[i] = value;
//            else if (i == count)
//                objectList.Add(value);
//            OnValueChanged(ESyncListOperation.OP_SET, i, value);
//        }

//        public IEnumerator<T> GetEnumerator()
//        {
//            return objectList.GetEnumerator();
//        }
//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }

//        #region IList
//        bool IList.IsFixedSize => ObjectList.IsFixedSize;
//        bool ICollection.IsSynchronized => ObjectList.IsSynchronized;
//        object ICollection.SyncRoot => ObjectList.SyncRoot;
//        object IList.this[int index] { get => ObjectList[index]; set => ObjectList[index] = value; }
//        int IList.Add(object value)
//        {
//            return ObjectList.Add(value);
//        }
//        bool IList.Contains(object value)
//        {
//            return ObjectList.Contains(value);
//        }
//        int IList.IndexOf(object value)
//        {
//            return ObjectList.IndexOf(value);
//        }
//        void IList.Insert(int index, object value)
//        {
//            ObjectList.Insert(index, value);
//        }
//        void IList.Remove(object value)
//        {
//            ObjectList.Remove(value);
//        }
//        void ICollection.CopyTo(Array array, int index)
//        {
//            ObjectList.CopyTo(array, index);
//        }
//        #endregion
//    }
//}