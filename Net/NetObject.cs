//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Poly.NetManager
//{
//    public enum ENetObjectType
//    {
//        NetObject,
//        NetEntity
//    }
//    public class NetObject : INetMessageProcessor, INetGroupObserveObject, IComparable<NetObject>
//    {
//        private uint globalObjectIdHash;
//        private int nbIdCounter;

//        private NetManager netManager;
//        private int netObjectId;
//        private NetMessageHandlerManager messageHandlerManager;
//        private NetRpcManager rpcManager;

//        private SortedList<int, ANetBehaviour> netBehaviourSL;

//        internal long ownerConnId = -1;
//        private List<long> observerConnIds;
//        private bool isPlayer;
//        private bool isSpawned;
//        private bool isSceneObj;
//        private NetObject parentNO;

//        protected Dictionary<NetObject, int> observerRefDict;
//        protected Dictionary<NetObject, int> observableRefDict;

//        public uint GlobalObjectIdHash { get => globalObjectIdHash; set => globalObjectIdHash = value; }
//        public NetManager NetManager => netManager;
//        public int NetObjectId => netObjectId;
//        public NetObject ParentNO => parentNO;
//        public NetMessageHandlerManager MessageHandlerManager => messageHandlerManager;
//        public NetRpcManager RpcManager => rpcManager;
//        public bool IsClient { get; private set; }
//        public bool IsServer { get; private set; }
//        public bool IsHost => IsClient && IsServer;
//        public bool IsClientOnly => IsClient && !IsServer;
//        public bool IsServerOnly => !IsClient && IsServer;
//        public bool IsPlayer => isPlayer;
//        public bool IsLocalPlayer => isPlayer && IsOwner && IsClient;
//        public long OwnerConnId => ownerConnId;
//        public bool IsOwner => (IsServer && ownerConnId == -1) || (IsClient && ownerConnId == 0);
//        public bool IsOwnedByClient => (IsClient && ownerConnId == 0);
//        public bool IsOwnedByServer => (IsServer && ownerConnId == -1);
//        public bool IsSpawned => isSpawned;
//        public bool IsSceneObj => isSceneObj;
//        public int NBIdCounter => nbIdCounter;
//        public int ObjectId => netObjectId;
//        public bool IsObservable => true;
//        public bool IsObserver => isPlayer;
//        public List<long> ObserverConnIds => observerConnIds;

//        public event Action<IObserveObject, bool> ObservableActiveEvent;
//        public event Action<IObserveObject, bool> ObserverActiveEvent;
//        public event EventHandler<IObserveObject> ObservableAddedEvent;
//        public event EventHandler<IObserveObject> ObservableRemovedEvent;
//        public event EventHandler<IObserveObject> ObserverAddedEvent;
//        public event EventHandler<IObserveObject> ObserverRemovedEvent;

//        public event Action<NetObject, long, long> OwnerChangedEvent;
//        public event Action<NetObject, long> ObserverConnAddedEvent;

//        public event Action<NetObject, long> ObserverConnRemovedEvent;
//        public event EventHandler<IObserveGroup> AddedToGroupEvent;
//        public event EventHandler<IObserveGroup> RemovedFromGroupEvent;

//        public event Action<NetObject> SpawnedEvent;
//        public event Action<NetObject> DespawnedEvent;

//        #region object
//        public override string ToString()
//        {
//            return $"{GetType().Name}:{{{netObjectId},{IsClient},{IsServer},{IsPlayer},{IsLocalPlayer},{IsOwner},{OwnerConnId},{observerConnIds.ToListString()}}}";
//        }
//        public int CompareTo(NetObject other)
//        {
//            // int result = 0;
//            // if (ReferenceEquals(this, other))
//            //     result = 0;
//            // else if (spawnOrder < other.spawnOrder)
//            //     result = -1;
//            // else if (spawnOrder > other.spawnOrder)
//            //     result = 1;
//            // else
//            //     result = netObjectId.CompareTo(other.netObjectId);
//            // logger.LogWarning(TAG, $"{this} compare {other} = {result}");
//            // return result;
//            if (ReferenceEquals(this, other))
//                return 0;
//            // if (spawnOrder < other.spawnOrder)
//            //     return -1;
//            // if (spawnOrder > other.spawnOrder)
//            //     return 1;
//            return netObjectId.CompareTo(other.netObjectId);
//        }
//        #endregion

//        #region NetObject
//        public void ResetHashId()
//        {
//            globalObjectIdHash = 0;
//            var nos = GetComponentsInChildren<NetObject>();
//            for (int i = 0; i < nos.Length; i++)
//                nos[i].globalObjectIdHash = 0;
//        }
//        internal void InitNBsId()
//        {
//            nbIdCounter = 0;
//            var netBehaviours = GetComponentsInChildren<ANetBehaviour>(true);
//            // Debug.Log($"UpdateNBId: {netBehaviours.Length}");
//            for (int i = 0; i < netBehaviours.Length; i++)
//            {
//                var netBehaviour = netBehaviours[i];
//                netBehaviour.InitId(nbIdCounter++);
//                // netBehaviour.NetBehaviourId = nbIdCounter++;
//                // Debug.Log($"UpdateNBId: {netBehaviour.GetType()}, {netBehaviour.NetBehaviourId}");
//            }
//        }
//        internal void OnNetInit(NetManager netManager, int netObjectId, bool isSceneObj, bool isPlayer)
//        {
//            this.netManager = netManager;
//            messageHandlerManager = new NetMessageHandlerManager();
//            messageHandlerManager.RegisterMessageHandler(this);

