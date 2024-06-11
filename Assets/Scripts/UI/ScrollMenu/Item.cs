using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ScrollMenu
{
    /// <summary>
    /// A scrolling menu's item.
    /// </summary>
    public sealed class Item : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private Button button;

        /// <summary>
        /// Sets the item's associated data and action.
        /// </summary>
        /// <param name="data">The underlying data for this item. When selecting this item, this data is passed
        /// to the selection function.</param>
        /// <param name="action">The function to run on selection.</param>
        public void Bind(ItemData data, Action<ItemData> action)
        {
            label.text = data.Label;
            button.onClick.AddListener(() => action(data));
        }
    }
}