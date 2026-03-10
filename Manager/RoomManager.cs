using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Shin
{
    public class RoomManager : MonoBehaviour
    {
        [Header("Events")]
        public UnityEvent<List<PlayerRef>> onPlayersUpdated;

        [Header("Player INfo UI")]

        [SerializeField]
        private Transform _playerInfoUIParent;
        private List<RoomUIPlayerInfo> _playerInfoUIList = new List<RoomUIPlayerInfo>();


        [SerializeField]
        private TextMeshProUGUI _textMeshProUGUI;

        // Fusion 호환: 닉네임 컬렉션을 받아 UI/로직 갱신
        public void UpdateRoomPlayers(IEnumerable<PlayerRef> playerNicknames)
        {
            if (!gameObject.activeSelf)
            {
                SetActive(true);
            }

            // _currentPlayerNicknames.Clear();
            // if (playerNicknames != null)
            // {
            //     _currentPlayerNicknames.AddRange(playerNicknames);
            // }
            //onPlayersUpdated?.Invoke(new List<PlayerRef>(_currentPlayerNicknames));

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

            var curRoomPlayerInfo = GameManager.Instance.NetworkManager.RoomPlayerInfo;
            var createCount = curRoomPlayerInfo.Count - _playerInfoUIList.Count;

            Debug.Log($"Create Count {createCount}");

            if (createCount > 0)
            {
                CreatePlayerInfoUI(createCount);
            }

            var index = 0;

            foreach (var player in curRoomPlayerInfo)
            {
                _playerInfoUIList[index].UpdateInfo(player.Key);
                index++;
            }
        }

        public void GameStart()
        {
            switch (GameManager.Instance.NetworkManager.Runner.GameMode)
            {
                case GameMode.Host:
                    break;
                case GameMode.Client:
                    var nm = GameManager.Instance.NetworkManager;
                    var localPlayer = nm.Runner.LocalPlayer;
                    var currentReady = nm.RoomPlayerInfo.TryGetValue(localPlayer, out var roomInfo) && roomInfo.IsReady;
                    NetworkManager.RpcRoomReady(nm.Runner, currentReady);
                    break;
            }

            // Debug.Log("Game Start");
            // var runner = GameManager.Instance.NetworkManager.Runner;
            // if (runner != null && runner.IsRunning)
            // {
            //     NetworkManager.RpcGameStart(runner);
            // }
            // else
            // {
            //     Debug.LogError("NetworkRunner가 실행 중이 아닙니다.");
            // }
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}
