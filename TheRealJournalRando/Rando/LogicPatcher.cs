using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System.IO;
using System.Linq;
using TheRealJournalRando.Data;

namespace TheRealJournalRando.Rando
{
    internal static class LogicPatcher
    {
        public static void Hook()
        {
            RCData.RuntimeLogicOverride.Subscribe(10f, ApplyLogic);
        }

        private static void ApplyLogic(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!RandoInterop.Settings.Enabled)
            {
                return;
            }

            AddTermsAndItems(lmb);
            AddMacrosAndWaypoints(lmb);
            AddLocationLogic(lmb);
        }

        private static void AddTermsAndItems(LogicManagerBuilder lmb)
        {
            Term hunterNotes = lmb.GetTerm("HUNTERNOTES");
            foreach (MinimalEnemyDef enemy in EnemyData.NormalData.Values.Concat(EnemyData.SpecialData.Values))
            {
                lmb.AddItem(new EmptyItem($"Journal_Entry_Only-{enemy.icName}"));
                string hunterNotesItemName = $"Hunter's_Notes-{enemy.icName}";
                if (enemy.ignoredForHunterMark)
                {
                    lmb.AddItem(new EmptyItem(hunterNotesItemName));
                }
                else
                {
                    lmb.AddItem(new SingleItem(hunterNotesItemName, new TermValue(hunterNotes, 1)));
                }
            }
        }

        private static void AddMacrosAndWaypoints(LogicManagerBuilder lmb)
        {
            using Stream s = typeof(LogicPatcher).Assembly.GetManifestResourceStream("TheRealJournalRando.Resources.Logic.macros.json");
            lmb.DeserializeJson(LogicManagerBuilder.JsonType.Macros, s);

            using Stream r = typeof(LogicPatcher).Assembly.GetManifestResourceStream("TheRealJournalRando.Resources.Logic.waypoints.json");
            lmb.DeserializeJson(LogicManagerBuilder.JsonType.Waypoints, r);
        }

        private static void AddLocationLogic(LogicManagerBuilder lmb)
        {
            // todo - logic for special enemy locations
            foreach (MinimalEnemyDef enemy in EnemyData.NormalData.Values)
            {
                string entryLocationName = $"Journal_Entry_Only-{enemy.icName}";
                string hunterNotesLocationName = $"Hunter's_Notes-{enemy.icName}";
                string logic = $"Defeated_Any_{enemy.icName}";
                lmb.AddLogicDef(new RawLogicDef(entryLocationName, logic));
                lmb.AddLogicDef(new RawLogicDef(hunterNotesLocationName, logic));
            }
        }
    }
}
