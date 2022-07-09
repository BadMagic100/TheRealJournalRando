using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.Locations;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheRealJournalRando.IC
{
    public class HuntersMarkLocation : ObjectLocation
    {
        public override void OnActiveSceneChanged(Scene to)
        {
            GetContainer(out GameObject obj, out string containerType);
            PlaceContainer(obj, containerType);

            GameObject hunterEntry = to.FindGameObject("Hunter Entry");
            PlayMakerFSM control = hunterEntry.LocateMyFSM("Control");
            control.GetState("Init").Actions[7] = new SetGameObject()
            {
                variable = control.FsmVariables.FindFsmGameObject("Shiny Item"),
                gameObject = obj,
                everyFrame = false,
            };

            FsmState item = control.GetState("Item");
            item.RemoveTransitionsOn("SHINY PICKED UP");
            item.AddTransition("FINISHED", "NPC Pause");
            item.AddLastAction(new Wait()
            {
                time = 5f,
                finishEvent = control.FsmEvents.First(e => e.Name == "FINISHED")
            });

            FsmState npcPause = control.GetState("NPC Pause");
            npcPause.RemoveTransitionsOn("FINISHED");
        }
    }
}
