using System;
using System.Collections.Generic;
using System.Reflection;

namespace Poly.Net
{
    public delegate void NetMessageHandlerDelegate(long connId, NetMessage netMessage);
    public class NetMessageHandlerData
    {
        public object Instance;
        public MethodInfo MethodInfo;
        public long Duration;
        public bool IsOnce;
        public long RequestTime;
    }
    public class NetMessageHandlerAttribute : Attribute
    {
        public ushort MsgId { get; }
        public NetMessageHandlerAttribute(ushort msgId)
        {
            this.MsgId = msgId;
        }
    }
    public interface INetMessageHandlerManager
    {
        void Process(long connId, NetMessage message);
        void Update();
    }
    public class NetMessageProcessor : INetMessageHandlerManager, IDisposable
    {
        private SortedList<ushort, List<NetMessageHandlerData>> messageHandlerDataSL;
        private readonly object[] tempParams = new object[2];

        public NetMessageProcessor()
        {
            // logger = LogUtil.LoggerFactory.CreateLogger(GetType());
            messageHandlerDataSL = new SortedList<ushort, List<NetMessageHandlerData>>();
        }
        public void Dispose()
        {
            UnregisterAllHandlers();
            messageHandlerDataSL = null;
        }
        public void Update()
        {
            //update handlers timeout
            var listCount = messageHandlerDataSL.Count;
            var listValues = messageHandlerDataSL.Values;
            var time = DateTimeOffset.UtcNow.Ticks;//.ToUnixTimeSeconds();
            for (int j = 0; j < listCount; j++)
            {
                var dataList = listValues[j];
                for (int i = 0; i < dataList.Count; i++)
                {
                    var wrapper = dataList[i];
                    if (wrapper.Duration > 0f && time - wrapper.RequestTime > wrapper.Duration)
                    {
                        tempParams[0] = null;
                        tempParams[1] = default(NetMessage);
                        wrapper.MethodInfo.Invoke(wrapper.Instance, tempParams);
                        dataList.RemoveAt(i--);
                    }
                }
            }
        }
        public void Process(long connId, NetMessage message)
        {
            var msgId = message.msgType;
            if (messageHandlerDataSL.TryGetValue(msgId, out var wrapperList))
            {
                for (int i = 0; i < wrapperList.Count; i++)
                {
                    var wrapper = wrapperList[i];
                    tempParams[0] = connId;
                    tempParams[1] = message;
                    wrapper.MethodInfo.Invoke(wrapper.Instance, tempParams);
                    if (wrapper.IsOnce)
                    {
                        wrapperList.RemoveAt(i--);
                    }
                }
            }
        }

        #region message handler
        public void RegisterMessageHandler(object instance)
        {
            var type = instance.GetType();
            var methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < methodInfos.Length; i++)
            {
                var methodInfo = methodInfos[i];
                var attribute = methodInfo.GetCustomAttribute(typeof(NetMessageHandlerAttribute)) as NetMessageHandlerAttribute;
                if (attribute != null)
                    RegisterMessageHandler(attribute.MsgId, instance, methodInfo);
            }
        }
        public void UnregisterMessageHandler(object instance)
        {
            foreach (var list in messageHandlerDataSL.Values)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                    if (list[i].Instance == instance)
                        list.RemoveAt(i);
            }
        }
        internal void RegisterMessageHandler(ushort msgType, object instance, MethodInfo methodInfo, bool isOnce = false, long duration = 0)
        {
            if (!messageHandlerDataSL.TryGetValue(msgType, out var list))
            {
                list = new List<NetMessageHandlerData>();
                messageHandlerDataSL.Add(msgType, list);
            }
            var handlerData = new NetMessageHandlerData
            {
                Instance = instance,
                MethodInfo = methodInfo,
                IsOnce = isOnce,
                Duration = duration,
                RequestTime = DateTimeOffset.UtcNow.Ticks//.ToUnixTimeSeconds()
            };
            list.Add(handlerData);
            // logger.Log(TAG, $"RegisterMessageHandler: {instance.GetType().Name},{methodInfo.Name}");
        }
        public void RegisterMessageHandler(ushort msgType, NetMessageHandlerDelegate handler, bool isOnce = false, long duration = 0)
        {
            RegisterMessageHandler(msgType, handler.Target, handler.Method, isOnce, duration);
        }
        public void UnregisterMessageHandler(ushort msgType, NetMessageHandlerDelegate handler)
        {
            if (!messageHandlerDataSL.TryGetValue(msgType, out var list)) return;
            var wrapper = list.Find((w) => w.Instance == handler.Target && w.MethodInfo == handler.Method);
            if(wrapper != null) list.Remove(wrapper);
        }
        public void UnregisterAllHandlers()
        {
            var listCount = messageHandlerDataSL.Count;
            var listValues = messageHandlerDataSL.Values;
            for (int j = 0; j < listCount; j++)
                listValues[j].Clear();
            messageHandlerDataSL.Clear();
        }
        #endregion
    }
}
