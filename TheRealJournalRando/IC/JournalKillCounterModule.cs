using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Modules;
using Modding;
using System.Collections.Generic;
using TheRealJournalRando.Fsm;

namespace TheRealJournalRando.IC
{
    public delegate void EnemyKillCounterChangedHandler(string pdName);

    public class JournalKillCounterModule : Module
    {
        public Dictionary<string, int> enemyKillCounts = new();

        public event EnemyKillCounterChangedHandler? OnKillCountChanged;

        public override void Initialize()
        {
            ModHooks.RecordKillForJournalHook += OnJournalRecord;
            On.PlayMakerFSM.OnEnable += OnFsmEnable;
            On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter += OnSetPlayerDataBoolAction;
        }

        public override void Unload()
        {
            ModHooks.RecordKillForJournalHook -= OnJournalRecord;
            On.PlayMakerFSM.OnEnable -= OnFsmEnable;
            On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter -= OnSetPlayerDataBoolAction;
        }

        public int GetKillCount(string pdName)
        {
            if (enemyKillCounts.TryGetValue(pdName, out int value))
            {
                return value;
            }
            return 0;
        }

        public void Record(string playerDataName)
        {
            if (playerDataName == "Dummy")
            {
                TheRealJournalRando.Instance.LogDebug($"Rejecting dummy kill");
                return;
            }
            TheRealJournalRando.Instance.LogDebug($"Recording kill for {playerDataName}");

            if (!enemyKillCounts.ContainsKey(playerDataName))
            {
                enemyKillCounts[playerDataName] = 0;
            }
            enemyKillCounts[playerDataName]++;
            OnKillCountChanged?.Invoke(playerDataName);
        }

        private void OnJournalRecord(EnemyDeathEffects enemyDeathEffects, string playerDataName, string killedBoolPlayerDataLookupKey,
            string killCountIntPlayerDataLookupKey, string newDataBoolPlayerDataLookupKey)
        {
            string goName = enemyDeathEffects.gameObject.name;
            // ignore waterways split/baby flukes
            if (goName.StartsWith("fluke_baby") || goName.StartsWith("Flukeman Top") || goName.StartsWith("Flukeman Bot"))
            {
                playerDataName = "Dummy";
            }

            Record(playerDataName);
        }

        private void OnSetPlayerDataBoolAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.orig_OnEnter orig, SetPlayerDataBool self)
        {
            orig(self);
            // this hook handles most special cases (i.e. bosses) that unconditionally set the killedX PD bool to grant the journal.
            bool settingKilledTrue = self.boolName.Value.StartsWith("killed") && self.value.Value;
            if (!settingKilledTrue)
            {
                return;
            }

            // FK journal grant - godhome has same FSM key (in GG_False_Knight) but is gated behind a GGCheckIfBossScene
            if (CheckIsFsm(self, "False Knight New", "FalseyControl") && self.Fsm.GameObject.scene.name == SceneNames.Crossroads_10_boss)
            {
                Record("FalseKnight");
            }
            // Mantis lords journal grant - reusable (in GG_Mantis_Lords) but gated behind GGCheckIfBossScene.
            // Sisters of Battle (in GG_Mantis_Lords_V) uses a different FSM and does not grant journal entries normally; will not be handled
            if (CheckIsFsm(self, "Mantis Battle", "Battle Control") && self.Fsm.GameObject.scene.name == SceneNames.Fungus2_15_boss)
            {
                Record("MantisLord");
            }
            // WK journal grant - the journal state in Battle Control still exists but is disconnected - will need to re-draw the edge from
            // Knight 6->NEXT to Journal state
            if (CheckIsFsm(self, "Battle Control", "Battle Control") && self.Fsm.GameObject.scene.name == SceneNames.Ruins2_03_boss)
            {
                Record("BlackKnight");
            }
            // Collector journal grant - reusable (in GG_Collector and GG_Collector_V) but gated behind GGCheckIfBossScene
            if (CheckIsFsm(self, "Jar Collector", "Death") && self.Fsm.GameObject.scene.name == SceneNames.Ruins2_11_boss)
            {
                Record("JarCollector");
            }
            // TMG journal grant - godhome TMG will need special handling. probably in Grimm Boss-Control between Death Explode and Send Death Event
            if (CheckIsFsm(self, "Defeated NPC", "Conversation Control") && self.Fsm.GameObject.scene.name == SceneNames.Grimm_Main_Tent)
            {
                Record("Grimm");
            }
            // NKG journal grant - in vanilla, godhome NKG can't grant you journal; in rando you can fight NKG even after banishment though
            if (CheckIsFsm(self, "Grimm Control", "Control") && self.Fsm.GameObject.scene.name == SceneNames.Grimm_Nightmare)
            {
                Record("NightmareGrimm");
            }
            // THK journal grant - verified reusable
            if (CheckIsFsm(self, "Hollow Knight Boss", "Phase Control") && self.Fsm.GameObject.scene.name == SceneNames.Room_Final_Boss_Core)
            {
                Record("HollowKnight");
            }
            // radiance journal grant - verified reusable
            if (CheckIsFsm(self, "Radiance", "Control") && self.Fsm.GameObject.scene.name == SceneNames.Dream_Final_Boss)
            {
                Record("FinalBoss");
            }
            if (CheckIsFsm(self, "Absolute Radiance", "Control") && self.Fsm.GameObject.scene.name == SceneNames.GG_Radiance)
            {
                Record("FinalBoss");
            }
            // oro & mato journal grant - gated behind a PDBoolTest
            if (CheckIsFsm(self, "Brothers", "Combo Control") && self.Fsm.GameObject.scene.name == SceneNames.GG_Nailmasters)
            {
                Record("NailBros");
            }
            // sly journal grant - gated behind a PDBoolTest
            if (CheckIsFsm(self, "Sly Boss", "Control") && self.Fsm.GameObject.scene.name == SceneNames.GG_Sly)
            {
                Record("NailSage");
            }
        }

