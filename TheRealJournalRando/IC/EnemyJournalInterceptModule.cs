using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.Modules;
using Modding;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using TheRealJournalRando.JournalManip;

namespace TheRealJournalRando.IC
{
    public class EnemyJournalInterceptModule : Module
    {
        public HashSet<string> registeredEnemies = new();
        public Dictionary<string, bool> hasEntry = new();
        public Dictionary<string, bool> hasNotes = new();
        public Dictionary<string, int> enemyKillCounts = new();


        public override void Initialize()
        {
            On.PlayMakerFSM.OnEnable += OnFsmEnable;
            IL.JournalList.UpdateEnemyList += ILUpdateEnemyList;
            IL.JournalEntryStats.OnEnable += ILEnableJournalStats;
            ModHooks.SetPlayerBoolHook += JournalDataSetOverride;
            ModHooks.GetPlayerBoolHook += JournalDataGetOverride;
        }

        public override void Unload()
        {
            On.PlayMakerFSM.OnEnable -= OnFsmEnable;
            IL.JournalList.UpdateEnemyList -= ILUpdateEnemyList;
            IL.JournalEntryStats.OnEnable -= ILEnableJournalStats;
            ModHooks.SetPlayerBoolHook -= JournalDataSetOverride;
            ModHooks.GetPlayerBoolHook -= JournalDataGetOverride;
        }

        public void RegisterEnemy(string pdName)
        {
            if (!EnemyIsRegistered(pdName))
            {
                registeredEnemies.Add(pdName);
                enemyKillCounts[pdName] = 0;
                hasEntry[pdName] = false;
                hasNotes[pdName] = false;
            }
        }

        public bool EnemyIsRegistered(string pdName)
        {
            return registeredEnemies.Contains(pdName);
        }

        private bool RemovePrefixAndCheckRegistered(string pdName, string prefix, ref string enemyName)
        {
            if (pdName.StartsWith(prefix))
            {
                enemyName = pdName.Substring(prefix.Length);
                return EnemyIsRegistered(enemyName);
            }
            return false;
        }

        private void ILUpdateEnemyList(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            if (cursor.TryGotoNext(
                i => i.MatchCallvirt(typeof(JournalEntryStats), nameof(JournalEntryStats.GetPlayerDataBoolName))
            ))
            {
                cursor.Remove();
                cursor.Emit(OpCodes.Ldfld, typeof(JournalEntryStats).GetField(nameof(JournalEntryStats.playerDataName)));
                cursor.EmitDelegate<Func<string, string>>(pdName =>
                {
                    if (EnemyIsRegistered(pdName))
                    {
                        return nameof(hasEntry) + pdName;
                    }
                    return "killed" + pdName;
                });
            }
        }
        private void ILEnableJournalStats(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            if (cursor.TryGotoNext(
                i => i.MatchLdfld(typeof(JournalEntryStats), nameof(JournalEntryStats.playerDataKillsName))
            ))
            {
                cursor.Remove();
                cursor.Remove();
                cursor.Emit(OpCodes.Ldfld, typeof(JournalEntryStats).GetField(nameof(JournalEntryStats.playerDataName)));
                cursor.EmitDelegate<Func<PlayerData, string, int>>((pd, pdName) =>
                {
                    if (EnemyIsRegistered(pdName))
                    {
                        return pd.GetBool(nameof(hasNotes) + pdName) ? 0 : 1;
                    }
                    return pd.GetInt("kills" + pdName);
                });
            }
        }

        private bool JournalDataGetOverride(string name, bool value)
        {
            string enemy = "";
            if (RemovePrefixAndCheckRegistered(name, nameof(hasEntry), ref enemy))
            {
                return hasEntry[enemy];
            }
            else if (RemovePrefixAndCheckRegistered(name, nameof(hasNotes), ref enemy))
            {
                return hasNotes[enemy];
            }
            return value;
        }

        private bool JournalDataSetOverride(string name, bool value)
        {
            string enemy = "";
            if (RemovePrefixAndCheckRegistered(name, nameof(hasEntry), ref enemy))
            {
                hasEntry[enemy] = value;
            }
            else if (RemovePrefixAndCheckRegistered(name, nameof(hasNotes), ref enemy))
            {
                hasNotes[enemy] = value;
            }
            return value;
        }

        private void OnFsmEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            if (self.gameObject.name == "Enemy List" && self.FsmName == "Item List Control")
            {
                FsmState notesCheck = self.GetState("Notes?");
                notesCheck.RemoveAction(3);
                notesCheck.AddLastAction(new NotesInterceptProxyCompare(this));
            }
        }
    }
}
