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
        public void RpcActiveInteractionStart(string uuid)
        {
            RpcActiveInteractionEnd(uuid);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RpcActiveInteractionEnd(string uuid)
        {
            // 상호작용 처리를 다른 클라이언트에게 전송

            var interactionObject = InteractionObjectList[uuid];
            if (interactionObject != null)
            {
                interactionObject.ActionInteractionEndResult();
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
        public INTERACTION_RESULT_TYPE ResultType;
        public Vector3 TeleportPosition;
    }
    /// <summary>
    /// 상호작용 결과 타입
    /// </summary>
    public enum INTERACTION_RESULT_TYPE
    {
        NONE,
        TELEPORT,
        SPRITE_CHANGE,
    }
}