//            rpcManager = new NetRpcManager(this);
//            rpcManager.RegisterRPC(this);

//            var isServer = netManager.IsServer;
//            var noType = netManager.NetObjectType;

//            // if (isServer)
//            //     parentNO = transform.parent?.GetComponent<NetObject>();

//            // netBehavioursDict = new SortedList<string, List<ANetBehaviour>>();
//            // transform.ForEachChildren((path, tr) =>
//            // {
//            //     var nbs = tr.gameObject.GetComponents<ANetBehaviour>();
//            //     if (nbs == null || nbs.Length == 0)
//            //         return;
//            //     netBehavioursDict.Add(path, nbs.ToList());
//            // });

//            // netBehaviours = GetComponentsInChildren<ANetBehaviour>(true);
//            netBehaviourSL = new SortedList<int, ANetBehaviour>();
//            ANetBehaviour[] netBehaviours = null;
//            if (noType == ENetObjectType.NetObject)
//                netBehaviours = GetComponentsInChildren<ANetBehaviour>(true);
//            else
//                netBehaviours = GetComponents<ANetBehaviour>();
//            for (int i = 0; i < netBehaviours.Length; i++)
//            {
//                var netBehaviour = netBehaviours[i];
//                // netBehaviour.netBehaviourId = (ushort)i;
//                var nbId = netBehaviour.NetBehaviourId;
//                if (nbId == -1)
//                    nbId = nbIdCounter++;
//                InitNB(netBehaviour, nbId);
//                netBehaviourSL.Add(nbId, netBehaviour);
//                // if (isServer)
//                // {
//                //     var nbId = ++nbIdCounter;
//                //     InitNB(netBehaviour, nbId);
//                //     netBehaviourSL.Add(nbId, netBehaviour);

//                //     // messageHandlerManager.RegisterMessageHandler(netBehaviour);
//                //     // rpcManager.RegisterRPC(netBehaviour, nbId.ToString());
//                // }
//                // else
//                // {
//                //     // netBehaviour.gameObject.RemoveComponent(netBehaviour);
//                //     DestroyImmediate(netBehaviour);
//                // }
//            }
//            observerConnIds = new List<long>();
//            observerRefDict = new Dictionary<NetObject, int>();
//            observableRefDict = new Dictionary<NetObject, int>();

//            this.netObjectId = netObjectId;
//            this.isSceneObj = isSceneObj;
//            this.isPlayer = isPlayer;

//            logger = netManager.Logger;
//            IsClient = netManager.IsClient;
//            IsServer = netManager.IsServer;
//        }
//        internal void OnNetRelease()
//        {
//            observerConnIds.Clear();
//            observerConnIds = null;
//            observerRefDict.Clear();
//            observerRefDict = null;
//            observableRefDict.Clear();
//            observableRefDict = null;

//            var netBehaviours = netBehaviourSL.Values;
//            for (int i = 0; i < netBehaviours.Count; i++)
//            {
//                var netBehaviour = netBehaviours[i];
//                if (netBehaviour == null)
//                    continue;
//                ReleaseNB(netBehaviour);
//            }
//            netBehaviourSL.Clear();
//            netBehaviourSL = null;
//            // netBehaviours = null;

//            rpcManager.Dispose();
//            rpcManager = null;

//            messageHandlerManager.Dispose();
//            messageHandlerManager = null;

//            ObservableActiveEvent = null;
//            ObserverActiveEvent = null;
//            // AuthorityChangedEvent = null;
//            OwnerChangedEvent = null;
//            ObserverConnAddedEvent = null;
//            ObserverConnRemovedEvent = null;

//            this.netObjectId = 0;
//            this.isPlayer = false;
//            this.netManager = null;
//            nbIdCounter = 0;

//            parentNO = null;

//            IsClient = false;
//            IsServer = false;
//            logger = null;
//        }

//        // internal void Init(ArraySegment<byte> payload)
//        // {
//        //     //this.netManager = netManager;
//        //     // this.netObjectId = netObjectId;
//        //     this.payload = payload;
//        //     // this.isPlayer = isPlayer;
//        //     //this.prefabId = prefabId;

//        //     // observerConnIds = new List<long>();
//        //     // observerRefDict = new Dictionary<NetObject, int>();
//        // }
//        // internal void Init(int netObjectId, bool isSceneObj, bool isPlayer)
//        // {
//        //     this.netObjectId = netObjectId;
//        //     this.isSceneObj = isSceneObj;
//        //     this.isPlayer = isPlayer;
//        //     // this.ownerConnId = ownerConnId;
//        // }

//        // internal virtual void Release()
//        // {
//        //     // OnDespawned();
//        //     observerConnIds.Clear();
//        //     // observerConnIds = null;
//        //     observerRefDict.Clear();
//        //     observableRefDict.Clear();
//        //     // observerRefDict = null;

//        //     ObservableActiveEvent = null;
//        //     ObserverActiveEvent = null;
//        //     AuthorityChangedEvent = null;
//        //     OwnerChangedEvent = null;
//        //     ObserverConnAddedEvent = null;
//        //     ObserverConnRemovedEvent = null;

