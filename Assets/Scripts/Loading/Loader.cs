using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AlphaTab.Importer;
using Score;
using UnityEngine;
using Graphics = Score.Graphics;

namespace Loading
{
    /// <summary>
    /// Loads scores from a file, notifies each loading step to loading listeners. 
    /// </summary>
    public sealed class Loader : MonoBehaviour
    {
        /// <summary>Loading events listeners.</summary>
        [SerializeField] private List<Listener> loadingListeners;

        /// <summary>Loading steps in order of intended progress.</summary>
        private enum LoadingStep
        {
            NoTrackLoaded,
            LoadingFromThread,
            WaitingUpdateAfterLoadingFromThread,
            LoadingFromCoroutine,
            WaitingUpdateAfterLoadingFromCoroutine,
            TrackLoaded
        }

        /// <summary>The current loading step.</summary>
        private LoadingStep _loadingStep = LoadingStep.NoTrackLoaded;

        /// <summary>Thread used to load all score data that can be loaded on a separate thread.</summary>
        private Thread _loadingDataThread;

        /// <summary>The graphics data loaded from a score.</summary>
        /// <para>Graphics data is particular in the fact that part of the loading process must be performed on the main
        /// thread, so after its initial loading it is kept in this member to handle the main thread loading part in a
        /// second moment.</para>
        private Graphics _graphics;

        /// <summary>
        /// Starts the loading process for a given score.
        /// </summary>
        /// <param name="fileName">The given score's file name.</param>
        public void Load(string fileName)
        {
            // Fire all loading start events
            foreach (var l in loadingListeners) l.OnStartLoading();

            // Load the score data from the scores folder.
            var data = FileSystem.LoadScoreData(fileName);

            // Setup the thread loading step.
            _loadingDataThread = new Thread(() =>
            {
                _loadingStep = LoadingStep.LoadingFromThread; // Indicate the current loading step.

                // Load the AlphaTab score.
                var score = ScoreLoader.LoadScoreFromBytes(data);

                // Check if the score contains a valid track.
                // TODO: generalize by moving this funcionality to a different place
                var instrumentChannel = (int)score.Tracks
                    .First(t =>
                        t.PlaybackInfo.Program is >= 32 and <= 39 &&
                        t.Staves[0].Tuning.Count == 4 &&
                        t.Staves[0].TuningName == "Bass Standard Tuning"
                    ).Index;
                var staff = score.Tracks[instrumentChannel].Staves[0];

                // Load the graphics data and metadata.
                Graphics.Load(staff, out _graphics, out var boundsLookup);

                // Load the audio data and feed it to the corresponding events.
                var audioData = Score.Audio.Load(score, instrumentChannel);
                foreach (var l in loadingListeners) l.OnLoad(audioData);

                // Load the beatmap and feed it to the corresponding events.
                var beatMap = BeatMap.Load(staff, _graphics, boundsLookup);
                foreach (var l in loadingListeners) l.OnLoad(beatMap);

                _loadingStep = LoadingStep.WaitingUpdateAfterLoadingFromThread; // Indicate the current loading step
                // From this point execution should proceed from the Update method in order to load the main thread
                // stuff.
            })
            {
                // Make it so the thread does not prevent process termination.
                IsBackground = true
            };

            // Start the actual loading process.
            _loadingDataThread.Start();
        }

        /// <summary>
        /// Iterate through all sheet segments and load them one per iteration, which corresponds to loading one sheet
        /// segment per frame.
        /// </summary>
        private IEnumerator LoadSprites()
        {
            _loadingStep = LoadingStep.LoadingFromCoroutine; // Indicate the current loading step.

            foreach (var segment in _graphics.SheetSegments)
            {
                // Load a single segment per cycle.
                segment.Load();
                yield return null;
            }

            // Feed the completely loaded graphics data the corresponding events.
            foreach (var l in loadingListeners) l.OnLoad(_graphics);

            _loadingStep = LoadingStep.WaitingUpdateAfterLoadingFromCoroutine; // Indicate the current loading step.
        }

        private void Update()
        {
            switch (_loadingStep)
            {
                case LoadingStep.WaitingUpdateAfterLoadingFromThread:
                    StartCoroutine(LoadSprites()); // Start the graphics loading coroutine.
                    break;
                case LoadingStep.WaitingUpdateAfterLoadingFromCoroutine:
                    // Fire all loading end events.
                    foreach (var l in loadingListeners) l.OnFinishLoading();
                    _loadingStep = LoadingStep.TrackLoaded; // Indicate the current loading step, this should be the
                    // end of the loading process.
                    break;
            }
        }
    }
}