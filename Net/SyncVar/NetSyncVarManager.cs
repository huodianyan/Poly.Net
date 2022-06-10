//using Poly.Serialization;
//using System;
//using System.Collections.Generic;
//using System.Reflection;

//namespace Poly.NetManager
//{
//    public struct NetSyncVarField
//    {
//        public int Index;
//        public ANetSyncVar SyncVar;
//        public Type VarType;
//    }
//    public struct NetSyncListField
//    {
//        public int Index;
//        public ANetSyncList SyncList;
//        public Type VarType;
//    }
//    public struct NetSyncArrayField
//    {
//        public int Index;
//        public ANetSyncArray SyncArray;
//        public Type VarType;
//    }
//    public interface INetSyncVarOwner
//    {
//        bool IsSpawned { get; }
//        bool IsOwner { get; }
//        bool IsServer { get; }
//        bool IsClient { get; }
//    }
//    public class NetSyncVarManager : IDisposable
//    {
//        private INetSyncVarOwner netBehaviour;
//        //private List<INetSyncVar> syncVars = new List<INetSyncVar>();
//        private List<NetSyncVarField> syncVarFields = new List<NetSyncVarField>();
//        private List<NetSyncListField> syncListFields = new List<NetSyncListField>();
//        private List<NetSyncArrayField> syncArrayFields = new List<NetSyncArrayField>();
//        private ulong dirtyFlag;

//        public bool IsDirty => dirtyFlag != 0;

//        public NetSyncVarManager(INetSyncVarOwner netObject)
//        {
//            this.netBehaviour = netObject;
//            // netObject.MessageHandlerManager.RegisterMessageHandler(NetMsgTypes.NOSyncListMessage, OnSyncListMessage);
//            // netObject.MessageHandlerManager.RegisterMessageHandler(NetMsgTypes.NOSyncArrayMessage, OnSyncArrayMessage);
//        }

//        public void Dispose()
//        {
//            // netObject.MessageHandlerManager.UnregisterMessageHandler(NetMsgTypes.NOSyncListMessage, OnSyncListMessage);
//            // netObject.MessageHandlerManager.UnregisterMessageHandler(NetMsgTypes.NOSyncArrayMessage, OnSyncArrayMessage);
//            this.netBehaviour = null;
//        }

//        // public void Update()
//        // {
//        // }
//        public bool UpdateDirtyFlag()
//        {
//            var count = syncVarFields.Count;
//            dirtyFlag = 0;
//            for (int i = 0; i < count; i++)
//            {
//                var syncVar = syncVarFields[i].SyncVar;
//                if (syncVar.IsDirty)
//                {
//                    ulong v = (ulong)(1 << i);
//                    dirtyFlag |= v;
//                }
//            }
//            return dirtyFlag != 0;
//        }
//        public void SerializeVars(ref PolyWriter writer, bool isInit)
//        {
//            var count = syncVarFields.Count;
//            //dirtyFlag = 0;
//            //for (int i = 0; i < count; i++)
//            //{
//            //    var syncVar = syncVarFields[i].SyncVar;
//            //    if (isInit || syncVar.IsDirty)
//            //        dirtyFlag |= (long)(1 << i);
//            //}
//            writer.WritePackedULong(dirtyFlag);
//            if (!isInit && dirtyFlag == 0)
//                return;

//            for (int i = 0; i < count; i++)
//            {
//                var syncVarField = syncVarFields[i];
//                var syncVar = syncVarField.SyncVar;
//                var varType = syncVarField.VarType;
//                if (isInit || syncVar.IsDirty)
//                {
//                    //logger.LogTrace($"SerializeVars: {netObject.NetObjectId},{i},{syncVar.Value},{isInit}");
//                    //if(typeof(ANetObject).IsAssignableFrom(varType))
//                    //if(syncVar.Value is ANetObject no)
//                    //    writer.WriteInt(no.NetObjectId);
//                    //else
//                    //    writer.WriteValue(syncVar.Value);
//                    if (isInit)
//                        writer.WriteByte((byte)syncVar.permission);
//                    writer.WriteObject(syncVar.Obj);
//                    if (!isInit)
//                        syncVar.IsDirty = false;
//                }
//            }

