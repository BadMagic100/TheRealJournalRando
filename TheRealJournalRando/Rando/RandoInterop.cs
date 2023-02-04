using ItemChanger;
using Modding;
using Newtonsoft.Json;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using TheRealJournalRando.IC;

namespace TheRealJournalRando.Rando
{
    internal static class RandoInterop
    {
        public static JournalRandomizationSettings Settings => TheRealJournalRando.Instance.GS.RandoSettings;

        public static void HookRandomizer()
        {
            ConnectionMenu.Hook();
            LogicPatcher.Hook();
            RequestModifier.Hook();

            if (ModHooks.GetMod("RandoSettingsManager") is Mod)
            {
                SettingsManagement.Hook();
            }

            SettingsLog.AfterLogSettings += AddJournalRandoSettings;
            RandoController.OnExportCompleted += OnExportCompleted;
        }

        private static void OnExportCompleted(RandoController rc)
        {
            if (!Settings.Enabled)
            {
                return;
            }

            ItemChangerMod.Modules.GetOrAdd<FixSiblingSpawnerModule>();
            ItemChangerMod.Modules.GetOrAdd<GrimmQuestAfterBanishment>();

            if (ItemSyncInterop.IsItemSync)
            {
                ItemChangerMod.Modules.GetOrAdd<ItemsyncKillSyncModule>();
            }
        }

        private static void AddJournalRandoSettings(LogArguments args, System.IO.TextWriter tw)
        {
            tw.WriteLine("Journal Rando Settings:");
            using JsonTextWriter jtw = new(tw) { CloseOutput = false };
            RandomizerMod.RandomizerData.JsonUtil._js.Serialize(jtw, Settings);
            tw.WriteLine();
        }
    }
}
