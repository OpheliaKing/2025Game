using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Shin
{
    public partial class InGamePlayerInfo
    {
        private Dictionary<string, InteractionObject> _mapObjectData = new Dictionary<string, InteractionObject>();

        private Dictionary<string, InteractionObject> InteractionObjectList { get; set; } = new Dictionary<string, InteractionObject>();

        /// <summary>
        /// 테스트용 => 
        /// </summary>
        public void MapDataInit(StageInfo stageInfo)
        {
            if (stageInfo == null)
            {
                Debug.LogError("StageInfo is null");
                return;
            }
            
            var index = 0;

            foreach (var interactiveObject in stageInfo.InteractiveObjectList)
            {
                _mapObjectData.Add(interactiveObject.UUID, interactiveObject);
                InteractionObjectList.Add(interactiveObject.UUID, interactiveObject);
                index++;
            }
        }

        //호스트가 상호작용 정보를 받고 처리함
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RpcActiveInteractionStart(string uuid,string masterPlayerId)
        {
            RpcActiveInteractionEnd(uuid,masterPlayerId);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RpcActiveInteractionEnd(string uuid,string masterPlayerId)
        {
            // 상호작용 처리를 다른 클라이언트에게 전송

            var interactionObject = InteractionObjectList[uuid];
            if (interactionObject != null)
            {
                interactionObject.ActionInteractionEndResult(masterPlayerId);
            }
        }
    }

    [Serializable]
    public struct InteractionData
    {
        [SerializeField]
        private string _uuid;
        public string UUID
        {
            get { return _uuid; }
        }

        [SerializeField]
        private INTERACTION_RESULT_TYPE _RESULT_TYPE;
        public INTERACTION_RESULT_TYPE ResultType
        {
            get { return _RESULT_TYPE; }
        }

        [SerializeField]
        private Vector3 _teleportPosition;
        public Vector3 TeleportPosition
        {
            get { return _teleportPosition; }
        }

        [SerializeField]
        private string _itemId;
        public string ItemId
        {
            get { return _itemId; }
        }
    }
    /// <summary>
    /// 상호작용 결과 타입
    /// </summary>
    public enum INTERACTION_RESULT_TYPE
    {
        NONE,
        TELEPORT,
        SPRITE_CHANGE,
        ITEM_GET,
        ITEM_USE,
    }
}
