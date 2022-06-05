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
            using Stream t = typeof(LogicPatcher).Assembly.GetManifestResourceStream("TheRealJournalRando.Resources.Logic.terms.json");
            lmb.DeserializeJson(LogicManagerBuilder.JsonType.Terms, t);

            Term hunterNotes = lmb.GetTerm("HUNTERNOTES");
            foreach (EnemyDef enemy in EnemyData.NormalData.Values.Concat(EnemyData.SpecialData.Values))
            {
                lmb.AddItem(new EmptyItem(enemy.icName.AsEntryName()));
                string hunterNotesItemName = enemy.icName.AsNotesName();
                if (enemy.ignoredForHunterMark)
                {
                    lmb.AddItem(new EmptyItem(hunterNotesItemName));
                }
                else
                {
                    lmb.AddItem(new SingleItem(hunterNotesItemName, new TermValue(hunterNotes, 1)));
                }
            }

            Term grimmkinNovices = lmb.GetTerm("GRIMMKINNOVICES");
            Term grimmkinMasters = lmb.GetTerm("GRIMMKINMASTERS");
            Term grimmkinNightmares = lmb.GetTerm("GRIMMKINNIGHTMARES");
            lmb.AddItem(new SingleItem(LogicItems.GrimmkinNovice, new TermValue(grimmkinNovices, 1)));
            lmb.AddItem(new SingleItem(LogicItems.GrimmkinMaster, new TermValue(grimmkinNovices, 1)));
            lmb.AddItem(new SingleItem(LogicItems.GrimmkinNightmare, new TermValue(grimmkinNovices, 1)));

            Term hornets = lmb.GetTerm("HORNETS");
            lmb.AddItem(new SingleItem(LogicItems.Hornet, new TermValue(hornets, 1)));

            Term crystalGuardians = lmb.GetTerm("CRYSTALGUARDIANS");
            lmb.AddItem(new SingleItem(LogicItems.CrystalGuardian, new TermValue(crystalGuardians, 1)));

            Term kingsmoulds = lmb.GetTerm("KINGSMOULDS");
            lmb.AddItem(new SingleItem(LogicItems.Kingsmould, new TermValue(kingsmoulds, 1)));
            lmb.AddItem(new SingleItem(LogicItems.RespawningKingsmould, new TermValue(kingsmoulds, 1000)));

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
            using Stream s = typeof(LogicPatcher).Assembly.GetManifestResourceStream("TheRealJournalRando.Resources.Logic.enemyLocations.json");
            lmb.DeserializeJson(LogicManagerBuilder.JsonType.Locations, s);

            foreach (EnemyDef enemy in EnemyData.NormalData.Values)
            {
                string entryLocationName = enemy.icName.AsEntryName();
                string hunterNotesLocationName = enemy.icName.AsNotesName();
                string logic = $"Defeated_Any_{enemy.icName}";
                lmb.AddLogicDef(new RawLogicDef(entryLocationName, logic));
                lmb.AddLogicDef(new RawLogicDef(hunterNotesLocationName, logic));
            }
        }
    }
}
