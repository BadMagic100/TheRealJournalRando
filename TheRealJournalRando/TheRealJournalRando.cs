using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using ItemChanger.Util;
using Modding;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using TheRealJournalRando.Data;
using TheRealJournalRando.IC;
using FormatString = TheRealJournalRando.IC.FormatString;

namespace TheRealJournalRando
{
    public class TheRealJournalRando : Mod, IGlobalSettings<GlobalSettings>
    {
        const string JOURNAL_ENTRIES = "Journal Entries";

        private static TheRealJournalRando? _instance;

        internal static TheRealJournalRando Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"{nameof(TheRealJournalRando)} was never initialized");
                }
                return _instance;
            }
        }

        public GlobalSettings GS { get; set; } = new();

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        private static Hook? lazyHook;
        private static readonly MethodInfo AddChangeSceneToShiny = typeof(ShinyUtility).GetMethod(nameof(ShinyUtility.AddChangeSceneToShiny));

        public TheRealJournalRando() : base()
        {
            _instance = this;
            lazyHook = new Hook(AddChangeSceneToShiny, AdjustDreamExitRule);
        }

        private static void AdjustDreamExitRule(Action<PlayMakerFSM, Transition> orig, PlayMakerFSM shinyFsm, Transition t)
        {
            if (t.GateName == "door_Land_of_Storms_return")
            {
                shinyFsm.FsmVariables.FindFsmBool("Exit Dream").Value = true;
                shinyFsm.FsmVariables.FindFsmString("Return Door").Value = "door_Land_of_Storms_return";
                shinyFsm.GetState("Fade Pause").AddFirstAction(new Lambda(() =>
                {
                    PlayerData.instance.SetString(nameof(PlayerData.dreamReturnScene), t.SceneName);
                    HeroController.instance.proxyFSM.FsmVariables.GetFsmBool("No Charms").Value = false;
                    // fixes minion spawning issue after Dream Nail, Dreamers, etc
                    // could extremely rarely be undesired, if the target scene is in Godhome
                }));
            }
            else
            {
                orig(shinyFsm, t);
            }
        }

        public override void Initialize()
        {
            Log("Initializing");
            HookIC();

            if (ModHooks.GetMod("Randomizer 4") is Mod)
            {
                Rando.RandoInterop.HookRandomizer();
            }

            Log("Initialized");
        }

        public void HookIC()
        {
            Events.OnItemChangerHook += LanguageData.Hook;
            Events.OnItemChangerUnhook += LanguageData.Unhook;

            Finder.GetItemOverride += ReplaceOriginalJournalUIDefs;

            // todo - deal with special ones

            foreach (EnemyDef enemyDef in EnemyData.NormalData.Values)
            {
                DefineStandardEntryAndNoteItems(enemyDef);
                DefineStandardEntryAndNoteLocations(enemyDef);
            }
            DefineStandardEntryAndNoteItems(EnemyData.SpecialData.Mossy_Vagabond);
            DefineFullEntryItem(EnemyData.SpecialData.Weathered_Mask);

            Finder.DefineCustomLocation(new ObjectLocation()
            {
                name = EnemyData.SpecialData.Weathered_Mask.icName.AsEntryName(),
                sceneName = SceneNames.GG_Land_of_Storms,
                objectName = "Shiny Item GG Storms",
                flingType = FlingType.DirectDeposit,
                forceShiny = true,
                tags = new List<Tag>()
                {
                    new ChangeSceneTag()
                    {
                        changeTo = new Transition(SceneNames.GG_Atrium_Roof, "door_Land_of_Storms_return")
                    }
                }
            });
        }

        private void ReplaceOriginalJournalUIDefs(GetItemEventArgs args)
        {
            switch (args.ItemName)
            {
                case ItemNames.Journal_Entry_Goam:
                case ItemNames.Journal_Entry_Garpede:
                case ItemNames.Journal_Entry_Charged_Lumafly:
                case ItemNames.Journal_Entry_Void_Tendrils:
                case ItemNames.Journal_Entry_Seal_of_Binding:
                    if (Finder.GetItemInternal(args.ItemName) is JournalEntryItem jei && jei.UIDef is MsgUIDef md)
                    {
                        md.sprite = new JournalBadgeSprite(jei.playerDataName);
                        args.Current = jei;
                    }
                    break;
                default:
                    break;
            }
        }

        private void DefineStandardEntryAndNoteItems(EnemyDef enemyDef)
        {
            string entryName = enemyDef.icName.AsEntryName();
            string notesName = enemyDef.icName.AsNotesName();
            LanguageString localizedEnemyName = new("Journal", $"NAME_{enemyDef.convoName}");

            Finder.DefineCustomItem(new EnemyJournalEntryOnlyItem(enemyDef.pdName)
            {
                name = entryName,
                UIDef = new MsgUIDef
                {
                    name = new FormatString(new LanguageString("Fmt", "ENTRY_ITEM_NAME"), localizedEnemyName.Clone()),
                    shopDesc = new FormatString(new LanguageString("Fmt", "ENTRY_SHOP_DESC"), localizedEnemyName.Clone()),
                    sprite = new JournalBadgeSprite(enemyDef.pdName),
                },
                tags = new()
                    {
                        InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES)
                    }
            });
            Finder.DefineCustomItem(new EnemyJournalNotesOnlyItem(enemyDef.pdName)
            {
                name = notesName,
                UIDef = new MsgUIDef
                {
                    name = new FormatString(new LanguageString("Fmt", "NOTES_ITEM_NAME"), localizedEnemyName.Clone()),
                    shopDesc = new FormatString(new LanguageString("Fmt", "NOTES_SHOP_DESC"), localizedEnemyName.Clone()),
                    sprite = new JournalBadgeSprite(enemyDef.pdName),
                },
                tags = new()
                    {
                        InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES)
                    }
            });
        }

        private void DefineFullEntryItem(EnemyDef enemyDef)
        {
            string name = enemyDef.icName.AsEntryName();
            LanguageString localizedEnemyName = new("Journal", $"NAME_{enemyDef.convoName}");
            Finder.DefineCustomItem(new JournalEntryItem()
            {
                name = name,
                playerDataName = enemyDef.pdName,
                UIDef = new MsgUIDef()
                {
                    name = new FormatString(new LanguageString("Fmt", "ENTRY_ITEM_NAME"), localizedEnemyName.Clone()),
                    shopDesc = new PaywallString("Journal", $"NOTE_{enemyDef.convoName}"),
                    sprite = new JournalBadgeSprite(enemyDef.pdName),
                }
            });
        }

        private void DefineStandardEntryAndNoteLocations(EnemyDef enemyDef)
        {
            string entryName = enemyDef.icName.AsEntryName();
            string notesName = enemyDef.icName.AsNotesName();

            Finder.DefineCustomLocation(new EnemyJournalLocation(enemyDef.pdName, EnemyJournalLocationType.Entry)
            {
                name = entryName,
                flingType = FlingType.Everywhere,
                tags = new()
                    {
                        InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES),
                        InteropTagFactory.RecentItemsLocationTag(sourceOverride: "the Hunter")
                    }
            });
            Finder.DefineCustomLocation(new EnemyJournalLocation(enemyDef.pdName, EnemyJournalLocationType.Notes)
            {
                name = notesName,
                flingType = FlingType.Everywhere,
                tags = new()
                    {
                        InteropTagFactory.CmiSharedTag(poolGroup: JOURNAL_ENTRIES),
                        InteropTagFactory.RecentItemsLocationTag(sourceOverride: "the Hunter")
                    }
            });
        }

        public void OnLoadGlobal(GlobalSettings s) => GS = s;

        public GlobalSettings OnSaveGlobal() => GS;
    }
}
