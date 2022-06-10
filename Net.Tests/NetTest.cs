using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Poly.Net.Transport;

namespace Poly.Net.Tests
{
    [TestClass]
    public partial class NetTest
    {
        private string address = "localhost";
        private int port = 9000;
        //private NetDriver host;
        //private NetDriver client;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
        }
        [ClassCleanup]
        public static void ClassCleanup()
        {
        }
        [TestInitialize]
        public void TestInitialize()
        {
            //sendThread = new Thread(() => UpdateLoop());
            //sendThread.IsBackground = true;
            //sendThread.Start();
        }
        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public async Task HostTest()
        {
            var tcs = new TaskCompletionSource<bool>();

            //init host
            var netSettings = NetSettings.DefaultSettings;
            var hostTransport = new PolyTcpTransport();
            var host = new TestNet1(-1, netSettings, hostTransport);
            host.Address = address;
            host.Port = port;

            //init client0
            var clientCount = 4;
            var clients = new TestNet1[clientCount];
            for (int i = 0; i < clientCount; i++)
            {
                var clientTransport = new PolyTcpTransport();
                var client = new TestNet1(i, netSettings, clientTransport);
                client.Address = address;
                client.Port = port;
                clients[i] = client;
            }
            //init timer
            var timer = new Timer((state) =>
            {
                host?.Update();
                //client0?.Update();
                for (int i = 0; i < clientCount; i++) clients[i]?.Update();
            }, null, 0, 50);

            //start host
            host.StartHost();
            Assert.IsTrue(host.IsStarted);
            Assert.IsTrue(host.IsHost());
            Assert.AreEqual(new TestMsgInfo(0, NetMsgTypes.ConnectMessage), host.lastServerMsgInfo);

            //start client0
            for (int i = 0; i < clientCount; i++)
            {
                var client = clients[i];
                client.StartClient();
                Assert.IsTrue(client.IsStarted);
                Assert.IsTrue(client.IsClient);
                await Task.Delay(100);
                Assert.AreEqual(new TestMsgInfo(i + 1, NetMsgTypes.ConnectMessage), host.lastServerMsgInfo);
            }
            //await Task.Delay(2000);
            //await tcs.Task;
            //for (int i = 0; i < 10; i++)
            //{
            //    client0.SendTestMessage($"info_{i}");
            //    await Task.Delay(300);
            //}
            var client0 = clients[0];
            var client2 = clients[2];
            //test C2SMessage
            host.SendC2SMessage($"C2SMessage");
            await Task.Delay(100);
            Assert.AreEqual(new TestMsgInfo(0, 1001, "C2SMessage"), host.lastServerMsgInfo);
            client0.SendC2SMessage($"C2SMessage");
            await Task.Delay(100);
            Assert.AreEqual(new TestMsgInfo(1, 1001, "C2SMessage"), host.lastServerMsgInfo);
            //test S2CMessage
            host.SendS2CMessage($"S2CMessage");
            await Task.Delay(100);
            Assert.AreEqual(new TestMsgInfo(0, 1000, "S2CMessage"), host.lastClientMsgInfo);
            Assert.AreEqual(new TestMsgInfo(0, 1000, "S2CMessage"), client0.lastClientMsgInfo);
            //test SRpcReq
            host.SRpcReq(0, "SRpcReq");
            await Task.Delay(200);
            Assert.AreEqual(new TestMsgInfo(0, 2000, "SRpcReq"), host.lastServerMsgInfo);
            Assert.AreEqual(new TestMsgInfo(0, 2001, "SRpcReq_R"), host.lastClientMsgInfo);
            //Assert.AreEqual(new TestMsgInfo(0, 2001, "SRpcReq_R"), client0.lastClientMsgInfo);

            client0.SRpcReq(client0.LocalClientId, "SRpcReq");
            await Task.Delay(200);
            Assert.AreEqual(new TestMsgInfo(1, 2000, "SRpcReq"), host.lastServerMsgInfo);
            //Assert.AreEqual(new TestMsgInfo(0, 2001, "SRpcReq_R"), host.lastClientMsgInfo);
            Assert.AreEqual(new TestMsgInfo(client0.LocalClientId, 2001, "SRpcReq_R"), client0.lastClientMsgInfo);

            host.CRpcBroadcast(-1, "CRpcBroadcast");
            await Task.Delay(200);
            Assert.AreEqual(new TestMsgInfo(host.LocalClientId, 2002, "CRpcBroadcast"), host.lastClientMsgInfo);
            Assert.AreEqual(new TestMsgInfo(client2.LocalClientId, 2002, "CRpcBroadcast"), client2.lastClientMsgInfo);

            host.CRpcBroadcast(1, "CRpcBroadcastExc");
            await Task.Delay(200);
            Assert.AreEqual(new TestMsgInfo(host.LocalClientId, 2002, "CRpcBroadcastExc"), host.lastClientMsgInfo);
            Assert.AreNotEqual(new TestMsgInfo(client0.LocalClientId, 2002, "CRpcBroadcastExc"), client0.lastClientMsgInfo);

            host.lastClientMsgInfo = client0.lastClientMsgInfo = default;
            host.CRpcBroadcast(0, "CRpcBroadcastExc");
            await Task.Delay(200);
            Assert.AreNotEqual(new TestMsgInfo(host.LocalClientId, 2002, "CRpcBroadcastExc"), host.lastClientMsgInfo);
            Assert.AreEqual(new TestMsgInfo(client0.LocalClientId, 2002, "CRpcBroadcastExc"), client0.lastClientMsgInfo);

            //stop client0
            for (int i = 0; i < clientCount; i++)
            {
                var client = clients[i];
                client.StopClient();
                Assert.IsFalse(client.IsStarted);
                await Task.Delay(100);
            }
            //stop host
            host.StopHost();
            Assert.IsFalse(host.IsStarted);

            //dispose timer
            await Task.Delay(1000);
            timer.Dispose();

            //dispose client0
            for (int i = 0; i < clientCount; i++)
            {
                var client = clients[i];
                client.Dispose();
                client = null;
            }
            //dispose host
            host.Dispose();
            host = null;

        }
        public struct TestMsgInfo : IEquatable<TestMsgInfo>
        {
            public long ConnId;
            public ushort MsgType;
            public string MsgInfo;
            public TestMsgInfo(long connId, ushort msgType, string msgInfo = null)
            {
                ConnId = connId;
                MsgType = msgType;
                MsgInfo = msgInfo;
            }
            public override string ToString()
            {
                return $"TestMsgInfo:{{{ConnId},{MsgType},{MsgInfo}}}";
            }
            public bool Equals(TestMsgInfo other)
            {
                return MsgInfo == other.MsgInfo && ConnId == other.ConnId && MsgType == other.MsgType;
            }
        }
        public class TestNet1 : ANetBase
        {
            public TestNet1(int hostId, NetSettings netSettings, ITransport transport) : base(hostId, netSettings, transport)
            {
                messageProcessor.RegisterMessageHandler(NetMsgTypes.ConnectMessage, OnConnectMessage);

            }
            public override void Dispose()
            {
                base.Dispose();
            }