//        //     //this.netManager = null;
//        //     this.netObjectId = 0;
//        //     // this.payload = default;
//        //     this.isPlayer = false;
//        //     //this.prefabId = null;
//        // }

//        // public void Spawn()
//        // {
//        //     if(isSpawned)
//        //         return;
//        //     netManager.SpawnManager.SpawnNO(this);
//        // }
//        // public void Despawn(bool isDestroy)
//        // {
//        //     if(!isSpawned)
//        //         return;
//        //     netManager.SpawnManager.DespawnNO(this, isDestroy);
//        // }

//        internal void OnSpawned()
//        {
//            // logger = netManager.Logger;
//            // IsClient = netManager.IsClient;
//            // IsServer = netManager.IsServer;

//            DoSetOwner(ownerConnId);
//            // if (IsServer)
//            // {
//            //     if (isPlayer)
//            //     {
//            //         AddObserver(this);
//            //     }
//            // }
//            isSpawned = true;

//            var netBehaviours = netBehaviourSL.Values;
//            for (int i = 0; i < netBehaviours.Count; i++)
//            {
//                var netBehaviour = netBehaviours[i];
//                // Debug.Log($"OnSpawned: {netObjectId},{netBehaviour.name},{netBehaviour.netBehaviourId},{i}");
//                SpawnNB(netBehaviour);
//            }
//            if (IsServer)
//            {
//                if (isPlayer)
//                {
//                    AddObserver(this);
//                }
//            }

//            SpawnedEvent?.Invoke(this);
//        }
//        internal void OnDespawned()
//        {
//            var netBehaviours = netBehaviourSL.Values;
//            for (int i = 0; i < netBehaviours.Count; i++)
//            {
//                var netBehaviour = netBehaviours[i];
//                if (netBehaviour == null)
//                    continue;
//                DespawnNB(netBehaviour);
//            }
//            DespawnedEvent?.Invoke(this);
//            isSpawned = false;
//            DoSetOwner(-1);
//            // IsClient = false;
//            // IsServer = false;
//            // logger = null;
//        }
//        internal void OnNetUpdate()
//        {
//            var netBehaviours = netBehaviourSL.Values;
//            var count = netBehaviours.Count;
//            if (isSpawned)
//            {
//                for (int i = 0; i < count; i++)
//                {
//                    var netBehaviour = netBehaviours[i];
//                    netBehaviour.OnNetUpdate();
//                }
//            }
//            if (IsOwner || IsServer)
//            {
//                //sync
//                var isDirty = UpdateDirtyFlag();
//                for (int i = 0; i < count; i++)
//                {
//                    var netBehaviour = netBehaviours[i];
//                    if (netBehaviour.UpdateDirtyFlag())
//                        isDirty = true;
//                }
//                if (isDirty)
//                {
//                    var nm = CreateMessage(NetMsgTypes.NOSyncMessage);
//                    var writer = nm.BeginWrite();
//                    // SerializeNO(ref writer, false);
//                    Serialize(ref writer, false);
//                    nm.EndWrite(writer);

//                    if (IsServer)
//                        SendMessage(null, NetManager.ServerClientId, EDeliveryMethod.ReliableOrdered, nm);
//                    //SendMessage(null, ownerConnId, EDeliveryMethod.ReliableOrdered, nm);
//                    else
//                        SendMessage(0, EDeliveryMethod.ReliableOrdered, nm);
//                }
//            }
//        }
//        #endregion

//        #region Serialize NO
//        private byte dirtyFlag;
//        private string curGOName;
//        private bool curGOActive;
//        private Transform curParentTr;
//        private bool UpdateDirtyFlag()
//        {
//            if (netManager.NetObjectType != ENetObjectType.NetEntity)
//                return false;
//            dirtyFlag = 0;
//            if (curGOName != name)
//            {
//                dirtyFlag |= 0x01;
//                curGOName = name;
//            }
//            if (curGOActive != gameObject.activeSelf)
//            {
//                dirtyFlag |= 0x02;
//                curGOActive = gameObject.activeSelf;
//            }
//            if (curParentTr != transform.parent)
//            {
//                dirtyFlag |= 0x04;
//                curParentTr = transform.parent;
//                parentNO = curParentTr?.GetComponent<NetObject>();
//            }
//            return dirtyFlag != 0;
//        }
//        internal void SerializeGO(ref NetDataWriter writer)
//        {
//            if (netManager.NetObjectType != ENetObjectType.NetEntity)
//                return;
//            writer.WriteString(name);
//            writer.WriteBool(gameObject.activeSelf);
//            var netBehaviours = netBehaviourSL.Values;
//            var count = netBehaviours.Count;
//            writer.WriteUShort((ushort)count);
//            //sync NBs
//            for (int i = 0; i < count; i++)
//            {
//                var netBehaviour = netBehaviours[i];
//                SerializeNB(ref writer, netBehaviour);
//            }
//        }
//        internal void DeserializeGO(ref NetDataReader reader)
//        {
//            if (netManager.NetObjectType != ENetObjectType.NetEntity)
//                return;
//            gameObject.name = reader.ReadString();
//            gameObject.SetActive(reader.ReadBool());
//            var count1 = reader.ReadUShort();
//            //sync NBs
//            for (int i = 0; i < count1; i++)
//            {
//                var netBehaviour = DeserializeNB(ref reader);
//            }
//        }
//        private void SerializeNO(ref NetDataWriter writer, bool isInit)
//        {
//            if (netManager.NetObjectType != ENetObjectType.NetEntity)
//                return;
//            if (isInit)
//            {
//                writer.WriteInt(parentNO ? parentNO.netObjectId : -1);
//            }
//            else
//            {
//                writer.WriteByte(dirtyFlag);
//                if ((dirtyFlag & 0x01) != 0)
//                    writer.WriteString(curGOName);
//                if ((dirtyFlag & 0x02) != 0)
//                    writer.WriteBool(curGOActive);
//                if ((dirtyFlag & 0x04) != 0)
//                    writer.WriteInt(parentNO ? parentNO.netObjectId : -1);
//                dirtyFlag = 0;
//            }
//        }
//        private void DeserializeNO(ref NetDataReader reader, bool isInit)
//        {
//            if (netManager.NetObjectType != ENetObjectType.NetEntity)
//                return;
//            if (isInit)
//            {
//                var parentId = reader.ReadInt();
//                if (parentId != -1)
//                {
//                    parentNO = netManager.SpawnManager.GetNO(parentId);
//                    transform.SetParent(parentNO.transform, false);
//                }
//            }
//            else
//            {
//                var dirtyFlag = reader.ReadByte();
//                if ((dirtyFlag & 0x01) != 0)
//                    gameObject.name = reader.ReadString();
//                if ((dirtyFlag & 0x02) != 0)
//                    gameObject.SetActive(reader.ReadBool());
//                if ((dirtyFlag & 0x04) != 0)
//                {
//                    var parentId = reader.ReadInt();
//                    if (parentId != -1)
//                    {
//                        parentNO = netManager.SpawnManager.GetNO(parentId);
//                        transform.SetParent(parentNO.transform, false);
//                    }
//                }
//            }
//        }
//        #endregion

