using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shin
{
    public class MapDataUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private TextMeshProUGUI _mapNameText;
        [SerializeField]
        private Image _mapImage;

        private StageDataItem _data;
        private Action<StageDataItem> _onSelectAction;

        public void UpdateUI(StageDataItem data, Action<StageDataItem> onSelectAction)
        {
            _mapNameText.text = data.StageName;

            var sprite = GameManager.Instance.ResourceManager.LoadSprite(data.MapImagePath, GameManager.Instance.ResourceManager.SpritePath);
            if (sprite != null)
            {
                _mapImage.sprite = sprite;
            }
            _data = data;
            _onSelectAction = onSelectAction;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            _onSelectAction?.Invoke(_data);
        }
    }
}