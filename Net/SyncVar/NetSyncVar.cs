//using Newtonsoft.Json;
//using Poly.Serialization;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Reflection;

//namespace Poly.NetManager
//{
//    public abstract class ANetSyncVar : ANetSyncBase, IPolySerializable
//    {
//        internal bool IsDirty { get; set; }
//        public abstract object Obj { get; }

//        public ANetSyncVar(ENetSyncVarPermission permission = ENetSyncVarPermission.OwnerOnly) : base(permission)
//        {
//        }
//        public abstract bool SetValue(object value);
//        internal abstract bool SetValueInternal(object value);
//        public abstract void Serialize(ref PolyWriter writer);
//        public abstract void Deserialize(ref PolyReader reader);
//        public abstract void SetDirty(bool isDirty);

//    }
//    //public interface INetSyncVar : INetSerializable
//    //{
//    //    //event Action<object> ValueChangedEvent;
//    //    ANetObject netObject { get; set; }
//    //    bool IsDirty { get; set; }
//    //    object Value { get; set; }
//    //    void SetValue(object value);
//    //}
//    public delegate void SyncVarChangedDelegate<T>(T oldValue, T newValue);
//    [Serializable]
//    public class NetSyncVar<T> : ANetSyncVar
//    {
//        private T value;
//        //private bool isDirty;
//        //private NetSyncVarPermission permission;

//        //public ANetObject netObject { get; set; }
//        public event SyncVarChangedDelegate<T> ValueChangedEvent;
//        //public bool IsDirty { get => isDirty; set => isDirty = value; }

//        public T Value
//        {
//            get => value;
//            set
//            {
//                if (!CanWrite())
//                    return;
//                if (SetValueInternal(value))
//                    IsDirty = true;
//            }
//        }

//        //object INetSyncVar.Value { get => value; set => Value = (T)value; }
//        public override object Obj => value;

//        //object INetSyncVar.Value => value;
//        public NetSyncVar(T value = default, ENetSyncVarPermission permission = ENetSyncVarPermission.OwnerOnly) : base(permission)
//        {
//            this.value = value;
//            //this.permission = permission;
//        }
//        // private bool IsEuqal(object value)
//        // {
//        //     if (value == null)
//        //     {
//        //         if (this.value == null)
//        //             return true;
//        //     }
//        //     else if (value.Equals(this.value))
//        //         return true;
//        //     return false;
//        // }
//        public override bool SetValue(object value)
//        {
//            if (!CanWrite())
//                return false;
//            if (SetValueInternal(value))
//            {
//                IsDirty = true;
//                return true;
//            }
//            return false;
//        }
//        internal override bool SetValueInternal(object value)
//        {
//            // if (value == null)
//            // {
//            //     if (this.value == null)
//            //         return false;
//            // }
//            // else if (value.Equals(this.value))
//            //     return false;
//            // if(typeof(T) == typeof(bool))
//            //     Debug.Log($"{GetType().Name}.SetValue:{value},{this.value},{IsEuqal(value)},{object.Equals(value, this.value)}");
//            if (object.Equals(value, this.value))
//                return false;
//            var oldValue = this.value;
//            this.value = (T)value;
//            OnValueChanged(oldValue, this.value);
//            return true;
//        }
//        public override void SetDirty(bool isDirty)
//        {
//            if (!CanWrite())
//                return;
//            IsDirty = isDirty;
//            OnValueChanged(value, value);
//        }

//        private void OnValueChanged(T oldValue, T value)
//        {
//            if (NetBehaviour == null || !NetBehaviour.IsSpawned)
//                return;
//            ValueChangedEvent?.Invoke(oldValue, value);
//        }

//        #region INetSerializable
//        public override void Serialize(ref PolyWriter writer)
//        {
//            writer.WriteObject(value);
//        }

//        public override void Deserialize(ref PolyReader reader)
//        {
//            value = reader.ReadObject<T>();
//            //Debug.Log($"{NetBehaviour.name}: {typeof(T)},{value}");
//        }
//        #endregion
//    }
//}