using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Modules;
using System.Linq;

namespace TheRealJournalRando.IC
{
    /// <summary>
    /// Module that allows the grimm quest (grimmkin nightmares and overworld nightmare king grimm) to be continued
    /// even after banishment to prevent losing access to Grimmkin Nightmares with vanilla flames and NKG.
    /// </summary>
    public class GrimmQuestAfterBanishment : Module
    {
        public override void Initialize()
        {
            Events.AddFsmEdit(SceneNames.Grimm_Main_Tent, new("/Grimm Holder/Grimm Scene/CamLock Normal", "Remove"), RemoveWhenEnoughFlames);
            Events.AddFsmEdit(SceneNames.Grimm_Main_Tent, new("Backroom Mask", "Remove"), RemoveWhenEnoughFlames);
            Events.AddFsmEdit(SceneNames.Grimm_Main_Tent, new("/Grimm Holder/Nightmare CamLocks", "Tricked"), RemoveWhenEnoughFlames);

            Events.AddFsmEdit(SceneNames.Grimm_Main_Tent, new("/Grimm Holder/Nightmare CamLocks/CamLock Normal", "Remove"), RemoveWhenNotEnoughFlames);
            Events.AddFsmEdit(SceneNames.Grimm_Main_Tent, new("/Grimm Holder/Nightmare CamLocks/CamLock Normal (1)", "Remove"), RemoveWhenNotEnoughFlames);

            foreach (string scene in new[] { SceneNames.Fungus2_30, SceneNames.Abyss_02, SceneNames.Hive_03 })
            {
                Events.AddFsmEdit(scene, new("Flamebearer Spawn", "Spawn Control"), SpawnNightmaresWithCarefreeMelody);
                Events.AddFsmEdit(scene, new("Control"), ControlNightmaresWithCarefreeMelody);
            }
        }

        public override void Unload()
        {
            Events.RemoveFsmEdit(SceneNames.Grimm_Main_Tent, new("/Grimm Holder/Grimm Scene/CamLock Normal", "Remove"), RemoveWhenEnoughFlames);
            Events.RemoveFsmEdit(SceneNames.Grimm_Main_Tent, new("Backroom Mask", "Remove"), RemoveWhenEnoughFlames);
            Events.RemoveFsmEdit(SceneNames.Grimm_Main_Tent, new("/Grimm Holder/Nightmare CamLocks", "Tricked"), RemoveWhenEnoughFlames);

            Events.RemoveFsmEdit(SceneNames.Grimm_Main_Tent, new("/Grimm Holder/Nightmare CamLocks/CamLock Normal", "Remove"), RemoveWhenNotEnoughFlames);
            Events.RemoveFsmEdit(SceneNames.Grimm_Main_Tent, new("/Grimm Holder/Nightmare CamLocks/CamLock Normal (1)", "Remove"), RemoveWhenNotEnoughFlames);

            foreach (string scene in new[] {SceneNames.Fungus2_30, SceneNames.Abyss_02, SceneNames.Hive_03})
            {
                Events.RemoveFsmEdit(scene, new("Flamebearer Spawn", "Spawn Control"), SpawnNightmaresWithCarefreeMelody);
                Events.RemoveFsmEdit(scene, new("Control"), ControlNightmaresWithCarefreeMelody);
            }
        }

        private void RemoveWhenNotEnoughFlames(PlayMakerFSM fsm)
        {
            FsmState check = fsm.GetState("Check");
            if (check.GetFirstActionOfType<IntCompare>() is IntCompare ic)
            {
                ic.greaterThan = fsm.FsmEvents.First(x => x.Name == "FINISHED");
            }
        }

        private void RemoveWhenEnoughFlames(PlayMakerFSM fsm)
        {
            FsmState check = fsm.GetState("Check");
            if (check.GetFirstActionOfType<IntCompare>() is IntCompare ic)
            {
                ic.greaterThan = null;
            }
        }

        private void SpawnNightmaresWithCarefreeMelody(PlayMakerFSM fsm)
        {
            // if the location is placed, the checks we're modifying will be removed and we'll let IC handle that case.
            // functionally, all we need to do is treat carefree melody like level 3 in the "vanilla" case so we can just concat that check
            FsmState state = fsm.GetState("State");
            // remove the check that cuts you off for having too many flames
            state.RemoveFirstActionOfType<IntCompare>();
            // modify the check that cuts you off if your charm level is too high
            if (state.GetFirstActionOfType<IntCompare>() is IntCompare ic)
            {
                ic.greaterThan = null;
            }
            if (state.GetFirstActionOfType<IntSwitch>() is IntSwitch sw)
            {
                sw.compareTo = sw.compareTo.Append(5).ToArray();
                sw.sendEvent = sw.sendEvent.Append(fsm.FsmEvents.First(x => x.Name == "LEVEL 3")).ToArray();
            }
        }

        private void ControlNightmaresWithCarefreeMelody(PlayMakerFSM fsm)
        {
            if (!fsm.gameObject.name.StartsWith("Flamebearer"))
            {
                return;
            }

            FsmState init = fsm.GetState("Init");
            FsmInt? storeValue = init.Actions[2] is GetPlayerDataInt gdpi ? gdpi.storeValue : (init.Actions[2] as SetIntValue)?.intVariable;
            if (storeValue != null)
            {
                // insert after grimmkin location modifies level; if it needs to make a modification it will have done it before reaching us.
                // if the value, after any modifications, indicates carefree melody, force it down to level 3
                init.InsertAction(new Lambda(() =>
                {
                    if (storeValue.Value == 5)
                    {
                        storeValue.Value = 3;
                    }
                }), 3);
            }
        }
    }
}
