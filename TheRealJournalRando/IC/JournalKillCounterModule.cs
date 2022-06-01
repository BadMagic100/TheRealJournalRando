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
            On.PlayMakerFSM.Awake += OnFsmAwake;
            On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter += OnSetPlayerDataBoolAction;
        }

        public override void Unload()
        {
            ModHooks.RecordKillForJournalHook -= OnJournalRecord;
            On.PlayMakerFSM.Awake -= OnFsmAwake;
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

            // FK journal grant
            if (CheckIsFsm(self, "False Knight New", "FalseyControl") && self.Fsm.GameObject.scene.name == SceneNames.Crossroads_10_boss)
            {
                Record("FalseKnight");
            }
            // Mantis lords journal grant
            if (CheckIsFsm(self, "Mantis Battle", "Battle Control") && self.Fsm.GameObject.scene.name == SceneNames.Fungus2_15_boss)
            {
                Record("MantisLord");
            }
            // WK journal grant
            if (CheckIsFsm(self, "Battle Control", "Battle Control") && self.Fsm.GameObject.scene.name == SceneNames.Ruins2_03_boss)
            {
                Record("BlackKnight");
            }
            // Collector journal grant
            if (CheckIsFsm(self, "Jar Collector", "Death") && self.Fsm.GameObject.scene.name == SceneNames.Ruins2_11_boss)
            {
                Record("JarCollector");
            }
            // grimm journal grants
            if (CheckIsFsm(self, "Defeated NPC", "Conversation Control") && self.Fsm.GameObject.scene.name == SceneNames.Grimm_Main_Tent)
            {
                Record("Grimm");
            }
            if (CheckIsFsm(self, "Grimm Control", "Control") && self.Fsm.GameObject.scene.name == SceneNames.Grimm_Nightmare)
            {
                Record("NightmareGrimm");
            }
            // THK journal grant
            if (CheckIsFsm(self, "Hollow Knight Boss", "Phase Control") && self.Fsm.GameObject.scene.name == SceneNames.Room_Final_Boss_Core)
            {
                Record("HollowKnight");
            }
            // radiance journal grant
            if (CheckIsFsm(self, "Radiance", "Control") && self.Fsm.GameObject.scene.name == SceneNames.Dream_Final_Boss)
            {
                Record("FinalBoss");
            }
            if (CheckIsFsm(self, "Absolute Radiance", "Control") && self.Fsm.GameObject.scene.name == SceneNames.GG_Radiance)
            {
                Record("FinalBoss");
            }
            // oro & mato journal grant
            if (CheckIsFsm(self, "Brothers", "Combo Control") && self.Fsm.GameObject.scene.name == SceneNames.GG_Nailmasters)
            {
                Record("NailBros");
            }
            // sly journal grant
            if (CheckIsFsm(self, "Sly Boss", "Control") && self.Fsm.GameObject.scene.name == SceneNames.GG_Sly)
            {
                Record("NailSage");
            }
        }

        private void OnFsmAwake(On.PlayMakerFSM.orig_Awake orig, PlayMakerFSM self)
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
            if (self.gameObject.name.StartsWith("Zoteling") || self.gameObject.name.StartsWith("Ordeal Zoteling") && self.FsmName == "Control")
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
