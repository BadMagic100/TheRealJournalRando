using GlobalEnums;
using ItemChanger;
using ItemChanger.StartDefs;
using ItemChangerTesting;
using System.Collections.Generic;
using TheRealJournalRando;
using TheRealJournalRando.Data.Generated;
using TheRealJournalRando.IC;

namespace TrjrICTests.Tests
{
    public class GrimmkinTest : Test
    {
        public override int Priority => -1;

        public override StartDef StartDef => TransitionBasedStartDef.FromGate("Town", "door_mapper", (int)MapZone.TOWN);

        public override void Start(TestArgs args)
        {
            base.Start(args);
            ItemChangerMod.Modules.Add<GrimmQuestAfterBanishment>();
            AbstractPlacement start = ItemChanger.Internal.Ref.Settings.Placements["Start"];
            start.Add(Finder.GetItem(ItemNames.Grimmchild1));
        }

        public override IEnumerable<AbstractPlacement> GetPlacements(TestArgs args)
        {
            AbstractPlacement p = Finder.GetLocation(EnemyNames.Grimmkin_Nightmare.AsEntryName()).Wrap();
            p.Add(Finder.GetItem(ItemNames.Baldur_Shell));
            yield return p;
        }
    }
}
