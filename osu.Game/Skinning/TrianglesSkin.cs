﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Beatmaps.Formats;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class TrianglesSkin : Skin
    {
        public static SkinInfo CreateInfo() => new SkinInfo
        {
            ID = osu.Game.Skinning.SkinInfo.TRIANGLES_SKIN,
            Name = "osu! \"triangles\" (2017)",
            Creator = "team osu!",
            Protected = true,
            InstantiationInfo = typeof(TrianglesSkin).GetInvariantInstantiationInfo()
        };

        private readonly IStorageResourceProvider resources;

        public TrianglesSkin(IStorageResourceProvider resources)
            : this(CreateInfo(), resources)
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public TrianglesSkin(SkinInfo skin, IStorageResourceProvider resources)
            : base(skin, resources)
        {
            this.resources = resources;
        }

        public override Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => Textures?.Get(componentName, wrapModeS, wrapModeT);

        public override ISample? GetSample(ISampleInfo sampleInfo)
        {
            foreach (string lookup in sampleInfo.LookupNames)
            {
                var sample = Samples?.Get(lookup) ?? resources.AudioManager?.Samples.Get(lookup);
                if (sample != null)
                    return sample;
            }

            return null;
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            if (base.GetDrawableComponent(lookup) is Drawable c)
                return c;

            switch (lookup)
            {
                case SkinnableSprite.SpriteComponentLookup spriteLookup:
                    switch (spriteLookup.LookupName)
                    {
                        // Temporary until default skin has a valid hit lighting.
                        case @"lighting":
                            return Drawable.Empty();
                    }

                    break;

                case GlobalSkinComponentLookup target:
                    switch (target.Lookup)
                    {
                        case GlobalSkinComponentLookup.LookupType.SongSelect:
                            var songSelectComponents = new SkinnableTargetComponentsContainer(_ =>
                            {
                                // do stuff when we need to.
                            });

                            return songSelectComponents;

                        case GlobalSkinComponentLookup.LookupType.MainHUDComponents:
                            var skinnableTargetWrapper = new SkinnableTargetComponentsContainer(container =>
                            {
                                var score = container.OfType<DefaultScoreCounter>().FirstOrDefault();
                                var accuracy = container.OfType<DefaultAccuracyCounter>().FirstOrDefault();
                                var combo = container.OfType<DefaultComboCounter>().FirstOrDefault();
                                var ppCounter = container.OfType<PerformancePointsCounter>().FirstOrDefault();

                                if (score != null)
                                {
                                    score.Anchor = Anchor.TopCentre;
                                    score.Origin = Anchor.TopCentre;

                                    // elements default to beneath the health bar
                                    const float vertical_offset = 30;

                                    const float horizontal_padding = 20;

                                    score.Position = new Vector2(0, vertical_offset);

                                    if (ppCounter != null)
                                    {
                                        ppCounter.Y = score.Position.Y + ppCounter.ScreenSpaceDeltaToParentSpace(score.ScreenSpaceDrawQuad.Size).Y - 4;
                                        ppCounter.Origin = Anchor.TopCentre;
                                        ppCounter.Anchor = Anchor.TopCentre;
                                    }

                                    if (accuracy != null)
                                    {
                                        accuracy.Position = new Vector2(-accuracy.ScreenSpaceDeltaToParentSpace(score.ScreenSpaceDrawQuad.Size).X / 2 - horizontal_padding, vertical_offset + 5);
                                        accuracy.Origin = Anchor.TopRight;
                                        accuracy.Anchor = Anchor.TopCentre;

                                        if (combo != null)
                                        {
                                            combo.Position = new Vector2(accuracy.ScreenSpaceDeltaToParentSpace(score.ScreenSpaceDrawQuad.Size).X / 2 + horizontal_padding, vertical_offset + 5);
                                            combo.Anchor = Anchor.TopCentre;
                                        }
                                    }

                                    var hitError = container.OfType<HitErrorMeter>().FirstOrDefault();

                                    if (hitError != null)
                                    {
                                        hitError.Anchor = Anchor.CentreLeft;
                                        hitError.Origin = Anchor.CentreLeft;
                                    }

                                    var hitError2 = container.OfType<HitErrorMeter>().LastOrDefault();

                                    if (hitError2 != null)
                                    {
                                        hitError2.Anchor = Anchor.CentreRight;
                                        hitError2.Scale = new Vector2(-1, 1);
                                        // origin flipped to match scale above.
                                        hitError2.Origin = Anchor.CentreLeft;
                                    }
                                }
                            })
                            {
                                Children = new Drawable[]
                                {
                                    new DefaultComboCounter(),
                                    new DefaultScoreCounter(),
                                    new DefaultAccuracyCounter(),
                                    new DefaultHealthDisplay(),
                                    new DefaultSongProgress(),
                                    new BarHitErrorMeter(),
                                    new BarHitErrorMeter(),
                                    new PerformancePointsCounter()
                                }
                            };

                            return skinnableTargetWrapper;
                    }

                    return null;
            }

            return null;
        }

        public override IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
        {
            // todo: this code is pulled from LegacySkin and should not exist.
            // will likely change based on how databased storage of skin configuration goes.
            switch (lookup)
            {
                case GlobalSkinColours global:
                    switch (global)
                    {
                        case GlobalSkinColours.ComboColours:
                            return SkinUtils.As<TValue>(new Bindable<IReadOnlyList<Color4>?>(Configuration.ComboColours));
                    }

                    break;

                case SkinComboColourLookup comboColour:
                    return SkinUtils.As<TValue>(new Bindable<Color4>(getComboColour(Configuration, comboColour.ColourIndex)));
            }

            return null;
        }

        private static Color4 getComboColour(IHasComboColours source, int colourIndex)
            => source.ComboColours[colourIndex % source.ComboColours.Count];
    }
}
