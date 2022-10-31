using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using RandomizerMod;
using RandomizerMod.Menu;
using System.Linq;
using UnityEngine;

namespace TheRealJournalRando.Rando
{
    public class ConnectionMenu
    {
        private const int VSPACE_SMALL = 50;
        private const int VSPACE_MED = 200;
        private const int VSPACE_LARGE = 350;
        private const int HSPACE_LARGE = 300;
        private const int HSPACE_XLARGE = 450;
        private const int HSPACE_XXLARGE = 750;

        private SmallButton? pageRootButton;
        internal MenuPage? journalRandoPage;
        internal MenuEnum<JournalRandomizationType>? randomizationTypeControl;
        internal MenuEnum<StartingItems>? startItemsControl;

        private MenuElementFactory<JournalRandomizationSettings>? toplevelMef;
        private MenuElementFactory<JournalRandomizationSettings.PoolSettings>? poolMef;
        private MenuElementFactory<JournalRandomizationSettings.CostSettings>? costsMef;
        private MenuElementFactory<JournalRandomizationSettings.LongLocationSettings>? llsMef;

        private static ConnectionMenu? instance = null;
        internal static ConnectionMenu Instance => instance ??= new();

        public static void OnExitMenu()
        {
            instance = null;
        }

        public static void Hook()
        {
            RandomizerMenuAPI.AddMenuPage(Instance.ConstructMenu, Instance.HandleButton);
            MenuChangerMod.OnExitMainMenu += OnExitMenu;
        }

        private bool HandleButton(MenuPage landingPage, out SmallButton button)
        {
            pageRootButton = new(landingPage, Localization.Localize("Journal Entries (Extended)"));
            pageRootButton.AddHideAndShowEvent(landingPage, journalRandoPage!);
            SetTopLevelButtonColor();
            button = pageRootButton;
            return true;
        }

