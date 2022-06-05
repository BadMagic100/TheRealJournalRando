using Newtonsoft.Json;
using RandomizerMod.Logging;

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

            SettingsLog.AfterLogSettings += AddJournalRandoSettings;
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
