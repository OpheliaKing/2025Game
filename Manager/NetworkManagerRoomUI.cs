using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace Shin
{
    /// <summary>
    /// RPC로 방 전체 플레이어 정보를 전달할 때 사용하는 스냅샷 구조체.
    /// </summary>
    [Serializable]
    public struct RoomPlayerInfoSnapshot : INetworkStruct
    {
        public PlayerRef PlayerRef;
        public NetworkString<_32> PlayerName;
        public bool IsReady;
        public PlayerRef HostPlayerRef;
    }

    public partial class NetworkManager
    {
        //private string _playerName = "Player";
        public string PlayerName => GameManager.Instance.LobbyManager.GetPlayerName();
        /// <summary>
        /// 로컬 플레이어의 준비 상태
        /// </summary>
        private bool _roomReady = false;
        public bool RoomReady => _roomReady;

        private string _selectedMapTid = "";
        public string SelectedMapTid => _selectedMapTid;

        public Action<string> OnChangeMapTidCallback;

        /// <summary>
        /// 플레이어별 방 정보(이름, 준비 여부 등).
        /// 호스트가 RpcSyncPlayerReady로 브로드캐스트하여 모든 클라이언트가 동일한 값을 유지.
        /// (외부에서는 읽기만 가능하게 노출)
        /// </summary>
        private readonly Dictionary<PlayerRef, RoomPlayerInfo> _roomPlayerInfo = new Dictionary<PlayerRef, RoomPlayerInfo>();
        public IReadOnlyDictionary<PlayerRef, RoomPlayerInfo> RoomPlayerInfo => _roomPlayerInfo;

        public void DataInit()
        {
            _roomPlayerInfo.Clear();
        }


        /// <summary>
        /// 호스트가 새로 입장한 클라이언트에게 현재 방 전체 플레이어 정보(이름, 준비 여부)를 전송할 때 호출.
        /// OnPlayerJoined(호스트)에서 새 플레이어에 대해 호출하면, 해당 클라이언트의 _roomPlayerInfo가 기존 유저 데이터로 채워짐.
        /// </summary>
        public static void SyncRoomPlayerInfo(NetworkRunner runner, PlayerRef newPlayer, PlayerRef hostPlayer)
        {
            if (runner == null || !runner.IsServer) return;

            var networkManager = runner.GetBehaviour<NetworkManager>();
            if (networkManager == null) return;

            var list = new List<RoomPlayerInfoSnapshot>();
            foreach (var kv in networkManager._roomPlayerInfo)
            {
                list.Add(new RoomPlayerInfoSnapshot
                {
                    PlayerRef = kv.Key,
                    PlayerName = kv.Value.PlayerName ?? "",
                    IsReady = kv.Value.IsReady,
                    HostPlayerRef = hostPlayer
                });
            }
            RpcSyncFullRoomState(runner, newPlayer, list.ToArray());
        }

        // /// <summary>
        // /// 호스트 → 특정 클라이언트로 방 전체 플레이어 스냅샷 전송. 입장 직후 클라이언트가 기존 유저 정보를 받을 때 사용.
        // /// </summary>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public static void RpcSyncFullRoomState(NetworkRunner runner, [RpcTarget] PlayerRef target, RoomPlayerInfoSnapshot[] snapshots)
        {
            var networkManager = runner.GetBehaviour<NetworkManager>();
            if (networkManager == null) return;

            networkManager.ReplaceRoomPlayerInfoFromSnapshot(snapshots);
        }

        /// <summary>
        /// 스냅샷으로 _roomPlayerInfo 전체 교체. RpcSyncFullRoomState 수신 시 호출됨.
        /// </summary>
        private void ReplaceRoomPlayerInfoFromSnapshot(RoomPlayerInfoSnapshot[] snapshots)
        {
            _roomPlayerInfo.Clear();
            if (snapshots == null) return;

            foreach (var s in snapshots)
            {
                _roomPlayerInfo[s.PlayerRef] = new RoomPlayerInfo
                {
                    PlayerName = s.PlayerName.ToString(),
                    IsReady = s.IsReady
                };
            }

            if (Runner != null && _roomPlayerInfo.TryGetValue(Runner.LocalPlayer, out var localInfo))
            {
                _roomReady = localInfo.IsReady;
            }

            if (GameManager.Instance?.LobbyManager?.RoomManager != null && Runner != null && Runner.IsRunning)
            {
                GameManager.Instance.LobbyManager.RoomManager.UpdateRoomPlayers();
            }
        }

        /// <summary>
        /// 클라이언트/호스트가 Ready 토글 시 호출. StateAuthority(호스트)에서만 처리 후 전원에게 동기화.
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public static void RpcRoomReady(NetworkRunner runner, PlayerRef player, bool isReady, RpcInfo info = default)
        {
            var networkManager = runner.GetBehaviour<NetworkManager>();
            if (networkManager == null) return;

            RpcSyncPlayerState(runner, player, isReady);
        }

        public void PlayerLeave(PlayerRef player)
        {
            var runner = GameManager.Instance.NetworkManager.Runner;
            if (runner == null) return;

            RpcSyncPlayerState(runner, player, false, false, true);
        }

        /// <summary>
        /// 호스트가 한 명의 유저 상태를 모든 클라이언트에게 공유할 때 사용. 전원이 로컬 _roomReadyStates를 갱신.
        /// </summary>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public static void RpcSyncPlayerState(NetworkRunner runner, PlayerRef player, bool isReady, bool isUpdateName = false, bool isLeave = false)
        {
            var networkManager = runner.GetBehaviour<NetworkManager>();
            if (networkManager == null) return;

            networkManager.UpdatePlayerRoomReady(player, isReady, isUpdateName, isLeave);
        }



        /// <summary>
        /// 특정 플레이어의 RoomReady 값을 갱신. 플레이어 입장, Ready 토글 등 유저 데이터가 바뀔 때마다 호출해도 됨.
        /// </summary>
        public void UpdatePlayerRoomReady(PlayerRef player, bool isReady, bool isUpdateName = false, bool isLeave = false)
        {
            if (isLeave)
            {
                if (_roomPlayerInfo.ContainsKey(player))
                {
                    _roomPlayerInfo.Remove(player);
                }
            }
            else
            {
                //유저 데이터 정리
                if (!_roomPlayerInfo.TryGetValue(player, out var info))
                {
                    info = new RoomPlayerInfo { IsReady = isReady, PlayerName = "" };
                    _roomPlayerInfo[player] = info;
                }
                else
                {
                    info.IsReady = isReady;
                }

                if (isUpdateName)
                {
                    RpcRequestPlayerName(Runner, player);
                }

                if (Runner != null && Runner.LocalPlayer == player)
                {
                    _roomReady = isReady;
                }
            }


            //데이터 UI에 업데이트

            Debug.Log("User Count" + _roomPlayerInfo.Count);
            GameManager.Instance.LobbyManager.RoomManager.UpdateRoomPlayers();
        }

        /// <summary>
        /// 현재 방 안에 있는 클라이언트 유저들 중
        /// RoomReady == true 인 플레이어 수를 반환
        /// </summary>
        public int GetReadyPlayerCount()
        {
            if (Runner == null || !Runner.IsRunning)
            {
                return 0;
            }

            int count = 0;

            // 현재 방에 실제로 접속해 있는 플레이어들만 대상으로 카운트
            foreach (var player in Runner.ActivePlayers)
            {
                if (_roomPlayerInfo.TryGetValue(player, out var info) && info.IsReady)
                {
                    count++;
                }
            }

            return count;
        }

        public bool IsAllPlayerReady()
        {
            if (Runner == null || !Runner.IsRunning)
            {
                return false;
            }

            //호스트를 제외한 모든 유저가 레디했는지 확인
            return GetReadyPlayerCount() == (Runner.ActivePlayers.Count() - 1);
        }

        /// <summary>
        /// 특정 플레이어의 Ready 상태를 조회 (없으면 false 반환).
        /// UI 쪽에서 개별 슬롯 Ready 표시용으로 사용할 수 있음.
        /// </summary>
        public bool GetPlayerReady(PlayerRef player)
        {
            if (_roomPlayerInfo.TryGetValue(player, out var info))
            {
                return info.IsReady;
            }
            return false;
        }


        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public static void RpcUpdateMapData(NetworkRunner runner, string mapTid)
        {
            var networkManager = runner.GetBehaviour<NetworkManager>();
            if (networkManager == null) return;

            networkManager.UpdateMapData(mapTid);
        }

        private void UpdateMapData(string mapTid)
        {
            _selectedMapTid = mapTid;
            OnChangeMapTidCallback?.Invoke(mapTid);
        }

        public void SyncMapData(PlayerRef newPlayer)
        {
            RpcRequestUpdateMapData(Runner, newPlayer, _selectedMapTid);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public static void RpcRequestUpdateMapData(NetworkRunner runner, [RpcTarget] PlayerRef target, string mapTid)
        {
            var networkManager = runner.GetBehaviour<NetworkManager>();
            if (networkManager == null) return;

            networkManager.UpdateMapData(mapTid);
        }

        public void UpdatePlayerName(string playerName)
        {
            //_playerName = playerName;
        }

        /// <summary>
        /// 해당 PlayerRef에 대한 방 내 이름을 갱신. RpcSyncPlayerName 수신 시 호출되며,
        /// 이후 GetPlayerName(playerRef)로 조회 가능.
        /// </summary>
        public void UpdatePlayerRoomPlayerName(PlayerRef player, string playerName)
        {
            if (!_roomPlayerInfo.TryGetValue(player, out var info))
            {
                info = new RoomPlayerInfo { PlayerName = playerName ?? "", IsReady = false };
                _roomPlayerInfo[player] = info;
            }
            else
            {
                info.PlayerName = playerName ?? "";
            }

            if (Runner != null && Runner.LocalPlayer == player)
            {
                //_playerName = playerName ?? "";
            }

            //닉네임 업데이트 이후 UI 업데이트
            GameManager.Instance.LobbyManager.RoomManager.UpdateRoomPlayers();
        }

        /// <summary>
        /// 해당 PlayerRef와 같은 유저의 PlayerName을 반환. 없으면 빈 문자열.
        /// </summary>
        public string GetPlayerName(PlayerRef player)
        {
            if (_roomPlayerInfo.TryGetValue(player, out var info))
            {
                return info.PlayerName ?? "";
            }
            return "";
        }

        /// <summary>
        /// 호출 시 룸 안에서 PlayerRef가 같은 유저를 찾아, 그 유저 로컬의 _playerName 값을 가져와 전원에게 동기화.
        /// RPC는 해당 PlayerRef를 가진 플레이어에게만 전달되며, 그쪽에서 자신의 로컬 이름을 RpcSyncPlayerName으로 브로드캐스트.
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.All)]
        public static void RpcRequestPlayerName(NetworkRunner runner, [RpcTarget] PlayerRef player)
        {
            var networkManager = runner.GetBehaviour<NetworkManager>();
            if (networkManager == null) return;

            // 이 RPC는 해당 player의 머신에서만 실행됨 → 로컬 _playerName 사용
            var localName = GameManager.Instance.NetworkManager.PlayerName;



            RpcSyncPlayerName(runner, player, localName);
        }

        /// <summary>
        /// 한 명의 플레이어 이름을 모든 클라이언트의 RoomPlayerInfo에 반영. (RpcRequestPlayerName 수신 측에서 호출)
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.All)]
        public static void RpcSyncPlayerName(NetworkRunner runner, PlayerRef player, string playerName)
        {
            var networkManager = runner.GetBehaviour<NetworkManager>();
            if (networkManager == null) return;

            Debug.Log("RpcRequestPlayerName: " + playerName);

            networkManager.UpdatePlayerRoomPlayerName(player, playerName ?? "");
        }
    }

    public class RoomPlayerInfo
    {
        public string PlayerName { get; set; }
        public bool IsReady { get; set; }
    }
}
