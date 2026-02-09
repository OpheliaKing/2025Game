using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    [CreateAssetMenu(fileName = "NewItemData", menuName = "Shin/Item Data", order = 0)]
    public class ItemDataSO : ScriptableObject
    {
        [SerializeField] private List<ItemData> _items = new List<ItemData>();

        public IReadOnlyList<ItemData> Items => _items;

        /// <summary>
        /// itemId에 해당하는 항목을 복사한 ItemData를 반환합니다.
        /// </summary>
        public ItemData CreateItemDataById(string itemId)
        {
            foreach (var src in _items)
            {
                if (src.ItemId == itemId)
                {
                    return new ItemData(src.ItemId, src.ItemName, src.HaveMaxCount);
                }
            }
            return null;
        }
    }
}
