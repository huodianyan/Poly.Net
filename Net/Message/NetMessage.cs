using Poly.Serialization;
using System;
using System.Runtime.CompilerServices;

namespace Poly.Net
{
    public static class NetMsgTypes
    {
        public const ushort ConnectMessage = 1;
        public const ushort DisconnectMessage = 2;
        public const ushort ErrorMessage = 3;
        public const ushort ConnectedMessage = 4;

        public const ushort RPCReqMessage = 5;
        public const ushort RPCRespMessage = 6;

        public const ushort HeartBeatMessage = 9;

        public const ushort CreatePlayerMessage = 10;
        public const ushort SyncListMessage = 11;
        internal const ushort SyncTimeMessage = 12;
        internal const ushort ConnectionRequestMessage = 13;
        internal const ushort ConnectionApprovedMessage = 14;

        public const ushort NOMessage = 20;
        
        //public const ushort NORPCReqMessage = 10;
        //public const ushort NORPCRespMessage = 11;
        public const ushort NOSpawnMessage = 30;
        public const ushort NOSpawnedMessage = 21;
        public const ushort NODespawnedMessage = 22;
        public const ushort NOSyncMessage = 23;
        public const ushort NOOwnerMessage = 24;
        public const ushort NOAddNBMessage = 28;
        public const ushort NORemoveNBMessage = 29;

        public const ushort NOSyncListMessage = 25;
        public const ushort NOSyncArrayMessage = 26;

        public const ushort PlayerDataMessage = 27;

        // public const ushort NEMessage = 50;
        // public const ushort NESpawnMessage = 60;
        // public const ushort NESpawnedMessage = 51;
        // public const ushort NEDespawnedMessage = 52;
        // public const ushort NESyncMessage = 53;
        // public const ushort NEOwnerMessage = 54;
        // public const ushort NEAddNBMessage = 58;
        // public const ushort NERemoveNBMessage = 59;

        // public const ushort NESyncListMessage = 55;
        // public const ushort NESyncArrayMessage = 56;
    }
    public struct NetMessage : IDisposable, IDataSerializable
    {
        public static int MaxLength = 1024;

        internal ushort msgType;
        private byte[] data;
        private readonly ushort offset;
        private ushort count;
        private ushort position;
        private readonly NetSettings settings;

        public bool IsCreated => data != null;
        public ArraySegment<byte> Segment => new ArraySegment<byte>(data, offset, count);

        public NetMessage(ushort msgType, int capacity = -1, NetSettings settings = null)
        {
            this.msgType = msgType;
            if (capacity < 0) capacity = MaxLength;
            this.settings = settings ?? NetSettings.DefaultSettings;
            data = settings.ArrayPool.Rent(capacity);
            offset = 0;
            count = 0;
            //segment = new ArraySegment<byte>(this.settings.ArrayPool.Rent(capacity));
            position = 0;
        }
        public NetMessage(ushort msgType, byte[] data, int offset, int count, int position, NetSettings settings = null)
        {
            this.msgType = msgType;
            //this.segment = segment;
            this.data = data;
            this.offset = (ushort)offset;
            this.count = (ushort)count;
            this.position = (ushort)position;
            this.settings = settings ?? NetSettings.DefaultSettings;
        }
        public override string ToString()
        {
            return $"NetMessage:{{{msgType},{offset},{position},{count}}}";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetReadPosition()
        {
            position = (ushort)(offset + 4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DataReader BeginRead()
        {
            return new DataReader(data, offset, count, position, settings.Context);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndRead(DataReader reader)
        {
            position = (ushort)reader.Position;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DataWriter BeginWrite()
        {
            var writer = new DataWriter(data, offset, position, settings.Context);
            if (offset == position)
            {
                writer.WriteUShort(0);
                writer.WriteUShort(msgType);
            }
            return writer;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndWrite(DataWriter writer)
        {
            data = writer.Data;
            count = (ushort)writer.Count;
            position = (ushort)writer.Position;
            writer.PutUShort(0, position);
        }
        public void Dispose()
        {
            settings.ArrayPool.Return(data);
            data = null;
        }
        public void Serialize(ref DataWriter writer)
        {
            writer.WriteUShort(msgType);
            writer.WriteUShort(count);
            writer.WriteBytes(data, offset, count);
        }
        public void Deserialize(ref DataReader reader)
        {
            msgType = reader.ReadUShort();
            count = reader.ReadUShort();
            data = settings.ArrayPool.Rent(count);
            position = 0;
            reader.ReadBytes(data, 0, count);
            //segment = new ArraySegment<byte>();
        }
    }
}