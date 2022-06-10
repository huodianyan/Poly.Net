//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Poly.NetManager
//{
//    public interface INetSpawnHandler
//    {
//        NetObject CreateNO(GameObject prefab, Vector3? position, Quaternion? rotation);
//        void DestroyNO(NetObject no);
//    }
//    public class PrefabSpawnHandler : INetSpawnHandler
//    {
//        private NetManager netManager;

//        public PrefabSpawnHandler(NetManager netManager)
//        {
//            this.netManager = netManager;
//        }
//        public NetObject CreateNO(GameObject prefab, Vector3? position, Quaternion? rotation)
//        {
//            GameObject go = null;
//            if (position == null)
//                go = GameObject.Instantiate<GameObject>(prefab);
//            else
//                go = GameObject.Instantiate<GameObject>(prefab, (Vector3)position, (Quaternion)rotation);

//            var no = go.GetComponent<NetObject>();
//            // no.NetManager = netManager;
//            // no.PrefabId = PrefabId;
//            return no;
//        }
//        public void DestroyNO(NetObject no)
//        {
//            GameObject.Destroy(no.gameObject);
//            no = null;
//        }
//    }
//    // public class TypeSpawnHandler : INetSpawnHandler
//    // {
//    //     private static object[] tempParams = new object[1];

//    //     private Type type;
//    //     private NetManager netManager;

//    //     public string PrefabId { get; }

//    //     public TypeSpawnHandler(NetManager netManager, Type type, string prefabId)
//    //     {
//    //         this.netManager = netManager;
//    //         this.type = type;
//    //         this.PrefabId = prefabId;
//    //     }

//    //     public NetObject CreateNO()
//    //     {
//    //         //tempParams[1] = netManager;
//    //         var no = (NetObject)Activator.CreateInstance(type);
//    //         no.NetManager = netManager;
//    //         no.PrefabId = PrefabId;
//    //         return no;
//    //     }

//    //     public void DestroyNO(NetObject no)
//    //     {
//    //         no.Dispose();
//    //         no = null;
//    //     }
//    // }

//    public class NetSpawnManager : IDisposable
//    {
//        private const string TAG = nameof(NetSpawnManager);
//        protected static ILogger logger = Debug.unityLogger;

//        protected SortedList<int, NetObject> noSList;
//        protected int noIdCounter = 1;
//        protected NetManager netManager;
//        // protected Dictionary<uint, INetSpawnHandler> spawnHandlerDict;
//        protected Dictionary<uint, GameObject> prefabDict;
//        //protected Dictionary<long, int> connPlayerNOIdDict;
//        protected Dictionary<long, NetObject> connPlayerNODict;
//        protected Dictionary<long, List<int>> connOwnedNOIdDict;
//        protected INetSpawnHandler SpawnHandler { get; set; }

//        //public Type PlayerType { get; set; }
//        // public string DefaultPrefabId { get; private set; }
//        public uint PlayerPrefabHash { get; private set; }
//        //[JsonIgnore]
//        public IList<NetObject> NOList => noSList.Values;
//        public Dictionary<long, NetObject> ConnPlayerNODict => connPlayerNODict;
//        public Dictionary<long, List<int>> ConnOwnedNOIdDict => connOwnedNOIdDict;

//        public event Action<NetObject> SpawnNOEvent;
//        public event Action<NetObject> DespawnNOEvent;
//        public event Action<NetObject> SpawnedNOEvent;
//        public event Action<NetObject> DespawnedNOEvent;

//        public NetSpawnManager(NetManager netManager)
//        {
//            // logger = LogUtil.LoggerFactory.CreateLogger(GetType());
//            this.netManager = netManager;
//            noSList = new SortedList<int, NetObject>();
//            SpawnHandler = new PrefabSpawnHandler(netManager);
//            // spawnHandlerDict = new Dictionary<uint, INetSpawnHandler>();
//            prefabDict = new Dictionary<uint, GameObject>();
//            //connPlayerNOIdDict = new Dictionary<long, int>();
//            connPlayerNODict = new Dictionary<long, NetObject>();
//            connOwnedNOIdDict = new Dictionary<long, List<int>>();

