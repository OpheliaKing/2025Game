using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


namespace Shin
{
    public class LobbyManager : UIBase
    {
        [Header("Options")]
        [SerializeField] private byte maxPlayersPerRoom = 2;
        public UnityEvent onJoinedRoom;
        public UnityEvent onLeftRoom;
        public UnityEvent<string> onError;
        public UnityEvent<List<string>> onPlayerListUpdated;

        private readonly Dictionary<string, SessionInfo> roomNameToInfo = new Dictionary<string, SessionInfo>();

        public bool IsConnected => GameManager.Instance.NetworkManager.Runner.IsRunning == true;
        public bool InRoom => GameManager.Instance.NetworkManager.Runner.IsCloudReady == true;

        [Header("UI")]

        [SerializeField]
        private List<TextSendButton> _textSendButtonList;

        [SerializeField]
        private TMP_InputField _playerNameInputField;

        [Header("Managers")]
        [SerializeField]
        private RoomManager roomManager;

        public RoomManager RoomManager => roomManager;

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
            EnsureRoomManager();
            if (roomManager != null)
            {
                // Fusion에 맞는 UI 업데이트로 교체 필요
                // roomManager에 적절한 업데이트 메서드가 있다면 여기서 호출하세요.
                var ruuner = GameManager.Instance.NetworkManager.Runner;
                roomManager.UpdateRoomPlayers();
            }
        }

        private void Awake()
        {
            // Fusion은 러너/씬 매니저가 동기화 처리

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetLobbyManager(this);
                GameManager.Instance.NetworkManager.OnShutDownCallback += DisConnectServer;
                GameManager.Instance.NetworkManager.OnPlayerLeftCallback += HandlePlayerLeft;
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

        private void InitRoomState()
        {
            GameManager.Instance.NetworkManager.DataInit();
            GameManager.Instance.LobbyManager.RoomManager.InitPlayerInfoUI();
        }


        // FusionBootstrap이 연결/세션을 관리. 필요 시 NetworkEvents에서 콜백 연결
        public void CreateRoom(string roomName)
        {
            if (string.IsNullOrWhiteSpace(roomName))
            {
                GameManager.Instance.UImanager.ShowSystemMessage("방 이름이 비어있습니다.");
                return;
            }
            StartCoroutine(StartHostSession(roomName));
        }

        private IEnumerator StartHostSession(string roomName)
        {
            InitRoomState();
            var runner = GameManager.Instance.NetworkManager.Runner;
            if (runner == null)
            {
                Debug.Log("Runner Null");
                yield break;
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
                var reason = GetRoomStartFailureReason(startTask.Exception, isHost: true);
                GameManager.Instance.UImanager.ShowSystemMessage(reason);
                onError?.Invoke($"방 생성 실패: {reason}");
                yield break;
            }

            var startResult = startTask.Result;
            if (!startResult.Ok)
            {
                var reason = GetRoomStartFailureReason(startResult, isHost: true);
                GameManager.Instance.UImanager.ShowSystemMessage(reason);
                onError?.Invoke($"방 생성 실패: {reason}");
                yield break;
            }

            onJoinedRoom?.Invoke();
            UpdatePlayerNicknameList();
            PushPlayersToRoomManager();
        }

        /// <summary>
        /// StartGame 실패 시 예외로부터 사용자에게 보여줄 실패 원인 문자열을 반환합니다.
        /// </summary>
        private static string GetRoomStartFailureReason(AggregateException aggregateEx, bool isHost)
        {
            var ex = aggregateEx?.GetBaseException();
            if (ex == null) return "알 수 없는 오류";

            var msg = ex.Message ?? "";

            // 예외 타입별 구분
            if (ex is SocketException socketEx)
            {
                return socketEx.SocketErrorCode switch
                {
                    SocketError.ConnectionRefused => "서버에 연결할 수 없습니다. (연결 거부)",
                    SocketError.TimedOut => "연결 시간이 초과되었습니다.",
                    SocketError.NetworkUnreachable => "네트워크에 연결되어 있지 않습니다.",
                    SocketError.HostNotFound => "호스트를 찾을 수 없습니다.",
                    _ => $"네트워크 오류: {socketEx.SocketErrorCode}"
                };
            }

            if (ex is TimeoutException)
                return "연결 시간이 초과되었습니다.";

            if (ex is OperationCanceledException)
                return "연결이 취소되었습니다.";

            // 메시지 키워드로 흔한 원인 추정
            if (msg.IndexOf("already exist", StringComparison.OrdinalIgnoreCase) >= 0 ||
                msg.IndexOf("duplicate", StringComparison.OrdinalIgnoreCase) >= 0)
                return "이미 같은 이름의 방이 존재합니다.";

            if (msg.IndexOf("session", StringComparison.OrdinalIgnoreCase) >= 0 &&
                msg.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0)
                return "해당 방을 찾을 수 없습니다. 이름을 확인하거나 호스트가 먼저 방을 만들어 주세요.";

            if (msg.IndexOf("connection", StringComparison.OrdinalIgnoreCase) >= 0 ||
                msg.IndexOf("connect", StringComparison.OrdinalIgnoreCase) >= 0)
                return "서버 연결에 실패했습니다. 인터넷 연결을 확인해 주세요.";

            if (msg.IndexOf("max", StringComparison.OrdinalIgnoreCase) >= 0 &&
                msg.IndexOf("player", StringComparison.OrdinalIgnoreCase) >= 0)
                return "방 인원이 가득 찼습니다.";

            // 기본: 실제 예외 메시지 그대로 노출 (디버깅에 유리)
            return string.IsNullOrWhiteSpace(msg) ? "알 수 없는 오류" : msg;
        }

        private static string GetRoomStartFailureReason(StartGameResult result, bool isHost)
        {
            var reason = result.ShutdownReason.ToString();
            var msg = result.ErrorMessage ?? "";

            // ShutdownReason 기반으로 흔한 원인 추정
            if (reason.IndexOf("NotFound", StringComparison.OrdinalIgnoreCase) >= 0 ||
                reason.IndexOf("GameNotFound", StringComparison.OrdinalIgnoreCase) >= 0)
                return "해당 방을 찾을 수 없습니다. 이름을 확인하거나 호스트가 먼저 방을 만들어 주세요.";

            if (reason.IndexOf("Full", StringComparison.OrdinalIgnoreCase) >= 0 ||
                reason.IndexOf("Max", StringComparison.OrdinalIgnoreCase) >= 0)
                return "방 인원이 가득 찼습니다.";

            if (reason.IndexOf("Invalid", StringComparison.OrdinalIgnoreCase) >= 0)
                return string.IsNullOrWhiteSpace(msg) ? $"잘못된 요청: {reason}" : msg;

            if (!string.IsNullOrWhiteSpace(msg))
                return $"{reason}: {msg}";

            return $"시작 실패: {reason}";
        }

        public void JoinRoom(string roomName)
        {
            roomName = roomName.Replace(" ", "");
            if (string.IsNullOrEmpty(roomName))
            {
                onError?.Invoke("방 이름이 비어있습니다.");
                return;
            }
            Debug.Log("Join Room: " + roomName);
            Debug.Log("Join Room: " + roomName.Length);
            Debug.Log("Room Empty Test" + roomName == "");

            StartCoroutine(StartClientSession(roomName));
        }

        private IEnumerator StartClientSession(string roomName)
        {
            InitRoomState();
            var runner = GameManager.Instance.NetworkManager.Runner;
            if (runner == null)
            {
                Debug.Log("Runner Null");
                onError?.Invoke("네트워크 러너가 초기화되지 않았습니다.");
                yield return null;
            }

            var sceneManager = runner.GetComponent<INetworkSceneManager>();
            var startTask = runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionName = roomName,
                SceneManager = sceneManager,
                // 방이 없을 때 클라이언트가 새 세션을 생성하지 않도록 비활성화.
                // 없으면 Join 실패 → IsFaulted == true 로 처리됨.
                EnableClientSessionCreation = false
            });

