using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using Modding;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheRealJournalRando.Data;
using TheRealJournalRando.Fsm;
using Module = ItemChanger.Modules.Module;

namespace TheRealJournalRando.IC
{
    public class JournalControlModule : Module
    {
        private record BossJournalUpdateInfo(string PlayerDataName, string JournalStateName = "Journal");
        private record MultiJournalUpdateInfo(string PlayerDataName, string GOPrefix);

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

        private ParametricFsmEditBuilder<BossJournalUpdateInfo, string>? journalBlockers;
        private ParametricFsmEditBuilder<MultiJournalUpdateInfo, string>? multiJournalBlockers;
        private Dictionary<string, Func<string>?> notesPreviews = new();
        private ILHook? ilOrigRecordKillForJournal;

        public override void Initialize()
        {
            IL.JournalList.UpdateEnemyList += ILUpdateEnemyList;
            IL.JournalEntryStats.OnEnable += ILEnableJournalStats;
            On.JournalEntryStats.Awake += RerouteShadeEntryPd;
            On.PlayerData.CountJournalEntries += RecountJournalEntries;
            ModHooks.SetPlayerBoolHook += JournalDataSetOverride;
            ModHooks.GetPlayerBoolHook += JournalDataGetOverride;

            journalBlockers = new(x => x.PlayerDataName, GetJournalMessageBlocker);
            multiJournalBlockers = new(x => x.PlayerDataName, GetMultiJournalMessageBlocker);

            Events.AddFsmEdit(new("Enemy List", "Item List Control"), EditJournalUI);
            Events.AddFsmEdit(new("False Knight New", "FalseyControl"), journalBlockers.GetOrAddEdit(new("FalseKnight", "Open Map Shop and Journal")));
            Events.AddFsmEdit(new("Mantis Battle", "Battle Control"), journalBlockers.GetOrAddEdit(new("MantisLord")));
            Events.AddFsmEdit(SceneNames.Ruins2_03_boss, new("Battle Control", "Battle Control"), journalBlockers.GetOrAddEdit(new("BlackKnight")));
            Events.AddFsmEdit(new("Jar Collector", "Death"), journalBlockers.GetOrAddEdit(new("JarCollector", "Set Data")));
            Events.AddFsmEdit(SceneNames.GG_Nailmasters, new("Brothers", "Combo Control"), journalBlockers.GetOrAddEdit(new("NailBros")));
            Events.AddFsmEdit(new("Sly Boss", "Control"), journalBlockers.GetOrAddEdit(new("Nailsage")));
            Events.AddFsmEdit(new("Control"), multiJournalBlockers.GetOrAddEdit(new("Sibling", "Shade Sibling")));
            Events.AddFsmEdit(new("Control"), multiJournalBlockers.GetOrAddEdit(new("PalaceFly", "White Palace Fly")));

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
            IL.JournalList.UpdateEnemyList -= ILUpdateEnemyList;
            IL.JournalEntryStats.OnEnable -= ILEnableJournalStats;
            On.JournalEntryStats.Awake -= RerouteShadeEntryPd;
            On.PlayerData.CountJournalEntries -= RecountJournalEntries;
            ModHooks.SetPlayerBoolHook -= JournalDataSetOverride;
            ModHooks.GetPlayerBoolHook -= JournalDataGetOverride;

            Events.RemoveFsmEdit(new("Enemy List", "Item List Control"), EditJournalUI);
            Events.RemoveFsmEdit(new("False Knight New", "FalseyControl"), journalBlockers?["FalseKnight"]);
            Events.RemoveFsmEdit(new("Mantis Battle", "Battle Control"), journalBlockers?["MantisLord"]);
            Events.RemoveFsmEdit(SceneNames.Ruins2_03_boss, new("Battle Control", "Battle Control"), journalBlockers?["BlackKnight"]);
            Events.RemoveFsmEdit(new("Jar Collector", "Death"), journalBlockers?["JarCollector"]);
            Events.RemoveFsmEdit(SceneNames.GG_Nailmasters, new("Brothers", "Combo Control"), journalBlockers?["NailBros"]);
            Events.RemoveFsmEdit(new("Sly Boss", "Control"), journalBlockers?["Nailsage"]);
            Events.RemoveFsmEdit(new("Control"), multiJournalBlockers?["Sibling"]);
            Events.RemoveFsmEdit(new("Control"), multiJournalBlockers?["PalaceFly"]);

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
                    return pdName == "Dummy" || EnemyEntryIsRegistered(pdName) || EnemyNotesIsRegistered(pdName);
                });
                cursor.Emit(OpCodes.Brtrue_S, il.Instrs.Last());
            }
        }

        private void RecountJournalEntries(On.PlayerData.orig_CountJournalEntries orig, PlayerData self)
        {
            // start with the shade counted
            int completedEntries = 1;
            int completedNotes = 1;
            int totalEntries = 146;
            int bonusEntries = 0; // for debugging purposes; not put in PD

            foreach (EnemyDef enemy in EnemyData.NormalData.Values.Append(EnemyData.SpecialData.Mossy_Vagabond))
            {
                bool entryComplete = EnemyEntryIsRegistered(enemy.pdName) ? hasEntry[enemy.pdName] : self.GetBool("killed" + enemy.pdName);
                bool notesComplete = EnemyNotesIsRegistered(enemy.pdName) ? hasNotes[enemy.pdName] : self.GetInt("kills" + enemy.pdName) <= 0;
                if (enemy.ignoredForHunterMark)
                {
                    // bonus entries add to the total count if entry is complete, and the corresponding completed count if completed
                    if (entryComplete || notesComplete)
                    {
                        totalEntries++;
                        bonusEntries++;
                    }
                }

                if (entryComplete)
                {
                    completedEntries++;
                }
                if (notesComplete)
                {
                    completedNotes++;
                }
            }
            TheRealJournalRando.Instance.LogDebug($"Overridden entry count ({totalEntries} total including {bonusEntries} extra). {completedEntries} completed entries, {completedNotes} completed notes.");
            self.SetInt(nameof(PlayerData.journalEntriesTotal), totalEntries);
            self.SetInt(nameof(PlayerData.journalEntriesCompleted), completedEntries);
            self.SetInt(nameof(PlayerData.journalNotesCompleted), completedNotes);
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

        private void EditJournalUI(PlayMakerFSM self)
        {
            FsmState notesCheck = self.GetState("Notes?");
            notesCheck.Actions[3] = new HasNotesComparisonProxy(this);

            FsmState displayKillsState = self.GetState("Display Kills");
            displayKillsState.Actions[4] = new KillCounterDisplayProxy(this);

            FsmState displayNotesState = self.GetState("Get Notes");
            displayNotesState.Actions[4] = new NotesDisplayProxy(this);
        }

        private void ReplaceHasJournalCheck(FsmState journalState, string pdName)
        {
            PlayerDataBoolTest journalCheckAction = journalState.GetActionsOfType<PlayerDataBoolTest>().First(x => x.boolName.Value == "hasJournal");
            int idx = journalState.Actions.IndexOf(journalCheckAction);
            journalState.Actions[idx] = new DelegateBoolTest(() =>
            {
                bool isRegistered = EnemyEntryIsRegistered(pdName) || EnemyNotesIsRegistered(pdName);

                return !isRegistered && PlayerData.instance.GetBool("hasJournal");
            }, journalCheckAction);
        }

        private Action<PlayMakerFSM> GetJournalMessageBlocker(BossJournalUpdateInfo info)
        {
            return (self) =>
            {
                FsmState journalState = self.GetState(info.JournalStateName);
                ReplaceHasJournalCheck(journalState, info.PlayerDataName);
            };
        }

        private Action<PlayMakerFSM> GetMultiJournalMessageBlocker(MultiJournalUpdateInfo info)
        {
            return (self) =>
            {
                if (self.gameObject.name.StartsWith(info.GOPrefix))
                {
                    FsmState entryState = self.GetState("Journal Entry?");
                    ReplaceHasJournalCheck(entryState, info.PlayerDataName);

                    FsmState updateState = self.GetState("Journal Update?");
                    ReplaceHasJournalCheck(updateState, info.PlayerDataName);
                }
            };
        }
    }
}
