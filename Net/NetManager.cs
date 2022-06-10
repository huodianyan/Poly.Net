//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Newtonsoft.Json;

//namespace Poly.NetManager
//{
//    public delegate void ConnectionApprovedDelegate(bool createPlayerObject, uint? playerPrefabHash, bool approved);
//    public class NetManager : ANetBase, INetMessageProcessor
//    {
//        public NetObject PlayerPrefab;
//        public List<NetObject> NetPrefabs;
//        public bool IsConnectionApproval = false;
//        public uint TickRate = 30;
//        public List<string> NetSceneIds;
//        private ENetObjectType netObjectType;

//        private bool IsSupportDynamicNB;

//        protected NetSpawnManager spawnManager;
//        protected NMObserveGroup observeGroup;

//        public ArraySegment<byte> connectionApprovalData { get; set; }
//        public NetSpawnManager SpawnManager => spawnManager;
//        public NetObject LocalPlayerNO => spawnManager?.GetConnPlayerNO(0);
//        public ENetObjectType NetObjectType => netObjectType;

//        public event Action<ArraySegment<byte>, long, ConnectionApprovedDelegate> ConnectionApprovalCallback = null;

//        protected override void Awake()
//        {
//            SerializerContext.RegisterSerialzationHandler("NetObject", typeof(NetObject),
//                (ref NetDataWriter writer, object value, Type type) =>
//                {
//                    var no = (NetObject)value;
//                    writer.WriteInt(no.NetObjectId);
//                },
//                (ref NetDataReader reader, Type type) =>
//                {
//                    var noId = reader.ReadInt();
//                    return spawnManager.GetNO(noId);
//                });
//            SerializerContext.RegisterSerialzationHandler("NetBehaviour", typeof(ANetBehaviour),
//                (ref NetDataWriter writer, object value, Type type) =>
//                {
//                    var no = ((ANetBehaviour)value).GetComponent<NetObject>();
//                    writer.WriteInt(no.NetObjectId);
//                },
//                (ref NetDataReader reader, Type type) =>
//                {
//                    var noId = reader.ReadInt();
//                    var no = spawnManager.GetNO(noId);
//                    Debug.Log($"{noId},{type.Name},{no}");
//                    return no?.GetComponent(type);
//                });
//            SerializerContext.RegisterSerialzationHandler("UGameObject", typeof(GameObject),
//                (ref NetDataWriter writer, object value, Type type) =>
//                {
//                    var no = ((GameObject)value).GetComponent<NetObject>();
//                    writer.WriteInt(no.NetObjectId);
//                },
//                (ref NetDataReader reader, Type type) =>
//                {
//                    var noId = reader.ReadInt();
//                    var no = spawnManager.GetNO(noId);
//                    return no?.gameObject;
//                });
//            SerializerContext.RegisterSerialzationHandler("UComponent", typeof(Component),
//                (ref NetDataWriter writer, object value, Type type) =>
//                {
//                    var no = ((Component)value).GetComponent<NetObject>();
//                    writer.WriteInt(no.NetObjectId);
//                },
//                (ref NetDataReader reader, Type type) =>
//                {
//                    var noId = reader.ReadInt();
//                    var no = spawnManager.GetNO(noId);
//                    // Debug.Log($"{noId},{type.Name},{no}");
//                    return no?.GetComponent(type);
//                });

//            spawnManager = new NetSpawnManager(this);
//            observeGroup = new NMObserveGroup();
//        }
//        protected override void OnDestroy()
//        {
//            base.OnDestroy();
//            // Transport.Dispose();
//            // assetManager.Dispose();
//            // assetManager = null;
//            spawnManager.Dispose();
//            spawnManager = null;

//            observeGroup.Dispose();
//            observeGroup = null;
//        }
//        public override void Update()
//        {
//            base.Update();
//            spawnManager?.OnNetUpdate();
//        }

//        #region connection
//        protected override void OnConnectionConnect(long connId)
//        {
//            if (connId == 0)
//            {
//                if (IsClient)
//                    SendConnectionRequestMessage(0, connectionApprovalData);
//            }
//        }
//        protected override void OnConnectionDisconnect(long connId)
//        {
//            if (IsServer)
//            {
//                var list = spawnManager.GetConnOwnedNOIds(connId);
//                if (list != null)
//                {
//                    var idList = list.ToList();
//                    var count = idList.Count;
//                    for (int i = 0; i < count; i++)
//                    {
//                        var noId = idList[i];
//                        var no = spawnManager.GetNO(noId);
//                        if (no == null)
//                            continue;
//                        no.SetOwner(-1);
//                    }
//                }
//            }
//            if (connId == 0)
//            {
//                spawnManager?.DespawnAllNOs();
//                if (IsServer)
//                    StopServer();
//                else if (IsClient)
//                    StopClient();
//            }
//            else
//            {
//                //TODO remove owned nos when conn disconnected
//                //despawn np
//                var playerNO = spawnManager.GetConnPlayerNO(connId);
//                if (playerNO != null)
//                {
//                    spawnManager.DespawnNO(playerNO);
//                    spawnManager.DestroyNO(playerNO);
//                }
//            }
//            OnConnectionDisconnected(connId);
//        }
//        #endregion

