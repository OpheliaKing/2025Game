using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

namespace Shin
{
    public class GameManager : SingletonObject<GameManager>
    {

        [SerializeField]
        private List<ManagerBase> _managers;


        #region  Managers


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
        private bool _recreatingNetworkManager;

        public NetworkManager NetworkManager
        {
            get
            {
                if (_netWorkManager == null)
                    _netWorkManager = GetComponentInChildren<NetworkManager>(true);

                return _netWorkManager;
            }
        }

        public void EnsureNetworkManagerExists()
        {
            // 이미 유효하면 그대로 사용
            if (_netWorkManager != null && _netWorkManager.Runner != null)
            {
                return;
            }

            // 씬/비활성 오브젝트까지 포함해 재탐색
            _netWorkManager = GetComponentInChildren<NetworkManager>(true);
            if (_netWorkManager != null && _netWorkManager.Runner != null)
            {
                return;
            }

            // 없으면 새 Runner+NetworkManager를 생성 (재시도용)
            var go = new GameObject("NetworkManager");
            go.transform.SetParent(transform, false);

            go.AddComponent<NetworkRunner>();
            go.AddComponent<NetworkSceneManagerDefault>();
            go.AddComponent<NetworkObjectProviderDefault>();

            _netWorkManager = go.AddComponent<NetworkManager>();

            DontDestroyOnLoad(go);
        }

        public void RecreateNetworkManagerNextFrame()
        {
            if (_recreatingNetworkManager) return;
            _recreatingNetworkManager = true;
            StartCoroutine(RecreateNetworkManagerRoutine());
        }

        private IEnumerator RecreateNetworkManagerRoutine()
        {
            // Fusion이 Shutdown 후 Destroy를 예약하는 경우가 있어 한 프레임 대기
            yield return null;
            EnsureNetworkManagerExists();
            _recreatingNetworkManager = false;
        }


        private SoundManager _soundManager;

        public SoundManager SoundManager
        {
            get
            {
                if (_soundManager == null)
                {
                    for (int i = 0; i < _managers.Count; i++)
                    {
                        if (_managers[i].GetComponent<SoundManager>() != null)
                        {
                            _soundManager = _managers[i].GetComponent<SoundManager>();
                            break;
                        }
                    }
                }

                return _soundManager;
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