//            if (isInit)
//            {
//                //sync list
//                for (int i = 0; i < syncListFields.Count; i++)
//                {
//                    var syncList = syncListFields[i].SyncList;
//                    writer.WriteByte((byte)syncList.permission);
//                    //var length = syncList.Count;
//                    writer.WriteList(syncList.ObjectList);
//                }
//                //sync array
//                for (int i = 0; i < syncArrayFields.Count; i++)
//                {
//                    var syncArray = syncArrayFields[i].SyncArray;
//                    writer.WriteByte((byte)syncArray.permission);
//                    writer.Write(syncArray);
//                    //for (int j = 0;j<syncArray.Length;j++)
//                    //{
//                    //    writer.WriteValue(syncArray[j]);
//                    //}
//                }
//            }
//        }
//        public void DeserializeVars(ref PolyReader reader, bool isInit)
//        {
//            var count = syncVarFields.Count;
//            dirtyFlag = reader.ReadPackedULong();
//            for (int i = 0; i < count; i++)
//            {
//                var syncVarField = syncVarFields[i];
//                var syncVar = syncVarField.SyncVar;
//                if (isInit || (dirtyFlag & (ulong)(1 << i)) != 0)
//                {
//                    object value = null;
//                    //if (typeof(ANetObject).IsAssignableFrom(syncVarField.VarType))
//                    //{
//                    //    var noId = reader.ReadInt();
//                    //    value = netObject.NetManager.SpawnManager.GetNO(noId);
//                    //}
//                    //else
//                    //    value = reader.ReadValue(syncVarField.VarType);
//                    if (isInit)
//                        syncVar.permission = (ENetSyncVarPermission)reader.ReadByte();
//                    value = reader.ReadObject(syncVarField.VarType);
//                    if (syncVarField.VarType.Name == "NTransform")
//                    {
//                        //Debug.Log($"{syncVar.NetBehaviour.name}: {syncVarField.VarType},{value},{syncVar.Obj},{object.Equals(value, syncVar.Obj)}");
//                    }
//                    //syncVarField.SyncVar.Value = value;
//                    syncVar.SetValueInternal(value);
//                    //logger.LogTrace($"DeserializeVars: {netObject.NetObjectId},{i},{syncVarField.SyncVar.Value},{isInit}");
//                }
//            }
//            if (isInit)
//            {
//                //sync list
//                for (int i = 0; i < syncListFields.Count; i++)
//                {
//                    var syncListField = syncListFields[i];
//                    var syncList = syncListField.SyncList;
//                    syncList.permission = (ENetSyncVarPermission)reader.ReadByte();
//                    syncList.ObjectList.Clear();
//                    reader.ReadList(syncList.ObjectList, syncListField.VarType);
//                }
//                //sync array
//                for (int i = 0; i < syncArrayFields.Count; i++)
//                {
//                    var syncArrayField = syncArrayFields[i];
//                    var syncArray = syncArrayField.SyncArray;
//                    syncArray.permission = (ENetSyncVarPermission)reader.ReadByte();
//                    //syncList.Clear();
//                    syncArray.Deserialize(ref reader);
//                }
//            }
//        }

//        public void Register(object instance)
//        {
//            var syncVarIndex = 0;
//            var syncListIndex = 0;
//            var syncArrayIndex = 0;
//            var type = instance.GetType();
//            var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//            for (int i = 0; i < fieldInfos.Length; i++)
//            {
//                var fieldInfo = fieldInfos[i];
//                var fieldType = fieldInfo.FieldType;
//                if (typeof(ANetSyncVar).IsAssignableFrom(fieldType))
//                {
//                    var syncVar = (ANetSyncVar)fieldInfo.GetValue(instance);
//                    syncVar.NetBehaviour = netBehaviour;
//                    var field = new NetSyncVarField
//                    {
//                        Index = syncVarIndex++,
//                        SyncVar = syncVar,
//                        VarType = fieldType.GenericTypeArguments[0]
//                    };
//                    syncVarFields.Add(field);
//                }
//                if (typeof(ANetSyncList).IsAssignableFrom(fieldType))
//                {
//                    var syncList = (ANetSyncList)fieldInfo.GetValue(instance);
//                    syncList.NetBehaviour = netBehaviour;
//                    var field = new NetSyncListField
//                    {
//                        Index = syncListIndex++,
//                        SyncList = syncList,
//                        VarType = fieldType.GenericTypeArguments[0]
//                    };
//                    syncList.ValueChangedEvent += (op, itemIndex, item) =>
//                    {
//                        OnSyncListChangedEvent(field, op, itemIndex, item);
//                    };
//                    syncListFields.Add(field);
//                }
//                if (typeof(ANetSyncArray).IsAssignableFrom(fieldType))
//                {
//                    var syncArray = (ANetSyncArray)fieldInfo.GetValue(instance);
//                    syncArray.NetBehaviour = netBehaviour;
//                    var field = new NetSyncArrayField
//                    {
//                        Index = syncArrayIndex++,
//                        SyncArray = syncArray,
//                        VarType = fieldType.GenericTypeArguments[0]
//                    };
//                    syncArray.ValueChangedEvent += (op, itemIndex, item) =>
//                    {
//                        OnSyncArrayChangedEvent(field.Index, op, itemIndex, item);
//                    };
//                    syncArrayFields.Add(field);
//                }
//            }
//        }