//            netManager.MessageHandlerManager.RegisterMessageHandler(this);

//        }
//        public void Dispose()
//        {
//            noSList.Clear();
//            noSList = null;
//            // spawnHandlerDict.Clear();
//            // spawnHandlerDict = null;
//            prefabDict.Clear();
//            prefabDict = null;

//            connPlayerNODict.Clear();
//            connPlayerNODict = null;
//            connOwnedNOIdDict.Clear();
//            connOwnedNOIdDict = null;
//        }
//        public override string ToString()
//        {
//            return $"NetSpawnManager:{{{noSList.ToListString()}}}";
//        }

//        // public void Update()
//        // {
//        //     // var list = noSList.Values;
//        //     // for (int i = 0; i < list.Count; i++)
//        //     // {
//        //     //     list[i].Update();
//        //     // }
//        // }
//        internal void OnNetStart()
//        {
//            noIdCounter = 1;
//            connPlayerNODict.Clear();
//            connOwnedNOIdDict.Clear();
//            var netPrefabs = netManager.NetPrefabs;
//            var playerPrefab = netManager.PlayerPrefab;
//            for (int i = 0; i < netPrefabs.Count; i++)
//            {
//                RegisterNetPrefab(netPrefabs[i]);
//            }
//            if (playerPrefab != null)
//            {
//                PlayerPrefabHash = RegisterNetPrefab(playerPrefab);
//                // DefaultPrefabId = defaultPrefab.name;
//            }
//        }
//        internal void OnNetStop()
//        {
//            noIdCounter = 1;
//            // connPlayerNODict.Clear();
//            // connOwnedNOIdDict.Clear();
//            // noSList.Clear();
//            DespawnAllNOs();
//            prefabDict.Clear();
//            PlayerPrefabHash = 0;
//        }
//        internal void OnNetUpdate()
//        {
//            var list = noSList.Values;
//            for (int i = 0; i < list.Count; i++)
//            {
//                list[i].OnNetUpdate();
//            }
//        }

//        public NetObject GetConnPlayerNO(long connId)
//        {
//            connPlayerNODict.TryGetValue(connId, out var playerNO);
//            return playerNO;
//        }
//        public List<int> GetConnOwnedNOIds(long connId)
//        {
//            connOwnedNOIdDict.TryGetValue(connId, out var list);
//            return list;
//        }
//        public void RemoveConnOwnedNO(long connId, int noId)
//        {
//            logger.LogWarning(TAG, $"RemoveConnOwnedNO: {connId},{noId}");
//            if (!connOwnedNOIdDict.TryGetValue(connId, out var list))
//                return;
//            list.Remove(noId);
//            if (list.Count == 0)
//                connOwnedNOIdDict.Remove(connId);
//        }
//        public void AddConnOwnedNO(long connId, int noId)
//        {
//            logger.LogWarning(TAG, $"AddConnOwnedNO: {connId},{noId}");
//            if (!connOwnedNOIdDict.TryGetValue(connId, out var list))
//            {
//                list = new List<int>();
//                connOwnedNOIdDict.Add(connId, list);
//            }
//            list.Add(noId);
//        }

//        #region register no type
//        // public void RegisterSpawnHandler(string id, INetSpawnHandler handler)
//        // {
//        //     spawnHandlerDict[id] = handler;
//        // }
//        // public void UnregisterSpawnHandler(string id)
//        // {
//        //     spawnHandlerDict.Remove(id);
//        // }
//        public uint RegisterNetPrefab(GameObject prefab)
//        {
//            // var prefabId = prefab.name;
//            var no = prefab.GetComponent<NetObject>();
//            no.InitNBsId();
//            var hash = no.GlobalObjectIdHash;
//            if (prefabDict.ContainsKey(hash))
//                return hash;
//            // handler = new PrefabSpawnHandler(netManager, prefab);
//            // RegisterSpawnHandler(prefabId, handler);
//            prefabDict.Add(hash, prefab);
//            return hash;
//        }
//        #endregion

