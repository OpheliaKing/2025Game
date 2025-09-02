using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public class GameManager : SingletonObject<GameManager>
    {

        [SerializeField]
        private List<ManagerBase> _managers;

        protected override void Awake()
        {
            base.Awake();

            Debug.Log("GameManager Awake");

            ManagerInit();

            Debug.Log("Manager Init");

            GameStart();
        }

        private void ManagerInit()
        {
            for (int i = 0; i < _managers.Count; i++)
            {
                _managers[i].ManagerInit();
            }
        }

        private void GameStart()
        {
            Debug.Log("Game Start");
        }
    }
}
