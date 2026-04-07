using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Shin
{
    public partial class GameManager
    {
        public Action<ShutdownReason> OnShutDownCallback;
        public Action<PlayerRef> OnPlayerLeftCallback;

        public void OnShutdown(ShutdownReason shutdownReason)
        {
            OnShutDownCallback?.Invoke(shutdownReason);
        }

        public void OnPlayerLeft(PlayerRef player)
        {
            OnPlayerLeftCallback?.Invoke(player);
        }
    }

}