//        #region netobject
//        // public NetObjectRef CreateNORef(int noId)
//        // {
//        //     var noRef = new NetObjectRef();
//        //     noRef.Init(this, noId);
//        //     return noRef;
//        // }
//        // public void DestroyNORef(NetObjectRef noRef)
//        // {
//        //     noRef.Release();
//        //     noRef = null;
//        // }
//        //public bool TryGetObject(int noId, out ANetObject no)
//        //{
//        //    return noSList.TryGetValue(noId, out no);
//        //}
//        public NetObject GetNO(int noId)
//        {
//            if (!noSList.TryGetValue(noId, out var no))
//            {
//                logger.LogWarning(TAG, $"GetNO: {noId} not exist!");
//            }
//            return no;
//        }
//        public NetObject CreateNO(GameObject prefab, Vector3? position = null, Quaternion? rotation = null)
//        {
//            var no = prefab.GetComponent<NetObject>();
//            if (no == null)
//                return null;
//            var hash = no.GlobalObjectIdHash;
//            return CreateNO(hash, position, rotation);
//        }
//        public NetObject CreateNO(uint hash, Vector3? position = null, Quaternion? rotation = null)
//        {
//            if (hash == 0)
//                hash = PlayerPrefabHash;
//            if (prefabDict.TryGetValue(hash, out var prefab))
//                return SpawnHandler.CreateNO(prefab, position, rotation);
//            var no = new GameObject().AddComponent<NetObject>();
//            return no;
//            // if (!prefabDict.TryGetValue(hash, out var prefab))
//            // {
//            //     logger.LogError(TAG, $"CreateNO: not register {hash}!");
//            //     return null;
//            // }
//            // return SpawnHandler.CreateNO(prefab, position, rotation);
//        }
//        public void DestroyNO(NetObject netObject)
//        {
//            // var prefabId = netObject.PrefabId;
//            // if (!spawnHandlerDict.TryGetValue(prefabId, out var handler))
//            //     return;
//            //netObject.Release();
//            // handler.DestroyNO(netObject);
//            SpawnHandler.DestroyNO(netObject);
//        }
//        internal int GetNetObjectId()
//        {
//            var noId = noIdCounter++;
//            return noId;
//        }
//        // internal void InitNO(NetObject netObject)
//        // {

//        // }
//        // public NetObject SpawnNO(GameObject go, long ownerConnId = -1, bool isPlayer = false, NetDataReader reader = default)
//        // {
//        //     var netObject = go.GetComponent<NetObject>();
//        //     if(netObject == null)
//        //         return null;
//        //     SpawnNO(netObject, ownerConnId,isPlayer, reader);
//        //     return netObject;
//        // }
//        public void SpawnNO(NetObject netObject, long ownerConnId = -1, bool isPlayer = false, NetDataReader reader = default)
//        {
//            var noId = GetNetObjectId();
//            netObject.OnNetInit(netManager, noId, false, isPlayer);
//            SpawnNO(netObject, ownerConnId, reader);
//        }
//        internal void SpawnNO(NetObject no, long ownerConnId, NetDataReader reader)
//        {
//            SpawnNO(no, ownerConnId);
//            OnSpawnNO(no, reader);
//        }
//        internal void SpawnNO(NetObject no, long ownerConnId)
//        {
//            var noId = no.NetObjectId;
//            // logger.LogWarning(TAG, $"SpawnNO: {noId}");
//            var isPlayer = no.IsPlayer;
//            no.ownerConnId = ownerConnId;
//            if (netManager.IsServer)
//            {
//                no.OwnerChangedEvent += OnNOOwnerChangedEvent;
//                no.ObserverConnAddedEvent += OnNOObserverAddedEvent;
//                no.ObserverConnRemovedEvent += OnNOObserverRemovedEvent;