        private void ConstructMenu(MenuPage landingPage)
        {
            journalRandoPage = new MenuPage(Localization.Localize("Journal Entries (Extended)"), landingPage);
            VerticalItemPanel toplevelVip = new(journalRandoPage, new Vector2(0, 400), VSPACE_LARGE, true);

            toplevelMef = new(journalRandoPage, RandoInterop.Settings);
            ToggleButton enabledControl = (ToggleButton)toplevelMef.ElementLookup[nameof(JournalRandomizationSettings.Enabled)];
            enabledControl.SelfChanged += EnabledChanged;
            randomizationTypeControl = (MenuEnum<JournalRandomizationType>)toplevelMef.ElementLookup[nameof(JournalRandomizationSettings.JournalRandomizationType)];
            randomizationTypeControl.SelfChanged += RandomizationTypeChanged;
            startItemsControl = (MenuEnum<StartingItems>)toplevelMef.ElementLookup[nameof(JournalRandomizationSettings.StartingItems)];
            startItemsControl.InterceptChanged += StartItemsChanging;
            MenuLabel headingLabel = new(journalRandoPage, "Journal Entries (Extended)");
            VerticalItemPanel toplevelSettingHolder = new(journalRandoPage, Vector2.zero, VSPACE_SMALL, false, toplevelMef.Elements);
            toplevelSettingHolder.Insert(0, headingLabel);
            toplevelVip.Add(toplevelSettingHolder);

            GridItemPanel categoryGrid1 = new(journalRandoPage, Vector2.zero, 2, VSPACE_MED, HSPACE_XXLARGE, false);
            GridItemPanel categoryGrid2 = new(journalRandoPage, Vector2.zero, 1, VSPACE_MED, HSPACE_XXLARGE, false);
            VerticalItemPanel categoryVip = new(journalRandoPage, Vector2.zero, VSPACE_MED, false, categoryGrid1, categoryGrid2);
            toplevelVip.Add(categoryVip);

            poolMef = new(journalRandoPage, RandoInterop.Settings.Pools);
            MenuLabel poolsLabel = new(journalRandoPage, "Pool Settings");
            GridItemPanel poolSettingsHolder1 = new(journalRandoPage, Vector2.zero, 2, VSPACE_SMALL, HSPACE_LARGE, false, poolMef.Elements.Take(2).ToArray());
            GridItemPanel poolSettingsHolder2 = new(journalRandoPage, Vector2.zero, 1, VSPACE_SMALL, HSPACE_LARGE, false, poolMef.Elements[2]);
            categoryGrid1.Add(new VerticalItemPanel(journalRandoPage, Vector2.zero, VSPACE_SMALL, false, poolsLabel, poolSettingsHolder1, poolSettingsHolder2));

            costsMef = new(journalRandoPage, RandoInterop.Settings.Costs);
            MenuLabel costsLabel = new(journalRandoPage, "Cost Randomization Settings");
            GridItemPanel costsSettingsHolder = new(journalRandoPage, Vector2.zero, 2, VSPACE_SMALL, HSPACE_LARGE, false, costsMef.Elements.Skip(1).ToArray());
            VerticalItemPanel costsLabelsHolder = new(journalRandoPage, Vector2.zero, VSPACE_SMALL, false, costsLabel, costsMef.Elements[0]);
            categoryGrid1.Add(new VerticalItemPanel(journalRandoPage, Vector2.zero, VSPACE_SMALL * 2 + 35, false, costsLabelsHolder, costsSettingsHolder));

            llsMef = new(journalRandoPage, RandoInterop.Settings.LongLocations);
            MenuLabel llsLabel = new(journalRandoPage, "Long Location Settings");
            GridItemPanel llsBoolSettingsHolder = new(journalRandoPage, Vector2.zero, 2, VSPACE_SMALL, HSPACE_XLARGE, false, llsMef.Elements.Take(4).ToArray());
            VerticalItemPanel llsSettingsHolder = new(journalRandoPage, Vector2.zero, VSPACE_SMALL * 2f, false, llsBoolSettingsHolder, llsMef.Elements[4]);
            categoryGrid2.Add(new VerticalItemPanel(journalRandoPage, Vector2.zero, VSPACE_SMALL, false, llsLabel, llsSettingsHolder));

            Localization.Localize(headingLabel);
            Localization.Localize(toplevelMef);
            Localization.Localize(poolsLabel);
            Localization.Localize(poolMef);
            Localization.Localize(costsLabel);
            Localization.Localize(costsMef);
            Localization.Localize(llsLabel);
            Localization.Localize(llsMef);

            toplevelVip.ResetNavigation();
            toplevelVip.SymSetNeighbor(Neighbor.Down, journalRandoPage.backButton);

        }

        private void SetTopLevelButtonColor()
        {
            if (pageRootButton != null)
            {
                pageRootButton.Text.color = RandoInterop.Settings.Enabled ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            }
        }

        private void EnabledChanged(IValueElement obj)
        {
            SetTopLevelButtonColor();
        }

        private void RandomizationTypeChanged(IValueElement obj)
        {
            if (randomizationTypeControl?.Value.HasFlag(JournalRandomizationType.EntriesOnly) == true)
            {
                startItemsControl?.SetValue(startItemsControl.Value & ~StartingItems.Entries);
            }
        }

        private void StartItemsChanging(MenuItem self, ref object newValue, ref bool cancelChange)
        {
            if (randomizationTypeControl?.Value.HasFlag(JournalRandomizationType.EntriesOnly) == true)
            {
                newValue = ((StartingItems)newValue) & ~StartingItems.Entries;
            }
        }

        public void Disable()
        {
            IValueElement? elem = toplevelMef?.ElementLookup[nameof(JournalRandomizationSettings.Enabled)];
            elem?.SetValue(false);
        }

        public void ApplySettingsToMenu(JournalRandomizationSettings settings)
        {
            toplevelMef?.SetMenuValues(settings);
            poolMef?.SetMenuValues(settings.Pools);
            costsMef?.SetMenuValues(settings.Costs);
            llsMef?.SetMenuValues(settings.LongLocations);
        }
    }
}
