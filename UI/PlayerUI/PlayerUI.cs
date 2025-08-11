using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  Shin
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private GameObject _hud;
        [SerializeField] private PlayerPartnerUI _playerPartnerUI;

        public PlayerPartnerUI PlayerPartnerUI
        {
            get
            {
                return _playerPartnerUI;
            }
        }

        public static PlayerUI instance;

        private void Awake()
        {
            instance = this;
            
            //해당 부분은 리소스매니저 추가후 해당 기능 이동
            Init();
        }

        private void Init()
        {
            PlayerPartnerUI.Init();
        }
    }
}