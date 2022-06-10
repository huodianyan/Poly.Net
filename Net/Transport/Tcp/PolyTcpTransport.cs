using Poly.Tcp;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Poly.Net.Transport
{
    public sealed class PolyTcpTransport : ITransport
    {
        private PolyTcpClient client;
        private PolyTcpServer server;
        private ConcurrentQueue<TransportEventData> clientEventQueue;
        private ConcurrentQueue<TransportEventData> serverEventQueue;

        public PolyTcpTransport()
        {
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsClientStarted() => client != null && client.IsConnected;
        public bool Connect(string address, int port)
        {
            if (IsClientStarted())
                return false;
            client = new PolyTcpClient();
            clientEventQueue = new ConcurrentQueue<TransportEventData>();
            //while (clientEventQueue.TryDequeue(out var _)) { }
            client.OnConnectEvent += (connId) => clientEventQueue.Enqueue(new TransportEventData() { Event = ETranportEvent.Connect, ConnId = connId });
            client.OnDisconnectEvent += (connId) => clientEventQueue.Enqueue(new TransportEventData() { Event = ETranportEvent.Disconnect, ConnId = connId });
            client.OnRecieveEvent += (connId, segment) =>
            {
                var count = segment.Count;
                var dest = client.ArrayPool.Rent(count);
                Array.Copy(segment.Array, segment.Offset, dest, 0, count);
                clientEventQueue.Enqueue(new TransportEventData() { Event = ETranportEvent.Data, ConnId = connId, Segment = new ArraySegment<byte>(dest, 0, count) });
            };
            var ok = client.Connect(address, port);
            if (!ok) Disconnect();
            return ok;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Disconnect()
        {
            client?.Disconnect();
            //clientEventQueue = null;
            client = null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsServerStarted() => server != null && server.IsStarted;
        public bool StartListen(int port, int maxConnections)
        {
            if (IsServerStarted())
                return false;
            server = new PolyTcpServer();
            serverEventQueue = new ConcurrentQueue<TransportEventData>();
            if (!server.Start(port)) return false;
            server.OnConnectEvent += (connId) => serverEventQueue.Enqueue(new TransportEventData() { Event = ETranportEvent.Connect, ConnId = connId });
            server.OnDisconnectEvent += (connId) => serverEventQueue.Enqueue(new TransportEventData() { Event = ETranportEvent.Disconnect, ConnId = connId });
            server.OnRecieveEvent += (connId, segment) =>
            {
                var count = segment.Count;
                var dest = server.ArrayPool.Rent(count);
                Array.Copy(segment.Array, segment.Offset, dest, 0, count);
                serverEventQueue.Enqueue(new TransportEventData() { Event = ETranportEvent.Data, ConnId = connId, Segment = new ArraySegment<byte>(dest, 0, count) });
            };
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CloseConnection(long connectionId)
        {
            if (server == null) return;
            server.Disconnect(connectionId);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopListen()
        {
            server?.Stop();
            server = null;
            //serverEventQueue = null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Disconnect();
            clientEventQueue = null;
            StopListen();
            serverEventQueue = null;
        }
        public bool Send(long connId, EDeliveryMethod deliveryMethod, ArraySegment<byte> data)
        {
            if (connId == 0)
            {
                if (!IsClientStarted()) return false;
                return client.Send(data);
            }
            else
            {
                if (!IsServerStarted()) return false;
                return server.Send(connId, data);
            }
        }
        public bool PollEvent(out TransportEventData eventData)
        {
            eventData = default;
            if (serverEventQueue != null && serverEventQueue.Count > 0)
                return serverEventQueue.TryDequeue(out eventData);
            if (clientEventQueue != null && clientEventQueue.Count > 0)
                return clientEventQueue.TryDequeue(out eventData);
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DisposeEvent(TransportEventData eventData)
        {
            var bytes = eventData.Segment.Array;
            if (bytes == null) return;
            if (server != null) server.ArrayPool.Return(bytes);
            else if (client != null) client.ArrayPool.Return(bytes);
        }
    }
}