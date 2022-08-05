using ItemChanger;
using ItemChanger.Tags;
using System.Collections.Generic;

namespace TheRealJournalRando.IC
{
    public static class InteropTagFactory
    {
        private static void SetProperty(this InteropTag t, string prop, object? value)
        {
            if (value != null)
            {
                t.Properties[prop] = value;
            }
        }

        private const string CmiModSourceProperty = "ModSource";
        private const string CmiPoolGroupProperty = "PoolGroup";
        private const string CmiSceneNamesProperty = "SceneNames";
        private const string CmiTitledAreaNamesProperty = "TitledAreaNames";
        private const string CmiMapAreaNamesProperty = "MapAreaNames";
        private const string CmiHighlightScenesProperty = "HighlightScenes";
        private const string CmiPinSpriteProperty = "PinSprite";
        private const string CmiMapLocationsProperty = "MapLocations";

        private const string RiMessageProperty = "DisplayMessage";
        private const string RiSourceProperty = "DisplaySource";
        private const string RiIgnoreProperty = "IgnoreItem";

        public static InteropTag CmiSharedTag(string? poolGroup = null, ISprite? pinSprite = null)
        {
            InteropTag t = new()
            {
                Message = "RandoSupplementalMetadata",
                Properties =
                {
                    [CmiModSourceProperty] = nameof(TheRealJournalRando)
                }
            };
            t.SetProperty(CmiPoolGroupProperty, poolGroup);
            t.SetProperty(CmiPinSpriteProperty, pinSprite);
            return t;
        }

        public static InteropTag CmiLocationTag(string? poolGroup = null, ISprite? pinSprite = null,
            IEnumerable<string>? sceneNames = null, IEnumerable<string>? titledAreaNames = null, IEnumerable<string>? mapAreaNames = null,
            string[]? highlightScenes = null, (string, float, float)[]? mapLocations = null)
        {
            InteropTag t = CmiSharedTag(poolGroup: poolGroup, pinSprite: pinSprite);
            t.SetProperty(CmiSceneNamesProperty, sceneNames);
            t.SetProperty(CmiTitledAreaNamesProperty, titledAreaNames);
            t.SetProperty(CmiMapAreaNamesProperty, mapAreaNames);
            t.SetProperty(CmiHighlightScenesProperty, highlightScenes);
            t.SetProperty(CmiMapLocationsProperty, mapLocations);
            return t;
        }

        public static InteropTag RecentItemsSharedTag(string? messageOverride = null)
        {
            InteropTag t = new()
            {
                Message = "RecentItems"
            };
            t.SetProperty(RiMessageProperty, messageOverride);
            return t;
        }

        public static InteropTag RecentItemsLocationTag(string? messageOverride = null, string? sourceOverride = null,
            bool? ignore = null)
        {
            InteropTag t = RecentItemsSharedTag(messageOverride: messageOverride);
            t.SetProperty(RiSourceProperty, sourceOverride);
            t.SetProperty(RiIgnoreProperty, ignore);
            return t;
        }
    }
}
