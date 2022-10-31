using RandoSettingsManager;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;
using RandoSettingsManager.SettingsManagement.Versioning.Comparators;

namespace TheRealJournalRando.Rando
{
    internal static class SettingsManagement
    {
        public static void Hook()
        {
            RandoSettingsManagerMod.Instance.RegisterConnection(new TrjrSettingsProxy());
        }
    }

    internal class TrjrSettingsProxy : RandoSettingsProxy<JournalRandomizationSettings, string>
    {
        public override string ModKey => TheRealJournalRando.Instance.GetName();

        public override VersioningPolicy<string> VersioningPolicy { get; }
            = new BackwardCompatiblityVersioningPolicy<string>("1.0", new SemVerComparator());

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
