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
        }

        public override void Unload()
        {
            ModHooks.RecordKillForJournalHook -= OnJournalRecord;
            On.PlayMakerFSM.Awake -= OnFsmAwake;
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

        private void OnFsmAwake(On.PlayMakerFSM.orig_Awake orig, PlayMakerFSM self)
        {
            orig(self);
            // WK journal grant
            if (self.gameObject.name == "Battle Control" && self.FsmName == "Battle Control" && self.gameObject.scene.name == "Ruins2_03_boss")
            {
                self.GetState("Journal").AddFirstAction(new Lambda(() => Record("BlackKnight")));
            }
            // collector journal grant
            if (self.gameObject.name == "Jar Collector" && self.FsmName == "Death" && self.gameObject.scene.name == "Ruins2_11_boss")
            {
                TheRealJournalRando.Instance.LogDebug($"Collector death FSM in {self.gameObject.scene.name}");
                self.GetState("Set Data").AddFirstAction(new Lambda(() => Record("JarCollector")));
            }
        }
    }
}