        private void OnFsmEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            // this hook handles any case where player data is set conditionally (and therefore the SetPlayerDataBool action is not hookable)
            // by injecting an additional state in the middle of an appropriate transition.

            // wingsmoulds
            if (self.gameObject.name.StartsWith("White Palace Fly") && self.FsmName == "Control")
            {
                InjectRecordState(self, "Journal Entry?", "FINISHED", "Journal Update?", "PalaceFly");
            }
            // siblings
            if (self.gameObject.name.StartsWith("Shade Sibling") && self.FsmName == "Control")
            {
                InjectRecordState(self, "Journal Entry?", "FINISHED", "Journal Update?", "Sibling");
            }
            // grimmkin
            if (self.gameObject.name.StartsWith("Flamebearer Small") && self.FsmName == "Control")
            {
                InjectRecordState(self, "Fanfare 1", "FINISHED", "Flash Start", "FlameBearerSmall");
            }
            if (self.gameObject.name.StartsWith("Flamebearer Med") && self.FsmName == "Control")
            {
                InjectRecordState(self, "Fanfare 2", "FINISHED", "Flash Start", "FlameBearerMed");
            }
            if (self.gameObject.name.StartsWith("Flamebearer Large") && self.FsmName == "Control")
            {
                InjectRecordState(self, "Fanfare 3", "FINISHED", "Flash Start", "FlameBearerLarge");
            }
            // hopping/winged zotelings
            if ((self.gameObject.name.StartsWith("Zoteling") || self.gameObject.name.StartsWith("Ordeal Zoteling"))
                && self.FsmName == "Control")
            {
                FsmBool livingVar = self.AddFsmBool("Alive", false);
                FsmString pdVar = self.AddFsmString("Zoteling PD Name", "Dummy");

                FsmState winged = self.GetState("Buzzer Start");
                winged.AddLastAction(new SetBoolValue()
                {
                    boolVariable = livingVar,
                    boolValue = true,
                    everyFrame = false,
                });
                winged.AddLastAction(new SetStringValue()
                {
                    stringVariable = pdVar,
                    stringValue = "ZotelingBuzzer",
                    everyFrame = false,
                });

                FsmState hopping = self.GetState("Hopper Start");
                hopping.AddLastAction(new SetBoolValue()
                {
                    boolVariable = livingVar,
                    boolValue = true,
                    everyFrame = false,
                });
                hopping.AddLastAction(new SetStringValue()
                {
                    stringVariable = pdVar,
                    stringValue = "ZotelingHopper",
                    everyFrame = false
                });

                self.InjectState("Die", "FINISHED", "Reset", new FsmState(self.Fsm)
                {
                    Name = "Record Kill Count",
                    Actions = new[]
                    {
                        new ZotelingRecordKill(this, pdVar, livingVar)
                    }
                });
            }
            // volatile zotelings
            if (self.gameObject.name.StartsWith("Zote Balloon") && self.FsmName == "Control")
            {
                InjectRecordState(self, "Die", "WAIT", "Reset", "ZotelingBalloon");
            }
        }

        private bool CheckIsFsm(FsmStateAction self, string goName, string fsmName)
        {
            return self.Fsm.GameObjectName == goName && self.Fsm.Name == fsmName;
        }

        private void InjectRecordState(PlayMakerFSM self, string fromState, string fromEvent, string toState, string pdName)
        {
            FsmState newState = new(self.Fsm)
            {
                Name = "Record Kill Count",
                Actions = new[]
                {
                    new Lambda(() => Record(pdName))
                }
            };
            self.InjectState(fromState, fromEvent, toState, newState);
        }
    }
}
