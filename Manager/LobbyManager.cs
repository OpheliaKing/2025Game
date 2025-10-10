using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.Events;

namespace Shin
{
    public class LobbyManager : MonoBehaviour
    {
        [Header("Options")]
        [SerializeField] private byte maxPlayersPerRoom = 2;
        [SerializeField] private bool autoJoinLobby = true;

        [Header("Events")]
        public UnityEvent onConnectedToMaster;
        public UnityEvent onJoinedLobby;
        public UnityEvent onLeftLobby;
        public UnityEvent onJoinedRoom;
        public UnityEvent onLeftRoom;
        public UnityEvent<string> onError;
        public UnityEvent<List<SessionInfo>> onRoomListUpdatedEvent;
        public UnityEvent<List<string>> onPlayerListUpdated;

        private readonly Dictionary<string, SessionInfo> roomNameToInfo = new Dictionary<string, SessionInfo>();

        public bool IsConnected => GameManager.Instance.NetworkManager.Runner.IsRunning == true;
        public bool InRoom => GameManager.Instance.NetworkManager.Runner.IsCloudReady == true;

        [Header("UI")]

        [SerializeField]
        private List<TextSendButton> _textSendButtonList;

        [Header("Managers")]
        [SerializeField]
        private RoomManager roomManager;

        private void EnsureRoomManager()
        {
            if (roomManager == null)
            {
                var roomManagerObj = GameObject.Find("RoomUI");

                if (roomManagerObj != null)
                {
                    roomManager = roomManagerObj.GetComponent<RoomManager>();
                }                
            }
        }

        private void PushPlayersToRoomManager()
        {
            Debug.Log("Test Room");

            EnsureRoomManager();
            if (roomManager != null)
            {
                // Fusion에 맞는 UI 업데이트로 교체 필요
                // roomManager에 적절한 업데이트 메서드가 있다면 여기서 호출하세요.
                var ruuner = GameManager.Instance.NetworkManager.Runner;
                roomManager.UpdateRoomPlayers(ruuner.IsRunning ? ruuner.ActivePlayers : null);
            }
        }

        private void Awake()
        {
            // Fusion은 러너/씬 매니저가 동기화 처리

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetLobbyManager(this);
            }
        }

        private void Start()
        {
            // FusionBootstrap이 네트워크 시작을 관리. 여기서는 UI만 초기화
            InitTextSendButton();
            EnsureRoomManager();
            if (roomManager != null) roomManager.SetActive(false);
        }

        private void InitTextSendButton()
        {
            for (int i = 0; i < _textSendButtonList.Count; i++)
            {
                switch (_textSendButtonList[i].Type)
                {
                    case TextSnedButtonType.CREATE_ROOM:
                        _textSendButtonList[i].SetCallback(CreateRoom);
                        break;
                    case TextSnedButtonType.JOIN_ROOM:
                        _textSendButtonList[i].SetCallback(JoinRoom);
                        break;
                }
            }
        }


        // FusionBootstrap이 연결/세션을 관리. 필요 시 NetworkEvents에서 콜백 연결
        public void CreateRoom(string roomName)
        {
            if (string.IsNullOrWhiteSpace(roomName))
            {
                onError?.Invoke("방 이름이 비어있습니다.");
                return;
            }
            StartCoroutine(StartHostSession(roomName));
        }

        private IEnumerator StartHostSession(string roomName)
        {
            var runner = GameManager.Instance.NetworkManager.Runner;
            if (runner == null)
            {
                Debug.Log("Runner Null");
                yield return null;
            }

            var sceneManager = runner.GetComponent<INetworkSceneManager>();
            var startTask = runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Host,
                SessionName = roomName,
                SceneManager = sceneManager
            });

            while (!startTask.IsCompleted)
            {
                yield return null;
            }

            if (startTask.IsFaulted)
            {
                onError?.Invoke($"방 생성 실패: {startTask.Exception?.GetBaseException().Message}");
                yield break;
            }

            onJoinedRoom?.Invoke();
            UpdatePlayerNicknameList();
            PushPlayersToRoomManager();
        }

        

        public void JoinRoom(string roomName)
        {
            if (string.IsNullOrWhiteSpace(roomName))
            {
                onError?.Invoke("방 이름이 비어있습니다.");
                return;
            }
            onError?.Invoke("FusionBootstrap UI에서 세션에 Join 하세요");
        }

        public void JoinRandomRoom()
        {
            onError?.Invoke("Fusion에서는 자동 매칭은 Runner.StartGame(GameMode.AutoHostOrClient) 사용");
        }

        

        

        public void OnJoinedSession()
        {
            onJoinedRoom?.Invoke();
            UpdatePlayerNicknameList();
            PushPlayersToRoomManager();
        }

        public void OnLeftSession()
        {
            onLeftRoom?.Invoke();
            PushPlayersToRoomManager();
        }

        public void LeaveRoom()
        {
            // FusionBootstrap 메뉴를 통해 세션 종료 권장
            EnsureRoomManager();
            if (roomManager != null)
            {
                roomManager.SetActive(false);
            }
        }

        // 세션 리스트 업데이트는 NetworkEvents 또는 별도 브라우저에서 처리

        public IReadOnlyList<SessionInfo> GetCachedRoomList()
        {
            return new List<SessionInfo>(roomNameToInfo.Values);
        }

        public void SetLocalNickname(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                onError?.Invoke("닉네임이 비어있습니다.");
                return;
            }
            // Fusion은 닉네임을 커스텀 데이터로 관리. Runner/PlayerRef 확장으로 설정 권장
        }

        private void UpdatePlayerNicknameList()
        {
            if (!InRoom)
            {
                onPlayerListUpdated?.Invoke(new List<string>());
                return;
            }

            List<string> nicknames = new List<string>();
            // Fusion: PlayerRef에 닉네임 보관 로직 필요
            onPlayerListUpdated?.Invoke(nicknames);
        }

        public void OnPlayerJoined()
        {
            UpdatePlayerNicknameList();
            PushPlayersToRoomManager();
        }

        public void OnPlayerLeft()
        {
            UpdatePlayerNicknameList();
            PushPlayersToRoomManager();
        }

        public void GameStart()
        {
            GameManager.Instance.NetworkManager.GameStart();
        }
    }
}


