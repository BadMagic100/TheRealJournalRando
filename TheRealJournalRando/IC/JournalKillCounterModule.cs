using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Modules;
using Modding;
using System.Collections.Generic;
using System.Linq;
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
            On.ScuttlerControl.Hit += OnScuttlerKilled;
            On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter += OnSetPlayerDataBoolAction;
        }

        public override void Unload()
        {
            ModHooks.RecordKillForJournalHook -= OnJournalRecord;
            On.PlayMakerFSM.OnEnable -= OnFsmEnable;
            On.ScuttlerControl.Hit -= OnScuttlerKilled;
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
            // ignore lost kin - other dream variants already do this
            if (goName == "Lost Kin")
            {
                playerDataName = "Dummy";
            }

            Record(playerDataName);
        }

        private void OnScuttlerKilled(On.ScuttlerControl.orig_Hit orig, ScuttlerControl self, HitInstance damageInstance)
        {
            bool wasAlive = ReflectionHelper.GetField<ScuttlerControl, bool>(self, "alive");

            orig(self, damageInstance);

            bool stillAlive = ReflectionHelper.GetField<ScuttlerControl, bool>(self, "alive");
            if (wasAlive && !stillAlive)
            {
                string pdName = self.killsPDBool.Substring(5);
                Record(pdName);
            }
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

            // overworld TMG journal grant - godhome TMG will need special handling. probably in Grimm Boss-Control between Death Explode and Send Death Event
            if (CheckIsFsm(self, "Defeated NPC", "Conversation Control") && self.Fsm.GameObject.scene.name == SceneNames.Grimm_Main_Tent)
            {
                Record("Grimm");
            }
            // NKG journal grant - in vanilla, godhome NKG can't grant you journal; in rando you can fight NKG even after banishment though
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
        }

        private void OnFsmEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            // this hook handles any case where player data is set conditionally (and therefore the SetPlayerDataBool action is not hookable)
            // by injecting an additional state in the middle of an appropriate transition.

            /***** Bosses *****/

            // False knight
            if (CheckFsmStrong(self, "False Knight New", "FalseyControl"))
            {
                InjectRecordState(self, "Open Map Shop and Journal", "FINISHED", "Steam", "FalseKnight");
            }
            // Mantis lords - Sisters of Battle (in GG_Mantis_Lords_V) uses a different FSM (same name)
            // and does not grant journal entries normally; will not be handled
            if (CheckFsmStrong(self, "Mantis Battle", "Battle Control", SceneNames.Fungus2_15_boss, SceneNames.GG_Mantis_Lords))
            {
                InjectRecordState(self, "Journal", "FINISHED", "Return 2", "MantisLord");
            }
            // watcher knights can otherwise be handled by hooking the PD setter, but unlike other reusable bosses you actually see a journal
            // update message from the journal state. just injecting a state later to avoid this
            if (CheckFsmStrong(self, "Battle Control", "Battle Control", SceneNames.Ruins2_03_boss, SceneNames.GG_Watcher_Knights))
            {
                InjectRecordState(self, "Pause 5", "FINISHED", "Music End", "BlackKnight");
            }
            // Collector
            if (CheckFsmStrong(self, "Jar Collector", "Death"))
            {
                InjectRecordState(self, "Set Data", "FINISHED", "Fall", "JarCollector");
            }
            // godhome troupe master grimm
            if (CheckFsmStrong(self, "Grimm Boss", "Control", SceneNames.GG_Grimm))
            {
                InjectRecordState(self, "Death Explode", "GG BOSS", "Send Death Event", "Grimm");
            }
            // oro & mato
            if (CheckFsmStrong(self, "Brothers", "Combo Control", SceneNames.GG_Nailmasters))
            {
                InjectRecordState(self, "Journal", "FINISHED", "Defeated 2", "NailBros");
            }
            // nailsage sly
            if (CheckFsmStrong(self, "Sly Boss", "Control"))
            {
                InjectRecordState(self, "Journal", "FINISHED", "Death Launch", "NailSage");
            }

            /***** "Special" Enemies *****/

            // wingsmoulds
            if (CheckFsmWeak(self, "White Palace Fly", "Control"))
            {
                InjectRecordState(self, "Journal Entry?", "FINISHED", "Journal Update?", "PalaceFly");
            }
            // siblings
            if (CheckFsmWeak(self, "Shade Sibling", "Control"))
            {
                InjectRecordState(self, "Journal Entry?", "FINISHED", "Journal Update?", "Sibling");
            }
            // grimmkin (this can be a strong comparison)
            if (CheckFsmWeak(self, "Flamebearer Small", "Control"))
            {
                InjectRecordState(self, "Fanfare 1", "FINISHED", "Flash Start", "FlameBearerSmall");
            }
            if (CheckFsmWeak(self, "Flamebearer Med", "Control"))
            {
                InjectRecordState(self, "Fanfare 2", "FINISHED", "Flash Start", "FlameBearerMed");
            }
            if (CheckFsmWeak(self, "Flamebearer Large", "Control"))
            {
                InjectRecordState(self, "Fanfare 3", "FINISHED", "Flash Start", "FlameBearerLarge");
            }
            // hopping/winged zotelings
            if (CheckFsmWeak(self, "Zoteling", "Control") || CheckFsmWeak(self, "Ordeal Zoteling", "Control"))
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
            if (CheckFsmWeak(self, "Zote Balloon", "Control"))
            {
                InjectRecordState(self, "Die", "WAIT", "Reset", "ZotelingBalloon");
            }
        }

        private bool CheckIsFsm(FsmStateAction self, string goName, string fsmName)
        {
            return self.Fsm.GameObjectName == goName && self.Fsm.Name == fsmName;
        }

        private bool CheckFsmStrong(PlayMakerFSM self, string goName, string fsmName, params string[] scenes)
        {
            if (self.gameObject.name != goName)
            {
                return false;
            }
            return CheckFsmWeak(self, goName, fsmName, scenes);
        }

        private bool CheckFsmWeak(PlayMakerFSM self, string goNamePrefix, string fsmName, params string[] scenes)
        {
            bool validScene = scenes.Length == 0 || scenes.Contains(self.gameObject.scene.name);
            return validScene && self.gameObject.name.StartsWith(goNamePrefix) && self.FsmName == fsmName;
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