//        #region netbehaviour
//        // internal void OnNBAwake(ANetBehaviour netBehaviour)
//        // {
//        //     if (!isSpawned || !IsOwner)
//        //         return;
//        //     AddNB(netBehaviour);
//        // }
//        private void InitNB(ANetBehaviour netBehaviour, int nbId)
//        {
//            try
//            {
//                netBehaviour.OnNetInit(this, nbId);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(TAG, $"OnNetInit: {netBehaviour.name}[{nbId}], {ex}");
//            }
//            messageHandlerManager.RegisterMessageHandler(netBehaviour);
//            rpcManager.RegisterRPC(netBehaviour, nbId.ToString());
//        }
//        private void ReleaseNB(ANetBehaviour netBehaviour)
//        {
//            var nbId = netBehaviour.NetBehaviourId;
//            try
//            {
//                netBehaviour.OnNetRelease();
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(TAG, $"OnNetRelease: {netBehaviour.name}[{nbId}], {ex}");
//            }
//            messageHandlerManager.UnregisterMessageHandler(netBehaviour);
//            rpcManager.UnregisterRPC(netBehaviour);
//        }
//        private void SpawnNB(ANetBehaviour netBehaviour)
//        {
//            var nbId = netBehaviour.NetBehaviourId;
//            try
//            {
//                netBehaviour.OnNetSpawn();
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(TAG, $"OnSpawned: {netBehaviour.name}[{nbId}], {ex}");
//            }
//        }
//        private void DespawnNB(ANetBehaviour netBehaviour)
//        {
//            var nbId = netBehaviour.NetBehaviourId;
//            try
//            {
//                netBehaviour.OnNetDespawn();
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(TAG, $"OnDespawned: {netBehaviour.name}[{nbId}], {ex}");
//            }
//        }
//        [SRpc]
//        public async Task<bool> SRpcAddNBAsync(Type type)
//        {
//            if (!type.IsSubclassOf(typeof(ANetBehaviour)))
//                return false;
//            if (IsServer)
//            {
//                var nb = gameObject.AddComponent(type) as ANetBehaviour;
//                var nbId = ++nbIdCounter;
//                InitNB(nb, nbId);
//                netBehaviourSL.Add(nbId, nb);
//                SpawnNB(nb);
//                CRpcAddNB(type, nb.NetBehaviourId);
//                return true;
//            }
//            else
//                return await InvokeRPCAsync<bool>(nameof(SRpcAddNBAsync), type);
//        }
//        [CRpc]
//        public void CRpcAddNB(Type type, int nbId)
//        {
//            if (IsClientOnly)
//            {
//                if (nbId > nbIdCounter)
//                    nbIdCounter = nbId;
//                var nb = gameObject.AddComponent(type) as ANetBehaviour;
//                Debug.Log($"CRpcAddNB:{type},{nbId},{nb}");
//                InitNB(nb, nbId);
//                netBehaviourSL.Add(nbId, nb);
//                SpawnNB(nb);
//            }
//            if (IsServer)
//                InvokeRPCAll(nameof(CRpcAddNB), type, nbId);
//        }
//        [SRpc]
//        public async Task<bool> SRpcRemoveNBAsync(ANetBehaviour nb)
//        {
//            if (IsServer)
//            {
//                var nbId = nb.NetBehaviourId;
//                netBehaviourSL.Remove(nbId);
//                DespawnNB(nb);
//                ReleaseNB(nb);
//                nb.gameObject.RemoveComponent(nb);
//                CRpcRemoveNB(nbId);
//                return true;
//            }
//            else
//                return await InvokeRPCAsync<bool>(nameof(SRpcRemoveNBAsync), nb);
//        }
//        [CRpc]
//        public void CRpcRemoveNB(int nbId)
//        {
//            if (IsClientOnly)
//            {
//                if (netBehaviourSL.TryGetValue(nbId, out var nb))
//                {
//                    Debug.Log($"CRpcRemoveNB:{nbId},{nb}");
//                    netBehaviourSL.Remove(nbId);
//                    DespawnNB(nb);
//                    ReleaseNB(nb);
//                    nb.gameObject.RemoveComponent(nb);
//                }
//            }
//            if (IsServer)
//                InvokeRPCAll(nameof(CRpcRemoveNB), nbId);
//        }

