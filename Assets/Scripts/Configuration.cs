#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
using Popcron;
#endif
using UnityEngine;

public sealed class Configuration : MonoBehaviour
{
    /// <summary>Amount of time in seconds to buffer the note inputs.</summary>
    public static float NoteBufferingDuration { get; private set; }

    [SerializeField] [Range(0.0f, 1.0f)] private float noteBufferingDuration  = 0.3f;
    
    /// <summary>Enable to make it so that a note is recognized as different octaves.</summary>
    public static bool NoteDistinguishOctaves { get; private set; }

    [SerializeField] private bool noteDistinguishOctaves = true;
    
    /// <summary>Represents the entire delay between playing a note and detection in Unity.</summary>
    public static float NoteDelay { get; private set; }
    
    [SerializeField] [Range(0.0f, 1.0f)] private float noteDelay = 0.1f;
    
    /// <summary>Represents the radius around a note where it can be considered played on time.</summary>
    public static float NoteDetectionRadius { get; private set; }
    
    [SerializeField] [Range(0.0f, 1.0f)] private float noteDetectionRadius = 0.4f;
    
    /// <summary>Amount of beats to display forward in time on overlays.</summary>
    public static int LookAheadBeats { get; private set; }
    
    [SerializeField] [Range(1, 16)] private int lookAheadBeats = 4;
    
    /// <summary>The rendering scale for the score graphics.</summary>
    public static float ScoreRenderScale { get; private set; }
    
    [SerializeField] [Range(1.0f, 3.0f)] private float scoreRenderScale = 1.75f;

    /// <summary>The bar count per sheet segment to render.</summary>
    /// <para>A greater value means less total segments, but it also means heavier loading costs per segment, which
    /// can lead to micro-stutter when loading the single segments.</para>
    public static int ScoreBarsPerSegment { get; private set; }
    
    [SerializeField] [Range(1, 10)] private int scoreBarsPerSegment = 3;

#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
    [SerializeField] private bool displayTrackingInformation = true;
#endif

    private void Start() => Update();
    

    private void Update()
    {
        NoteBufferingDuration = noteBufferingDuration;
        NoteDistinguishOctaves = noteDistinguishOctaves;
        NoteDelay = noteDelay;
        NoteDetectionRadius = noteDetectionRadius;
        LookAheadBeats = lookAheadBeats; 
        ScoreRenderScale = scoreRenderScale;
        ScoreBarsPerSegment = scoreBarsPerSegment;

#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        Popcron.Gizmos.Enabled = displayTrackingInformation;
#endif
    }
}