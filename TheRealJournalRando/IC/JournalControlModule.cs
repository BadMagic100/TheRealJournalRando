using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using Modding;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheRealJournalRando.Fsm;
using Module = ItemChanger.Modules.Module;

namespace TheRealJournalRando.IC
{
    public class JournalControlModule : Module
    {
        private static readonly MethodInfo origRecordKillForJournal = typeof(EnemyDeathEffects).GetMethod("orig_RecordKillForJournal",
            BindingFlags.NonPublic | BindingFlags.Instance);

        public bool hasResetCrawlidPd = false;

        public Dictionary<string, bool> hasEntry = new()
        {
            ["Shade"] = true,
        };
        public Dictionary<string, bool> hasNotes = new()
        {
            ["Shade"] = true,
        };

        private Dictionary<string, Func<string>?> notesPreviews = new();
        private ILHook? ilOrigRecordKillForJournal;

        public override void Initialize()
        {
            On.PlayMakerFSM.OnEnable += OnFsmEnable;
            IL.JournalList.UpdateEnemyList += ILUpdateEnemyList;
            IL.JournalEntryStats.OnEnable += ILEnableJournalStats;
            On.JournalEntryStats.Awake += RerouteShadeEntryPd;
            ModHooks.SetPlayerBoolHook += JournalDataSetOverride;
            ModHooks.GetPlayerBoolHook += JournalDataGetOverride;

            ilOrigRecordKillForJournal = new ILHook(origRecordKillForJournal, ILOverrideJournalMessage);

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

            ilOrigRecordKillForJournal?.Dispose();
        }

        public void RegisterEnemyEntry(string pdName)
        {
            if (!EnemyEntryIsRegistered(pdName))
            {
                hasEntry[pdName] = false;
            }
        }

        public void RegisterEnemyNotes(string pdName)
        {
            if (!EnemyNotesIsRegistered(pdName))
            {
                hasNotes[pdName] = false;
            }
        }

        public void RegisterNotesPreviewHandler(string pdName, Func<string> handlePreview)
        {
            if (notesPreviews.ContainsKey(pdName))
            {
                notesPreviews[pdName] += handlePreview;
            }
            else
            {
                notesPreviews[pdName] = handlePreview;
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

        public string? GetNotesPreview(string pdName)
        {
            if (notesPreviews.ContainsKey(pdName))
            {
                return notesPreviews[pdName]?.Invoke();
            }
            return null;
        }

        public void DeregisterNotesPreviewHandler(string pdName, Func<string> handlePreview)
        {
            if (notesPreviews.ContainsKey(pdName))
            {
                notesPreviews[pdName] -= handlePreview;
            }
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

        private void ILOverrideJournalMessage(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            if (cursor.TryGotoNext(
                i => i.MatchLdsfld(typeof(EnemyDeathEffects), "journalUpdateMessageSpawned")
            ))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(EnemyDeathEffects).GetField("playerDataName", BindingFlags.NonPublic | BindingFlags.Instance));
                cursor.EmitDelegate<Func<string, bool>>(pdName =>
                {
                    return EnemyEntryIsRegistered(pdName) || EnemyNotesIsRegistered(pdName);
                });
                cursor.Emit(OpCodes.Brtrue_S, il.Instrs.Last());
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
                notesCheck.Actions[3] = new HasNotesComparisonProxy(this);

                FsmState displayKillsState = self.GetState("Display Kills");
                displayKillsState.Actions[4] = new KillCounterDisplayProxy(this);

                FsmState displayNotesState = self.GetState("Get Notes");
                displayNotesState.Actions[4] = new NotesDisplayProxy(this);
            }
        }
        
    }
}
