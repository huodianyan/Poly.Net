//using Poly.Serialization;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Reflection;

//namespace Poly.NetManager
//{
//    public abstract class ANetSyncArray : ANetSyncBase, IPolySerializable
//    {
//        public event SyncArrayChangedDelegate ValueChangedEvent;
//        //IList ItemList { get; }
//        //object this[int i] { get; set; }
//        public abstract int Length { get; }

//        public ANetSyncArray(ENetSyncVarPermission permission = ENetSyncVarPermission.OwnerOnly) : base(permission)
//        {
//        }

//        internal abstract void SetValue(ESyncArrayOperation op, int itemIndex, object obj);
//        protected void OnValueChanged(ESyncArrayOperation op, int itemIndex, object obj)
//        {
//            if (!NetBehaviour.IsSpawned)
//                return;
//            ValueChangedEvent?.Invoke(op, itemIndex, obj);
//        }
//        public abstract void Serialize(ref PolyWriter writer);
//        public abstract void Deserialize(ref PolyReader reader);
//        //void Clear();
//    }
//    //public interface INetSyncArray : INetSerializable
//    //{
//    //    event SyncArrayChangedDelegate ValueChangedEvent;
//    //    //IList ItemList { get; }
//    //    object this[int i] { get; set; }
//    //    int Length { get; }
//    //    void SetValue(ESyncArrayOperation op, int itemIndex, object obj);
//    //    void Clear();
//    //}
//    public enum ESyncArrayOperation
//    {
//        OP_CLEAR,
//        OP_SET,
//        OP_DIRTY
//    };
//    public delegate void SyncArrayChangedDelegate(ESyncArrayOperation op, int index, object obj);

//    [Serializable]
//    public class NetSyncArray<T> : ANetSyncArray
//    {
//        private T[] objects;

//        public override int Length => objects == null ? 0 : objects.Length;
//        public T[] Objects => objects;

//        //object INetSyncArray.this[int i] { get => this[i]; set => this[i] = (T)value; }

//        //public bool IsReadOnly { get { return false; } }
//        //public IList ItemList => objects;

//        //public event SyncArrayChangedDelegate ValueChangedEvent;

//        public T this[int i]
//        {
//            get { return objects[i]; }
//            set
//            {
//                if (!CanWrite())
//                    return;
//                if (objects[i] == null)
//                {
//                    if (value == null)
//                        return;
//                }
//                else
//                {
//                    if (objects[i].Equals(value))
//                        return;
//                }
//                //objects[i] = value;
//                //OnValueChanged(ESyncArrayOperation.OP_SET, i, value);
//                DoSetValue(i, value);
//            }
//        }

//        public NetSyncArray(ENetSyncVarPermission permission = ENetSyncVarPermission.OwnerOnly) : base(permission)
//        {
//        }

//        internal override void SetValue(ESyncArrayOperation op, int itemIndex, object obj)
//        {
//            var value = (T)obj;
//            switch ((ESyncArrayOperation)op)
//            {
//                case ESyncArrayOperation.OP_CLEAR:
//                    DoClear();
//                    break;

//                case ESyncArrayOperation.OP_SET:
//                case ESyncArrayOperation.OP_DIRTY:
//                    //this[itemIndex] = item;
//                    DoSetValue(itemIndex, value);
//                    break;
//            }
//        }
//        public void Init(int num)
//        {
//            objects = new T[num];
//        }
//        public void Clear()
//        {
//            if (!CanWrite())
//                return;
//            DoClear();
//        }
//        public bool Contains(T item)
//        {
//            var index = Array.FindIndex(objects, (obj) => item.Equals(obj));
//            return index >= 0;
//        }
//        public void CopyTo(T[] array, int index)
//        {
//            objects.CopyTo(array, index);
//        }
//        public int IndexOf(T item)
//        {
//            var index = Array.FindIndex(objects, (obj) => item.Equals(obj));
//            return index;
//        }
//        public void Dirty(int index)
//        {
//            if (!CanWrite())
//                return;
//            OnValueChanged(ESyncArrayOperation.OP_DIRTY, index, objects[index]);
//        }

//        private void DoSetValue(int index, T value)
//        {
//            objects[index] = value;
//            OnValueChanged(ESyncArrayOperation.OP_SET, index, value);
//        }
//        private void DoClear()
//        {
//            if (objects == null)
//                return;
//            Array.Clear(objects, 0, objects.Length);
//            OnValueChanged(ESyncArrayOperation.OP_CLEAR, 0, default(T));
//        }

//        #region INetSerializable
//        public override void Serialize(ref PolyWriter writer)
//        {
//            writer.WriteArray(objects, 0, objects.Length);
//        }

//        public override void Deserialize(ref PolyReader reader)
//        {
//            objects = reader.ReadArray<T>();
//        }
//        #endregion
//    }
//}