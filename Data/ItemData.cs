using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    [Serializable]
    public class ItemData
    {
        [SerializeField] private string _itemId;
        public string ItemId
        {
            get { return _itemId; }
            set { _itemId = value; }
        }

        [SerializeField] private string _itemName;
        public string ItemName
        {
            get { return _itemName; }
        }

        [SerializeField] private int _haveMaxCount;
        public int HaveMaxCount
        {
            get { return _haveMaxCount; }
        }

        public ItemData(string itemId, string itemName, int haveMaxCount)
        {
            _itemId = itemId;
            _itemName = itemName;
            _haveMaxCount = haveMaxCount;
        }
    }

    /// <summary>
    /// 인게임에서 사용하는 아이템 인포 데이터
    /// </summary>
    public class ItemInfo
    {
        private string _itemId;

        public string ItemId
        {
            get { return _itemId; }
        }

        private int _haveCount;
        public int HaveCount
        {
            get { return _haveCount; }
        }

        public ItemInfo(string itemId, int haveCount)
        {
            _itemId = itemId;
            _haveCount = haveCount;
        }

        public void AddItemCount(int count)
        {
            _haveCount += count;
        }

        public void RemoveItemCount(int count)
        {
            _haveCount -= count;
        }

        public void SetItemCount(int count)
        {
            _haveCount = count;
        }
    }
}
