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
            InGameManager.Instance.GameClearInput();
        }
    }
}