//                // netObject.SetOwner(ownerConnId, true);
//                // if (isPlayer)
//                // {
//                //     netObject.AddObserver(netObject);
//                //     //var conn = netManager.GetConnection(ownerConnId);
//                //     //conn.PlayerNOId = netObject.NetObjectId;
//                // }
//            }
//            else
//            {
//                // netObject.Init(payload);
//                // netObject.SetOwner(ownerConnId, true);
//            }

//            if (netManager.IsServer)
//            {
//                if (isPlayer)
//                {
//                    connPlayerNODict.Add(ownerConnId, no);
//                }
//            }
//            else if (netManager.IsClient)
//            {
//                // if (netObject.IsLocalPlayer)
//                if (isPlayer && ownerConnId == 0)
//                {
//                    connPlayerNODict.Add(0, no);
//                }
//            }
//            noSList.Add(noId, no);
//        }        
//        internal void OnSpawnNO(NetObject no, NetDataReader reader)
//        {
//            if (!netManager.IsServer)
//            {
//                no.Deserialize(ref reader, true);
//            }
//            OnSpawnNO(no);
//        }

//        public void DespawnNO(NetObject no, bool isDestroy = false)
//        {
//            var noId = no.NetObjectId;
//            // logger.LogWarning(TAG, $"DespawnNO: {noId}");

//            // if (no.IsPlayer)
//            // {
//            //     no.RemoveObserver(no);
//            //     if(no.OwnerConnId != -1)
//            //         connPlayerNODict.Remove(no.OwnerConnId);
//            // }
//            if (no.IsServer)
//            {
//                if (no.IsPlayer)
//                {
//                    no.RemoveObserver(no);
//                    connPlayerNODict.Remove(no.OwnerConnId);
//                }
//            }
//            else
//            {
//                if (no.IsLocalPlayer)
//                {
//                    connPlayerNODict.Remove(0);
//                }
//            }
//            noSList.Remove(noId);
//            OnDespawnNO(no);
//            no.OnNetRelease();
//            if (isDestroy)
//                DestroyNO(no);
//        }
//        public void DespawnAllNOs()
//        {
//            var noList = noSList.Values.ToList();
//            for (int i = 0; i < noList.Count; i++)
//            {
//                var no = noList[i];
//                DespawnNO(no);
//                if (!no.IsSceneObj)
//                    DestroyNO(no);
//            }
//        }
//        private void OnSpawnNO(NetObject no)
//        {
//            SpawnNOEvent?.Invoke(no);
//            no.OnSpawned();
//            SpawnedNOEvent?.Invoke(no);
//        }
//        private void OnDespawnNO(NetObject no)
//        {
//            DespawnNOEvent?.Invoke(no);
//            no.OnDespawned();
//            DespawnedNOEvent?.Invoke(no);
//        }

//        private async void OnNOObserverAddedEvent(NetObject no, long observerConnId)
//        {
//            // if(!no.IsSceneObj)
//            SendNOSpawnMessage(observerConnId, no);
//            await Awaiters.NextFrame;
//            SendNOSpawnedMessage(observerConnId, no);
//        }
//        private void OnNOObserverRemovedEvent(NetObject no, long observerConnId)
//        {
//            SendNODespawnedMessage(observerConnId, no);
//        }

//        private void OnNOOwnerChangedEvent(NetObject no, long newOwnerConnId, long oldOwnerConnId)
//        {
//            //if (oldOwnerConnId != 0)
//            //    SendNOAuthorityMessage(oldOwnerConnId, no, false);
//            //if (newOwnerConnId != 0)
//            //    SendNOAuthorityMessage(newOwnerConnId, no, true);
//        }
//        #endregion

//        #region no spawned message

//        [NetMessageHandler(NetMsgTypes.NOMessage)]
//        protected void OnNOMessage(long connId, NetMessage message)
//        {
//            var reader = message.BeginRead();
//            var noId = reader.ReadInt();

