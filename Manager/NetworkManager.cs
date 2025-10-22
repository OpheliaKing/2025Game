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
    public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField]
        private NetworkRunner _runner;

        public NetworkRunner Runner
        {
            get
            {
                if (_runner == null)
                {
                    FindRuuner();
                }

                return _runner;
            }
        }

        private int _mapLoadPlayerCount = 0;

        private bool FindRuuner()
        {
            var runner = FindObjectOfType<NetworkRunner>();
            if (runner == null)
            {
                Debug.LogError("NetworkRunner가 씬에 필요합니다. (FusionBootstrap 또는 Runner 프리팹 배치)");
                return false;
            }

            return true;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RpcGameStart()
        {
            if (!FindRuuner())
            {
                Debug.LogError("NetworkRunner가 씬에 필요합니다. (FusionBootstrap 또는 Runner 프리팹 배치)");
                return;
            }

            // 씬 로드는 NetworkSceneManagerDefault가 관리. 부트스트랩/러너 설정에 따름
            StartCoroutine(StartGameRoutine());
        }

        private IEnumerator StartGameRoutine()
        {
            RpcGameStartInit();
            // 러너가 준비될 시간을 조금 준 뒤 시작 로직 실행
            yield return new WaitForSeconds(1f);

            var loadScene = Runner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Additive);

            loadScene.AddOnCompleted((x) =>
            {
                Debug.Log("Scene Load End!!!");

                //씬 전환 후 맵 로드 코루틴 시작
                StartCoroutine(WaitForMapLoadComplete());
            });

            // GameManager.Instance.SceneController.LoadScene("InGameScene", () =>
            // {
            //     Debug.Log("Game Start Load Scene End");
            //     InGameManager.Instance.StartGame(null);
            // });
        }

        /// <summary>
        /// 인게임 시작 전 모든 플레이어 초기화 작업
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RpcGameStartInit()
        {
            Debug.Log("RpcGameStartInit Run!!!");

            GameManager.Instance.UImanager.Clear();
            GameManager.Instance.InputManager.SetInputMode(INPUT_MODE.Player);
        }

        private IEnumerator WaitForMapLoadComplete()
        {
            var playerCount = Runner.ActivePlayers.Count();
            InGameManager.Instance.StageInit("Stage_0001", () =>
            {
                RpcMapLoadComplete();
            });

            yield return new WaitUntil(() => _mapLoadPlayerCount == playerCount);


            Debug.Log("MapLoad All End!!!");

            //InGameManager.Instance.StartGame(null);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcMapLoadComplete()
        {
            _mapLoadPlayerCount++;
            Debug.Log($"NetworkManager: 맵 로드 완료 플레이어 수: {_mapLoadPlayerCount}");
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
            RpcTestLog();
        }


        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RpcTestLog()
        {
            if (Runner != null && Runner.IsRunning)
            {
                Debug.Log("RpcTestLog Run!!!");
            }
            else
            {
                Debug.LogError("NetworkRunner is not running");
            }

        }
    }
}

