//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Reflection;

//namespace Poly.NetManager
//{
//    public enum ENetSyncVarPermission
//    {
//        OwnerOnly,
//        ServerOnly,
//    }
//    public abstract class ANetSyncBase
//    {
//        public ENetSyncVarPermission permission;

//        public INetSyncVarOwner NetBehaviour { get; set; }

//        public ANetSyncBase(ENetSyncVarPermission permission = ENetSyncVarPermission.OwnerOnly)
//        {
//            this.permission = permission;
//        }

//        protected virtual bool CanWrite()
//        {
//            if (NetBehaviour == null)
//                return true;
//            if (!NetBehaviour.IsSpawned)
//                return true;
//            if (permission == ENetSyncVarPermission.OwnerOnly && !NetBehaviour.IsOwner)
//                return false;
//            if (permission == ENetSyncVarPermission.ServerOnly && !NetBehaviour.IsServer)
//                return false;
//            return true;
//        }
//    }
//}