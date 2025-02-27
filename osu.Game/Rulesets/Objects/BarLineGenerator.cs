// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Objects
{
    public class BarLineGenerator<TBarLine>
        where TBarLine : class, IBarLine, new()
    {
        /// <summary>
        /// The generated bar lines.
        /// </summary>
        public readonly List<TBarLine> BarLines = new List<TBarLine>();

        /// <summary>
        /// Constructs and generates bar lines for provided beatmap.
        /// </summary>
        /// <param name="beatmap">The beatmap to generate bar lines for.</param>
        public BarLineGenerator(IBeatmap beatmap)
        {
            if (beatmap.HitObjects.Count == 0)
                return;

            HitObject lastObject = beatmap.HitObjects.Last();
            double lastHitTime = 1 + lastObject.GetEndTime();

            var timingPoints = beatmap.ControlPointInfo.TimingPoints;

            if (timingPoints.Count == 0)
                return;

            for (int i = 0; i < timingPoints.Count; i++)
            {
                TimingControlPoint currentTimingPoint = timingPoints[i];
                EffectControlPoint currentEffectPoint = beatmap.ControlPointInfo.EffectPointAt(currentTimingPoint.Time);
                int currentBeat = 0;

                // Stop on the next timing point, or if there is no next timing point stop slightly past the last object
                double endTime = i < timingPoints.Count - 1 ? timingPoints[i + 1].Time : lastHitTime + currentTimingPoint.BeatLength * currentTimingPoint.TimeSignature.Numerator;

                double startTime = currentTimingPoint.Time;
                double barLength = currentTimingPoint.BeatLength * currentTimingPoint.TimeSignature.Numerator;

                if (currentEffectPoint.OmitFirstBarLine)
                {
                    startTime += barLength;
                }

                for (double t = startTime; Precision.AlmostBigger(endTime, t); t += barLength, currentBeat++)
                {
                    double roundedTime = Math.Round(t, MidpointRounding.AwayFromZero);

                    // in the case of some bar lengths, rounding errors can cause t to be slightly less than
                    // the expected whole number value due to floating point inaccuracies.
                    // if this is the case, apply rounding.
                    if (Precision.AlmostEquals(t, roundedTime))
                    {
                        t = roundedTime;
                    }

                    BarLines.Add(new TBarLine
                    {
                        StartTime = t,
                        Major = currentBeat % currentTimingPoint.TimeSignature.Numerator == 0
                    });
                }
            }
        }
    }
}
