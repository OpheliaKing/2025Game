using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public class PlayerEmotionUI : UIBase
    {
        private PlayerUnit _player;
        private Camera _camera;
        private List<PlayerEmotionSlot> _emotionSlotList = new List<PlayerEmotionSlot>();

        private Animator _anim;
        private Animator Anim
        {
            get
            {
                if (_anim == null)
                {
                    _anim = GetComponentInChildren<Animator>();
                }
                return _anim;
            }
        }
        public void Init(PlayerUnit player, Camera camera)
        {
            _player = player;
            _camera = camera;
            var reManager = GameManager.Instance.ResourceManager;
            var sprite = GameManager.Instance.ResourceManager.LoadSprites("Emotion_Sprite", reManager.SpritePath);
            _emotionSlotList.Clear();
            var childs = transform.GetComponentsInChildren<PlayerEmotionSlot>();

            Debug.Log("Childs Count: " + childs.Length);

            for (int i = 0; i < childs.Length; i++)
            {
                _emotionSlotList.Add(childs[i]);
                childs[i].Init(sprite, OnEmotionSlotClick);
            }
        }

        private void LateUpdate()
        {
            UpdateTargerPosition();
        }

        private void UpdateTargerPosition()
        {
            if (_player == null) return;
            transform.position = _camera.WorldToScreenPoint(_player.transform.position);
        }

        public void Toggle()
        {
            if (IsVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Anim.Rebind();
            Anim.Play("UI_Emotion_Slot_Parent_Show");
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnEmotionSlotClick(EMOETION_TYPE emotionType)
        {
            _player.ActiveEmotion(emotionType);
            Hide();
        }
    }
}