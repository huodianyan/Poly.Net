using Poly.Net.Transport;
using Poly.Serialization;
using Poly.Tcp;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Poly.Net
{
    public enum ENetServiceType
    {
        None,
        Client,
        Server,
        Host
    }
    public class NetSettings
    {
        public static NetSettings DefaultSettings = new NetSettings
        {
            Context = DataSerializationContext.DefaultContext,
            ArrayPool = new ByteArrayPool()
        };
        public IPolySerializationContext Context;
        public IByteArrayPool ArrayPool;
        //public string Address { get; set; }
        //public int Port { get; set; }
        //public int MaxConnections = 1000;
    }
    public interface INetBase
    {
        bool IsServer { get; }
        bool IsClient { get; }
        //NetSettings NetSettings { get; }
        NetMessageProcessor MessageProcessor { get; }
        long LocalClientId { get; }

        void SendMessage(long connId, NetMessage message, EDeliveryMethod method = EDeliveryMethod.ReliableOrdered);
        void SendMessage(IList<long> connIds, NetMessage message, long excludeId = -1, EDeliveryMethod method = EDeliveryMethod.ReliableOrdered);
        NetMessage CreateMessage(ushort msgId);
    }
    public static class NetBaseExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHost(this INetBase netBase) => netBase.IsClient && netBase.IsServer;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsClientOnly(this INetBase netBase) => netBase.IsClient && !netBase.IsServer;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsServerOnly(this INetBase netBase) => !netBase.IsClient && netBase.IsServer;
    }
    public abstract class ANetBase : INetBase, IDisposable
    {
        protected List<long> connIdList = new List<long>();
        protected NetMessageProcessor messageProcessor;
        protected NetRpcManager rpcManager;
        protected long localClientId = 0;
        //protected long serverClientId = 0;
        protected bool isConnected;
        protected bool isListening;
        protected ENetServiceType serviceType;

        public int HostId { get; }
        public string Address { get; set; }
        public int Port { get; set; }
        public int MaxConns { get; set; } = 100;
        public NetMessageProcessor MessageProcessor => messageProcessor;
        public NetRpcManager RpcManager => rpcManager;
        public ITransport Transport { get; }
        // public bool IsStarted => IsClient || IsServer;
        public bool IsStarted { get; private set; }
        public bool IsClient { get; protected set; }
        public bool IsServer { get; protected set; }
        //public bool IsHost => IsClient && IsServer;
        //public bool IsClientOnly => IsClient && !IsServer;
        //public bool IsServerOnly => !IsClient && IsServer;
        public NetSettings NetSettings { get; }
        public bool IsConnected =>isConnected;
        public bool IsListening => isListening;
        public IList<long> ConnIdList => connIdList;
        public long LocalClientId => localClientId;
        //public long ServerClientId => serverClientId;

        public event Action<long> ClientConnectedEvent;
        public event Action<long> ClientDisconnectedEvent;
        public event Action<int> NetStartedEvent;
        public event Action<int> NetStoppedEvent;

        #region ANetBase
        public ANetBase(int hostId, NetSettings netSettings, ITransport transport)
        {
            HostId = hostId;
            NetSettings = netSettings ?? NetSettings.DefaultSettings;
            Transport = transport ?? new PolyTcpTransport();

            messageProcessor = new NetMessageProcessor();
            messageProcessor.RegisterMessageHandler(this);
            rpcManager = new NetRpcManager(this);
            rpcManager.RegisterRPC(this);
        }
        public override string ToString()
        {
            return $"{GetType().Name}:{{{IsClient},{IsServer},{string.Join(",", connIdList)}}}";
        }
        public virtual void Dispose()
        {
            rpcManager.Dispose();
            rpcManager = null;
            messageProcessor.Dispose();
            messageProcessor = null;
            if (IsStarted)
            {
                if (IsClient)
                    Transport.Disconnect();
                if (IsServer)
                    Transport.StopListen();
            }
        }
        public virtual void Update()
        {
            messageProcessor?.Update();
            while (Transport.PollEvent(out var eventData))
            {
                ProcessEventData(eventData);
                Transport.DisposeEvent(eventData);
            }
            OnUpdate();
        }
        protected virtual void OnUpdate() { }
        protected virtual void OnNetStart()
        {
            NetStartedEvent?.Invoke(HostId);
        }
        protected virtual void OnNetStop()
        {
            NetStoppedEvent?.Invoke(HostId);
        }
        #endregion

        #region connection
        protected virtual void ProcessEventData(TransportEventData eventData)
        {
            var connId = eventData.ConnId;
            //Console.WriteLine($"ProcessEventData: {HostId}:{connId},{eventData.Event}!");
            switch (eventData.Event)
            {
                case ETranportEvent.Connect:
                    if (connIdList.Contains(connId))
                        Console.Error.WriteLine($"ProcessEventData: connId exsit {connId}!");
                    else
                    {
                        connIdList.Add(connId);
                        var message = new NetMessage { msgType = NetMsgTypes.ConnectMessage };
                        OnConnectionReceiveMessage(connId, message);
                        OnConnectionConnect(connId);
                    }
                    break;
                case ETranportEvent.Data:
                    var reader = new DataReader(eventData.Segment);
                    var size = reader.GetUShort(0);
                    var msgId = reader.GetUShort(2);
                    if (reader.AvailableCount != size)
                        Console.Error.WriteLine($"OnConnectionReceiveData: {reader}->{size},{msgId}");
                    else
                        OnConnectionReceiveMessage(eventData.ConnId, new NetMessage(msgId, eventData.Segment.Array, 0, size, 4, NetSettings));
                    eventData.Dispose();
                    //OnConnectionReceiveData(eventData);
                    break;
                case ETranportEvent.Disconnect:
                    if (!connIdList.Contains(connId))
                        Console.Error.WriteLine($"ProcessEventData: connId not exsit {connId}!");
                    else
                    {
                        connIdList.Remove(connId);
                        var message = new NetMessage { msgType = NetMsgTypes.DisconnectMessage };
                        OnConnectionReceiveMessage(connId, message);
                        OnConnectionDisconnect(connId);
                    }
                    break;
                case ETranportEvent.Error:
                    if (!connIdList.Contains(connId))
                        Console.Error.WriteLine($"ProcessEventData: connId not exsit {connId}!");
                    else
                        OnConnectionReceiveMessage(connId, new NetMessage { msgType = NetMsgTypes.ErrorMessage });
                    break;
            }
        }
        protected virtual void OnConnectionConnect(long connId)
        {
            if(IsClient && !IsServer)
            {
                localClientId = connId;
            }
            //if (IsServer)
            //{
            //    localClientId = connId;
            //}
            OnConnectionConnected(connId);
        }
        protected virtual void OnConnectionDisconnect(long connId)
        {
            OnConnectionDisconnected(connId);
        }
        protected virtual void OnConnectionConnected(long connId)
        {
            //logger.LogWarning(TAG, $"OnConnectionConnected: {connId}");
            if(connId == 0) isConnected = true;
            ClientConnectedEvent?.Invoke(connId);
        }
        protected virtual void OnConnectionDisconnected(long connId)
        {
            //logger.LogWarning(TAG, $"OnConnectionDisconnected: {connId}");
            if(connId == 0) isConnected = false;
            ClientDisconnectedEvent?.Invoke(connId);
        }
        //protected virtual void OnConnectionError(long connId)
        //{
        //}
        //protected void OnConnectionReceiveData(TransportEventData eventData)
        //{
        //}
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnConnectionReceiveMessage(long connId, NetMessage message)
        {
            // logger.Log(TAG, $"OnReceiveMessage: {connId},{message}");
            //var msgId = message.MsgType;
            messageProcessor.Process(connId, message);
            //message.Dispose();
        }
        #endregion

        //#region ConnectedMessage
        //internal void SendConnectedMessage(long connId)
        //{
        //    if (!IsServer) return;
        //    var nm = CreateMessage(NetMsgTypes.ConnectedMessage);
        //    var writer = nm.BeginWrite();
        //    writer.WritePackedLong(connId);
        //    nm.EndWrite(writer);
        //    //Console.WriteLine($"{GetType().Name}.SendS2CMessage: {HostId},{0},{info}");
        //    SendMessage(connId, nm);
        //    nm.Dispose();
        //}
        //[NetMessageHandler(NetMsgTypes.ConnectedMessage)]
        //void OnConnectedMessage(long connId, NetMessage message)
        //{
        //    if (!IsClient) return;
        //    var reader = message.BeginRead();
        //    var info = reader.ReadString();
        //    message.EndRead(reader);
        //    //Console.WriteLine($"{GetType().Name}.OnS2CMessage: {HostId},{connId},{info}");
        //    lastClientMsgInfo = new TestMsgInfo(connId, 1000, info);
        //}
        //#endregion

        #region send message
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NetMessage CreateMessage(ushort msgId)
        {
            return new NetMessage(msgId, -1, NetSettings);
        }
        public void SendMessage(long connId, NetMessage message, EDeliveryMethod method = EDeliveryMethod.ReliableOrdered)
        {
            if (IsServer && connId == localClientId)
            {
                message.ResetReadPosition();
                OnConnectionReceiveMessage(connId, message);
                return;
            }
            //if (!connIdList.Contains(connId)) return;
            Transport.Send(connId, method, message.Segment);
            //netMessage.Dispose();
        }
        public void SendMessage(IList<long> connIds, NetMessage message, long excludeId = -1, EDeliveryMethod method = EDeliveryMethod.ReliableOrdered)
        {
            if (connIds == null) connIds = connIdList;
            //Console.WriteLine($"SendMessage: {HostId},connIds:[{string.Join(",", connIds)}],{excludeId}");
            for (int i = 0; i < connIds.Count; i++)
            {
                var connId = connIds[i];
                //if (connId == 0) continue;
                if (excludeId != -1 && connId == excludeId) continue;
                if (IsServer && connId == localClientId)
                {
                    message.ResetReadPosition();
                    OnConnectionReceiveMessage(connId, message);
                    continue;
                }
                Transport.Send(connId, method, message.Segment);
            }
            //netMessage.Dispose();
        }
        #endregion

        #region client
        public bool StartClient()
        {
            if (IsStarted) return false;
            IsStarted = true;
            var ok = Transport.Connect(Address, Port);
            if (!ok)
            {
                Console.Error.WriteLine($"Cannot connect to server");
                return false;
            }
            IsClient = true;
            OnNetStart();
            return ok;
        }
        public void StopClient()
        {
            if (!IsStarted) return;
            IsStarted = false;
            IsClient = false;
            Transport.Disconnect();
            OnNetStop();
        }
        #endregion

        #region server
        public bool StartServer()
        {
            if (IsStarted) return false;
            IsStarted = true;
            IsServer = Transport.StartListen(Port, MaxConns);
            isListening = true;
            OnNetStart();
            return IsServer;
        }
        public void StopServer()
        {
            if (!IsStarted) return;
            IsStarted = false;
            IsServer = false;
            Transport.StopListen();
            isListening = false;
            OnNetStop();
        }
        public void ServerDisconnect(long connId)
        {
            if (connId == 0) return;
            Transport.CloseConnection(connId);
        }
        #endregion

        #region host
        public bool StartHost()
        {
            if (IsStarted) return false;
            IsStarted = true;
            IsServer = Transport.StartListen(Port, MaxConns);
            IsClient = true;
            OnNetStart();
            ProcessEventData(new TransportEventData { ConnId = 0, Event = ETranportEvent.Connect});
            //OnConnectionConnect(ServerClientId);
            return IsServer;
        }
        public void StopHost()
        {
            if (!IsStarted) return;
            IsStarted = false;
            Transport.StopListen();
            IsClient = false;
            IsServer = false;
            ProcessEventData(new TransportEventData { ConnId = 0, Event = ETranportEvent.Disconnect });
            //OnConnectionDisconnect(ServerClientId);
            OnNetStop();
        }
        #endregion

        #region service
        public bool StartService(ENetServiceType serviceType)
        {
            this.serviceType = serviceType;
            if (serviceType == ENetServiceType.Client)
                return StartClient();
            else if (serviceType == ENetServiceType.Server)
                return StartServer();
            else if (serviceType == ENetServiceType.Host)
                return StartHost();
            return false;
        }
        public void StopService()
        {
            if (serviceType == ENetServiceType.Client)
                StopClient();
            else if (serviceType == ENetServiceType.Server)
                StopServer();
            else if (serviceType == ENetServiceType.Host)
                StopHost();
            serviceType = ENetServiceType.None;
        }
        #endregion
    }
}