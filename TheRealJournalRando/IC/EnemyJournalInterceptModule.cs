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
        public bool hasResetCrawlidPd = false;

        public Dictionary<string, bool> hasEntry = new()
        {
            ["Shade"] = true,
        };
        public Dictionary<string, bool> hasNotes = new()
        {
            ["Shade"] = true,
        };
        public Dictionary<string, int> enemyKillCounts = new()
        {
            ["Shade"] = 0,
        };


        public override void Initialize()
        {
            On.PlayMakerFSM.OnEnable += OnFsmEnable;
            IL.JournalList.UpdateEnemyList += ILUpdateEnemyList;
            IL.JournalEntryStats.OnEnable += ILEnableJournalStats;
            On.JournalEntryStats.Awake += RerouteShadeEntryPd;
            ModHooks.SetPlayerBoolHook += JournalDataSetOverride;
            ModHooks.GetPlayerBoolHook += JournalDataGetOverride;

            if (!hasResetCrawlidPd)
            {
                PlayerData.instance.SetBool("killedCrawler", false);
                PlayerData.instance.SetInt("killsCrawler", 30);
                hasResetCrawlidPd = true;
            }
        }

        public override void Unload()
        {
            On.PlayMakerFSM.OnEnable -= OnFsmEnable;
            IL.JournalList.UpdateEnemyList -= ILUpdateEnemyList;
            IL.JournalEntryStats.OnEnable -= ILEnableJournalStats;
            On.JournalEntryStats.Awake -= RerouteShadeEntryPd;
            ModHooks.SetPlayerBoolHook -= JournalDataSetOverride;
            ModHooks.GetPlayerBoolHook -= JournalDataGetOverride;
        }

        private void EnsureKillCounter(string pdName)
        {
            if (!enemyKillCounts.ContainsKey(pdName))
            {
                enemyKillCounts[pdName] = 0;
            }
        }

        public void RegisterEnemyEntry(string pdName)
        {
            if (!EnemyEntryIsRegistered(pdName))
            {
                EnsureKillCounter(pdName);
                hasEntry[pdName] = false;
            }
        }

        public void RegisterEnemyNotes(string pdName)
        {
            if (!EnemyNotesIsRegistered(pdName))
            {
                EnsureKillCounter(pdName);
                hasNotes[pdName] = false;
            }
        }

        public bool EnemyEntryIsRegistered(string pdName)
        {
            return hasEntry.ContainsKey(pdName);
        }

        public bool EnemyNotesIsRegistered(string pdName)
        {
            return hasNotes.ContainsKey(pdName);
        }

        private bool CheckPdEntryRegistered(string pdName, ref string enemyName)
        {
            string prefix = nameof(hasEntry);
            if (pdName.StartsWith(prefix))
            {
                enemyName = pdName.Substring(prefix.Length);
                return EnemyEntryIsRegistered(enemyName);
            }
            return false;
        }

        private bool CheckPdNotesRegistered(string pdName, ref string enemyName)
        {
            string prefix = nameof(hasNotes);
            if (pdName.StartsWith(prefix))
            {
                enemyName = pdName.Substring(prefix.Length);
                return EnemyNotesIsRegistered(enemyName);
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
                    if (EnemyEntryIsRegistered(pdName))
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
                    if (EnemyNotesIsRegistered(pdName))
                    {
                        return pd.GetBool(nameof(hasNotes) + pdName) ? 0 : 1;
                    }
                    return pd.GetInt("kills" + pdName);
                });
            }
        }

        private void RerouteShadeEntryPd(On.JournalEntryStats.orig_Awake orig, JournalEntryStats self)
        {
            if (self.playerDataName == "Crawler" && self.gameObject.name.Contains("Hollow Shade"))
            {
                self.playerDataName = "Shade";
            }
            orig(self);
        }

        private bool JournalDataGetOverride(string name, bool value)
        {
            string enemy = "";
            if (CheckPdEntryRegistered(name, ref enemy))
            {
                return hasEntry[enemy];
            }
            else if (CheckPdNotesRegistered(name, ref enemy))
            {
                return hasNotes[enemy];
            }
            return value;
        }

        private bool JournalDataSetOverride(string name, bool value)
        {
            string enemy = "";
            if (CheckPdEntryRegistered(name, ref enemy))
            {
                hasEntry[enemy] = value;
            }
            else if (CheckPdNotesRegistered(name, ref enemy))
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
