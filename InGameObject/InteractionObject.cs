using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Shin
{
    /// <summary>
    /// 플레이어와 상호작용 가능한 오브젝트
    /// </summary>
    /// 
    public class InteractionObject : MonoBehaviour
    {
        [SerializeField]
        private InteractionData _interactionData;

        [SerializeField]
        private SpriteRenderer sprite;

        public string UUID => _interactionData.UUID;

        /// <summary>
        /// 플레이어에 의해 상호작용을 했을때
        /// </summary>
        public void ActiveInteraction()
        {
            var masterPlayerId = InGameManager.Instance.PlayerInfo.PlayerUnit.MasterPlayerId;
            InGameManager.Instance.PlayerInfo.RpcActiveInteractionStart(_interactionData.UUID,masterPlayerId);
        }

        public void ActionInteractionEndResult(string masterPlayerId)
        {
            switch (_interactionData.ResultType)
            {
                case INTERACTION_RESULT_TYPE.TELEPORT:
                    var player = InGameManager.Instance.PlayerInfo.CharacterUnitList[masterPlayerId];
                    player.PlayerTeleport(_interactionData.TeleportPosition);
                    break;
                case INTERACTION_RESULT_TYPE.SPRITE_CHANGE:
                    sprite.color = Color.blue;
                    break;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                var player = other.GetComponent<CharacterUnit>();
                if (player != null)
                {
                    player.SetInteractionObject(this);
                    ActiveInteractionState(true);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                var player = other.GetComponent<CharacterUnit>();
                if (player != null)
                {
                    player.RemoveInteractionObject(this);
                    ActiveInteractionState(false);
                }
            }
        }

        /// <summary>
        /// 플레이어와 닿았을때 활성화 및 비활성화 상태 변경
        /// </summary>
        /// <param name="isActive"></param>
        public void ActiveInteractionState(bool isActive)
        {
            if (isActive)
            {
                Debug.Log("ActiveInteractionState: true");
            }
            else
            {
                Debug.Log("ActiveInteractionState: false");
            }
        }
    }
}

