using GlobalEnums;
using ItemChanger;
using ItemChangerTesting;
using System.Collections.Generic;
using System.Linq;
using TheRealJournalRando;
using TheRealJournalRando.Data;
using TheRealJournalRando.Data.Generated;

namespace TrjrICTests.Tests
{
    public class HuntersMarkTest : Test
    {
        public override int Priority => -1;

        public override StartDef StartDef => new() { MapZone = (int)MapZone.GREEN_PATH, SceneName = SceneNames.Fungus1_08, X = 36.0f, Y = 38.4f };

        public override void Start(TestArgs args)
        {
            base.Start(args);
            AbstractPlacement start = ItemChanger.Internal.Ref.Settings.Placements["Start"];
            start.Add(Finder.GetItem(ItemNames.Hunters_Journal));
            foreach (EnemyDef e in EnemyData.Enemies.Values.Where(x => !x.icIgnore).Append(EnemyData.Enemies[EnemyNames.Mossy_Vagabond]))
            {
                start.Add(Finder.GetItem(e.icName.AsEntryName()));
                start.Add(Finder.GetItem(e.icName.AsNotesName()));
            }
            start.Add(Finder.GetItem(ItemNames.Journal_Entry_Goam));
            start.Add(Finder.GetItem(ItemNames.Journal_Entry_Garpede));
            start.Add(Finder.GetItem(ItemNames.Journal_Entry_Charged_Lumafly));
            start.Add(Finder.GetItem(ItemNames.Journal_Entry_Void_Tendrils));
        }

        public override IEnumerable<AbstractPlacement> GetPlacements(TestArgs args)
        {
            yield return Finder.GetLocation(EnemyNames.Hunters_Mark).Wrap().Add(Finder.GetItem(ItemNames.Grub));
        }
    }
}
