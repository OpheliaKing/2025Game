using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Shin
{
    public partial class CharacterUnit
    {

        private PlayerEmotion _playerEmotion;
        public PlayerEmotion PlayerEmotion
        {
            get
            {
                if (_playerEmotion == null)
                {
                    _playerEmotion = GetComponentInChildren<PlayerEmotion>(true);
                }
                return _playerEmotion;
            }
        }
        // Start is called before the first frame update
        public virtual void ActiveAbilityA()
        {
            Debug.Log("ActiveAbilityA");
        }

        public virtual void ActiveAbilityB()
        {
            Debug.Log("ActiveAbilityB");
        }

        public virtual void ActiveAbilityC()
        {
            Debug.Log("ActiveAbilityC");
        }

        public void ActiveEmotion()
        {
            RpcActiveEmotion(EMOETION_TYPE.LOVE);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RpcActiveEmotion(EMOETION_TYPE emotionType)
        {
            PlayerEmotion.ShowEmotion(emotionType);
        }
    }
}