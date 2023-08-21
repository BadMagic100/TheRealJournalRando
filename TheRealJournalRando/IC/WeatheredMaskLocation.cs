using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.Locations;

namespace TheRealJournalRando.IC
{
    public class WeatheredMaskLocation : ObjectLocation
    {
        protected override void OnLoad()
        {
            base.OnLoad();
            Events.AddFsmEdit(SceneNames.GG_Atrium_Roof, new("GG_secret_door", "Deactivate"), KeepLandOfStormsDoorOpen);
        }

        protected override void OnUnload()
        {
            Events.RemoveFsmEdit(SceneNames.GG_Atrium_Roof, new("GG_secret_door", "Deactivate"), KeepLandOfStormsDoorOpen);
            base.OnUnload();
        }

        private void KeepLandOfStormsDoorOpen(PlayMakerFSM fsm)
        {
            // don't test to see if you have the mask to re-close the door.
            fsm.GetState("Get Bindings").RemoveAction(3);
        }
    }
}