//        public void AddNB(ANetBehaviour netBehaviour)
//        {
//            if (!isSpawned || !IsServer)
//                return;
//            // if (netBehaviour.gameObject != gameObject)
//            //     return;
//            if (netBehaviourSL.Values.Contains(netBehaviour))
//                return;
//            var nbId = ++nbIdCounter;
//            // DoAddNB(netBehaviour, nbId);
//            InitNB(netBehaviour, nbId);
//            netBehaviourSL.Add(nbId, netBehaviour);
//            SpawnNB(netBehaviour);

//            SendNOAddNBMessage(0, netBehaviour);
//        }
//        public void RemoveNB(ANetBehaviour netBehaviour, bool isDestroy = true)
//        {
//            if (!isSpawned || !IsServer)
//                return;
//            if (!netBehaviourSL.Values.Contains(netBehaviour))
//                return;

//            SendNORemoveNBMessage(0, netBehaviour);

//            var nbId = netBehaviour.NetBehaviourId;
//            netBehaviourSL.Remove(nbId);
//            DespawnNB(netBehaviour);
//            ReleaseNB(netBehaviour);
//            if (isDestroy)
//                netBehaviour.gameObject.RemoveComponent(netBehaviour);
//        }
//        private void SerializeNB(ref NetDataWriter writer, ANetBehaviour netBehaviour)
//        {
//            writer.WriteType(netBehaviour.GetType());
//            writer.WriteInt(netBehaviour.NetBehaviourId);
//            var path = netBehaviour.transform.GetPath(transform);
//            writer.WriteString(path);
//            // writer.WriteString(netBehaviour.TransformPath);
//            // netBehaviour.Serialize(ref writer, true);
//        }
//        private ANetBehaviour DeserializeNB(ref NetDataReader reader)
//        {
//            var type = reader.ReadType();
//            var nbId = reader.ReadInt();
//            var path = reader.ReadString();

//            if (nbId > nbIdCounter)
//                nbIdCounter = nbId;
//            if (netBehaviourSL.TryGetValue(nbId, out var netBehaviour))
//            {
//                //check sync
//                var path1 = netBehaviour.transform.GetPath(transform);
//                var type1 = netBehaviour.GetType();
//                if (path != path1 || type != type1)
//                    Debug.LogError($"NetObject.DeserializeNB: [{path},{type},{nbId}] != [{path1},{type1}]");
//            }
//            else
//            {
//                var nbGO = gameObject;
//                if (!string.IsNullOrEmpty(path))
//                    nbGO = transform.Find(path)?.gameObject;
//                if (nbGO != null)
//                {
//                    netBehaviour = nbGO.AddComponent(type) as ANetBehaviour;
//                    InitNB(netBehaviour, nbId);
//                    netBehaviourSL.Add(nbId, netBehaviour);
//                    // netBehaviour.Deserialize(ref reader, true);
//                    // SpawnNB(netBehaviour);
//                }
//                else
//                {
//                    Debug.LogError($"NetObject.DeserializeNB: cant find path {path}, {type}, {nbId}");
//                }
//            }
//            return netBehaviour;
//        }
//        //server -> client
//        private void SendNOAddNBMessage(long connId, ANetBehaviour netBehaviour)
//        {
//            if (!IsServer)
//                return;
//            var message = CreateMessage(NetMsgTypes.NOAddNBMessage);
//            var writer = message.BeginWrite();
//            SerializeNB(ref writer, netBehaviour);
//            netBehaviour.Serialize(ref writer, true);
//            message.EndWrite(writer);

