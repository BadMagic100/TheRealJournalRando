using ItemChanger;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerCore.LogicItems.Templates;
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

            AddTermsAndItems(lmb);
            OverrideBaseRandoJournalItems(lmb);
            AddMacrosAndWaypoints(lmb);
            AddLocationLogic(lmb);
        }

        private static void AddTermsAndItems(LogicManagerBuilder lmb)
        {
            using Stream t = typeof(LogicPatcher).Assembly.GetManifestResourceStream("TheRealJournalRando.Resources.Logic.terms.json");
            lmb.DeserializeJson(LogicManagerBuilder.JsonType.Terms, t);

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
                    // entry: if notes are progressive and you have one of the things already, gives the entry part and notes
                    //        else, give an entry part
                    // notes: if you already have one of the entry parts, give the notes and entry part
                    //           (regardless whether notes are progressive you should definitely get it)
                    //        else (if you don't have one of the things)
                    //          if notes are progressive, you only get one of the entry parts (you have to earn the notes!)
                    //          else get the entry part and the notes
                    lmb.AddTemplateItem(new BranchedItemTemplate(journalEntryItemName, $"{Terms.PROGRESSIVENOTES} + {progressiveEntry.Name}",
                        new MultiItem(journalEntryItemName, new TermValue[] { new(progressiveEntry, 1), new(hunterNotes, 1) }),
                        new SingleItem(journalEntryItemName, new(progressiveEntry, 1))));

                    MultiItemTemplate notesAndEntryPart = new(hunterNotesItemName, new[] { (progressiveEntry.Name, 1), (hunterNotes.Name, 1) });
                    lmb.AddTemplateItem(new BranchedItemTemplate(hunterNotesItemName, progressiveEntry.Name,
                        notesAndEntryPart,
                        new BranchedItemTemplate(hunterNotesItemName, Terms.PROGRESSIVENOTES,
                            new SingleItem(hunterNotesItemName, new(progressiveEntry, 1)),
                            notesAndEntryPart)
                    ));
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
