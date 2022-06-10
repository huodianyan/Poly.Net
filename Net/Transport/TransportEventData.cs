using System;
namespace Poly.Net
{
    public enum ETranportEvent : byte
    {
        Data,
        Connect,
        Disconnect,
        Error,
    }
    public struct TransportEventData : IDisposable
    {
        public ETranportEvent Event;
        public long ConnId;
        public ArraySegment<byte> Segment;
        //public IPEndPoint EndPoint;
        //public SocketError SocketError;
        public void Dispose()
        {
        }
        public override string ToString()
        {
            return $"TransportEventData:{{{Event},{ConnId},{Segment.Count}}}";
        }
    }
}