            while (!startTask.IsCompleted)
            {
                yield return null;
            }

            if (startTask.IsFaulted)
            {
                var reason = GetRoomStartFailureReason(startTask.Exception, isHost: false);
                GameManager.Instance.UImanager.ShowSystemMessage(reason);
                onError?.Invoke($"방 입장 실패: {reason}");
                yield break;
            }

            var startResult = startTask.Result;
            if (!startResult.Ok)
            {
                var reason = GetRoomStartFailureReason(startResult, isHost: false);
                GameManager.Instance.UImanager.ShowSystemMessage(reason);
                onError?.Invoke($"방 입장 실패: {reason}");
                yield break;
            }

            onJoinedRoom?.Invoke();
            UpdatePlayerNicknameList();
            PushPlayersToRoomManager();
        }

        public void OnJoinedSession()
        {
            Debug.Log("Check Join");

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

        // public void OnPlayerJoined()
        // {
        //     UpdatePlayerNicknameList();
        //     PushPlayersToRoomManager();
        // }

        // public void OnPlayerLeft()
        // {
        //     UpdatePlayerNicknameList();
        //     PushPlayersToRoomManager();
        // }

        // 포톤퓨전에서 플레이어가 방에 입장할 때마다 호출되는 RPC 함수
        public void OnPlayerJoinedSession(PlayerRef player)
        {
            Debug.Log($"플레이어 {player}가 세션에 입장했습니다.");

            // 플레이어 입장 시 처리할 로직
            UpdatePlayerNicknameList();
            PushPlayersToRoomManager();

            // 추가적인 플레이어 입장 이벤트 발생
            onPlayerJoined?.Invoke();
        }

        // 포톤퓨전에서 플레이어가 방을 떠날 때마다 호출되는 RPC 함수
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void OnPlayerLeftSession(PlayerRef player)
        {
            Debug.Log($"플레이어 {player}가 세션을 떠났습니다.");

            // 플레이어 퇴장 시 처리할 로직
            UpdatePlayerNicknameList();
            PushPlayersToRoomManager();

            // 추가적인 플레이어 퇴장 이벤트 발생
            onPlayerLeft?.Invoke();
        }


        // 플레이어 입장/퇴장 이벤트
        public UnityEvent onPlayerJoined;
        public UnityEvent onPlayerLeft;

        // 포톤퓨전에서 플레이어 입장을 감지하는 메서드
        // 이 메서드는 NetworkManager나 다른 네트워크 컴포넌트에서 호출되어야 합니다
        public void HandlePlayerJoined(PlayerRef player)
        {
            Debug.Log($"Lobby Manager : 플레이어 {player}가 방에 입장했습니다.");

            // RPC로 모든 클라이언트에게 플레이어 입장 알림
            if (GameManager.Instance?.NetworkManager?.Runner != null)
            {
                OnPlayerJoinedSession(player);
            }
        }

        // 포톤퓨전에서 플레이어 퇴장을 감지하는 메서드
        // 이 메서드는 NetworkManager나 다른 네트워크 컴포넌트에서 호출되어야 합니다
        public void HandlePlayerLeft(PlayerRef player)
        {
            Debug.Log($"플레이어 {player}가 방을 떠났습니다.");

            var runner = GameManager.Instance.NetworkManager.Runner;

            if (runner == null) return;

            GameManager.Instance.NetworkManager.PlayerLeave(player);
            //UI 업데이트
            roomManager.UpdatePlayerUI();
        }

        public void DisConnectServer()
        {
            var runner = GameManager.Instance.NetworkManager.Runner;
            //runner.Shutdown();
            roomManager.HostLeft();
            GameManager.Instance.UImanager.ShowSystemMessage("호스트가 방을 나갔습니다.");
        }

        // // RPC 호출을 위한 헬퍼 함수들
        // public void BroadcastPlayerJoined(PlayerRef player)
        // {
        //     if (GameManager.Instance?.NetworkManager?.Runner != null && GameManager.Instance.NetworkManager.Runner.IsRunning)
        //     {
        //         OnPlayerJoinedSession(player);
        //     }
        // }

        // public void BroadcastPlayerLeft(PlayerRef player)
        // {
        //     if (GameManager.Instance?.NetworkManager?.Runner != null && GameManager.Instance.NetworkManager.Runner.IsRunning)
        //     {
        //         OnPlayerLeftSession(player);
        //     }
        // }

        // 포톤퓨전 러너의 플레이어 리스트를 주기적으로 체크하는 메서드
        // private void CheckPlayerChanges()
        // {
        //     if (!InRoom) return;

        //     var runner = GameManager.Instance.NetworkManager.Runner;
        //     if (runner != null && runner.ActivePlayers != null)
        //     {
        //         // 현재 활성 플레이어 수가 변경되었는지 확인
        //         int currentPlayerCount = runner.ActivePlayers.Count();

        //         // 이전 플레이어 수와 비교하여 변경사항 감지
        //         // 실제 구현에서는 이전 상태를 저장하고 비교해야 합니다
        //         UpdatePlayerNicknameList();
        //         PushPlayersToRoomManager();
        //     }
        // }

        public override void Show()
        {
            base.Show();
            UpdateUI();
        }

        public void UpdateUI()
        {
            _playerNameInputField.text = GameManager.Instance.NetworkManager.PlayerName;
        }

        public void OnClickPlayerNameUpdate()
        {
            GameManager.Instance.NetworkManager.UpdatePlayerName(_playerNameInputField.text);
        }

        public string GetPlayerName()
        {
            return _playerNameInputField.text;
        }



        //         public void RpcGameStart()
        //         {
        //             var runner = GameManager.Instance.NetworkManager.Runner;

        // Debug.Log($"runner is running: {runner.IsRunning}");

        //             GameManager.Instance.NetworkManager.RpcTestLog();
        //             Debug.Log("RpcGameStart Run!!!");

        //             GameManager.Instance.NetworkManager.RpcGameStart();



        //             Debug.Log($"runner is running: {runner.IsRunning}");


        //             if (GameManager.Instance.NetworkManager.Runner != null && GameManager.Instance.NetworkManager.Runner.IsRunning)
        //             {
        //                 Debug.Log("networkmanager runner is running");

        //                 GameManager.Instance.NetworkManager.RpcTestLog();
        //             }
        //             else
        //             {
        //                 Debug.LogError("NetworkRunner is not running");
        //             }

        //             // var runner = GameManager.Instance.NetworkManager.Runner;
        //             // var sceneManager = runner.GetComponent<INetworkSceneManager>();
        //             // var parameters = new NetworkLoadSceneParameters { };
        //             // runner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Additive);
        //         }
    }
}



