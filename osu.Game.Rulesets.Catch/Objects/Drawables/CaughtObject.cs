// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    /// <summary>
    /// Represents a <see cref="PalpableCatchHitObject"/> caught by the catcher.
    /// </summary>
    [Cached(typeof(IHasCatchObjectState))]
    public abstract class CaughtObject : SkinnableDrawable, IHasCatchObjectState
    {
        public PalpableCatchHitObject HitObject { get; private set; }
        public Bindable<Color4> AccentColour { get; } = new Bindable<Color4>();
        public Bindable<bool> HyperDash { get; } = new Bindable<bool>();
        public Bindable<int> IndexInBeatmap { get; } = new Bindable<int>();

        public Vector2 DisplaySize => Size * Scale;

        public float DisplayRotation => Rotation;

        public double DisplayStartTime => HitObject.StartTime;

        /// <summary>
        /// Whether this hit object should stay on the catcher plate when the object is caught by the catcher.
        /// </summary>
        public virtual bool StaysOnPlate => true;

        public override bool RemoveWhenNotAlive => true;

        protected CaughtObject(CatchSkinComponents skinComponent, Func<ISkinComponentLookup, Drawable> defaultImplementation)
            : base(new CatchSkinComponentLookup(skinComponent), defaultImplementation)
        {
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.None;
            Size = new Vector2(CatchHitObject.OBJECT_RADIUS * 2);
        }

        /// <summary>
        /// Copies the hit object visual state from another <see cref="IHasCatchObjectState"/> object.
        /// </summary>
        public virtual void CopyStateFrom(IHasCatchObjectState objectState)
        {
            HitObject = objectState.HitObject;
            Scale = Vector2.Divide(objectState.DisplaySize, Size);
            Rotation = objectState.DisplayRotation;
            AccentColour.Value = objectState.AccentColour.Value;
            HyperDash.Value = objectState.HyperDash.Value;
            IndexInBeatmap.Value = objectState.IndexInBeatmap.Value;
        }

        protected override void FreeAfterUse()
        {
            ClearTransforms();
            Alpha = 1;

            base.FreeAfterUse();
        }
    }
}
