using ItemChanger.Tags;

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

        private const string RiMessageProperty = "DisplayMessage";
        private const string RiSourceProperty = "DisplaySource";
        private const string RiIgnoreProperty = "IgnoreItem";

        public static InteropTag CmiSharedTag(string? poolGroup = null)
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
            InteropTag t = RecentItemsSharedTag(messageOverride);
            t.SetProperty(RiSourceProperty, sourceOverride);
            t.SetProperty(RiIgnoreProperty, ignore);
            return t;
        }
    }
}
