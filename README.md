# Poly.Net
a lightweight Net framework for any C# (or .Net) project.

## Features
- Zero third-part dependencies (need Poly.Serialization and Poly.Tcp)
- Minimal core
- Lightweight and fast
- Support Transport implements ITransport
- Support MessageHandler and MessageProcessor
- Support Rpc and async Rpc callback
- Adapted to all C# game engine

## Installation

## Overview

```csharp

public class TestNet1 : ANetBase
{
    public TestNet1(int hostId, NetSettings netSettings, ITransport transport) : base(hostId, netSettings, transport)
    {
    }
    internal void SendC2SMessage(string info)
    {
        if (!IsClient) return;
        var nm = CreateMessage(1001);
        var writer = nm.BeginWrite();
        writer.WriteString(info);
        nm.EndWrite(writer);
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
    }
    [SRpc]
    internal void SRpcReq(long connId, string req)
    {
        if(!rpcManager.InvokeSRPC(nameof(SRpcReq), req)) return;

        lastServerMsgInfo = new TestMsgInfo(connId, 2000, req);
        CRpcResp(connId, $"{req}_R");
    }
    [CRpc]
    internal void CRpcResp(long connId, string resp)
    {
        if(!rpcManager.InvokeCRPC(ECRpcCallType.Target, connId, nameof(CRpcResp), resp)) return;

        lastClientMsgInfo = new TestMsgInfo(0, 2001, resp);
    }
}

...

//init host
var netSettings = NetSettings.DefaultSettings;
var hostTransport = new PolyTcpTransport();
var host = new TestNet1(-1, netSettings, hostTransport);
host.Address = address;
host.Port = port;

//start host
host.StartHost();

//init timer
var timer = new Timer((state) =>
{
    host?.Update();
}, null, 0, 50);

host.SendC2SMessage($"C2SMessage");
host.SRpcReq(0, "SRpcReq");

//stop host
host.StopHost();

//dispose timer
timer.Dispose();

//dispose host
host.Dispose();
host = null;

```

## License
The software is released under the terms of the [MIT license](./LICENSE.md).

## FAQ

## References

### Documents

### Projects
- [vis2k/Mirror](https://github.com/vis2k/Mirror)
- [insthync/LiteNetLibManager](https://github.com/insthync/LiteNetLibManager)
- [Unity-Technologies/com.unity.netcode.gameobjects](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects)

### Benchmarks
