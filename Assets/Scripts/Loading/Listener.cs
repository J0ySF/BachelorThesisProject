using Score;
using UnityEngine;
using Graphics = Score.Graphics;

namespace Loading
{
    /// <summary>
    /// Abstract class to inherit to listen to loading events.
    /// </summary>
    public abstract class Listener : MonoBehaviour
    {
        /// <summary>Called when the loading process is about to start.</summary>
        public virtual void OnStartLoading()
        {
        }

        /// <summary>Called when the loading process yields the score's beatmap.</summary>
        public virtual void OnLoad(BeatMap _)
        {
        }

        /// <summary>Called when the loading process yields the score's audio.</summary>
        public virtual void OnLoad(Score.Audio _)
        {
        }

        /// <summary>Called when the loading process yields the score's graphics.</summary>
        public virtual void OnLoad(Graphics _)
        {
        }

        /// <summary>Called when the loading process is done and all resources have been loaded.</summary>
        public virtual void OnFinishLoading()
        {
        }
    }
}