//            SendMessage(null, 0, EDeliveryMethod.ReliableOrdered, message);
//            // if (IsServer)
//            //     SendMessage(null, 0, EDeliveryMethod.ReliableOrdered, message);
//            // else
//            //     SendMessage(0, EDeliveryMethod.ReliableOrdered, message);
//        }
//        //client
//        [NetMessageHandler(NetMsgTypes.NOAddNBMessage)]
//        private void OnNOAddNBMessage(long connId, NetMessage message)
//        {
//            if (!IsClientOnly)
//                return;
//            var reader = message.BeginRead();
//            var netBehaviour = DeserializeNB(ref reader);
//            if (netBehaviour != null)
//            {
//                // netBehaviourSL.Add(netBehaviour.NetBehaviourId, netBehaviour);
//                netBehaviour.Deserialize(ref reader, true);
//                SpawnNB(netBehaviour);
//            }
//            message.EndRead(reader);
//            // if (IsServer)
//            //     SendMessage(null, connId, EDeliveryMethod.ReliableOrdered, message);
//        }
//        //server -> client
//        private void SendNORemoveNBMessage(long connId, ANetBehaviour netBehaviour)
//        {
//            if (!IsServer)
//                return;
//            var message = CreateMessage(NetMsgTypes.NORemoveNBMessage);
//            var writer = message.BeginWrite();
//            writer.WriteInt(netBehaviour.NetBehaviourId);
//            message.EndWrite(writer);
//            SendMessage(null, 0, EDeliveryMethod.ReliableOrdered, message);
//        }
//        //client
//        [NetMessageHandler(NetMsgTypes.NORemoveNBMessage)]
//        private void OnNORemoveNBMessage(long connId, NetMessage message)
//        {
//            if (!IsClientOnly)
//                return;
//            var reader = message.BeginRead();
//            var nbId = reader.ReadInt();
//            if (netBehaviourSL.TryGetValue(nbId, out var netBehaviour))
//            {
//                netBehaviourSL.Remove(nbId);
//                DespawnNB(netBehaviour);
//                ReleaseNB(netBehaviour);
//                netBehaviour.gameObject.RemoveComponent(netBehaviour);
//            }
//            message.EndRead(reader);
//            // if (IsServer)
//            //     SendMessage(null, connId, EDeliveryMethod.ReliableOrdered, message);
//        }

//        #endregion

//        #region observer
//        public void AddObserverConn(long observerId)
//        {
//            if (!IsServer)
//                return;
//            if (observerConnIds.Contains(observerId))
//                return;
//            observerConnIds.Add(observerId);
//            //netBase.SendNOSpawnedMessage(observerId, this);
//            OnObserverConnAdded(observerId);
//        }
//        public void RemoveObserverConn(long observerId)
//        {
//            if (!IsServer)
//                return;
//            if (!observerConnIds.Contains(observerId))
//                return;
//            observerConnIds.Remove(observerId);
//            OnObserverConnRemoved(observerId);
//        }
//        protected void OnObserverConnAdded(long observerId)
//        {
//            ObserverConnAddedEvent?.Invoke(this, observerId);
//        }
//        protected void OnObserverConnRemoved(long observerId)
//        {
//            ObserverConnRemovedEvent?.Invoke(this, observerId);
//        }
//        #endregion

//        #region serialize
//        public void Serialize(ref NetDataWriter writer, bool isInit)
//        {
//            SerializeNO(ref writer, isInit);
//            var netBehaviours = netBehaviourSL.Values;
//            var count = netBehaviours.Count;
//            // if (isInit)
//            // {
//            //     if (netManager.NetObjectType == ENetObjectType.NetEntity)
//            //     {
//            //         //parent no
//            //         writer.WriteInt(parentNO ? parentNO.netObjectId : -1);
//            //     }
//            //     // writer.WriteUShort((ushort)count);
//            //     // //sync NBs
//            //     // for (int i = 0; i < count; i++)
//            //     // {
//            //     //     var netBehaviour = netBehaviours[i];
//            //     //     SerializeNB(ref writer, netBehaviour);
//            //     // }
//            // }
//            for (int i = 0; i < count; i++)
//            {
//                var netBehaviour = netBehaviours[i];
//                // if (netBehaviour == null)
//                // {
//                //     writer.WriteByte(0);
//                //     continue;
//                // }
//                if (isInit || netBehaviour.IsDirty)
//                {
//                    writer.WriteByte(1);
//                    netBehaviour.Serialize(ref writer, isInit);
//                }
//                else
//                    writer.WriteByte(0);
//            }
//        }
//        public void Deserialize(ref NetDataReader reader, bool isInit)
//        {
//            DeserializeNO(ref reader, isInit);
//            // if (isInit)
//            // {
//            //     if (netManager.NetObjectType == ENetObjectType.NetEntity)
//            //     {
//            //         var parentId = reader.ReadInt();
//            //         if (parentId != -1)
//            //         {
//            //             parentNO = netManager.SpawnManager.GetNO(parentId);
//            //             transform.SetParent(parentNO.transform, false);
//            //         }
//            //     }
//            //     // var count1 = reader.ReadUShort();
//            //     // //sync NBs
//            //     // for (int i = 0; i < count1; i++)
//            //     // {
//            //     //     var netBehaviour = DeserializeNB(ref reader);
//            //     //     // netBehaviourSL.Add(netBehaviour.NetBehaviourId, netBehaviour);
//            //     // }
//            // }
//            var netBehaviours = netBehaviourSL.Values;
//            var count = netBehaviours.Count;
//            for (int i = 0; i < count; i++)
//            {
//                var netBehaviour = netBehaviours[i];
//                var isDirty = reader.ReadByte() != 0;
//                // if (netBehaviour == null)
//                //     continue;
//                if (isDirty)
//                {
//                    // Debug.Log($"{netBehaviour.GetType().Name}, {netBehaviour.name}");
//                    netBehaviour.Deserialize(ref reader, isInit);
//                }
//            }
//        }
//        #endregion

