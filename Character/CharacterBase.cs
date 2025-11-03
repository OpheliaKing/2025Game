using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;



namespace Shin
{
    //Photon Object
    public class CharacterBase : NetworkBehaviour
    {
        protected float _currentHp;
        protected float _maxHp;

        private NetworkObject _netWorkObject;

        public NetworkObject NetworkObject
        {
            get
            {
                if (_netWorkObject == null)
                {
                    _netWorkObject = GetComponent<NetworkObject>();
                }

                return _netWorkObject;
            }
        }

        public float GetCurrentHp()
        {
            return _currentHp;
        }

        public float GetMaxHp()
        {
            return _maxHp;
        }

        public NetworkId GetNetworkId()
        {
            return Object.Id;  // NetworkBehaviour의 Object 프로퍼티 사용
        }
    }

}
