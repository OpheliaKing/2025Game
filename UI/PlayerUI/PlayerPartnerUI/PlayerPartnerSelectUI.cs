using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Shin
{
    public class PlayerPartnerSelectUI : MonoBehaviour
    {
        public PublicVariable.CharacterAICommandState CommandType;
        
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _typeTextUI;

        [SerializeField] private GameObject _selectImage;
        
        private RectTransform _rectTransform;

        private RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }

                return _rectTransform;
            }
        }
        
        
        public void Init()
        {
            InitTextUI();
            InitLocalPos();
        }

        private void InitLocalPos()
        {
            switch (CommandType)
            {
                case PublicVariable.CharacterAICommandState.FOLLOW:
                    RectTransform.localPosition = new Vector2(-150f, 0f);
                    break;
                case PublicVariable.CharacterAICommandState.PATROL:
                    RectTransform.localPosition = new Vector2(0f, 150f);
                    break;
                case PublicVariable.CharacterAICommandState.STAND_BY:
                    RectTransform.localPosition = new Vector2(150f, 0f);
                    break;
            }
        }

        private void InitTextUI()
        {
            if (_typeTextUI == null)
            {
                Debug.LogError($"Not Found _typeTextUI");
                return;
            }
            switch (CommandType)
            {
                case PublicVariable.CharacterAICommandState.FOLLOW:
                    _typeTextUI.SetText("FOLLOW");
                    break;
                case PublicVariable.CharacterAICommandState.PATROL:
                    _typeTextUI.SetText("PATROL");
                    break;
                case PublicVariable.CharacterAICommandState.STAND_BY:
                    _typeTextUI.SetText("STAND BY");
                    break;
            }
        }

        public void UpdateSelectUI(bool isSelect)
        {
            _selectImage.SetActive(isSelect);
        }
    }
}