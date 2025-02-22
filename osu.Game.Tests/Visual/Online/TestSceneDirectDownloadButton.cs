// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Rulesets.Osu;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneDirectDownloadButton : OsuTestScene
    {
        private TestDownloadButton downloadButton;

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Test]
        public void TestDownloadableBeatmap()
        {
            createButton(true);
            assertEnabled(true);
        }

        [Test]
        public void TestUndownloadableBeatmap()
        {
            createButton(false);
            assertEnabled(false);
        }

        [Test]
        public void TestDownloadState()
        {
            AddUntilStep("ensure manager loaded", () => beatmaps != null);
            ensureSoleilyRemoved();
            createButtonWithBeatmap(createSoleily());
            AddAssert("button state not downloaded", () => downloadButton.DownloadState == DownloadState.NotDownloaded);
            AddStep("import soleily", () => beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()));

            AddUntilStep("wait for beatmap import", () => beatmaps.GetAllUsableBeatmapSets().Any(b => b.OnlineBeatmapSetID == 241526));
            AddAssert("button state downloaded", () => downloadButton.DownloadState == DownloadState.LocallyAvailable);

            createButtonWithBeatmap(createSoleily());
            AddAssert("button state downloaded", () => downloadButton.DownloadState == DownloadState.LocallyAvailable);
            ensureSoleilyRemoved();
            AddAssert("button state not downloaded", () => downloadButton.DownloadState == DownloadState.NotDownloaded);
        }

        private void ensureSoleilyRemoved()
        {
            AddStep("remove soleily", () =>
            {
                var beatmap = beatmaps.QueryBeatmapSet(b => b.OnlineBeatmapSetID == 241526);

                if (beatmap != null) beatmaps.Delete(beatmap);
            });
        }

        private void assertEnabled(bool enabled)
        {
            AddAssert($"button {(enabled ? "enabled" : "disabled")}", () => downloadButton.DownloadEnabled == enabled);
        }

        private void createButtonWithBeatmap(IBeatmapSetInfo beatmap)
        {
            AddStep("create button", () =>
            {
                Child = downloadButton = new TestDownloadButton(beatmap)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(75, 50),
                };
            });
        }

        private void createButton(bool downloadable)
        {
            AddStep("create button", () =>
            {
                Child = downloadButton = new TestDownloadButton(downloadable ? getDownloadableBeatmapSet() : getUndownloadableBeatmapSet())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(75, 50),
                };
            });
        }

        private IBeatmapSetInfo createSoleily()
        {
            return new APIBeatmapSet
            {
                OnlineID = 241526,
                Availability = new BeatmapSetOnlineAvailability
                {
                    DownloadDisabled = false,
                    ExternalLink = string.Empty,
                },
            };
        }

        private IBeatmapSetInfo getDownloadableBeatmapSet()
        {
            var apiBeatmapSet = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo).BeatmapSetInfo.OnlineInfo;

            apiBeatmapSet.HasVideo = true;
            apiBeatmapSet.HasStoryboard = true;

            return apiBeatmapSet;
        }

        private IBeatmapSetInfo getUndownloadableBeatmapSet()
        {
            var apiBeatmapSet = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo).BeatmapSetInfo.OnlineInfo;

            apiBeatmapSet.Artist = "test";
            apiBeatmapSet.Title = "undownloadable";
            apiBeatmapSet.AuthorString = "test";

            apiBeatmapSet.HasVideo = true;
            apiBeatmapSet.HasStoryboard = true;

            apiBeatmapSet.Availability = new BeatmapSetOnlineAvailability
            {
                DownloadDisabled = true,
                ExternalLink = "http://osu.ppy.sh",
            };

            return apiBeatmapSet;
        }

        private class TestDownloadButton : BeatmapPanelDownloadButton
        {
            public new bool DownloadEnabled => base.DownloadEnabled;

            public DownloadState DownloadState => State.Value;

            public TestDownloadButton(IBeatmapSetInfo beatmapSet)
                : base(beatmapSet)
            {
            }
        }
    }
}
