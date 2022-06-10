using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Poly.Net
{
    public class RPCMethod
    {
        public string RpcId;
        public MethodInfo MethodInfo;
        public object Instance;
        public Type[] ParameterTypes;

        public Type ReturnType;
        public Type TaskType;
        public PropertyInfo TaskResultProperty;
        internal bool IsSRpc;

        public override string ToString()
        {
            return $"RPCMethod:{{{RpcId},{ParameterTypes.Length},{ReturnType},{TaskType},{TaskResultProperty}}}";
        }
    }
    public struct RPCRequest
    {
        //public string RpcId;
        //public ushort ReqId;
        public Type ReturnType;
        public Action<object> Callback;

        //public float TimeoutTime;
    }
    public class SRpcAttribute : Attribute
    { 
    }
    public class CRpcAttribute : Attribute
    {
    }

    public class NetRpcManager : IDisposable
    {
        protected INetBase netBase;
        protected Dictionary<string, RPCMethod> rpcMethodDict;
        protected Dictionary<ushort, RPCRequest> rpcRequestDict;
        protected ushort rpcReqMsgId;

        public Dictionary<string, RPCMethod> RpcMethodDict => rpcMethodDict;

        public NetRpcManager(INetBase messageProcessor)
        {
            this.netBase = messageProcessor;
            //this.NetSettings = netSettings;
            rpcMethodDict = new Dictionary<string, RPCMethod>();
            rpcRequestDict = new Dictionary<ushort, RPCRequest>();
            var messageHandlerManager = messageProcessor.MessageProcessor;
            messageHandlerManager.RegisterMessageHandler(NetMsgTypes.RPCReqMessage, RPCReqHandler);
            messageHandlerManager.RegisterMessageHandler(NetMsgTypes.RPCRespMessage, RPCRespHandler);
        }
        public virtual void Dispose()
        {
            var messageHandlerManager = netBase.MessageProcessor;
            messageHandlerManager.UnregisterMessageHandler(NetMsgTypes.RPCReqMessage, RPCReqHandler);
            messageHandlerManager.UnregisterMessageHandler(NetMsgTypes.RPCRespMessage, RPCRespHandler);
            UnregisterAllRPCs();
            rpcMethodDict = null;
            rpcRequestDict.Clear();
            rpcRequestDict = null;
        }

        #region rpc register
        public void RegisterRPC(object instance, string instanceId = null)
        {
            var type = instance.GetType();
            var methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < methodInfos.Length; i++)
            {
                var methodInfo = methodInfos[i];
                //var methodName = methodInfo.Name;
                if(methodInfo.GetCustomAttribute<SRpcAttribute>() != null
                    || methodInfo.GetCustomAttribute<CRpcAttribute>() != null)
                //if (methodName.StartsWith("SRpc") || methodName.StartsWith("CRpc"))
                    RegisterRPC(instance, methodInfo, instanceId);
            }
        }
        public void UnregisterRPC(object instance)
        {
            var list = rpcMethodDict.Values.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                var rpcMethod = list[i];
                if (rpcMethod.Instance == instance)
                    rpcMethodDict.Remove(rpcMethod.RpcId);
            }
        }
        public bool RegisterRPC(object instance, string methodName, string instanceId = null)
        {
            if (rpcMethodDict.ContainsKey(methodName))
                return false;
            var type = instance.GetType();
            var methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo == null)
            {
                Console.Error.WriteLine($"RegisterRPC: no method {instance}, {type}, {methodName}");
                return false;
            }
            return RegisterRPC(instance, methodInfo, instanceId);
        }
        protected bool RegisterRPC(object instance, MethodInfo methodInfo, string instanceId = null)
        {
            var rpcId = methodInfo.Name;
            if(instanceId != null)
                rpcId = $"{instanceId}_{rpcId}";
            if (rpcMethodDict.ContainsKey(rpcId))
                return false;
            var parameterInfos = methodInfo.GetParameters();
            var parameterNum = parameterInfos.Length;
            var parameterTypes = new Type[parameterNum];
            for (int i = 0; i < parameterNum; i++)
            {
                var parameterInfo = parameterInfos[i];
                parameterTypes[i] = parameterInfo.ParameterType;
            }
            var returnType = methodInfo.ReturnType;

            PropertyInfo taskResultProperty = null;
            Type taskType = null;
            if (typeof(Task).IsAssignableFrom(returnType))
            {
                taskType = returnType;
                if (!returnType.IsGenericType)
                {
                    Console.Error.WriteLine($"RegisterRPC: ReturnType should be Task<T>");
                    return false;
                }
                var genericTypes = taskType.GetGenericArguments();
                taskResultProperty = taskType.GetProperty("Result");
                returnType = genericTypes[0];
            }
            var rpcMethod = new RPCMethod();
            rpcMethod.RpcId = rpcId;
            rpcMethod.Instance = instance;
            rpcMethod.MethodInfo = methodInfo;
            rpcMethod.ParameterTypes = parameterTypes;
            rpcMethod.ReturnType = returnType;
            rpcMethod.TaskType = taskType;
            rpcMethod.TaskResultProperty = taskResultProperty;
            rpcMethod.IsSRpc = methodInfo.GetCustomAttribute<SRpcAttribute>() != null;

            //logger.LogTrace($"RegisterRPC: {rpcMethod}");
            rpcMethodDict.Add(rpcId, rpcMethod);
            return true;
        }
        public void UnregisterAllRPCs()
        {
            rpcMethodDict.Clear();
        }
        #endregion

        #region rpc handler
        private void RPCReqHandler(long connId, NetMessage netMessage)
        {
            var reader = netMessage.BeginRead();
            var rpcId = reader.ReadString();
            var reqId = reader.ReadUShort();
            var parameterNum = reader.ReadByte();
            //logger.Log(TAG, $"RPCReqHandler: {connId},{rpcId}, {reqId}");
            if (!rpcMethodDict.TryGetValue(rpcId, out var rpcMethod))
            {
                Console.Error.WriteLine($"RPCReqHandler: RPC[{rpcId}] doesn't exist!");
                return;
            }
            if (rpcMethod.IsSRpc != netBase.IsServer)
            {
                //Console.WriteLine($"RPCReqHandler: RPC[{rpcId}]: {rpcMethod.IsSRpc} != {messageProcessor.IsServer}");
                return;
            }
            var parameterTypes = rpcMethod.ParameterTypes;
            parameterNum = (byte)parameterTypes.Length;
            object[] parameters = null;
            if (parameterNum > 0)
            {
                parameters = new object[parameterNum];
                for (int i = 0; i < parameterNum; i++)
                {
                    var type = parameterTypes[i];
                    if (type == typeof(long))
                        parameters[i] = connId;
                    else
                        parameters[i] = reader.ReadObject(type);
                }
            }
            var returnObj = rpcMethod.MethodInfo.Invoke(rpcMethod.Instance, parameters);
            var returnType = rpcMethod.ReturnType;
            // Debug.Log($"RPCReqHandler: {rpcId}, {reqId}, {returnObj}, {rpcMethod.ReturnType}");
            if (returnType != typeof(void))
                //SendRPCRespMsg(connection.ConnectionId, rpcMethod, reqId, returnObj);
                SendRPCRespMsg(connId, rpcId, reqId, returnObj, rpcMethod.TaskResultProperty);
        }
        private void RPCRespHandler(long connId, NetMessage netMessage)
        {
            var reader = netMessage.BeginRead();
            var rpcId = reader.ReadString();
            var reqId = reader.ReadUShort();

            if (!rpcRequestDict.TryGetValue(reqId, out var rpcRequest))
            {
                Console.Error.WriteLine($"RPCRespHandler: RPC[{rpcId}, {reqId}] resp handler dosnt exist!");
                return;
            }
            rpcRequestDict.Remove(reqId);
            var returnObj = reader.ReadObject(rpcRequest.ReturnType);
            rpcRequest.Callback(returnObj);
        }
        private async void SendRPCRespMsg(long connectionId, string rpcId, ushort reqId, object returnObj, PropertyInfo resultProperty)
        {
            if (returnObj is Task task)
            {
                await task;
                returnObj = resultProperty.GetValue(returnObj);
            }
            var netMsg = netBase.CreateMessage(NetMsgTypes.RPCRespMessage);
            var writer = netMsg.BeginWrite();
            writer.WriteString(rpcId);
            writer.WriteUShort(reqId);
            writer.WriteObject(returnObj);
            netMsg.EndWrite(writer);
            netBase.SendMessage(connectionId, netMsg);
            netMsg.Dispose();
        }
        #endregion

        #region rpc invoke
        private NetMessage GetRpcMessage(string rpcId, out ushort reqId, params object[] ps)
        {
            var netMsg = netBase.CreateMessage(NetMsgTypes.RPCReqMessage);
            var writer = netMsg.BeginWrite();
            writer.WriteString(rpcId);
            //generate request id
            reqId = rpcReqMsgId++;
            //if (rpcReqMsgId == ushort.MaxValue)
            //    rpcReqMsgId = 0;
            writer.WriteUShort(reqId);

            var parameterNum = ps.Length;
            writer.WriteByte((byte)parameterNum);

            for (int i = 0; i < parameterNum; i++)
                writer.WriteObject(ps[i]);
            netMsg.EndWrite(writer);
            return netMsg;
        }
        public void InvokeRPC(IList<long> connIds, long excludeId, string rpcId, params object[] ps)
        {
            //logger.LogTrace($"InvokeRPC: {rpcId},{connIds.ToListString()},{excludeId}");
            var netMsg = GetRpcMessage(rpcId, out var reqId, ps);
            netBase.SendMessage(connIds, netMsg, excludeId);
            netMsg.Dispose();
        }
        public void InvokeRPC(long connId, string rpcId, params object[] ps)
        {
            var netMsg = GetRpcMessage(rpcId, out var reqId, ps);
            netBase.SendMessage(connId, netMsg);
            netMsg.Dispose();
        }
        public void InvokeRPC<T>(long connId, string rpcId, Action<T> callback, params object[] ps)
        {
            var netMsg = GetRpcMessage(rpcId, out var reqId, ps);
            //add rpc request
            var rpcRequest = new RPCRequest
            {
                //RpcId = rpcId,
                //ReqId = reqId,
                ReturnType = typeof(T),
                Callback = (obj) => callback((T)obj)
            };
            rpcRequestDict.Add(reqId, rpcRequest);
            netBase.SendMessage(connId, netMsg);
            netMsg.Dispose();
        }
        #endregion

        #region rpc check
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public bool IsSRpcLocal(long connId = -1)
        //{
        //    return netBase.IsServer;
        //}
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public bool IsSRpcRemote(long connId = -1)
        //{
        //    return !netBase.IsServer;
        //}
        public bool InvokeSRPC(string rpcId, params object[] ps)
        {
            if (netBase.IsServer) return true;
            InvokeRPC(netBase.LocalClientId, rpcId, ps);
            return false;
        }
        private bool IsCRpcLocal(ECRpcCallType type, long connId = -1)
        {
            if (type == ECRpcCallType.Target)
                return netBase.IsClient && connId == netBase.LocalClientId;
            else if (type == ECRpcCallType.Broadcast)
            {
                if(connId == -1)
                    return netBase.IsClient;
                else
                    return netBase.IsClientOnly() || (netBase.IsHost() && connId != netBase.LocalClientId);
            }
            return false;
        }
        private bool IsCRpcRemote(ECRpcCallType type, long connId)
        {
            if (type == ECRpcCallType.Target)
                return netBase.IsServer && connId != netBase.LocalClientId;
            else if (type == ECRpcCallType.Broadcast)
                return netBase.IsServer;
            return false;
        }
        public bool InvokeCRPC(ECRpcCallType type, long connId, string rpcId, params object[] ps)
        {
            if (IsCRpcRemote(type, connId))
            {
                if (type == ECRpcCallType.Target)
                    InvokeRPC(connId, rpcId, ps);
                else if (type == ECRpcCallType.Broadcast)
                    InvokeRPC(null, connId, rpcId, ps);
            }
            return IsCRpcLocal(type, connId);
        }
        #endregion
    }
    public enum ECRpcCallType
    {
        Target,
        Broadcast,
    }
}
