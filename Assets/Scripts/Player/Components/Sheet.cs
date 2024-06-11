using System;
using System.Linq;
using Score;
using UI.Sheet;

namespace Player.Components
{
    /// <summary>
    /// Mediates between a player's playback of a music sheet and the visualization/interaction events with an UI view.
    /// </summary>
    public sealed class Sheet : State.Component, IController
    {
        public IView View { get; set; }

        /// <summary>Used to convert between sheet positions and ticks.</summary>
        private BeatMap _beatMap;

        public int Position
        {
            get => _beatMap.TickToSheetPosition(Math.Max(State.Tick, 0));
            private set => SetTick(_beatMap.SheetPositionToTick(value)); // Convert from sheet position to tick.
        }

        public void StartScroll(int position)
        {
            // Start seeking and set position.
            Position = position;
            SetPlayback(Playback.Seeking);
        }

        public void ScrollUpdate(int position) => Position = position; // Update position while seeking.

        public void EndScroll(int position)
        {
            // Snap the position to the closest beat before or at the seeked position.
            var snappedPosition = _beatMap.LastBeatBeforeOrAtSheetPosition(position).SheetPosition;
            View.OnPositionChange(snappedPosition);

            // Pause and set position
            Position = snappedPosition;
            SetPlayback(Playback.Paused);
        }

        public Sheet(State state) : base(state)
        {
        }

        /// <summary>Stored so that whem both graphics and beatmap have been loaded then the view can be set up.</summary>
        private Graphics _graphics;

        /// <summary>Auxiliary method to check for all required resources before loading.</summary>
        private void OnLoad(Graphics graphics, BeatMap beatMap)
        {
            // Check if both graphics and beatmap have been loaded.
            if (graphics == null || beatMap == null) return;

            // Use the first and last beat positions as scrolling bounds.
            var firstBeatPosition = beatMap.Beats[0].SheetPosition;
            var lastBeatPosition = beatMap.Beats.Last().SheetPosition; //TODO: check if sheet.Width is ok
            View.Load(graphics, firstBeatPosition, lastBeatPosition);
        }

        public override void OnStartLoading()
        {
            _graphics = null;
            _beatMap = null;
        }

        public override void OnLoad(BeatMap beatMap) => OnLoad(_graphics, _beatMap = beatMap);

        public override void OnLoad(Graphics graphics) => OnLoad(_graphics = graphics, _beatMap);

        protected override void OnTickUpdate(int v) => View.OnPositionChange(Position);

        protected override void OnPlaybackUpdate(Playback v) =>
            View.Interactable = v is not Playback.CountingIn;
    }
}