            #region CommonMessage
            void OnConnectMessage(long connId, NetMessage message)
            {
                //Console.WriteLine($"{GetType().Name}.OnConnectMessage: {HostId},{connId},{message}");
                lastServerMsgInfo = new TestMsgInfo(connId, NetMsgTypes.ConnectMessage);
            }
            [NetMessageHandler(NetMsgTypes.DisconnectMessage)]
            void OnDisconnectMessage(long connId, NetMessage message)
            {
                //Console.WriteLine($"{GetType().Name}.OnDisconnectMessage: {HostId},{connId},{message}");
                lastServerMsgInfo = new TestMsgInfo(connId, NetMsgTypes.DisconnectMessage);
            }
            #endregion

            #region C2SMessage
            internal TestMsgInfo lastServerMsgInfo;
            internal TestMsgInfo lastClientMsgInfo;
            internal void SendC2SMessage(string info)
            {
                if (!IsClient) return;
                var nm = CreateMessage(1001);
                var writer = nm.BeginWrite();
                writer.WriteString(info);
                nm.EndWrite(writer);
                //Console.WriteLine($"{GetType().Name}.SendC2SMessage: {HostId},{0},{info}");
                SendMessage(0, nm);
                nm.Dispose();
            }
            [NetMessageHandler(1001)]
            void OnC2SMessage(long connId, NetMessage message)
            {
                if (!IsServer) return;
                var reader = message.BeginRead();
                var info = reader.ReadString();
                message.EndRead(reader);
                //Console.WriteLine($"{GetType().Name}.OnC2SMessage: {HostId},{connId},{info}");
                lastServerMsgInfo = new TestMsgInfo(connId, 1001, info);
            }
            #endregion

