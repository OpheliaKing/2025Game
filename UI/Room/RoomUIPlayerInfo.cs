using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

namespace Shin
{
    public class RoomUIPlayerInfo : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _playerName;

        [SerializeField]
        private TextMeshProUGUI _readyText;

        public void UpdateInfo(PlayerRef player)
        {
            gameObject.SetActive(true);
            var playerName = GameManager.Instance.NetworkManager.GetPlayerName(player);
            _playerName.SetText(playerName);
            var readyText = (GameManager.Instance.NetworkManager.RoomPlayerInfo.TryGetValue(player, out var info) && info.IsReady) ? "Ready" : "";
            _readyText.SetText(readyText);
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}