using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Modules;

namespace TheRealJournalRando.IC
{
    public class FixSiblingSpawnerModule : Module
    {
        public override void Initialize()
        {
            Events.AddFsmEdit(new FsmID("Shade Sibling Spawner", "Spawn"), RemoveVisitedCheck);
        }
        public override void Unload()
        {
            Events.RemoveFsmEdit(new FsmID("Shade Sibling Spawner", "Spawn"), RemoveVisitedCheck);
        }

        private void RemoveVisitedCheck(PlayMakerFSM self)
        {
            FsmState init = self.GetState("Init");
            init.RemoveFirstActionOfType<PlayerDataBoolTest>();

            init.AddLastAction(new Lambda(() => self.SendEvent("SHADE SPAWN READY")));
        }
    }
}
