using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace Shin
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public void GameStart()
        {
            Action startScene = () =>
            {
                if (photonView != null)
                {
                    photonView.RPC("InGameLoad", RpcTarget.All);
                }
                else
                {
                    Debug.Log("photonView is null");
                }
            };

            GameManager.Instance.SceneController.LoadScene("InGameScene", startScene);
        }

        [PunRPC]
        private void InGameLoad()
        {
            GameManager.Instance.UImanager.Clear();
            GameManager.Instance.StartCoroutine(TestCo());
        }

        private IEnumerator TestCo()
        {
            yield return new WaitForSeconds(1f);
            InGameManager.Instance.StartGame(null);
        }
    }
}

