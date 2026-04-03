using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public class PlayerUnit : CharacterUnit
    {
        public override void CharacterInit()
        {
            base.CharacterInit();
            EmotionUIInit();
        }

        private void EmotionUIInit()
        {
            var canvas = InGameManager.Instance.InGameUIManager.Canvas;
            _playerEmotionUI = GetComponentInChildren<PlayerEmotionUI>(true);
            _playerEmotionUI.transform.SetParent(canvas);
            _playerEmotionUI.transform.localPosition = Vector3.zero;
            _playerEmotionUI.transform.localScale = Vector3.one;
            _playerEmotionUI.transform.localRotation = Quaternion.identity;
            PlayerEmotion.Init();
            _playerEmotionUI.Init(this, Camera);
            _playerEmotionUI.Hide();
        }
    }
}

