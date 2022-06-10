using System;

namespace Poly.Net
{
    public enum EDeliveryMethod : byte
    {
        /// <summary>
        /// Unreliable. Packets can be dropped, can be duplicated, can arrive without order.
        /// </summary>
        Unreliable = 4,

        /// <summary>
        /// Reliable. Packets won't be dropped, won't be duplicated, can arrive without order.
        /// </summary>
        ReliableUnordered = 0,

        /// <summary>
        /// Unreliable. Packets can be dropped, won't be duplicated, will arrive in order.
        /// </summary>
        Sequenced = 1,

        /// <summary>
        /// Reliable and ordered. Packets won't be dropped, won't be duplicated, will arrive in order.
        /// </summary>
        ReliableOrdered = 2,

        /// <summary>
        /// Reliable only last packet. Packets can be dropped (except the last one), won't be duplicated, will arrive in order.
        /// </summary>
        ReliableSequenced = 3
    }
    public interface ITransport : IDisposable
    {
        //bool IsClientStarted();
        //bool Connect(string address, int port, Action<bool> callback);
        bool Connect(string address, int port);
        void Disconnect();
        //bool IsServerStarted();
        bool StartListen(int port, int maxConns);
        void StopListen();
        void CloseConnection(long connId);
        //void Dispose();
        //int GetServerPeersCount();

        bool Send(long connId, EDeliveryMethod method, ArraySegment<byte> segment);
        bool PollEvent(out TransportEventData eventData);
        void DisposeEvent(TransportEventData eventData);
    }
    public interface ITransportFactory
    {
        bool CanUseWithWebGL { get; }
        ITransport Build();
    }
}