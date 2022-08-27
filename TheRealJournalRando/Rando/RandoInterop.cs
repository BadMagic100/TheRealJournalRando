using ItemChanger;
using ItemChanger.Internal;
using ItemChanger.Tags;
using Newtonsoft.Json;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using System.Linq;
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

            SettingsLog.AfterLogSettings += AddJournalRandoSettings;
            RandoController.OnExportCompleted += OnExportCompleted;
        }

        private static void OnExportCompleted(RandoController rc)
        {
            if (!Settings.Enabled)
            {
                return;
            }

            // if GK nightmares or NKG will be randomized
            if (Settings.Pools.BonusEntries)
            {
                ItemChangerMod.Modules.GetOrAdd<GrimmQuestAfterBanishment>();
            }

            // don't double up pins - aside from cost (which helperlog/rmm don't know), logic is the same
            if (Settings.JournalRandomizationType == JournalRandomizationType.All)
            {
                foreach (AbstractPlacement p in Ref.Settings.Placements
                    .Where(pair => !pair.Key.EndsWith("Mossy_Vagabond") && pair.Key.StartsWith("Hunter's_Notes"))
                    .Select(pair => pair.Value))
                {
                    Tag? t = p.GetPlacementAndLocationTags()
                        .FirstOrDefault(t => t is IInteropTag iit && iit.Message == "RandoSupplementalMetadata"
                                        && iit.TryGetProperty("ModSource", out string mod) && mod == nameof(TheRealJournalRando));
                    if (t != null)
                    {
                        p.tags.Add(InteropTagFactory.CmiLocationTag(noPin: true));
                    }
                }
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
