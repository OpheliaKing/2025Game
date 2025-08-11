using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public class PlayerPartnerUI : MonoBehaviour
    {
        [SerializeField] private List<PlayerPartnerSelectUI> _selectUIList;
        [SerializeField] private Transform _selectUIParent;
        
        
        private Dictionary<PublicVariable.CharacterAICommandState, PlayerPartnerSelectUI> _uiDict =
            new Dictionary<PublicVariable.CharacterAICommandState, PlayerPartnerSelectUI>();

        private PlayerPartnerSelectUI _selectedUI;

        public PlayerPartnerSelectUI SelectedUI
        {
            get => _selectedUI;
        }
        private bool _isActive;
        [SerializeField] private PublicVariable.CharacterAICommandState _defaultSelectUI;

        #region Init

        public void Init()
        {
            for (int i = 0; i < _selectUIList.Count; i++)
            {
                if (_selectUIList[i] == null)
                {
                    Debug.LogError($"Select UI {i} Is NULL");
                    return;
                }

                var posType = _selectUIList[i].CommandType;
                
                if (_uiDict.ContainsKey(posType))
                {
                    Debug.LogError($"Dict Have key {posType.ToString()}");
                }
                else
                {
                    _uiDict.Add(posType, _selectUIList[i]);
                    _selectUIList[i].Init();
                }
            }

            InitSelectUI();
            
            ActiveUI(false);
        }

        private void InitSelectUI()
        {
            if (_selectUIList == null)
            {
                return;
            }

            if (_selectUIList.Count == 0)
            {
            }

            _selectedUI = null;

            for (int i = 0; i < _selectUIList.Count; i++)
            {
                if (_selectUIList[i] == null)
                {
                    continue;
                }

                if (_selectUIList[i].CommandType == _defaultSelectUI)
                {
                    _selectedUI = _selectUIList[i];
                    return;
                }
            }

            if (_selectUIList.Count > 0)
            {
            }
            
            UpdateSelectUI();
        }

        #endregion

        public void ToggleSelectUI()
        {
            _isActive = !_isActive;
            ActiveUI(_isActive);
        }
        
        public void ActiveUI(bool isActive)
        {
            switch (isActive)
            {
                case true:
                    ShowUI();
                    break;

                case false:
                    HideUI();
                    break;
            }
        }

        public void UpdateSelectUI()
        {
            foreach (var data in _uiDict)
            {
                var selected = data.Key == _selectedUI.CommandType;
                data.Value.UpdateSelectUI(selected);
            }
        }

        private void ShowUI()
        {
            _selectUIParent.gameObject.SetActive(true);
            UpdateSelectUI();
        }

        private void HideUI()
        {
            _selectUIParent.gameObject.SetActive(false);
            UpdateSelectUI();
        }

        public void ChangeSelectUI(int target)
        {
            int currentIndex = (int)SelectedUI.CommandType;
            int targetIndex = 0;
            int count = System.Enum.GetValues(typeof(PublicVariable.CharacterAICommandState)).Length;

            if (target == 1)
            {
                if (currentIndex + 1 >= count)
                {
                    targetIndex = 0;
                }
                else
                {
                    targetIndex = currentIndex + 1;
                }
            }
            else
            {
                if (currentIndex == 0)
                {
                    targetIndex = count -1;
                }
                else
                {
                    targetIndex = currentIndex - 1;
                }
            }

            var targetType = (PublicVariable.CharacterAICommandState)targetIndex;

            if (_uiDict.ContainsKey(targetType))
            {
                _selectedUI = _uiDict[targetType];
            }

            UpdateSelectUI();
        }
    }
}