//        #region synclist
//        private void OnSyncListChangedEvent(NetSyncListField field, ESyncListOperation op, int itemIndex, object item)
//        {
//            var syncList = field.SyncList;
//            var listIndex = field.Index;
//            var netObject = netBehaviour.NetObject;
//            var excludeId = netObject.OwnerConnId;
//            if (syncList.permission == ENetSyncVarPermission.ServerOnly)
//            {
//                if (!netBehaviour.IsServer)
//                    return;
//            }
//            else
//            {
//                if (!netBehaviour.IsOwner)
//                    return;
//            }
//            // if (!netBehaviour.IsOwner)
//            //     return;
//            var nm = netObject.CreateMessage(NetMsgTypes.NOSyncListMessage);
//            var writer = nm.BeginWrite();
//            writer.WriteInt(netBehaviour.NetBehaviourId);
//            writer.WriteByte((byte)listIndex);
//            writer.WriteByte((byte)op);
//            writer.WriteUShort((ushort)itemIndex);
//            writer.WriteValue(item);
//            // Debug.Log($"OnSyncListChangedEvent: {writer}");
//            nm.EndWrite(writer);
//            if (netBehaviour.IsServer)
//                // netObject.SendMessage(null, netObject.OwnerConnId, EDeliveryMethod.ReliableOrdered, nm);
//                netObject.SendMessage(null, -1, EDeliveryMethod.ReliableOrdered, nm);
//            else
//                netObject.SendMessage(0, EDeliveryMethod.ReliableOrdered, nm);
//        }

//        internal void OnSyncListMessage(long connId, NetMessage message)
//        {
//            var reader = message.BeginRead();
//            var listIndex = reader.ReadByte();
//            var syncListField = syncListFields[listIndex];
//            var op = (ESyncListOperation)reader.ReadByte();
//            var itemIndex = reader.ReadUShort();
//            Debug.Log($"OnSyncListMessage: {reader}");
//            var item = reader.ReadValue(syncListField.VarType);
//            message.EndRead(reader);

//            syncListField.SyncList.SetValue(op, itemIndex, item);
//        }
//        #endregion

//        #region syncarray
//        private void OnSyncArrayChangedEvent(int listIndex, ESyncArrayOperation op, int itemIndex, object item)
//        {
//            if (!netBehaviour.IsOwner)
//                return;
//            var netObject = netBehaviour.NetObject;
//            var nm = netObject.CreateMessage(NetMsgTypes.NOSyncArrayMessage);
//            var writer = nm.BeginWrite();
//            writer.WriteInt(netBehaviour.NetBehaviourId);
//            writer.WriteByte((byte)listIndex);
//            writer.WriteByte((byte)op);
//            writer.WriteUShort((ushort)itemIndex);
//            writer.WriteValue(item);

//            nm.EndWrite(writer);
//            if (netBehaviour.IsServer)
//                netObject.SendMessage(null, netObject.OwnerConnId, EDeliveryMethod.ReliableOrdered, nm);
//            else
//                netObject.SendMessage(0, EDeliveryMethod.ReliableOrdered, nm);
//        }

//        internal void OnSyncArrayMessage(long connId, NetMessage message)
//        {
//            var reader = message.BeginRead();
//            var listIndex = reader.ReadByte();
//            var syncArrayField = syncArrayFields[listIndex];
//            var op = (ESyncArrayOperation)reader.ReadByte();
//            var itemIndex = reader.ReadUShort();
//            var item = reader.ReadValue(syncArrayField.VarType);
//            message.EndRead(reader);

//            syncArrayField.SyncArray.SetValue(op, itemIndex, item);
//        }
//        #endregion
//    }
//}