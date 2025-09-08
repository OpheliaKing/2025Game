using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Shin
{
    public class GameManager : SingletonObject<GameManager>
    {

        [SerializeField]
        private List<ManagerBase> _managers;


        #region  Managers

        [SerializeField]
        private SceneController _sceneController;

        public SceneController SceneController
        {
            get
            {
                return _sceneController;
            }
        }

        private ResourceManager _resourceManager;

        public ResourceManager ResourceManager
        {
            get
            {
                if (_resourceManager == null)
                {
                    for (int i = 0; i < _managers.Count; i++)
                    {
                        if (_managers[i].GetComponent<ResourceManager>() != null)
                        {
                            _resourceManager = _managers[i].GetComponent<ResourceManager>();
                            break;
                        }
                    }
                }

                return _resourceManager;
            }
        }


        private UIManager _uiManager;

        public UIManager UImanager
        {
            get
            {
                if (_uiManager == null)
                {
                    for (int i = 0; i < _managers.Count; i++)
                    {
                        if (_managers[i].GetComponent<UIManager>() != null)
                        {
                            _uiManager = _managers[i].GetComponent<UIManager>();
                            break;
                        }
                    }
                }

                return _uiManager;
            }
        }

        private InputManager _inputManager;

        public InputManager InputManager
        {
            get
            {
                if (_inputManager == null)
                {
                    for (int i = 0; i < _managers.Count; i++)
                    {
                        if (_managers[i].GetComponent<InputManager>() != null)
                        {
                            _inputManager = _managers[i].GetComponent<InputManager>();
                            break;
                        }
                    }
                }

                return _inputManager;
            }
        }



        #endregion

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

            UImanager.ShowUI("StartUI");
        }
    }
}
