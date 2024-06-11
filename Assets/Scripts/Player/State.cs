#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Score;
using UnityEngine;
using Graphics = Score.Graphics;

namespace Player
{
    /// <summary>
    /// Represents a <c>Player</c>'s internal shared state, which is referenced and modified by player <c>Component</c>s,
    /// this way state is all stored and accessed in an uniform manner across all <c>Component</c>s and update callbacks
    /// can be provided to notify changes coming from other components.
    /// </summary>
    public sealed class State
    {
        /// <summary>All components sharing this state.</summary>
        private readonly List<Component> _components;

        public State(List<Component> components) => _components = components;

        /// <summary>
        /// Relays an update event to all components but the updater.
        /// </summary>
        /// <param name="updater">The component that updated the value.</param>
        /// <param name="operation">A function which applies the event call to a specific <c>Component</c> event
        /// with its new value.</param>
        private void NotifyUpdate(Component? updater, Action<Component> operation)
        {
            // Call the event on every component that is not the updater
            foreach (var component in _components.Where(component => component != updater)) operation(component);
        }

        /// <summary>A player's current playback state.</summary>
        public Playback Playback { get; private set; }

        /// <summary>A player's current playback position.</summary>
        public int Tick { get; private set; }

        /// <summary>Whether the player is playing muted or not.</summary>
        public bool Solo { get; private set; }

        /// <summary>Whether the player is playing solo or not.</summary>
        public bool Mute { get; private set; }

        /// <summary>Whether the player's metronome sound is enabled.</summary>
        public bool Metronome { get; private set; } = true;
        
        /// <summary>Whether the player's count-in setting is enabled.</summary>
        public bool CountIn { get; private set; } = true;
        
        /// <summary>Whether the player's auto pause setting is enabled.</summary>
        public bool AutoPause { get; private set; } = true;

        /// <summary>A player's playback speed expressed in the 0 to 1 range (extremes included).</summary>
        /// <remarks>The value gets clamped between 0.1 and 1 when set.</remarks>
        public float Speed { get; private set; } = 1.0f;

        /// <summary> Utility class used to reset state when loading a beatmap.</summary>
        private sealed class ResetComponent : Component
        {
            public ResetComponent(State state) : base(state)
            {
            }

            /// <summary> Resets all playback related state.</summary>
            public void Reset()
            {
                SetPlayback(Playback.Paused);
                SetTick(0);
            }
        }

        /// <summary>Sets the state properties to the appropriate value they need to have at the start of playback.</summary>
        /// <para>If given a beatmap it will be set as the new beatmap reference for all operations involving it.</para>
        public void ResetPlayback()
        {
            new ResetComponent(this).Reset();
        }

        /// <summary>Class which accesses and modifies the shared state via controlled means.</summary>
        public abstract class Component
        {
            /// <summary>This istance's shared <c>State</c></summary>
            protected State State { get; }

            protected Component(State state) => State = state;

            /// <summary>Sets the shared playback state.</summary>
            /// <para>The updated value is relaid via events to all <c>Component</c>s except for the caller.</para>
            /// <para>The associated event is fired only if the previous and current value differ.</para>
            protected void SetPlayback(Playback v)
            {
                if (v == State.Playback) return;
                
                State.Playback = v;
                State.NotifyUpdate(this, c => c.OnPlaybackUpdate(v));
            }

            /// <summary>Sets the shared tick state.</summary>
            /// <para>The updated value is relaid via events to all <c>Component</c>s except for the caller.</para>
            /// <para>The associated event is fired only if the previous and current value differ.</para>
            protected void SetTick(int v)
            {
                if (v == State.Tick) return;
                
                State.Tick = v;
                State.NotifyUpdate(this, c => c.OnTickUpdate(v));
            }

            /// <summary>Sets the shared solo state.</summary>
            /// <para>The updated value is relaid via events to all <c>Component</c>s except for the caller.</para>
            /// <para>The associated event is fired only if the previous and current value differ.</para>
            protected void SetSolo(bool v)
            {
                if (v == State.Solo) return;
                
                State.Solo = v;
                State.NotifyUpdate(this, c => c.OnSoloUpdate(v));
            }

            /// <summary>Sets the shared mute state.</summary>
            /// <para>The updated value is relaid via events to all <c>Component</c>s except for the caller.</para>
            /// <para>The associated event is fired only if the previous and current value differ.</para>
            protected void SetMute(bool v)
            {
                if (v == State.Mute) return;
                
                State.Mute = v;
                State.NotifyUpdate(this, c => c.OnMuteUpdate(v));
            }

