using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shin
{
    public class PlayerEmotionSlot : MonoBehaviour, IPointerClickHandler
    {
        private Image _spriteRenderer;
        private Image SpriteRenderer
        {
            get
            {
                if (_spriteRenderer == null)
                {
                    _spriteRenderer = GetComponent<Image>();
                }
                return _spriteRenderer;
            }
        }

        [SerializeField]
        private EMOETION_TYPE _emotionType;
        public EMOETION_TYPE EmotionType
        {
            get
            {
                return _emotionType;
            }
        }

        private Action<EMOETION_TYPE> _onPointerClick;

        public void Init(Sprite[] sprites, Action<EMOETION_TYPE> onPointerClick)
        {
            var sprite = sprites.FirstOrDefault(x => x.name == EmotionType.ToFileName());
            if (sprite != null)
            {
                SpriteRenderer.sprite = sprite;
            }
            _onPointerClick = onPointerClick;
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            HandleSlotClicked();
        }

        public void HandleSlotClicked()
        {
            Debug.Log("HandleSlotClicked: " + EmotionType);
            // 클릭 시 PlayerEmotionUI의 OnEmotionSlotClick으로 전달됩니다.
            _onPointerClick?.Invoke(EmotionType);
        }
    }
}