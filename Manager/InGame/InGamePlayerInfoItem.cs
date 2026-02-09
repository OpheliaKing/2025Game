using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Shin
{
    public partial class InGamePlayerInfo
    {
        private ItemDataSO _itemSO;
        private ItemDataSO ItemSO
        {
            get
            {
                if (_itemSO == null)
                {
                    LoadSO();
                }
                return _itemSO;
            }
        }

        /// <summary>
        /// 플레이어가 소지하고 있는 아이템 데이터
        /// 해당 값은 호스트만 관리하고 있으며, 클라이언트는 호스트의 값을 받아서 사용
        /// </summary>
        private Dictionary<string, ItemInfo> _itemInfoList = new Dictionary<string, ItemInfo>();
        public Dictionary<string, ItemInfo> ItemInfoList
        {
            get { return _itemInfoList; }
        }

        private Action<string, int> _onGetItemCountCallback;

        private void LoadSO()
        {
            var resourceManager = GameManager.Instance.ResourceManager;
            var so = resourceManager.LoadSO<ItemDataSO>("ItemData", resourceManager.SOPath);
            if (so == null)
            {
                Debug.Log("Not Found Stage Data SO");
            }
            _itemSO = so;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RpcAddItemCount(string itemId, int count, RpcInfo info = default)
        {
            if (_itemInfoList.ContainsKey(itemId))
            {
                _itemInfoList[itemId].AddItemCount(count);
            }
            else
            {
                _itemInfoList.Add(itemId, new ItemInfo(itemId, count));
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RpcRemoveItemCount(string itemId, int count, RpcInfo info = default)
        {
            if (_itemInfoList.ContainsKey(itemId))
            {
                _itemInfoList[itemId].RemoveItemCount(count);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        public void RpcGetItemCountRequest(string itemId, RpcInfo info = default)
        {
            if (_itemInfoList.ContainsKey(itemId))
            {
                var count = _itemInfoList[itemId].HaveCount;
                RpcGetItemCount(itemId, count, info);
            }
            else
            {
                RpcGetItemCount(itemId, 0, info);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All,HostMode = RpcHostMode.SourceIsHostPlayer)]
        public void RpcGetItemCount(string itemId, int count, RpcInfo info = default)
        {
            //요청한 플레이어
            if (info.Source == Runner.LocalPlayer)
            {
                _onGetItemCountCallback?.Invoke(itemId, count);
            }
        }

        public void RequestItemCount(string itemId, Action<string,int> onGetItemCountCallback)
        {
            _onGetItemCountCallback = onGetItemCountCallback;
            RpcGetItemCountRequest(itemId);
        }
    }
}

