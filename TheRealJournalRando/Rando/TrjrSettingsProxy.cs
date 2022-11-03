using RandoSettingsManager;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;
using RandoSettingsManager.SettingsManagement.Versioning.Comparators;
using System.IO;
using System.Reflection;

namespace TheRealJournalRando.Rando
{
    internal static class SettingsManagement
    {
        public static void Hook()
        {
            RandoSettingsManagerMod.Instance.RegisterConnection(new TrjrSettingsProxy());
        }
    }

    internal class TrjrSettingsProxy : RandoSettingsProxy<JournalRandomizationSettings, 
        (string, string, string, string, string, string)>
    {
        public override string ModKey => TheRealJournalRando.Instance.GetName();

        public override VersioningPolicy<(string, string, string, string, string, string)> VersioningPolicy { get; }

        public TrjrSettingsProxy()
        {
            Assembly a = typeof(TrjrSettingsProxy).Assembly;
            using Stream locations = a.GetManifestResourceStream("TheRealJournalRando.Resources.Logic.enemyLocations.json");
            using Stream macros = a.GetManifestResourceStream("TheRealJournalRando.Resources.Logic.macros.json");
            using Stream terms = a.GetManifestResourceStream("TheRealJournalRando.Resources.Logic.terms.json");
            using Stream waypoints = a.GetManifestResourceStream("TheRealJournalRando.Resources.Logic.waypoints.json");
            using Stream enemyData = a.GetManifestResourceStream("TheRealJournalRando.Resources.enemyData.json");

            // version based off major.minor for settings (changes to settings will bump mod version),
            // and the content of all logic-modifying files
            VersioningPolicy = CompoundVersioningPolicy.Of(
                new EqualityVersioningPolicy<string>(TheRealJournalRando.Instance.GetVersion(), new SemVerComparator(places: 2)),
                new ContentHashVersioningPolicy(locations),
                new ContentHashVersioningPolicy(macros),
                new ContentHashVersioningPolicy(terms),
                new ContentHashVersioningPolicy(waypoints),
                new ContentHashVersioningPolicy(enemyData)
            );
        }

        public override void ReceiveSettings(JournalRandomizationSettings? settings)
        {
            if (settings != null)
            {
                ConnectionMenu.Instance.ApplySettingsToMenu(settings);
            }
            else
            {
                ConnectionMenu.Instance.Disable();
            }
        }

        public override bool TryProvideSettings(out JournalRandomizationSettings? settings)
        {
            settings = RandoInterop.Settings;
            return settings.Enabled;
        }
    }
}