//            if (noSList.TryGetValue(noId, out var no))
//            {
//                no.OnReceiveData(connId, ref reader);
//            }
//            //logger.LogTrace($"OnNOMessage: {connId},{noId},{no}");
//            message.EndRead(reader);
//        }

//        [NetMessageHandler(NetMsgTypes.NOSpawnedMessage)]
//        protected void OnNOSpawnedMessage(long connId, NetMessage message)
//        {
//            if (!netManager.IsClient)
//                return;
//            var reader = message.BeginRead();
//            // var prefabId = reader.ReadUInt();
//            var noId = reader.ReadInt();
//            // var isPlayer = reader.ReadBool();
//            // var isSceneObj = reader.ReadBool();
//            // var ownerConnId = reader.ReadPackedLong();

//            // if (isSceneObj)
//            // {
//            //     var no = netManager.SceneManager.GetSceneNO(noId);
//            //     if (no == null)
//            //         logger.LogError(TAG, $"OnNOSpawnedMessage: cant get sceneobj no[{noId}]");
//            //     else
//            //     {
//            //         // no.Init(noId, true, false);
//            //         SpawnNO(no, ownerConnId, reader);
//            //     }
//            // }
//            // else
//            // {
//            //     var no = CreateNO(prefabId);
//            //     no.OnNetInit(netManager, noId, false, isPlayer);
//            //     SpawnNO(no, ownerConnId, reader);
//            // }
//            if (noSList.TryGetValue(noId, out var no))
//            {
//                OnSpawnNO(no, reader);
//                // no.OnReceiveData(connId, ref reader);
//            }
//            message.EndRead(reader);
//        }
//        internal void SendNOSpawnedMessage(long connId, NetObject no)
//        {
//            if (!netManager.IsServer)
//                return;
//            if (connId == netManager.ServerClientId)
//            {
//                return;
//            }
//            var nm = netManager.CreateMessage(NetMsgTypes.NOSpawnedMessage);
//            var writer = nm.BeginWrite();
//            // writer.WriteUInt(no.GlobalObjectIdHash);
//            writer.WriteInt(no.NetObjectId);
//            // writer.WriteBool(no.IsPlayer);
//            // writer.WriteBool(no.IsSceneObj);
//            // writer.WritePackedLong(no.OwnerConnId == connId ? 0 : (long)-1);
//            no.Serialize(ref writer, true);
//            nm.EndWrite(writer);

//            netManager.SendMessage(connId, EDeliveryMethod.ReliableOrdered, nm);
//            nm.Dispose();
//        }
//        [NetMessageHandler(NetMsgTypes.NOSpawnMessage)]
//        protected void OnNOSpawnMessage(long connId, NetMessage message)
//        {
//            if (!netManager.IsClient)
//                return;
//            //var connId = connection.ConnectionId;
//            var reader = message.BeginRead();
//            var prefabId = reader.ReadUInt();
//            var noId = reader.ReadInt();
//            // var payload = new ArraySegment<byte>(reader.ReadByteArray());
//            var isPlayer = reader.ReadBool();
//            var isSceneObj = reader.ReadBool();
//            //var authority = reader.ReadBool();
//            var ownerConnId = reader.ReadPackedLong();

