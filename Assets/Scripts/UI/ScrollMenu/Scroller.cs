using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI.ScrollMenu
{
    /// <summary>
    /// Handled a scroll menu's item population and interaction logic.
    /// </summary>
    public sealed class Scroller : MonoBehaviour
    {
        [SerializeField] private Item menuItemPrefab;
        [SerializeField] private Transform itemsParent;

        /// <summary>
        /// Loads a series of items into the scroller, performing the give action when an items is selected.
        /// </summary>
        /// <param name="data">The underlying data for each menu item. When selecting an item, an item's data is passed
        /// to the selection function.</param>
        /// <param name="action">The function that handles the selected item's associated data.</param>
        public void Load(IEnumerable<ItemData> data, Action<ItemData> action)
        {
            foreach (var item in data)
            {
                var button = Instantiate(menuItemPrefab, itemsParent);
                button.Bind(item, action);
            }
        }
    }
}