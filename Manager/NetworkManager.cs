using System;
using System.Collections;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Shin
{
    /// <summary>
    /// SimulationBehaviour로 변경하여 Static RPC를 지원
    /// NetworkRunner GameObject에 이 컴포넌트를 연결해야 함
    /// </summary>
    public partial class NetworkManager : SimulationBehaviour, INetworkRunnerCallbacks
    {
        private void Awake()
        {
            Debug.Log($"NetworkManager Awake - GO: {gameObject.name}, scene: {gameObject.scene.name}");
        }

        private void OnDestroy()
        {
            Debug.LogWarning($"NetworkManager OnDestroy - GO: {gameObject.name}, scene: {gameObject.scene.name}");
        }

        /// <summary>
        /// 안전한 Runner 접근자
        /// SimulationBehaviour의 base.Runner를 사용하며, null 체크를 포함
        /// </summary>
        public new NetworkRunner Runner
        {
            get
            {
                // base.Runner를 직접 반환 (SimulationBehaviour가 관리)
                var runner = base.Runner;

                // null인 경우 경고 로그 (디버그 모드에서만)
                if (runner == null)
                {
                    Debug.LogWarning("NetworkManager: Runner가 null입니다. NetworkRunner GameObject에 연결되어 있는지 확인하세요.");
                    runner = GetComponent<NetworkRunner>();
                }

                return runner;
            }
        }

        // Static RPC에서 접근해야 하므로 internal로 변경
        internal int _mapLoadPlayerCount = 0;

        /// <summary>
        /// Runner가 유효한지 확인하는 메서드
        /// </summary>
        private bool FindRuuner()
        {
            var runner = Runner;

            if (runner == null)
            {
                Debug.LogError("NetworkManager: Runner가 null입니다. " +
                              "NetworkManager 컴포넌트가 NetworkRunner GameObject에 연결되어 있는지 확인하세요.");
                return false;
            }

            if (!runner.IsRunning)
            {
                Debug.LogWarning("NetworkManager: Runner가 실행 중이 아닙니다.");
                return false;
            }

            return true;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public static void RpcGameStart(NetworkRunner runner)
        {
            var networkManager = runner.GetBehaviour<NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogError("NetworkManager를 찾을 수 없습니다.");
                return;
            }

            if (!networkManager.FindRuuner())
            {
                Debug.LogError("NetworkRunner가 씬에 필요합니다. (FusionBootstrap 또는 Runner 프리팹 배치)");
                return;
            }

            // 씬 로드는 NetworkSceneManagerDefault가 관리. 부트스트랩/러너 설정에 따름
            networkManager.StartCoroutine(networkManager.StartGameRoutine());
        }

        private IEnumerator StartGameRoutine()
        {
            RpcGameStartInit(Runner);
            // 러너가 준비될 시간을 조금 준 뒤 시작 로직 실행
            yield return new WaitForSeconds(1f);

            // 호스트(서버)만 씬 로드를 실행할 수 있음
            if (Runner.IsServer)
            {
                _mapLoadPlayerCount = 0;
                Debug.Log("호스트가 씬 로드를 시작합니다.");

                SceneLoad("InGameScene", LoadSceneMode.Additive, () =>
                {
                    Debug.Log("Scene Load End!!!");
                    StartCoroutine(WaitForMapLoadComplete());
                    RpcMapLoadToClient(Runner);
                });
            }
            else
            {
                Debug.Log("클라이언트는 호스트의 씬 로드를 기다립니다.");
                // 클라이언트는 호스트가 씬을 로드할 때까지 대기
                // 씬 로드는 자동으로 동기화되므로 OnSceneLoadDone 콜백에서 처리될 것임
                // 여기서는 간단히 대기만 함 (실제 씬 로드 완료는 OnSceneLoadDone에서 처리)
                yield return new WaitForSeconds(2f); // 호스트가 씬 로드하는 시간 대기

                Debug.Log("클라이언트: 호스트의 씬 로드를 대기 중 (OnSceneLoadDone에서 처리)");
            }
        }


        [Rpc(RpcSources.All, RpcTargets.All)]
        public static void RpcMapLoadToClient(NetworkRunner runner)
        {
            if (runner.IsServer)
            {
                return;
            }
            var networkManager = runner.GetBehaviour<NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogError("NetworkManager를 찾을 수 없습니다.");
                return;
            }
            networkManager.StartCoroutine(networkManager.WaitForMapLoadComplete());
        }

        /// <summary>
        /// 인게임 시작 전 모든 플레이어 초기화 작업
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.All)]
        private static void RpcGameStartInit(NetworkRunner runner)
        {
            Debug.Log("RpcGameStartInit Run!!!");

            GameManager.Instance.UImanager.SetActiveCanvas(false);
            GameManager.Instance.InputManager.SetInputMode(INPUT_MODE.Player);
        }

        private IEnumerator WaitForMapLoadComplete()
        {
            Debug.Log("Load Test 11");

            if (InGameManager.Instance.PlayerInfo != null)
            {
                InGameManager.Instance.PlayerInfo.InitCharacterUnitList();
            }

            var playerCount = Runner.ActivePlayers.Count();
            StageInfo mapData = null;
            InGameManager.Instance.StageInit("Stage_0001", (loadMapData) =>
            {
                RpcMapLoadComplete(Runner);
                mapData = loadMapData;
            });

            Debug.Log("Load Test 22");

            yield return new WaitUntil(() => _mapLoadPlayerCount == playerCount);

            Debug.Log("Load Test 33");
            Debug.Log("MapLoad All End!!!");

            yield return new WaitUntil(() => InGameManager.Instance.PlayerInfo != null);

            var playerInfo = InGameManager.Instance.PlayerInfo;
            playerInfo.MapDataInit(mapData);

            //InGameManager.Instance.StartGame(null);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private static void RpcMapLoadComplete(NetworkRunner runner)
        {
            var networkManager = runner.GetBehaviour<NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogError("NetworkManager를 찾을 수 없습니다.");
                return;
            }

            networkManager._mapLoadPlayerCount++;
            Debug.Log($"NetworkManager: 맵 로드 완료 플레이어 수: {networkManager._mapLoadPlayerCount}");
        }

        // INetworkRunnerCallbacks 구현
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"NetworkManager: 플레이어 {player}가 입장했습니다.");

            // 호스트: 새 플레이어를 RoomReady 테이블에 추가하고 전원에게 동기화
            if (runner.IsServer)
            {
                UpdatePlayerRoomReady(player, false);
                RpcSyncPlayerReady(runner, player, false);
            }

            // LobbyManager에 플레이어 입장 알림
            if (GameManager.Instance?.LobbyManager != null)
            {
                GameManager.Instance.LobbyManager.HandlePlayerJoined(player);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"NetworkManager: 플레이어 {player}가 퇴장했습니다.");

            // LobbyManager에 플레이어 퇴장 알림
            if (GameManager.Instance?.LobbyManager != null)
            {
                GameManager.Instance.LobbyManager.HandlePlayerLeft(player);
            }
        }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.LogWarning($"NetworkManager OnShutdown - reason: {shutdownReason}, runnerState: {runner?.State}, GO: {gameObject.name}");
            // GameNotFound 등으로 러너가 Shutdown되면, Fusion 권장사항대로 새 Runner 인스턴스로 재시도해야 합니다.
            // 이 오브젝트가 파괴될 수 있어 GameManager(DDOL)에서 다음 프레임에 재생성을 예약합니다.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RecreateNetworkManagerNextFrame();
            }
        }
        void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }
        void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log("NetworkManager: 씬 로드 완료");
        }

        public void OnSceneLoadStart(NetworkRunner runner){}

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }


        public void SceneLoad(string sceneName, LoadSceneMode mode, Action loadComplete = null)
        {
            var runner = Runner;
            if (runner == null)
            {
                Debug.LogError("NetworkManager: Runner가 null입니다. NetworkRunner GameObject에 연결되어 있는지 확인하세요.");
                return;
            }

            if (runner.IsServer)
            {
                var loadScene = Runner.LoadScene(sceneName, LoadSceneMode.Single);

                loadScene.AddOnCompleted((x) =>
                {
                    GameManager.Instance.SetInGameState(sceneName == "InGameScene");
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
                    loadComplete?.Invoke();
                });
            }
        }
    }
}
