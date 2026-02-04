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
    public class NetworkManager : SimulationBehaviour, INetworkRunnerCallbacks
    {
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
                Debug.Log("호스트가 씬 로드를 시작합니다.");
                var loadScene = Runner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Additive);

                loadScene.AddOnCompleted((x) =>
                {
                    Debug.Log("Scene Load End!!!");

                    //씬 전환 후 맵 로드 코루틴 시작
                    StartCoroutine(WaitForMapLoadComplete());
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

        /// <summary>
        /// 인게임 시작 전 모든 플레이어 초기화 작업
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.All)]
        private static void RpcGameStartInit(NetworkRunner runner)
        {
            Debug.Log("RpcGameStartInit Run!!!");

            GameManager.Instance.UImanager.Clear();
            GameManager.Instance.InputManager.SetInputMode(INPUT_MODE.Player);
        }

        private IEnumerator WaitForMapLoadComplete()
        {
            var playerCount = Runner.ActivePlayers.Count();
            StageInfo mapData = null;
            InGameManager.Instance.StageInit("Stage_0001", (loadMapData) =>
            {
                RpcMapLoadComplete(Runner);
                mapData = loadMapData;
            });

            yield return new WaitUntil(() => _mapLoadPlayerCount == playerCount);


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
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
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
            // SceneController에 씬 로드 완료 알림
            if (GameManager.Instance?.SceneController != null)
            {
                GameManager.Instance.SceneController.OnNetworkSceneLoadDone();
                GameManager.Instance.SceneController.OnSceneLoadDone(runner);
            }
            
            // 클라이언트의 경우 맵 로드 코루틴 시작
            // 호스트는 StartGameRoutine에서 이미 시작했으므로 중복 방지
            if (!runner.IsServer)
            {
                Debug.Log("클라이언트: 씬 로드 완료, 맵 로드 시작");
                StartCoroutine(WaitForMapLoadComplete());
            }
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            Debug.Log("NetworkManager: 씬 로드 시작");
            // SceneController에 씬 로드 시작 알림
            if (GameManager.Instance?.SceneController != null)
            {
                GameManager.Instance.SceneController.OnSceneLoadStart(runner);
            }
        }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void Test()
        {
            var runner = Runner;
            
            if (runner == null)
            {
                Debug.LogError("NetworkManager: Runner가 null입니다. NetworkRunner GameObject에 연결되어 있는지 확인하세요.");
                return;
            }

            if (!runner.IsRunning)
            {
                Debug.LogError("NetworkManager: Runner가 실행 중이 아닙니다.");
                return;
            }

            // Static RPC를 호출 - 이제 SimulationBehaviour를 상속했으므로 정상 작동
            RpcTestLog(runner);
        }


        /// <summary>
        /// Static RPC: NetworkBehaviour가 아닌 MonoBehaviour에서 사용
        /// 첫 번째 인수로 NetworkRunner가 필요함
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.All)]
        public static void RpcTestLog(NetworkRunner runner)
        {
            if (runner != null && runner.IsRunning)
            {
                Debug.Log($"RpcTestLog Run!!! (Local Player: {runner.LocalPlayer})");
            }
            else
            {
                Debug.LogError("NetworkRunner is not running");
            }
            GameManager.Instance.LobbyManager.RoomManager.TestPush();
        }
    }
}

