using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

namespace Shin
{
    public class RoomManager : MonoBehaviour
    {
        [Header("Events")]
        public UnityEvent<List<string>> onPlayersUpdated;

        private readonly List<string> _currentPlayerNicknames = new List<string>();

        public IReadOnlyList<string> CurrentPlayerNicknames => _currentPlayerNicknames;

        [Header("Player INfo UI")]

        [SerializeField]
        private Transform _playerInfoUIParent;
        private List<RoomUIPlayerInfo> _playerInfoUIList = new List<RoomUIPlayerInfo>();

        // LobbyManager에서 호출: 현재 방의 플레이어 배열을 받아 UI/로직 갱신
        public void UpdateRoomPlayers(Photon.Realtime.Player[] players)
        {
            if (!gameObject.activeSelf)
            {
                SetActive(true);
            }

            _currentPlayerNicknames.Clear();
            if (players != null)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    var p = players[i];
                    string nick = string.IsNullOrEmpty(p.NickName) ? $"Player_{p.ActorNumber}" : p.NickName;
                    _currentPlayerNicknames.Add(nick);
                }
            }
            onPlayersUpdated?.Invoke(new List<string>(_currentPlayerNicknames));

            UpdatePlayerUI();
        }

        private void InitPlayerInfoUI()
        {
            for (int i = 0; i < _playerInfoUIList.Count; i++)
            {
                _playerInfoUIList[i].SetActive(false);
            }
        }

        private void CreatePlayerInfoUI(int createCount)
        {
            for (int i = 0; i < createCount; i++)
            {
                var reManager = GameManager.Instance.ResourceManager;
                var uiInstance = reManager.InstantiatePrefab<RoomUIPlayerInfo>("RoomUIPlayerInfo", _playerInfoUIParent, $"{reManager.UIPrefabPath}/Room");
                _playerInfoUIList.Add(uiInstance);
            }
        }

        public void UpdatePlayerUI()
        {
            InitPlayerInfoUI();

            var createCount = CurrentPlayerNicknames.Count - _playerInfoUIList.Count;

            Debug.Log($"Create Count {createCount}");

            if (createCount > 0)
            {
                CreatePlayerInfoUI(createCount);
            }

            for (int i = 0; i < CurrentPlayerNicknames.Count; i++)
            {
                _playerInfoUIList[i].UpdateInfo(CurrentPlayerNicknames[i]);
            }
        }

        // 필요 시 수동 갱신: Photon 상태에서 직접 읽어 반영
        public void RefreshFromPhoton()
        {
            if (!PhotonNetwork.InRoom)
            {
                UpdateRoomPlayers(null);
                return;
            }
            UpdateRoomPlayers(PhotonNetwork.PlayerList);
        }

        public void GameStart()
        {
            GameManager.Instance.LobbyManager.GameStart();
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}
