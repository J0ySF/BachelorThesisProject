using System.Collections.Generic;
using System.Threading.Tasks;
using AlphaTab;
using AlphaTab.Model;
using AlphaTab.Rendering;
using AlphaTab.Rendering.Utils;
using SkiaSharp;
using static Configuration;

namespace Score
{
    /// <summary> A score's graphics data. </summary>
    /// <param name="Width">The score's total sheet width.</param>
    /// <param name="StaffPosition">The score's vertical staff position.</param>
    /// <param name="StaffHeight">The score's staff height.</param>
    /// <param name="SheetSegments">The score's sheet segments.</param>
    public sealed record Graphics(int Width, int StaffPosition, int StaffHeight,
        IReadOnlyList<SheetSegment> SheetSegments)
    {
        /// <summary>Calculates a beat's horizional position from its visual bounds.</summary>
        public static int BeatPosition(Bounds visualBounds) => (int)(visualBounds.X + visualBounds.W / 2);

        /// <summary>Load the graphics data and metadata from the given score data.</summary>
        public static void Load(Staff staff, out Graphics graphics, out BoundsLookup boundsLookup)
        {
            var resources = new RenderingResources(); // Setup the rendering scale.
            resources.BarNumberFont.Size *= ScoreRenderScale;
            resources.CopyrightFont.Size *= ScoreRenderScale;
            resources.EffectFont.Size *= ScoreRenderScale;
            resources.FingeringFont.Size *= ScoreRenderScale;
            resources.FretboardNumberFont.Size *= ScoreRenderScale;
            resources.GraceFont.Size *= ScoreRenderScale;
            resources.MarkerFont.Size *= ScoreRenderScale;
            resources.SubTitleFont.Size *= ScoreRenderScale;
            resources.TablatureFont.Size *= ScoreRenderScale;
            resources.TitleFont.Size *= ScoreRenderScale;
            resources.WordsFont.Size *= ScoreRenderScale;

            var settings = new Settings // Setup the rendering configuration.
            {
                Core =
                {
                    Engine = "skia",
                    EnableLazyLoading = false
                },
                Display =
                {
                    LayoutMode = LayoutMode.Horizontal,
                    StaveProfile = StaveProfile.Tab,
                    Scale = ScoreRenderScale,
                    Padding = new[] { 0.0, 0.0, 0.0, 0.0 },
                    Resources = resources,
                    BarCountPerPartial = ScoreBarsPerSegment,
                },
                Notation =
                {
                    RhythmMode = TabRhythmMode.ShowWithBars
                }
            };

            var renderer = new ScoreRenderer(settings) // Create a new score renderer.
            {
                Width = 1 // This is a workaround around a weird AlphaTab quirk: horizontal mode rendering should not
                // need an horizontal width value, since according to the documentation this value is only used in the 
                // page layout, but leaving this value at the default value (0) leads to no segment rendering at all.
            };

            var segments = new List<SheetSegment>(); // Create a segment list where newly create segments will be added.

            // Signal that we want to render all partial layouts. 
            renderer.PartialLayoutFinished.On(result => renderer.RenderResult(result.Id));

            // Filter through all segments that do not refer to a the staff part of the score and create the sheet
            // segments.
            renderer.PartialRenderFinished.On(result =>
            {
                // This filters out all the non-staff parts, like titles and watermarks.
                if (result.FirstMasterBarIndex >= 0) segments.Add(new SheetSegment((SKImage)result.RenderResult!));
            });
            
            var completionSignal = new TaskCompletionSource<bool>(); // Set up the async operation awaiting signal.
            
            var width = 0; // The render's total width.
            renderer.RenderFinished.On(r =>
            {
                width = (int)r.TotalWidth;
                completionSignal.SetResult(true); // Indicate that the rendering is complete.
            });
            
            renderer.RenderTracks(new[] { staff.Track }); // Render the segmentes 
            completionSignal.Task.Wait(); // and wait for the rendering to complete.

            // Compute the staff vertical position and height.
            boundsLookup = renderer.BoundsLookup!;
            var beatBounds = boundsLookup.StaveGroups[0].Bars[0].Bars[0].Beats[0].RealBounds;
            var staffPosition = (int)beatBounds.Y;
            var staffHeight = (int)beatBounds.H;

            graphics = new Graphics(width, staffPosition, staffHeight, segments.ToArray());
        }
    }
}