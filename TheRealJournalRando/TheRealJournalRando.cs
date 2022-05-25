using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.Items;
using ItemChanger.Modules;
using ItemChanger.UIDefs;
using MagicUI.Core;
using MagicUI.Elements;
using Modding;
using System;
using System.Linq;
using TheRealJournalRando.IC;

namespace TheRealJournalRando
{
    public class TheRealJournalRando : Mod
    {
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

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public TheRealJournalRando() : base()
        {
            _instance = this;
        }

        (string, string)[] mappings =
            {
                ("Squit", "Mosquito"),
                ("Tiktik", "Climber"),
                ("Lifeseed", "HealthScuttler")
            };
        private LayoutRoot? layout;

        // if you need preloads, you will need to implement GetPreloadNames and use the other signature of Initialize.
        public override void Initialize()
        {
            Log("Initializing");

            ModHooks.RecordKillForJournalHook += OnJournalRecord;
            On.HeroController.Awake += GameStarted;
            On.QuitToMenu.Start += GameExited;

            On.UIManager.StartNewGame += UIManager_StartNewGame;
            AbstractItem.ModifyRedundantItemGlobal += AbstractItem_ModifyRedundantItemGlobal;


            foreach ((string name, string pdName) in mappings)
            {
                string icName = name.Replace(' ', '_');
                AbstractItem entry = new JournalEntryOnlyItem
                {
                    PlayerDataName = pdName,
                    name = $"Journal_Entry_Only-{icName}",
                    UIDef = new MsgUIDef
                    {
                        name = new BoxedString($"{name} Journal Entry"),
                        shopDesc = new BoxedString("something something monty python pet shop"),
                        sprite = new JournalBadgeSprite(pdName)
                    }
                };
                AbstractItem notes = new JournalNotesOnlyItem
                {
                    PlayerDataName = pdName,
                    name = $"Hunter's_Notes-{icName}",
                    UIDef = new MsgUIDef
                    {
                        name = new BoxedString($"{name} Hunter's Notes"),
                        shopDesc = new BoxedString("something something monty python pet shop 2"),
                        sprite = new JournalBadgeSprite(pdName)
                    }
                };
                AbstractItem fullEntry = new JournalEntryItem
                {
                    playerDataName = pdName,
                    name = $"Journal_Entry-{icName}",
                    UIDef = new MsgUIDef
                    {
                        name = new BoxedString($"{name} Journal Entry"),
                        shopDesc = new BoxedString("This is a real bargain, you're getting a whole journal entry!"),
                        sprite = new JournalBadgeSprite(pdName)
                    }
                };
                Finder.DefineCustomItem(entry);
                Finder.DefineCustomItem(notes);
                Finder.DefineCustomItem(fullEntry);
            }

            Log("Initialized");
        }

        private void AbstractItem_ModifyRedundantItemGlobal(GiveEventArgs obj)
        {
            obj.Item = null;
        }

        private void UIManager_StartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
        {
            ItemChangerMod.CreateSettingsProfile(false, false);
            ItemChangerMod.Modules.GetOrAdd<AutoUnlockIselda>();

            AbstractPlacement iseldaShop = Finder.GetLocation(LocationNames.Iselda).Wrap();
            foreach ((string name, _) in mappings)
            {
                string icName = name.Replace(' ', '_');
                iseldaShop.Items.Add(Finder.GetItem($"Journal_Entry_Only-{icName}"));
                iseldaShop.Items.Add(Finder.GetItem($"Hunter's_Notes-{icName}"));
                iseldaShop.Items.Add(Finder.GetItem($"Journal_Entry-{icName}"));
            }
            ItemChangerMod.AddPlacements(iseldaShop.Yield());

            orig(self, permaDeath, bossRush);
        }

        private void GameStarted(On.HeroController.orig_Awake orig, HeroController self)
        {
            orig(self);
            layout = new LayoutRoot(true)
            {
                VisibilityCondition = GameManager.instance.IsGamePaused,
            };
            var btn = new Button(layout)
            {
                Content = "Dump Enemy Names",
                Margin = 20,
                Font = UI.TrajanBold,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Padding = new Padding(15, 200),
            };
            btn.Click += (_) =>
            {
                foreach (string mapping in GameCameras.instance.hudCamera.GetComponentsInChildren<JournalEntryStats>(true)
                    .Select(s => $"{Language.Language.Get(s.nameConvo, "Journal")} => {s.playerDataName}"))
                {
                    LogDebug(mapping);
                }
            };
        }

        private System.Collections.IEnumerator GameExited(On.QuitToMenu.orig_Start orig, QuitToMenu self)
        {
            layout?.Destroy();
            return orig(self);
        }

        private void OnJournalRecord(EnemyDeathEffects enemyDeathEffects, string playerDataName,
            string killedBoolPlayerDataLookupKey, string killCountIntPlayerDataLookupKey, string newDataBoolPlayerDataLookupKey)
        {
            JournalEntryStats stats = GameCameras.instance.hudCamera.GetComponentsInChildren<JournalEntryStats>(true)
                .Where(j => j.playerDataName == playerDataName)
                .FirstOrDefault();

            string realName = Language.Language.Get(stats.nameConvo, "Journal");

            Log($"Killed {realName} ({playerDataName})");
        }


    }
}
