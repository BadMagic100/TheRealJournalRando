using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Modules;
using Modding;
using System.Collections.Generic;

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

        private void Record(string playerDataName)
        {
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
            if (playerDataName == "Dummy")
            {
                return;
            }

            Record(playerDataName);
        }

        private void OnSetPlayerDataBoolAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetPlayerDataBool self)
        {
            orig(self);
            // this hook handles most special cases (i.e. bosses) that unconditionally set the killedX PD bool to grant the journal.
            if (!self.boolName.Value.StartsWith("killed") && self.value.Value)
            {
                return;
            }

            // FK journal grant
            if (self.Fsm.GameObjectName == "False Knight New" && self.Fsm.Name == "FalseyControl" && self.Fsm.GameObject.scene.name == "Crossroads_10_boss")
            {
                Record("FalseKnight");
            }
            // WK journal grant
            if (self.Fsm.GameObjectName == "Battle Control" && self.Fsm.Name == "Battle Control" && self.Fsm.GameObject.scene.name == "Ruins2_03_boss")
            {
                Record("BlackKnight");
            }
            // Collector journal grant
            if (self.Fsm.GameObjectName == "Jar Collector" && self.Fsm.Name == "Death" && self.Fsm.GameObject.scene.name == "Ruins2_11_boss")
            {
                Record("JarCollector");
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
        }

        private void InjectRecordState(PlayMakerFSM self, string fromState, string fromEvent, string toState, string pdName)
        {
            FsmState from = self.GetState(fromState);
            FsmState to = self.GetState(toState);
            FsmState newState = new(self.Fsm)
            {
                Name = "Record Kill Count",
                Actions = new[]
                {
                    new Lambda(() => Record(pdName))
                }
            };

            self.AddState(newState);
            from.RemoveTransitionsOn(fromEvent);
            from.AddTransition(fromEvent, newState);
            newState.AddTransition("FINISHED", to);
        }
    }
}