            /// <summary>Sets the shared metronome state.</summary>
            /// <para>The updated value is relaid via events to all <c>Component</c>s except for the caller.</para>
            /// <para>The associated event is fired only if the previous and current value differ.</para>
            protected void SetMetronome(bool v)
            {
                if (v == State.Metronome) return;
                
                State.Metronome = v;
                State.NotifyUpdate(this, c => c.OnMetronomeUpdate(v));
            }
            
            /// <summary>Sets the shared count in setting state.</summary>
            /// <para>The updated value is relaid via events to all <c>Component</c>s except for the caller.</para>
            /// <para>The associated event is fired only if the previous and current value differ.</para>
            protected void SetCountIn(bool v)
            {
                if (v == State.CountIn) return;
                
                State.CountIn = v;
                State.NotifyUpdate(this, c => c.OnCountInUpdate(v));
            }
            
            /// <summary>Sets the shared auto pause setting state.</summary>
            /// <para>The updated value is relaid via events to all <c>Component</c>s except for the caller.</para>
            /// <para>The associated event is fired only if the previous and current value differ.</para>
            protected void SetAutoPause(bool v)
            {
                if (v == State.AutoPause) return;
                
                State.AutoPause = v;
                State.NotifyUpdate(this, c => c.OnAutoPauseUpdate(v));
            }

            /// <summary>Sets the shared speed state.</summary>
            /// <para>The updated value is relaid via events to all <c>Component</c>s except for the caller.</para>
            /// <para>The associated event is fired only if the previous and current value differ.</para>
            protected void SetSpeed(float v)
            {
                // Comparison taking into account floating point rounding
                if (Math.Abs(v - State.Speed) < 0.001f) return;
                
                State.Speed = Math.Clamp(v, 0.1f, 1.0f);
                State.NotifyUpdate(this, c => c.OnSpeedUpdate(v));
            }

            /// <summary>Called when the player is starting to load a score.</summary>
            public virtual void OnStartLoading()
            {
            }

            /// <summary>Called when the loading process yields the score's beatmap.</summary>
            public virtual void OnLoad(BeatMap _)
            {
            }

            //// <summary>Called when the loading process yields the score's audio data.</summary>
            public virtual void OnLoad(Score.Audio _)
            {
            }

            /// <summary>Called when the loading process yields the score's graphics data.</summary>
            public virtual void OnLoad(Graphics _)
            {
            }

            /// <summary>Called when the score loading process is completed.</summary>
            public virtual void OnEndLoading()
            {
            }

            /// <summary>Called when the state's playback property is updated.</summary>
            /// <para>This event is only fired when another instance updates the value.</para>
            protected virtual void OnPlaybackUpdate(Playback _)
            {
            }

            /// <summary>Called when the state's tick is updated.</summary>
            /// <para>This event is only fired when another instance updates the value(s).</para>
            protected virtual void OnTickUpdate(int _)
            {
            }

            /// <summary>Called when the state's solo property is updated.</summary>
            /// <para>This event is only fired when another instance updates the value.</para>
            protected virtual void OnSoloUpdate(bool _)
            {
            }

            /// <summary>Called when the state's mute property is updated.</summary>
            /// <para>This event is only fired when another instance updates the value.</para>
            protected virtual void OnMuteUpdate(bool _)
            {
            }

            /// <summary>Called when the state's metronome property is updated.</summary>
            /// <para>This event is only fired when another instance updates the value.</para>
            protected virtual void OnMetronomeUpdate(bool _)
            {
            }
            
            /// <summary>Called when the state's count in setting property is updated.</summary>
            /// <para>This event is only fired when another instance updates the value.</para>
            protected virtual void OnCountInUpdate(bool _)
            {
            }
            
            /// <summary>Called when the state's auto pause setting property is updated.</summary>
            /// <para>This event is only fired when another instance updates the value.</para>
            protected virtual void OnAutoPauseUpdate(bool _)
            {
            }

            /// <summary>Called when the state's speed property is updated.</summary>
            /// <para>This event is only fired when another instance updates the value.</para>
            protected virtual void OnSpeedUpdate(float _)
            {
            }

            /// <summary>Called at (semi)regular time intervals. Useful for components that need to update constantly
            /// outside of state change events.</summary>
            public virtual void Poll()
            {
            }

#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
            /// <summary>Corresponds to the Unity OnGUI method.</summary>
            public virtual void OnGUI()
            {
            }
#endif
        }
    }
}