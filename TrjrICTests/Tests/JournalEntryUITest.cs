using ItemChanger;
using ItemChangerTesting;
using System.Collections.Generic;
using System.Linq;
using TheRealJournalRando;
using TheRealJournalRando.Data;
using TheRealJournalRando.Data.Generated;

namespace TrjrICTests.Tests
{
    public class JournalEntryUITest : Test
    {
        public override int Priority => -1;

        public override void Start(TestArgs args)
        {
            base.Start(args);
            AbstractPlacement start = ItemChanger.Internal.Ref.Settings.Placements["Start"];
            start.Add(Finder.GetItem(ItemNames.Hunters_Journal));
        }

        public override IEnumerable<AbstractPlacement> GetPlacements(TestArgs args)
        {
            AbstractPlacement iselda = Finder.GetLocation(LocationNames.Iselda).Wrap();
            iselda.Add(Finder.GetItem(ItemNames.Journal_Entry_Goam));
            iselda.Add(Finder.GetItem(ItemNames.Journal_Entry_Garpede));
            iselda.Add(Finder.GetItem(ItemNames.Journal_Entry_Charged_Lumafly));
            iselda.Add(Finder.GetItem(ItemNames.Journal_Entry_Void_Tendrils));
            iselda.Add(Finder.GetItem(ItemNames.Journal_Entry_Seal_of_Binding));
            iselda.Add(Finder.GetItem(EnemyNames.Void_Idol_1.AsEntryName()));
            iselda.Add(Finder.GetItem(EnemyNames.Void_Idol_2.AsEntryName()));
            iselda.Add(Finder.GetItem(EnemyNames.Void_Idol_3.AsEntryName()));
            iselda.Add(Finder.GetItem(EnemyNames.Weathered_Mask.AsEntryName()));
            iselda.Add(Finder.GetItem(EnemyNames.Hunters_Mark));
            foreach (EnemyDef e in EnemyData.Enemies.Values.Where(x => !x.icIgnore).Append(EnemyData.Enemies[EnemyNames.Mossy_Vagabond]))
            {
                iselda.Add(Finder.GetItem(e.icName.AsEntryName()));
            }
            foreach (EnemyDef e in EnemyData.Enemies.Values.Where(x => !x.icIgnore).Append(EnemyData.Enemies[EnemyNames.Mossy_Vagabond]))
            {
                iselda.Add(Finder.GetItem(e.icName.AsNotesName()));
            }
            yield return iselda;
        }
    }
}