            #region S2CMessage
            internal void SendS2CMessage(string info)
            {
                if (!IsServer) return;
                var nm = CreateMessage(1000);
                var writer = nm.BeginWrite();
                writer.WriteString(info);
                nm.EndWrite(writer);
                //Console.WriteLine($"{GetType().Name}.SendS2CMessage: {HostId},{0},{info}");
                SendMessage(null, nm);
                nm.Dispose();
            }
            [NetMessageHandler(1000)]
            void OnS2CMessage(long connId, NetMessage message)
            {
                if (!IsClient) return;
                var reader = message.BeginRead();
                var info = reader.ReadString();
                message.EndRead(reader);
                //Console.WriteLine($"{GetType().Name}.OnS2CMessage: {HostId},{connId},{info}");
                lastClientMsgInfo = new TestMsgInfo(connId, 1000, info);
            }
            #endregion

            #region rpc
            [SRpc]
            internal void SRpcReq(long connId, string req)
            {
                //if (rpcManager.IsSRpcLocal())
                //{
                //    //Console.WriteLine($"{GetType().Name}.SRpcReq: {HostId},{req}");
                //    lastServerMsgInfo = new TestMsgInfo(connId, 2000, req);
                //    CRpcResp(connId, $"{req}_R");
                //}
                //if (rpcManager.IsSRpcRemote())
                //    rpcManager.InvokeRPC(localClientId, nameof(SRpcReq), req);
                if(!rpcManager.InvokeSRPC(nameof(SRpcReq), req)) return;

                lastServerMsgInfo = new TestMsgInfo(connId, 2000, req);
                CRpcResp(connId, $"{req}_R");
            }
            [CRpc]
            internal void CRpcResp(long connId, string resp)
            {
                //if (rpcManager.IsCRpcLocal(ECRpcCallType.Target, connId))
                //{
                //    lastClientMsgInfo = new TestMsgInfo(0, 2001, resp);
                //    //Console.WriteLine($"{GetType().Name}.CRpcResp: {HostId},{resp}");
                //}
                if(!rpcManager.InvokeCRPC(ECRpcCallType.Target, connId, nameof(CRpcResp), resp)) return;
                //if (rpcManager.IsCRpcRemote(ECRpcCallType.Target, connId))
                //    rpcManager.InvokeRPC(connId, nameof(CRpcResp), resp);
                lastClientMsgInfo = new TestMsgInfo(0, 2001, resp);
            }
            //[CRpc]
            //internal void CRpcBroadcast(string resp)
            //{
            //    if (rpcManager.IsCRpcLocal(ECRpcCallType.Broadcast))
            //    {
            //        lastClientMsgInfo = new TestMsgInfo(0, 2002, resp);
            //        //Console.WriteLine($"{GetType().Name}.CRpcBroadcast: {HostId},{resp}");
            //    }
            //    if (rpcManager.IsCRpcRemote(ECRpcCallType.Broadcast))
            //        rpcManager.InvokeRPC(null, -1, nameof(CRpcBroadcast), resp);
            //}
            [CRpc]
            internal void CRpcBroadcast(long connId, string resp)
            {
                //if (rpcManager.IsCRpcLocal(ECRpcCallType.Broadcast, connId))
                //{
                //    lastClientMsgInfo = new TestMsgInfo(0, 2002, resp);
                //    //Console.WriteLine($"{GetType().Name}.CRpcBroadcastExc: {HostId},{resp}");
                //}
                if(!rpcManager.InvokeCRPC(ECRpcCallType.Broadcast, connId, nameof(CRpcBroadcast), resp)) return;
                //if (rpcManager.IsCRpcRemote(ECRpcCallType.Broadcast, connId))
                //    rpcManager.InvokeRPC(null, connId, nameof(CRpcBroadcast), resp);
                lastClientMsgInfo = new TestMsgInfo(0, 2002, resp);
            }
            #endregion

        }
    }
}