//        #region net event
//        protected override void OnNetStart()
//        {
//            spawnManager.OnNetStart();
//            spawnManager.SpawnNOEvent += OnSpawnNO;
//            spawnManager.DespawnNOEvent += OnDespawnNO;
//            base.OnNetStart();
//        }
//        protected override void OnNetStop()
//        {
//            base.OnNetStop();
//            spawnManager.OnNetStop();
//            spawnManager.SpawnNOEvent -= OnSpawnNO;
//            spawnManager.DespawnNOEvent -= OnDespawnNO;
//        }
//        #endregion

//        #region observeGroup
//        protected virtual void OnDespawnNO(NetObject obj)
//        {
//            observeGroup.RemoveObject(obj);
//        }
//        protected virtual void OnSpawnNO(NetObject obj)
//        {
//            observeGroup.AddObject(obj);
//        }
//        #endregion

//        #region connection approve
//        protected void SendConnectionRequestMessage(uint hash, ArraySegment<byte> payload)
//        {
//            if (IsServer)
//            {
//                OnConnectionRequest(serverClientId, payload);
//                return;
//            }
//            var netMessage = new NetMessage(NetMsgTypes.ConnectionRequestMessage, -1, NetSettings);
//            var writer = netMessage.BeginWrite();
//            writer.WriteUShort((ushort)payload.Count);
//            writer.WriteBytes(payload);
//            netMessage.EndWrite(writer);
//            SendMessage(0, EDeliveryMethod.ReliableOrdered, netMessage);
//            netMessage.Dispose();
//        }
//        [NetMessageHandler(NetMsgTypes.ConnectionRequestMessage)]
//        protected void OnConnectionRequestMessage(long connId, NetMessage message)
//        {
//            var reader = message.BeginRead();
//            var count = reader.ReadUShort();
//            var data = NetSettings.ArrayPool.Rent(count);
//            reader.ReadBytes(data, 0, count);
//            message.EndRead(reader);
//            var payload = new ArraySegment<byte>(data, 0, count);
//            OnConnectionRequest(connId, payload);
//        }

//        private void OnConnectionRequest(long connId, ArraySegment<byte> payload)
//        {
//            if (IsConnectionApproval)
//            {
//                InvokeConnectionApproval(payload, connId,
//                    (createPlayerObject, playerPrefabHash, approved) =>
//                    {
//                        HandleApproval(connId, createPlayerObject, playerPrefabHash, approved);
//                    });
//            }
//            else
//            {
//                HandleApproval(connId, PlayerPrefab != null, null, true);
//            }
//        }
//        internal void InvokeConnectionApproval(ArraySegment<byte> payload, long clientId, ConnectionApprovedDelegate action)
//        {
//            ConnectionApprovalCallback?.Invoke(payload, clientId, action);
//        }
//        internal void HandleApproval(long connId, bool createPlayerObject, uint? playerPrefabHash, bool approved)
//        {
//            // logger.LogWarning(TAG, $"HandleApproval: ");
//            if (approved)
//            {
//                if (createPlayerObject)
//                {
//                    var hash = playerPrefabHash ?? 0;
//                    var playerNO = spawnManager.CreateNO(hash);
//                    spawnManager.SpawnNO(playerNO, connId, true, default);
//                }
//                if (!IsClient)
//                    SendConnectionApprovedMessage(connId);
//                OnConnectionConnected(connId);
//            }
//            else
//            {
//                //disconnect!
//            }
//        }
//        //server
//        protected void SendConnectionApprovedMessage(long ownerConnId)
//        {
//            var netMessage = CreateMessage(NetMsgTypes.ConnectionApprovedMessage);
//            var writer = netMessage.BeginWrite();
//            writer.WriteLong(ownerConnId);
//            netMessage.EndWrite(writer);
//            SendMessage(0, EDeliveryMethod.ReliableOrdered, netMessage);
//            netMessage.Dispose();
//        }
//        [NetMessageHandler(NetMsgTypes.ConnectionApprovedMessage)]
//        protected void OnConnectionApprovedMessage(long connId, NetMessage message)
//        {
//            var reader = message.BeginRead();
//            var ownerClientId = reader.ReadLong();
//            //var networkTick = reader.ReadInt();
//            message.EndRead(reader);

//            //set client id
//            // networkManager.LocalClientId = ownerClientId;
//            OnConnectionConnected(connId);
//        }
//        #endregion

//        #region time

//        #endregion
//    }
//}
