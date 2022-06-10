using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Poly.Net
{
    public delegate void NetActionDelegate();
    public delegate void NetActionDelegate<T1>(T1 param1);
    public delegate void NetActionDelegate<T1, T2>(T1 param1, T2 param2);
    public delegate void NetActionDelegate<T1, T2, T3>(T1 param1, T2 param2, T3 param3);
    public delegate void NetActionDelegate<T1, T2, T3, T4>(T1 param1, T2 param2, T3 param3, T4 param4);
    public delegate void NetActionDelegate<T1, T2, T3, T4, T5>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5);
    //public delegate void NetActionDelegate<T1, T2, T3, T4, T5, T6>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6);
    //public delegate void NetActionDelegate<T1, T2, T3, T4, T5, T6, T7>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7);
    //public delegate void NetActionDelegate<T1, T2, T3, T4, T5, T6, T7, T8>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8);
    //public delegate void NetActionDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9);
    //public delegate void NetActionDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, T10 param10);
    public delegate void NetActionDelegate1(long connId);
    public delegate void NetActionDelegate1<T1>(long connId, T1 param1);
    public delegate void NetActionDelegate1<T1, T2>(long connId, T1 param1, T2 param2);
    public delegate void NetActionDelegate1<T1, T2, T3>(long connId, T1 param1, T2 param2, T3 param3);
    public delegate void NetActionDelegate1<T1, T2, T3, T4>(long connId, T1 param1, T2 param2, T3 param3, T4 param4);
    public delegate void NetActionDelegate1<T1, T2, T3, T4, T5>(long connId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5);

    public delegate TResult NetFuncDelegate<TResult>();
    public delegate TResult NetFuncDelegate<T1, TResult>(T1 param1);
    public delegate TResult NetFuncDelegate<T1, T2, TResult>(T1 param1, T2 param2);
    public delegate TResult NetFuncDelegate<T1, T2, T3, TResult>(T1 param1, T2 param2, T3 param3);
    public delegate TResult NetFuncDelegate<T1, T2, T3, T4, TResult>(T1 param1, T2 param2, T3 param3, T4 param4);
    public delegate TResult NetFuncDelegate<T1, T2, T3, T4, T5, TResult>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5);

    public delegate TResult NetFuncDelegate1<TResult>(long connId);
    public delegate TResult NetFuncDelegate1<T1, TResult>(long connId, T1 param1);
    public delegate TResult NetFuncDelegate1<T1, T2, TResult>(long connId, T1 param1, T2 param2);
    public delegate TResult NetFuncDelegate1<T1, T2, T3, TResult>(long connId, T1 param1, T2 param2, T3 param3);
    public delegate TResult NetFuncDelegate1<T1, T2, T3, T4, TResult>(long connId, T1 param1, T2 param2, T3 param3, T4 param4);
    public delegate TResult NetFuncDelegate1<T1, T2, T3, T4, T5, TResult>(long connId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5);

    public delegate Task<TResult> NetTaskFuncDelegate<TResult>();
    public delegate Task<TResult> NetTaskFuncDelegate<T1, TResult>(T1 param1);
    public delegate Task<TResult> NetTaskFuncDelegate<T1, T2, TResult>(T1 param1, T2 param2);
    public delegate Task<TResult> NetTaskFuncDelegate<T1, T2, T3, TResult>(T1 param1, T2 param2, T3 param3);
    public delegate Task<TResult> NetTaskFuncDelegate<T1, T2, T3, T4, TResult>(T1 param1, T2 param2, T3 param3, T4 param4);
    public delegate Task<TResult> NetTaskFuncDelegate<T1, T2, T3, T4, T5, TResult>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5);

    public static class NetRpcManagerExtensions
    {
        // #region register action RPC
        // public static void RegisterRPC(this NetRpcManager netBase, object instance, NetActionDelegate func)
        // {
        //     netBase.RegisterRPC(instance, func.Method.Name);
        // }
        // public static void RegisterRPC<T1>(this NetRpcManager netBase, object instance, NetActionDelegate<T1> func)
        // {
        //     netBase.RegisterRPC(instance, func.Method.Name);
        // }
        // public static void RegisterRPC<T1, T2>(this NetRpcManager netBase, object instance, NetActionDelegate<T1, T2> func)
        // {
        //     netBase.RegisterRPC(instance, func.Method.Name);
        // }
        // public static void RegisterRPC<T1, T2, T3>(this NetRpcManager netBase, object instance, NetActionDelegate<T1, T2, T3> func)
        // {
        //     netBase.RegisterRPC(instance, func.Method.Name);
        // }
        // public static void RegisterRPC<T1, T2, T3, T4>(this NetRpcManager netBase, object instance, NetActionDelegate<T1, T2, T3, T4> func)
        // {
        //     netBase.RegisterRPC(instance, func.Method.Name);
        // }
        // public static void RegisterRPC<T1, T2, T3, T4, T5>(this NetRpcManager netBase, object instance, NetActionDelegate<T1, T2, T3, T4, T5> func)
        // {
        //     netBase.RegisterRPC(instance, func.Method.Name);
        // }

        // public static void RegisterRPC(this NetRpcManager netBase, object instance, NetActionDelegate1 func)
        // {
        //     netBase.RegisterRPC(instance, func.Method.Name);
        // }
        // public static void RegisterRPC<T1>(this NetRpcManager netBase, object instance, NetActionDelegate1<T1> func)
        // {
        //     netBase.RegisterRPC(instance, func.Method.Name);
        // }
        // public static void RegisterRPC<T1, T2>(this NetRpcManager netBase, object instance, NetActionDelegate1<T1, T2> func)
        // {
        //     netBase.RegisterRPC(instance, func.Method.Name);
        // }
        // public static void RegisterRPC<T1, T2, T3>(this NetRpcManager netBase, object instance, NetActionDelegate1<T1, T2, T3> func)
        // {
        //     netBase.RegisterRPC(instance, func.Method.Name);
        // }
        // public static void RegisterRPC<T1, T2, T3, T4>(this NetRpcManager netBase, object instance, NetActionDelegate1<T1, T2, T3, T4> func)
        // {
        //     netBase.RegisterRPC(instance, func.Method.Name);
        // }
        // public static void RegisterRPC<T1, T2, T3, T4, T5>(this NetRpcManager netBase, object instance, NetActionDelegate1<T1, T2, T3, T4, T5> func)
        // {
        //     netBase.RegisterRPC(instance, func.Method.Name);
        // }
        // #endregion

        #region invoke action RPC
        public static void InvokeRPC(this NetRpcManager netBase, long connectionId, NetActionDelegate func)
        {
            netBase.InvokeRPC(connectionId, func.Method.Name);
        }
        public static void InvokeRPC<T1>(this NetRpcManager netBase, long connectionId, NetActionDelegate<T1> func, T1 param1)
        {
            netBase.InvokeRPC(connectionId, func.Method.Name, param1);
        }
        public static void InvokeRPC<T1, T2>(this NetRpcManager netBase, long connectionId, NetActionDelegate<T1, T2> func, T1 param1, T2 param2)
        {
            netBase.InvokeRPC(connectionId, func.Method.Name, param1, param2);
        }
        public static void InvokeRPC<T1, T2, T3>(this NetRpcManager netBase, long connectionId, NetActionDelegate<T1, T2, T3> func, T1 param1, T2 param2, T3 param3)
        {
            netBase.InvokeRPC(connectionId, func.Method.Name, param1, param2, param3);
        }
        public static void InvokeRPC<T1, T2, T3, T4>(this NetRpcManager netBase, long connectionId, NetActionDelegate<T1, T2, T3, T4> func, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            netBase.InvokeRPC(connectionId, func.Method.Name, param1, param2, param3, param4);
        }
        public static void InvokeRPC<T1, T2, T3, T4, T5>(this NetRpcManager netBase, long connectionId, NetActionDelegate<T1, T2, T3, T4, T5> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            netBase.InvokeRPC(connectionId, func.Method.Name, param1, param2, param3, param4, param5);
        }

        public static void InvokeRPC(this NetRpcManager netBase, long connectionId, NetActionDelegate1 func)
        {
            netBase.InvokeRPC(connectionId, func.Method.Name);
        }
        public static void InvokeRPC<T1>(this NetRpcManager netBase, long connectionId, NetActionDelegate1<T1> func, T1 param1)
        {
            netBase.InvokeRPC(connectionId, func.Method.Name, param1);
        }
        public static void InvokeRPC<T1, T2>(this NetRpcManager netBase, long connectionId, NetActionDelegate1<T1, T2> func, T1 param1, T2 param2)
        {
            netBase.InvokeRPC(connectionId, func.Method.Name, param1, param2);
        }
        public static void InvokeRPC<T1, T2, T3>(this NetRpcManager netBase, long connectionId, NetActionDelegate1<T1, T2, T3> func, T1 param1, T2 param2, T3 param3)
        {
            netBase.InvokeRPC(connectionId, func.Method.Name, param1, param2, param3);
        }
        public static void InvokeRPC<T1, T2, T3, T4>(this NetRpcManager netBase, long connectionId, NetActionDelegate1<T1, T2, T3, T4> func, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            netBase.InvokeRPC(connectionId, func.Method.Name, param1, param2, param3, param4);
        }
        public static void InvokeRPC<T1, T2, T3, T4, T5>(this NetRpcManager netBase, long connectionId, NetActionDelegate1<T1, T2, T3, T4, T5> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            netBase.InvokeRPC(connectionId, func.Method.Name, param1, param2, param3, param4, param5);
        }

        public static void InvokeRPC(this NetRpcManager netBase, IList<long> connections, long excludeId, NetActionDelegate func)
        {
            netBase.InvokeRPC(connections, excludeId, func.Method.Name);
        }
        public static void InvokeRPC<T1>(this NetRpcManager netBase, IList<long> connections, long excludeId, NetActionDelegate<T1> func, T1 param1)
        {
            netBase.InvokeRPC(connections, excludeId, func.Method.Name, param1);
        }
        public static void InvokeRPC<T1, T2>(this NetRpcManager netBase, IList<long> connections, long excludeId, NetActionDelegate<T1, T2> func, T1 param1, T2 param2)
        {
            netBase.InvokeRPC(connections, excludeId, func.Method.Name, param1, param2);
        }
        public static void InvokeRPC<T1, T2, T3>(this NetRpcManager netBase, IList<long> connections, long excludeId, NetActionDelegate<T1, T2, T3> func, T1 param1, T2 param2, T3 param3)
        {
            netBase.InvokeRPC(connections, excludeId, func.Method.Name, param1, param2, param3);
        }
        public static void InvokeRPC<T1, T2, T3, T4>(this NetRpcManager netBase, IList<long> connections, long excludeId, NetActionDelegate<T1, T2, T3, T4> func, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            netBase.InvokeRPC(connections, excludeId, func.Method.Name, param1, param2, param3, param4);
        }
        public static void InvokeRPC<T1, T2, T3, T4, T5>(this NetRpcManager netBase, IList<long> connections, long excludeId, NetActionDelegate<T1, T2, T3, T4, T5> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            netBase.InvokeRPC(connections, excludeId, func.Method.Name, param1, param2, param3, param4, param5);
        }

        public static void InvokeRPC(this NetRpcManager netBase, IList<long> connections, long excludeId, NetActionDelegate1 func)
        {
            netBase.InvokeRPC(connections, excludeId, func.Method.Name);
        }
        public static void InvokeRPC<T1>(this NetRpcManager netBase, IList<long> connections, long excludeId, NetActionDelegate1<T1> func, T1 param1)
        {
            netBase.InvokeRPC(connections, excludeId, func.Method.Name, param1);
        }
        public static void InvokeRPC<T1, T2>(this NetRpcManager netBase, IList<long> connections, long excludeId, NetActionDelegate1<T1, T2> func, T1 param1, T2 param2)
        {
            netBase.InvokeRPC(connections, excludeId, func.Method.Name, param1, param2);
        }
        public static void InvokeRPC<T1, T2, T3>(this NetRpcManager netBase, IList<long> connections, long excludeId, NetActionDelegate1<T1, T2, T3> func, T1 param1, T2 param2, T3 param3)
        {
            netBase.InvokeRPC(connections, excludeId, func.Method.Name, param1, param2, param3);
        }
        public static void InvokeRPC<T1, T2, T3, T4>(this NetRpcManager netBase, IList<long> connections, long excludeId, NetActionDelegate1<T1, T2, T3, T4> func, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            netBase.InvokeRPC(connections, excludeId, func.Method.Name, param1, param2, param3, param4);
        }
        public static void InvokeRPC<T1, T2, T3, T4, T5>(this NetRpcManager netBase, IList<long> connections, long excludeId, NetActionDelegate1<T1, T2, T3, T4, T5> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            netBase.InvokeRPC(connections, excludeId, func.Method.Name, param1, param2, param3, param4, param5);
        }
        #endregion

        #region register func RPC
        public static void RegisterRPC<TResult>(this NetRpcManager netBase, object instance, NetFuncDelegate<TResult> func)
        {
            netBase.RegisterRPC(instance, func.Method.Name);
        }
        public static void RegisterRPC<T1, TResult>(this NetRpcManager netBase, object instance, NetFuncDelegate<T1, TResult> func)
        {
            netBase.RegisterRPC(instance, func.Method.Name);
        }
        public static void RegisterRPC<T1, T2, TResult>(this NetRpcManager netBase, object instance, NetFuncDelegate<T1, T2, TResult> func)
        {
            netBase.RegisterRPC(instance, func.Method.Name);
        }
        public static void RegisterRPC<T1, T2, T3, TResult>(this NetRpcManager netBase, object instance, NetFuncDelegate<T1, T2, T3, TResult> func)
        {
            netBase.RegisterRPC(instance, func.Method.Name);
        }
        public static void RegisterRPC<T1, T2, T3, T4, TResult>(this NetRpcManager netBase, object instance, NetFuncDelegate<T1, T2, T3, T4, TResult> func)
        {
            netBase.RegisterRPC(instance, func.Method.Name);
        }
        public static void RegisterRPC<T1, T2, T3, T4, T5, TResult>(this NetRpcManager netBase, object instance, NetFuncDelegate<T1, T2, T3, T4, T5, TResult> func)
        {
            netBase.RegisterRPC(instance, func.Method.Name);
        }
        #endregion

        #region invoke func RPC
        public static void InvokeRPC<TResult>(this NetRpcManager netBase, long connectionId, NetFuncDelegate<TResult> func, Action<TResult> callback)
        {
            netBase.InvokeRPC<TResult>(connectionId, func.Method.Name, callback);
        }
        public static void InvokeRPC<T1, TResult>(this NetRpcManager netBase, long connectionId, NetFuncDelegate<T1, TResult> func, Action<TResult> callback, T1 param1)
        {
            netBase.InvokeRPC<TResult>(connectionId, func.Method.Name, callback, param1);
        }
        public static void InvokeRPC<T1, T2, TResult>(this NetRpcManager netBase, long connectionId, NetFuncDelegate<T1, T2, TResult> func, Action<TResult> callback, T1 param1, T2 param2)
        {
            netBase.InvokeRPC<TResult>(connectionId, func.Method.Name, callback, param1, param2);
        }
        public static void InvokeRPC<T1, T2, T3, TResult>(this NetRpcManager netBase, long connectionId, NetFuncDelegate<T1, T2, T3, TResult> func, Action<TResult> callback, T1 param1, T2 param2, T3 param3)
        {
            netBase.InvokeRPC<TResult>(connectionId, func.Method.Name, callback, param1, param2, param3);
        }
        public static void InvokeRPC<T1, T2, T3, T4, TResult>(this NetRpcManager netBase, long connectionId, NetFuncDelegate<T1, T2, T3, T4> func, Action<TResult> callback, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            netBase.InvokeRPC<TResult>(connectionId, func.Method.Name, callback, param1, param2, param3, param4);
        }
        public static void InvokeRPC<T1, T2, T3, T4, T5, TResult>(this NetRpcManager netBase, long connectionId, NetFuncDelegate<T1, T2, T3, T4> func, Action<TResult> callback, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            netBase.InvokeRPC<TResult>(connectionId, func.Method.Name, callback, param1, param2, param3, param4, param5);
        }

        public static void InvokeRPC<TResult>(this NetRpcManager netBase, long connectionId, NetFuncDelegate1<TResult> func, Action<TResult> callback)
        {
            netBase.InvokeRPC<TResult>(connectionId, func.Method.Name, callback);
        }
        public static void InvokeRPC<T1, TResult>(this NetRpcManager netBase, long connectionId, NetFuncDelegate1<T1, TResult> func, Action<TResult> callback, T1 param1)
        {
            netBase.InvokeRPC<TResult>(connectionId, func.Method.Name, callback, param1);
        }
        public static void InvokeRPC<T1, T2, TResult>(this NetRpcManager netBase, long connectionId, NetFuncDelegate1<T1, T2, TResult> func, Action<TResult> callback, T1 param1, T2 param2)
        {
            netBase.InvokeRPC<TResult>(connectionId, func.Method.Name, callback, param1, param2);
        }
        public static void InvokeRPC<T1, T2, T3, TResult>(this NetRpcManager netBase, long connectionId, NetFuncDelegate1<T1, T2, T3, TResult> func, Action<TResult> callback, T1 param1, T2 param2, T3 param3)
        {
            netBase.InvokeRPC<TResult>(connectionId, func.Method.Name, callback, param1, param2, param3);
        }
        public static void InvokeRPC<T1, T2, T3, T4, TResult>(this NetRpcManager netBase, long connectionId, NetFuncDelegate1<T1, T2, T3, T4> func, Action<TResult> callback, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            netBase.InvokeRPC<TResult>(connectionId, func.Method.Name, callback, param1, param2, param3, param4);
        }
        public static void InvokeRPC<T1, T2, T3, T4, T5, TResult>(this NetRpcManager netBase, long connectionId, NetFuncDelegate1<T1, T2, T3, T4> func, Action<TResult> callback, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            netBase.InvokeRPC<TResult>(connectionId, func.Method.Name, callback, param1, param2, param3, param4, param5);
        }
        #endregion

        #region invoke action RPC async
        public static async Task<T> InvokeRPCAsync<T>(this NetRpcManager netBase, long connection, string rpcId, params object[] ps)
        {
            var task = new TaskCompletionSource<T>();
            netBase.InvokeRPC<T>(connection, rpcId, (result) =>
            {
                task.SetResult(result);
            }, ps);
            return await task.Task;
        }
        public static async Task<TResult> InvokeRPCAsync<TResult>(this NetRpcManager netBase, long connection, NetFuncDelegate<Task<TResult>> func)
        {
            return await netBase.InvokeRPCAsync<TResult>(connection, func.Method.Name);
        }
        public static async Task<TResult> InvokeRPCAsync<T1, TResult>(this NetRpcManager netBase, long connection, NetFuncDelegate<T1, Task<TResult>> func, T1 param1)
        {
            return await netBase.InvokeRPCAsync<TResult>(connection, func.Method.Name, param1);
        }
        public static async Task<TResult> InvokeRPCAsync<T1, T2, TResult>(this NetRpcManager netBase, long connection, NetFuncDelegate<T1, T2, Task<TResult>> func, T1 param1, T2 param2)
        {
            return await netBase.InvokeRPCAsync<TResult>(connection, func.Method.Name, param1, param2);
        }
        public static async Task<TResult> InvokeRPCAsync<T1, T2, T3, TResult>(this NetRpcManager netBase, long connection, NetFuncDelegate<T1, T2, T3, Task<TResult>> func, T1 param1, T2 param2, T3 param3)
        {
            return await netBase.InvokeRPCAsync<TResult>(connection, func.Method.Name, param1, param2, param3);
        }
        public static async Task<TResult> InvokeRPCAsync<T1, T2, T3, T4, TResult>(this NetRpcManager netBase, long connection, NetFuncDelegate<T1, T2, T3, T4, Task<TResult>> func, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            return await netBase.InvokeRPCAsync<TResult>(connection, func.Method.Name, param1, param2, param3, param4);
        }
        public static async Task<TResult> InvokeRPCAsync<T1, T2, T3, T4, T5, TResult>(this NetRpcManager netBase, long connection, NetFuncDelegate<T1, T2, T3, T4, T5, Task<TResult>> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            return await netBase.InvokeRPCAsync<TResult>(connection, func.Method.Name, param1, param2, param3, param4, param5);
        }

        public static async Task<TResult> InvokeRPCAsync<TResult>(this NetRpcManager netBase, long connection, NetFuncDelegate1<Task<TResult>> func)
        {
            return await netBase.InvokeRPCAsync<TResult>(connection, func.Method.Name);
        }
        public static async Task<TResult> InvokeRPCAsync<T1, TResult>(this NetRpcManager netBase, long connection, NetFuncDelegate1<T1, Task<TResult>> func, T1 param1)
        {
            return await netBase.InvokeRPCAsync<TResult>(connection, func.Method.Name, param1);
        }
        public static async Task<TResult> InvokeRPCAsync<T1, T2, TResult>(this NetRpcManager netBase, long connection, NetFuncDelegate1<T1, T2, Task<TResult>> func, T1 param1, T2 param2)
        {
            return await netBase.InvokeRPCAsync<TResult>(connection, func.Method.Name, param1, param2);
        }
        public static async Task<TResult> InvokeRPCAsync<T1, T2, T3, TResult>(this NetRpcManager netBase, long connection, NetFuncDelegate1<T1, T2, T3, Task<TResult>> func, T1 param1, T2 param2, T3 param3)
        {
            return await netBase.InvokeRPCAsync<TResult>(connection, func.Method.Name, param1, param2, param3);
        }
        public static async Task<TResult> InvokeRPCAsync<T1, T2, T3, T4, TResult>(this NetRpcManager netBase, long connection, NetFuncDelegate1<T1, T2, T3, T4, Task<TResult>> func, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            return await netBase.InvokeRPCAsync<TResult>(connection, func.Method.Name, param1, param2, param3, param4);
        }
        public static async Task<TResult> InvokeRPCAsync<T1, T2, T3, T4, T5, TResult>(this NetRpcManager netBase, long connection, NetFuncDelegate1<T1, T2, T3, T4, T5, Task<TResult>> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            return await netBase.InvokeRPCAsync<TResult>(connection, func.Method.Name, param1, param2, param3, param4, param5);
        }
        #endregion

        #region register task func RPC
        public static void RegisterRPC<TResult>(this NetRpcManager netBase, object instance, NetTaskFuncDelegate<TResult> func)
        {
            netBase.RegisterRPC(instance, func.Method.Name);
        }
        #endregion
    }
}