//            if (isSceneObj)
//            {
//                var no = netManager.SceneManager.GetSceneNO(noId);
//                if (no == null)
//                    logger.LogError(TAG, $"OnNOSpawnedMessage: cant get sceneobj no[{noId}]");
//                else
//                {
//                    // no.Init(noId, true, false);
//                    SpawnNO(no, ownerConnId);
//                }
//            }
//            else
//            {
//                var no = CreateNO(prefabId);
//                no.OnNetInit(netManager, noId, false, isPlayer);
//                no.DeserializeGO(ref reader);
//                SpawnNO(no, ownerConnId);
//            }
//            message.EndRead(reader);
//        }
//        internal void SendNOSpawnMessage(long connId, NetObject no)
//        {
//            if (!netManager.IsServer)
//                return;
//            if (connId == netManager.ServerClientId)
//            {
//                return;
//            }
//            var nm = netManager.CreateMessage(NetMsgTypes.NOSpawnMessage);
//            var writer = nm.BeginWrite();
//            writer.WriteUInt(no.GlobalObjectIdHash);
//            writer.WriteInt(no.NetObjectId);
//            // writer.WriteByteArray(no.Payload);
//            writer.WriteBool(no.IsPlayer);
//            writer.WriteBool(no.IsSceneObj);
//            //writer.WriteBool(no.OwnerConnId == connId);
//            writer.WritePackedLong(no.OwnerConnId == connId ? 0 : (long)-1);
//            if(!no.IsSceneObj)
//                no.SerializeGO(ref writer);
//            nm.EndWrite(writer);

//            netManager.SendMessage(connId, EDeliveryMethod.ReliableOrdered, nm);
//            nm.Dispose();
//        }

//        [NetMessageHandler(NetMsgTypes.NODespawnedMessage)]
//        protected void OnNODespawnedMessage(long connId, NetMessage message)
//        {
//            if (!netManager.IsClient)
//                return;
//            //var connId = connection.ConnectionId;
//            var reader = message.BeginRead();
//            var noId = reader.ReadInt();
//            message.EndRead(reader);
//            if (noSList.TryGetValue(noId, out var no))
//            {
//                DespawnNO(no);
//                //DespawnObject(player, payload, connId, isPlayerObject);
//                DestroyNO(no);
//            }

//            //logger.LogDebug($"OnTestMessage: {connection.ConnectionId}, {info}");
//        }
//        internal void SendNODespawnedMessage(long connId, NetObject no)
//        {
//            if (!netManager.IsServer)
//                return;
//            // logger.LogWarning(TAG, $"SendNODespawnedMessage: {connId},{no.NetObjectId}");
//            if (connId == netManager.ServerClientId)
//                return;
//            //var nm = new NetMessage(NetMsgTypes.NODespawnedMessage, -1, NetSettings.SerializerContext);
//            var nm = netManager.CreateMessage(NetMsgTypes.NODespawnedMessage);
//            var writer = nm.BeginWrite();
//            writer.WriteInt(no.NetObjectId);
//            nm.EndWrite(writer);
//            netManager.SendMessage(connId, EDeliveryMethod.ReliableOrdered, nm);
//            nm.Dispose();
//        }
//        #endregion

//        //#region no authority
//        //private void SendNOAuthorityMessage(long connId, ANetObject no, bool hasAuthority)
//        //{
//        //    if (!netManager.IsServer)
//        //        return;
//        //    if (connId == netManager.ServerClientId)
//        //        return;
//        //    //var nm = new NetMessage(NetMsgTypes.NOAuthorityMessage, -1, NetSettings.SerializerContext);
//        //    var nm = netManager.CreateMessage(NetMsgTypes.NOAuthorityMessage);
//        //    var writer = nm.BeginWrite();
//        //    writer.WriteInt(no.NetObjectId);
//        //    writer.WriteBool(hasAuthority);
//        //    nm.EndWrite(writer);

//        //    netManager.SendMessage(connId, EDeliveryMethod.ReliableOrdered, nm);
//        //    //nm.Dispose();
//        //}
//        //[NetMessageHandler(NetMsgTypes.NOAuthorityMessage)]
//        //protected void OnNOAuthorityMessage(NetConnection connection, NetMessage message)
//        //{
//        //    if (!netManager.IsClient)
//        //        return;
//        //    var reader = message.BeginRead();
//        //    var noId = reader.ReadInt();
//        //    var authority = reader.ReadBool();
//        //    if (noSList.TryGetValue(noId, out var no))
//        //        no.SetAuthority(authority);
//        //    message.EndRead(reader);
//        //}
//        //#endregion
//    }
//}
