using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Shin
{
    public class CharacterBase : MonoBehaviourPunCallbacks
    {
        protected float _currentHp;
        protected float _maxHp;

        public float GetCurrentHp()
        {
            return _currentHp;
        }

        public float GetMaxHp()
        {
            return _maxHp;
        }
    }

}
