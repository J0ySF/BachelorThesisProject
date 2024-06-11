using System.Linq;
using UI.ScrollMenu;
using UnityEngine;

namespace Loading.Menu
{
    /// <summary>
    /// Handles the score selection via a scrolling menu.
    /// </summary>
    public sealed class ScoreSelector : MonoBehaviour
    {
        [SerializeField] private Scroller scroller;

        [SerializeField] private Loader loader;

        private void Start()
        {
            scroller.Load(FileSystem.LoadScoreNames().Select(t => new ItemData(t)).ToList(),
                item => Load(((ItemData)item).FileName));
        }

        private void Load(string fileName) => loader.Load(fileName);
    }
}