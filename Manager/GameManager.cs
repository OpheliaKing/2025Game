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

        [SerializeField]
        private NetworkManager _netWorkManager;

        public NetworkManager NetworkManager
        {
            get
            {
                if (_netWorkManager == null)
                {
                    _netWorkManager = GetComponentInChildren<NetworkManager>();
                }

                return _netWorkManager;
            }
        }


        private LobbyManager _lobbyManager;

        public LobbyManager LobbyManager
        {
            get
            {
                if (_lobbyManager == null)
                {
                    Debug.Log("Not FOund Lobby Manager !!!!");
                    return null;
                }

                return _lobbyManager;
            }
        }

        private bool _isInGame = false;
        public bool IsInGame
        {
            get
            {
                return _isInGame;
            }
        }

        public void SetLobbyManager(LobbyManager manager)
        {
            _lobbyManager = manager;
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

        public void AddManager(ManagerBase manager)
        {
            _managers.Add(manager);
        }

        private void GameStart()
        {
            Debug.Log("Game Start");

            UImanager.ShowUI("StartUI");
        }
        
        public void SetInGameState(bool state)
        {
            _isInGame = state;
        }
    }
}
