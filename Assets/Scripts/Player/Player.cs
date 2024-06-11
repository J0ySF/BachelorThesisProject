using System.Collections.Generic;
using Audio;
using Player.Components;
using Player.Components.Panels;
using Player.Components.Settings;
using Score;
using UI.Fretboard;
using UnityEngine;
using static UI.ViewController;
using Graphics = Score.Graphics;

namespace Player
{
    /// <summary>
    /// Class that sets up a player from scene components, handles loading scores to the components and fires polling
    /// events to components.
    /// </summary>
    public sealed class Player : Loading.Listener
    {
        [SerializeField] private SynthesizerOutput synthesizerOutput;
        [SerializeField] private MidiInput midiInput;

        [Header("Sheet")] [SerializeField] private UI.SheetScroller sheetScroller;

        [Header("Fretboard")] [SerializeField] private Overlay overlay;

        [Header("UI Panels")] [SerializeField] private UI.Panel mainPanel;

        [SerializeField] private UI.Panel scoresPanel;
        [SerializeField] private UI.Panel settingsPanel;

        [Header("UI Settings")] [SerializeField] private UI.Toggle panelsToggle;

        [SerializeField] private UI.Toggle playPauseToggle;
        [SerializeField] private UI.Toggle soloToggle;
        [SerializeField] private UI.Toggle muteToggle;
        [SerializeField] private UI.Toggle metronomeToggle;
        [SerializeField] private UI.Toggle countInToggle;
        [SerializeField] private UI.Toggle autoPauseToggle;
        [SerializeField] private UI.Slider speedSlider;

        /// <summary>Shared state between all components.</summary>
        private State _state;

        /// <summary>All components sharing the same player's state.</summary>
        private List<State.Component> _components;

        private void Start()
        {
            // Instantiate shared state and all player components.

            _components = new List<State.Component>();
            _state = new State(_components);

            RegisterComponent(new MainPanel(_state, mainPanel));
            RegisterComponent(new SettingsPanel(_state, settingsPanel));
            Bind(panelsToggle, RegisterComponent(new ScoresPanel(_state, scoresPanel)));

            Bind(playPauseToggle, RegisterComponent(new PlayPause(_state)));
            Bind(soloToggle, RegisterComponent(new Solo(_state)));
            Bind(muteToggle, RegisterComponent(new Mute(_state)));
            Bind(metronomeToggle, RegisterComponent(new Metronome(_state)));
            Bind(countInToggle, RegisterComponent(new CountIn(_state)));
            Bind(autoPauseToggle, RegisterComponent(new AutoPause(_state)));
            Bind(speedSlider, RegisterComponent(new Speed(_state)));

            Bind(sheetScroller, RegisterComponent(new Sheet(_state)));
            RegisterComponent(new Synthesizer(_state, synthesizerOutput));
            RegisterComponent(new PerformanceListener(_state, midiInput));
            RegisterComponent(new Fretboard(_state, overlay));

            return;

            // Utility function to make setup more coincise.
            T RegisterComponent<T>(T component) where T : State.Component
            {
                _components.Add(component);
                return component;
            }
        }

        private void Update()
        {
            // Fire polling events
            foreach (var c in _components) c.Poll();
        }

#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        private void OnGUI()
        {
            // Fire OnGUI events
            foreach (var c in _components) c.OnGUI();
        }
#endif

        public override void OnStartLoading()
        {
            foreach (var c in _components) c.OnStartLoading();
        }

        public override void OnLoad(BeatMap v)
        {
            foreach (var c in _components) c.OnLoad(v);
        }

        public override void OnLoad(Score.Audio v)
        {
            foreach (var c in _components) c.OnLoad(v);
        }

        public override void OnLoad(Graphics v)
        {
            foreach (var c in _components) c.OnLoad(v);
        }

        public override void OnFinishLoading()
        {
            _state.ResetPlayback();
            foreach (var c in _components) c.OnEndLoading();
        }
    }
}