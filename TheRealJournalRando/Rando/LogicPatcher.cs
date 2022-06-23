using ItemChanger;
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
        private const int ARBITRARILY_LARGE_ENEMY_VALUE = 10000;

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
            OverrideBaseRandoJournalItems(lmb);
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
            lmb.AddItem(new SingleItem(LogicItems.GrimmkinMaster, new TermValue(grimmkinMasters, 1)));
            lmb.AddItem(new SingleItem(LogicItems.GrimmkinNightmare, new TermValue(grimmkinNightmares, 1)));

            Term hornets = lmb.GetTerm("HORNETS");
            lmb.AddItem(new SingleItem(LogicItems.Hornet, new TermValue(hornets, 1)));

            Term mimics = lmb.GetTerm("MIMICS");
            // this overrides the mimic item from base rando logic
            // it's reasonable to expect folks to 2 colo1 twice for VFK costs, not super reasonable to expect folks to do colo2 5 times for mimic costs
            lmb.AddItem(new SingleItem(LogicItems.MimicGrub, new TermValue(mimics, 1)));

            Term crystalGuardians = lmb.GetTerm("CRYSTALGUARDIANS");
            lmb.AddItem(new SingleItem(LogicItems.CrystalGuardian, new TermValue(crystalGuardians, 1)));

            Term kingsmoulds = lmb.GetTerm("KINGSMOULDS");
            lmb.AddItem(new SingleItem(LogicItems.Kingsmould, new TermValue(kingsmoulds, 1)));
            lmb.AddItem(new SingleItem(LogicItems.RespawningKingsmould, new TermValue(kingsmoulds, ARBITRARILY_LARGE_ENEMY_VALUE)));

            Term elderbaldurs = lmb.GetTerm("ELDERBALDURS");
            lmb.AddItem(new SingleItem(LogicItems.ElderBaldur, new TermValue(elderbaldurs, 1)));

            Term gruzMothers = lmb.GetTerm("GRUZMOTHERS");
            lmb.AddItem(new SingleItem(LogicItems.GruzMother, new TermValue(gruzMothers, 1)));
            lmb.AddItem(new SingleItem(LogicItems.RespawningGruzMother, new TermValue(gruzMothers, ARBITRARILY_LARGE_ENEMY_VALUE)));

            Term vengeflyKings = lmb.GetTerm("VENGEFLYKINGS");
            lmb.AddItem(new SingleItem(LogicItems.VengeflyKing, new TermValue(vengeflyKings, 1)));
            lmb.AddItem(new SingleItem(LogicItems.RespawningVengeflyKing, new TermValue(vengeflyKings, ARBITRARILY_LARGE_ENEMY_VALUE)));

        }

        private static void OverrideBaseRandoJournalItems(LogicManagerBuilder lmb)
        {
            Term hunterNotes = lmb.GetTerm("HUNTERNOTES");
            foreach (string entry in new[] { 
                ItemNames.Journal_Entry_Goam, ItemNames.Journal_Entry_Garpede, 
                ItemNames.Journal_Entry_Charged_Lumafly, ItemNames.Journal_Entry_Void_Tendrils})
            {
                lmb.AddItem(new SingleItem(entry, new TermValue(hunterNotes, 1)));
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
