using ItemChanger;
using RandomizerCore;
using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerCore.StringItems;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System.IO;
using System.Linq;
using TheRealJournalRando.Data;
using TheRealJournalRando.Data.Generated;
using TheRealJournalRando.Rando.Generated;

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

            JsonLogicFormat fmt = new();
            AddTermsAndItems(lmb, fmt);
            OverrideBaseRandoJournalItems(lmb);
            AddMacrosAndWaypoints(lmb, fmt);
            AddLocationLogic(lmb, fmt);
        }

        private static void AddTermsAndItems(LogicManagerBuilder lmb, JsonLogicFormat fmt)
        {
            using Stream t = typeof(LogicPatcher).Assembly.GetManifestResourceStream("TheRealJournalRando.Resources.Logic.terms.json");
            lmb.DeserializeFile(LogicFileType.Terms, fmt, t);

            Term hunterNotes = lmb.GetTerm(Terms.HUNTERNOTES);
            foreach (EnemyDef enemy in EnemyData.Enemies.Values.Where(x => !x.logicItemIgnore))
            {
                string journalEntryItemName = enemy.icName.AsEntryName();
                string hunterNotesItemName = enemy.icName.AsNotesName();
                if (enemy.ignoredForHunterMark)
                {
                    lmb.AddItem(new EmptyItem(journalEntryItemName));
                    lmb.AddItem(new EmptyItem(hunterNotesItemName));
                }
                else
                {
                    Term progressiveEntry = lmb.GetOrAddTerm($"PARTIALENTRY[{enemy.icName}]", TermType.SignedByte);
                    // entry: always give the entry part, if notes are progressive also give notes.
                    // notes: always get at least the partial entry. generally, you can also have notes too, except for when
                    //        notes are progressive and you don't already have the entry
                    string entryEffect = $"""
                        `{Terms.PROGRESSIVENOTES} + {progressiveEntry.Name}` => {hunterNotes.Name}++
                            >> {progressiveEntry.Name}++
                        """;
                    lmb.AddItem(new StringItemTemplate(journalEntryItemName, entryEffect));

                    string notesEffect = $"""
                        !`{Terms.PROGRESSIVENOTES} + {progressiveEntry.Name}=0` => {hunterNotes.Name}++
                            >> {progressiveEntry.Name}++
                        """;
                    lmb.AddItem(new StringItemTemplate(hunterNotesItemName, notesEffect));
                }
            }
            lmb.AddItem(new EmptyItem(EnemyNames.Weathered_Mask.AsEntryName()));
            lmb.AddItem(new EmptyItem(EnemyNames.Void_Idol_1.AsEntryName()));
            lmb.AddItem(new EmptyItem(EnemyNames.Void_Idol_2.AsEntryName()));
            lmb.AddItem(new EmptyItem(EnemyNames.Void_Idol_3.AsEntryName()));
            lmb.AddItem(new EmptyItem(EnemyNames.Hunters_Mark));

            Term grimmkinNovices = lmb.GetTerm(Terms.GRIMMKINNOVICES);
            Term grimmkinMasters = lmb.GetTerm(Terms.GRIMMKINMASTERS);
            Term grimmkinNightmares = lmb.GetTerm(Terms.GRIMMKINNIGHTMARES);
            lmb.AddItem(new SingleItem(LogicItems.GrimmkinNovice, new TermValue(grimmkinNovices, 1)));
            lmb.AddItem(new SingleItem(LogicItems.GrimmkinMaster, new TermValue(grimmkinMasters, 1)));
            lmb.AddItem(new SingleItem(LogicItems.GrimmkinNightmare, new TermValue(grimmkinNightmares, 1)));

            Term hornets = lmb.GetTerm(Terms.HORNETS);
            lmb.AddItem(new SingleItem(LogicItems.Hornet, new TermValue(hornets, 1)));

            Term bluggsacs = lmb.GetTerm(Terms.BLUGGSACS);
            lmb.AddItem(new SingleItem(LogicItems.Bluggsac, new TermValue(bluggsacs, 1)));

            Term mimics = lmb.GetTerm(Terms.MIMICS);
            // this overrides the mimic item from base rando logic
            // it's reasonable to expect folks to 2 colo1 twice for VFK costs, not super reasonable to expect folks to do colo2 5 times for mimic costs
            lmb.AddItem(new SingleItem(ItemNames.Mimic_Grub, new TermValue(mimics, 1)));

            Term crystalGuardians = lmb.GetTerm(Terms.CRYSTALGUARDIANS);
            lmb.AddItem(new SingleItem(LogicItems.CrystalGuardian, new TermValue(crystalGuardians, 1)));

            Term kingsmoulds = lmb.GetTerm(Terms.KINGSMOULDS);
            lmb.AddItem(new SingleItem(LogicItems.Kingsmould, new TermValue(kingsmoulds, 1)));
            lmb.AddItem(new SingleItem(LogicItems.RespawningKingsmould, new TermValue(kingsmoulds, int.MaxValue)));

            Term elderbaldurs = lmb.GetTerm(Terms.ELDERBALDURS);
            lmb.AddItem(new SingleItem(LogicItems.ElderBaldur, new TermValue(elderbaldurs, 1)));

            Term gruzMothers = lmb.GetTerm(Terms.GRUZMOTHERS);
            lmb.AddItem(new SingleItem(LogicItems.GruzMother, new TermValue(gruzMothers, 1)));
            lmb.AddItem(new SingleItem(LogicItems.RespawningGruzMother, new TermValue(gruzMothers, int.MaxValue)));

            Term vengeflyKings = lmb.GetTerm(Terms.VENGEFLYKINGS);
            lmb.AddItem(new SingleItem(LogicItems.VengeflyKing, new TermValue(vengeflyKings, 1)));
            lmb.AddItem(new SingleItem(LogicItems.RespawningVengeflyKing, new TermValue(vengeflyKings, int.MaxValue)));

        }

        private static void OverrideBaseRandoJournalItems(LogicManagerBuilder lmb)
        {
            Term hunterNotes = lmb.GetTerm(Terms.HUNTERNOTES);
            foreach (string entry in new[] { 
                ItemNames.Journal_Entry_Goam, ItemNames.Journal_Entry_Garpede, 
                ItemNames.Journal_Entry_Charged_Lumafly, ItemNames.Journal_Entry_Void_Tendrils})
            {
                lmb.AddItem(new SingleItem(entry, new TermValue(hunterNotes, 1)));
            }
        }

        private static void AddMacrosAndWaypoints(LogicManagerBuilder lmb, JsonLogicFormat fmt)
        {
            using Stream s = typeof(LogicPatcher).Assembly.GetManifestResourceStream("TheRealJournalRando.Resources.Logic.macros.json");
            lmb.DeserializeFile(LogicFileType.Macros, fmt, s);

            using Stream r = typeof(LogicPatcher).Assembly.GetManifestResourceStream("TheRealJournalRando.Resources.Logic.waypoints.json");
            lmb.DeserializeFile(LogicFileType.Waypoints, fmt, r);
        }

        private static void AddLocationLogic(LogicManagerBuilder lmb, JsonLogicFormat fmt)
        {
            using Stream s = typeof(LogicPatcher).Assembly.GetManifestResourceStream("TheRealJournalRando.Resources.Logic.enemyLocations.json");
            lmb.DeserializeFile(LogicFileType.Locations, fmt, s);

            foreach (EnemyDef enemy in EnemyData.Enemies.Values.Where(x => !x.logicLocationIgnore))
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
