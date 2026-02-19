using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public class GameClearUI : UIBase
    {
        override protected void OnConfirmImpl()
        {
            base.OnConfirmImpl();

            if (InGameManager.Instance.PlayerInfo.Object.HasStateAuthority)
            {
                InGameManager.Instance.PlayerInfo.RpcGameClearInputCheck();
            }
        }
    }
}
