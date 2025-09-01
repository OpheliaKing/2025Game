using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public class GameManager : SingletonObject<GameManager>
    {

        [SerializeField]
        private List<GameObject> _managers;

        protected override void Awake()
        {
            base.Awake();

            Debug.Log("GameManager Awake");
        }

        private void ManagerInit()
        {
            
        }
    }
}