//        #region ownership
//        public void SetOwner(long newOwnerConnId)
//        {
//            if (!IsServer)
//                return;
//            if (ownerConnId == newOwnerConnId)
//                return;
//            var oldOwnerConnId = ownerConnId;
//            DoSetOwner(newOwnerConnId);
//            if (oldOwnerConnId != -1)
//            {
//                // netManager.SpawnManager.RemoveConnOwnedNO(oldOwnerConnId, netObjectId);
//                SendNOOwnerMessage(oldOwnerConnId, -1);
//            }
//            if (newOwnerConnId != -1)
//            {
//                // netManager.SpawnManager.AddConnOwnedNO(newOwnerConnId, netObjectId);
//                SendNOOwnerMessage(newOwnerConnId, 0);
//            }
//        }
//        internal void DoSetOwner(long newOwnerConnId)
//        {
//            var oldOwnerConnId = ownerConnId;
//            if (oldOwnerConnId != -1)
//                netManager.SpawnManager.RemoveConnOwnedNO(oldOwnerConnId, netObjectId);
//            ownerConnId = newOwnerConnId;
//            if (newOwnerConnId != -1)
//                netManager.SpawnManager.AddConnOwnedNO(newOwnerConnId, netObjectId);
//            if (isSpawned)
//            {
//                OnOwnerChanged(newOwnerConnId, oldOwnerConnId);
//            }
//        }
//        protected void OnOwnerChanged(long newOwnerConnId, long oldOwnerConnId)
//        {
//            var netBehaviours = netBehaviourSL.Values;
//            var count = netBehaviours.Count;
//            for (int i = 0; i < count; i++)
//            {
//                var netBehaviour = netBehaviours[i];
//                if (netBehaviour == null)
//                    continue;
//                if (IsOwner)
//                    netBehaviour.OnGainedOwnership();
//                else
//                    netBehaviour.OnLostOwnership();
//            }

//            OwnerChangedEvent?.Invoke(this, newOwnerConnId, oldOwnerConnId);
//        }
//        #endregion

//        #region receive message
//        internal void OnReceiveData(long connId, ref NetDataReader reader)
//        {
//            //var size = reader.GetUShort(0);
//            var size = reader.AvailableCount;
//            //reader.ReadUShort();
//            var msgId = reader.GetUShort(reader.Position - reader.Offset);
//            //var netMessage = new NetMessage(msgId, size, netBase.NetSettings.SerializerContext);
//            //reader.ReadBytes(netMessage.Data, 0, size);
//            ////netMessage.Offset = 4;
//            //netMessage.Position = 4;
//            //netMessage.Count = size;
//            var netMessage = new NetMessage(msgId, reader.Data, reader.Position, size, reader.Position + 2, netManager.SerializerContext);
//            OnReceiveMessage(connId, netMessage);
//        }
//        protected void OnReceiveMessage(long connId, NetMessage message)
//        {
//            // logger.Log(TAG, $"OnReceiveMessage: {connId},{message}");
//            messageHandlerManager.ProcessMessage(connId, message);
//            //message.Dispose();
//        }
//        [NetMessageHandler(NetMsgTypes.NOSyncMessage)]
//        protected void OnNOSyncMessage(long connId, NetMessage message)
//        {
//            if (IsServer && connId == 0)
//                return;
//            var reader = message.BeginRead();
//            // DeserializeNO(ref reader, false);
//            Deserialize(ref reader, false);
//            message.EndRead(reader);

//            if (IsServer)
//            {
//                var netMessage = new NetMessage(NetMsgTypes.NOMessage, message.Data, 0, message.Offset + message.Count, 4, netManager.SerializerContext);
//                SendMessage(null, OwnerConnId, EDeliveryMethod.ReliableOrdered, netMessage);
//                // logger.Log(TAG, $"OnNOSyncMessage: {connId}=={OwnerConnId}");
//            }
//            //logger.LogDebug($"OnTestMessage: {connection.ConnectionId}, {info}");
//        }
//        [NetMessageHandler(NetMsgTypes.NOSyncListMessage)]
//        private void OnSyncListMessage(long connId, NetMessage message)
//        {
//            if (IsServer && connId == 0)
//                return;
//            var reader = message.BeginRead();
//            var nbIndex = reader.ReadInt();
//            Debug.Log($"OnSyncListMessage: {reader}");
//            message.EndRead(reader);
//            var netBehaviour = netBehaviourSL[nbIndex];
//            netBehaviour.syncVarManager.OnSyncListMessage(connId, message);
//        }
//        [NetMessageHandler(NetMsgTypes.NOSyncArrayMessage)]
//        private void OnSyncArrayMessage(long connId, NetMessage message)
//        {
//            if (IsServer && connId == 0)
//                return;
//            var reader = message.BeginRead();
//            var nbIndex = reader.ReadInt();
//            message.EndRead(reader);
//            var netBehaviour = netBehaviourSL[nbIndex];
//            netBehaviour.syncVarManager.OnSyncArrayMessage(connId, message);
//        }
//        //server -> client
//        private void SendNOOwnerMessage(long connId, long ownerConnId)
//        {
//            if (connId == 0)
//                return;
//            var message = CreateMessage(NetMsgTypes.NOOwnerMessage);
//            var writer = message.BeginWrite();
//            writer.WriteLong(ownerConnId);
//            message.EndWrite(writer);
//            SendMessage(connId, EDeliveryMethod.ReliableOrdered, message);
//        }
//        //client
//        [NetMessageHandler(NetMsgTypes.NOOwnerMessage)]
//        private void OnOwnerMessage(long connId, NetMessage message)
//        {
//            if (IsServer && connId == 0)
//                return;
//            var reader = message.BeginRead();
//            var ownerConnId = reader.ReadLong();
//            message.EndRead(reader);

