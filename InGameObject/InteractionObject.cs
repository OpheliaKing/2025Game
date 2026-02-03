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
    public class InteractionObject : NetworkBehaviour
    {
        [SerializeField]
        private InteractionData _interactionData;

        [SerializeField]
        private SpriteRenderer sprite;

        /// <summary>
        /// Object.Id 기반 고유 ID. Init 코루틴에서 Object가 유효해진 후 설정.
        /// </summary>
        private string _uuid;

        public string UUID => _uuid;

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            StartCoroutine(InitCo());
        }

        private IEnumerator InitCo()
        {
            Debug.Log("테스트 000000");
            var manager = InGameManager.Instance.PlayerInfo;

            yield return new WaitUntil(() => manager != null);
            Debug.Log("테스트 11111");
            yield return new WaitUntil(() => Object != null);


            Debug.Log("테스트 334343");
            _uuid = Object.Id.ToString();
            _interactionData.SetUUID(_uuid);
            manager.AddInteractionObject(this);
        }

        /// <summary>
        /// 플레이어에 의해 상호작용을 했을때
        /// </summary>
        public void ActiveInteraction()
        {
            InGameManager.Instance.PlayerInfo.RpcActiveInteractionStart(_interactionData.UUID);
        }

        public void ActionInteractionEndResult()
        {
            switch (_interactionData.ResultType)
            {
                case INTERACTION_RESULT_TYPE.TELEPORT:
                    break;
                case INTERACTION_RESULT_TYPE.SPRITE_CHANGE:
                    sprite.color = Color.blue;
                    break;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("TEst 0");
            if (other.gameObject.CompareTag("Player"))
            {

                Debug.Log("Test 1");

                var player = other.GetComponent<CharacterUnit>();
                if (player != null)
                {
                    Debug.Log("Test 2");
                    player.SetInteractionObject(this);
                    ActiveInteractionState(true);
                }
                else
                {
                    Debug.Log("Player not found");
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

        public string GetUUID()
        {
            return _interactionData.UUID;
        }
    }
}

