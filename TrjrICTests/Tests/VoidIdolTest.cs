using ItemChanger;
using ItemChangerTesting;
using System.Collections.Generic;
using TheRealJournalRando;
using TheRealJournalRando.Data.Generated;

namespace TrjrICTests.Tests
{
    public class VoidIdolTest : Test
    {
        public override int Priority => -1;

        public override void Start(TestArgs args)
        {
            base.Start(args);
            AbstractPlacement start = ItemChanger.Internal.Ref.Settings.Placements["Start"];
            start.Add(Finder.GetItem(ItemNames.Hunters_Journal)!);
            start.Items.RemoveAll(i => i.name == ItemNames.Vengeful_Spirit);
            start.Items.RemoveAll(i => i.name == ItemNames.Shade_Soul);
        }

        public override IEnumerable<AbstractPlacement> GetPlacements(TestArgs args)
        {
            yield return Finder.GetLocation(LocationNames.Iselda)!.Wrap()
                .Add(Finder.GetItem(EnemyNames.Void_Idol_1.AsEntryName())!)
                .Add(Finder.GetItem(EnemyNames.Void_Idol_2.AsEntryName())!)
                .Add(Finder.GetItem(EnemyNames.Void_Idol_3.AsEntryName())!);

            yield return Finder.GetLocation(EnemyNames.Void_Idol_1.AsEntryName())!.Wrap().Add(Finder.GetItem(ItemNames.Soul_Totem_A)!);
            yield return Finder.GetLocation(EnemyNames.Void_Idol_2.AsEntryName())!.Wrap().Add(Finder.GetItem(ItemNames.Dream_Nail)!);
            yield return Finder.GetLocation(EnemyNames.Void_Idol_3.AsEntryName())!.Wrap().Add(Finder.GetItem(ItemNames.Vengeful_Spirit)!);
        }
    }
}
