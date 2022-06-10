//using System;
//using System.Threading.Tasks;
//using Poly.Serialization;

//namespace Poly.NetManager
//{
//    public abstract class ANetBehaviour : IDisposable, INetSyncVarOwner
//    {
//        internal readonly int netBehaviourId = -1;
//        protected readonly NetObject netObject;

//        internal NetSyncVarManager syncVarManager;
//        protected bool isDirty;
//        private bool isCreated;

//        public NetObject NetObject => netObject;
//        public NetManager NetManager => netObject.NetManager;
//        public int NetObjectId => netObject == null ? 0 : netObject.NetObjectId;
//        public int NetBehaviourId => netBehaviourId;
//        public bool IsClient => netObject == null ? false : netObject.IsClient;
//        public bool IsServer => netObject == null ? false : netObject.IsServer;
//        public bool IsHost => netObject == null ? false : netObject.IsHost;
//        public bool IsClientOnly => netObject == null ? false : netObject.IsClientOnly;
//        public bool IsServerOnly => netObject == null ? false : netObject.IsServerOnly;
//        public bool IsLocalPlayer => netObject == null ? false : netObject.IsLocalPlayer;
//        public bool IsOwner => netObject == null ? false : netObject.IsOwner;
//        public bool IsSpawned => netObject == null ? false : netObject.IsSpawned;
//        public bool IsDirty => isDirty || syncVarManager == null ? false : syncVarManager.IsDirty;

//        public ANetBehaviour(NetObject netObject, int id)
//        {
//            this.netObject = netObject;
//            netBehaviourId = id;
//            syncVarManager = new NetSyncVarManager(this);
//            syncVarManager.Register(this);
//        }
//        public void Dispose()
//        {
//            syncVarManager.Dispose();
//            syncVarManager = null;
//            //netObject = null;
//            //netBehaviourId = -1;
//        }
//        public virtual void OnNetSpawn()
//        {
//        }
//        public virtual void OnNetDespawn()
//        {
//        }
//        public virtual void OnGainedOwnership()
//        {
//        }
//        public virtual void OnLostOwnership()
//        {
//        }
//        public virtual void OnNetUpdate()
//        {
//        }

//        #region serialize
//        internal bool UpdateDirtyFlag()
//        {
//            if (!IsSpawned)
//                return false;
//            return syncVarManager.UpdateDirtyFlag();
//        }
//        public void Serialize(ref PolyWriter writer, bool isInit)
//        {
//            syncVarManager.SerializeVars(ref writer, isInit);
//            OnSerialize(ref writer, isInit);
//            // isDirty = false;
//            if(isInit)
//                OnSerialized(true);
//        }
//        protected virtual void OnSerialize(ref PolyWriter writer, bool isInit)
//        {
//        }
//        public virtual void OnSerialized(bool isNet)
//        {
//        }
//        public void Deserialize(ref PolyReader reader, bool isInit)
//        {
//            syncVarManager.DeserializeVars(ref reader, isInit);
//            OnDeserialize(ref reader, isInit);
//            if(isInit)
//                OnDeserialized(true);
//        }
//        protected virtual void OnDeserialize(ref PolyReader reader, bool isInit)
//        {
//        }
//        public virtual void OnDeserialized(bool isNet)
//        {
//        }
//        #endregion

//        #region RPC
//        public void InvokeRPC(string rpcId, params object[] ps)
//        {
//            rpcId = $"{netBehaviourId}_{rpcId}";
//            netObject.InvokeRPC(rpcId, ps);
//        }
//        public async Task<T> InvokeRPCAsync<T>(string rpcId, params object[] ps)
//        {
//            rpcId = $"{netBehaviourId}_{rpcId}";
//            return await netObject.InvokeRPCAsync<T>(rpcId, ps);
//        }
//        public void InvokeRPCAll(string rpcId, params object[] ps)
//        {
//            rpcId = $"{netBehaviourId}_{rpcId}";
//            netObject.InvokeRPCAll(rpcId, ps);
//        }
//        public void InvokeRPCOthers(string rpcId, params object[] ps)
//        {
//            rpcId = $"{netBehaviourId}_{rpcId}";
//            netObject.InvokeRPCOthers(rpcId, ps);
//        }
//        #endregion
//    }
//}