//            DoSetOwner(ownerConnId);
//        }

//        #endregion

//        #region send message
//        public NetMessage CreateMessage(ushort msgId)
//        {
//            var nm = new NetMessage(NetMsgTypes.NOMessage, -1, netManager.SerializerContext);
//            var writer = nm.BeginWrite();
//            writer.WriteInt(netObjectId);
//            //writer.WriteUShort(0);
//            writer.WriteUShort(msgId);
//            //writer.WriteBytes(netMessage.Data, 0, length);
//            nm.EndWrite(writer);
//            //var message = new NetMessage(msgId, -1, netBase.NetSettings.SerializerContext);
//            return nm;
//        }
//        public void SendMessage(long connId, EDeliveryMethod deliveryMethod, NetMessage netMessage)
//        {
//            //if (!HasAuthority)
//            //    return;
//            //var nm = CreateNOMessage(netMessage);
//            //netBase.SendMessage(connId, deliveryMethod, nm);
//            //nm.Dispose();
//            netManager.SendMessage(connId, deliveryMethod, netMessage);
//            //netMessage.Dispose();
//        }
//        public void SendMessage(IList<long> connectionIds, long excludeId, EDeliveryMethod deliveryMethod, NetMessage netMessage)
//        {
//            //if (!HasAuthority)
//            //    return;
//            if (connectionIds == null)
//                connectionIds = observerConnIds;
//            //var nm = CreateNOMessage(netMessage);
//            //netBase.SendMessage(connectionIds, excludeId, deliveryMethod, nm);
//            //nm.Dispose();
//            netManager.SendMessage(connectionIds, excludeId, deliveryMethod, netMessage);
//            //netMessage.Dispose();
//        }
//        #endregion

//        #region observe
//        public void AddObservable(IObserveObject observable)
//        {
//            if (!IsServer)
//                return;
//            var no = observable as NetObject;
//            if (!observableRefDict.TryGetValue(no, out var refNum))
//            {
//                ObservableAddedEvent?.Invoke(this, observable);
//                refNum = 0;
//            }
//            observableRefDict[no] = ++refNum;
//        }
//        public void RemoveObservable(IObserveObject observable)
//        {
//            if (!IsServer)
//                return;
//            var no = observable as NetObject;
//            if (observableRefDict.TryGetValue(no, out var refNum))
//            {
//                refNum--;
//                if (refNum == 0)
//                {
//                    ObservableRemovedEvent?.Invoke(this, observable);
//                    observableRefDict.Remove(no);
//                }
//                else
//                    observableRefDict[no] = refNum;
//            }
//        }
//        public void AddObserver(IObserveObject observer)
//        {
//            if (!IsServer)
//                return;
//            var no = observer as NetObject;
//            if (!observerRefDict.TryGetValue(no, out var refNum))
//            {
//                AddObserverConn(no.ownerConnId);
//                ObserverAddedEvent?.Invoke(this, observer);
//                refNum = 0;
//            }
//            observerRefDict[no] = ++refNum;
//        }
//        public void RemoveObserver(IObserveObject observer)
//        {
//            if (!IsServer)
//                return;
//            var observerNO = observer as NetObject;
//            if (observerRefDict.TryGetValue(observerNO, out var refNum))
//            {
//                refNum--;
//                if (refNum == 0)
//                {
//                    ObserverRemovedEvent?.Invoke(this, observer);
//                    RemoveObserverConn(observerNO.ownerConnId);
//                    observerRefDict.Remove(observerNO);
//                }
//                else
//                    observerRefDict[observerNO] = refNum;
//            }
//        }
//        #endregion

//        #region netgroup
//        public void OnAddedToGroup(IObserveGroup group)
//        {
//            AddedToGroupEvent?.Invoke(this, group);
//        }
//        public void OnRemovedFromGroup(IObserveGroup group)
//        {
//            RemovedFromGroupEvent?.Invoke(this, group);
//        }

//        #endregion

//        #region RPC
//        public void InvokeRPC(string rpcId, params object[] ps)
//        {
//            var isSRpc = rpcId.StartsWith("SRpc");
//            if (isSRpc && !(IsClient && IsOwner))
//                return;
//            if (!isSRpc && !(IsServer && !IsOwner))
//                return;
//            rpcManager.InvokeRPC(ownerConnId, rpcId, ps);
//        }
//        public async Task<T> InvokeRPCAsync<T>(string rpcId, params object[] ps)
//        {
//            var isSRpc = rpcId.StartsWith("SRpc");
//            if (isSRpc && !(IsClient && IsOwner))
//                return default(T);
//            if (!isSRpc && !(IsServer && !IsOwner))
//                return default(T);
//            return await rpcManager.InvokeRPCAsync<T>(ownerConnId, rpcId, ps);
//        }
//        public void InvokeRPCAll(string rpcId, params object[] ps)
//        {
//            var isSRpc = rpcId.StartsWith("SRpc");
//            if (isSRpc)
//                return;
//            rpcManager.InvokeRPC(observerConnIds, 0, rpcId, ps);
//        }
//        public void InvokeRPCOthers(string rpcId, params object[] ps)
//        {
//            var isSRpc = rpcId.StartsWith("SRpc");
//            if (isSRpc)
//                return;
//            rpcManager.InvokeRPC(observerConnIds, ownerConnId, rpcId, ps);
//        }
//        #endregion
//    }
//}
