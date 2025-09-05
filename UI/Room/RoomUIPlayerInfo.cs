using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomUIPlayerInfo : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _playerName;

    public void UpdateInfo(string nickName)
    {
        gameObject.SetActive(true);
        _playerName.SetText(nickName);
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
