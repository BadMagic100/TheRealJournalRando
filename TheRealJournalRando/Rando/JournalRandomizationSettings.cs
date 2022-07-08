using MenuChanger.Attributes;
using System;

namespace TheRealJournalRando.Rando
{
    [Flags]
    public enum JournalRandomizationType
    {
        None = 0,
        EntriesOnly = 1,
        NotesOnly = 2,
        [MenuLabel("Full Journal Rando")]
        All = 3,
    }

    public enum CostRandomizationType
    {
        Unrandomized,
        [MenuLabel("Random (Fixed Weight)")]
        RandomFixedWeight,
        [MenuLabel("Random (Per Entry)")]
        RandomPerEntry,
    }

    [Flags]
    public enum CostItemPreview
    {
        CostAndName = 0,
        CostOnly = 1,
        NameOnly = 2,
        None = 3,
    }

    [Flags]
    public enum StartingItems
    {
        None = 0,
        [MenuLabel("Hunter's Journal")]
        Journal = 1,
        [MenuLabel("Tier 1 Entries")]
        Entries = 2,
        JournalAndEntries = 3,
    }

    public enum VoidIdol
    {
        None = 0,
        Attuned = 1,
        Ascended = 2,
        Radiant = 3,
    }

    public static class VoidIdolExtensions
    {
        public static int Level(this VoidIdol v) => (int)v;
    }

    public class JournalRandomizationSettings
    {
        [MenuLabel("Enable Connection")]
        public bool Enabled { get; set; } = false;

        [MenuLabel("Randomization Type")]
        public JournalRandomizationType JournalRandomizationType { get; set; } = JournalRandomizationType.All;

        public StartingItems StartingItems { get; set; } = StartingItems.Journal;

        [MenuLabel("Hunter's Notes Previews")]
        public CostItemPreview JournalPreviews { get; set; } = CostItemPreview.CostAndName;

        [MenuLabel("Dupe Hunter's Journal")]
        public bool DupeJournal { get; set; } = false;

        public class PoolSettings
        {
            public bool RegularEntries { get; set; } = true;
            public bool BossEntries { get; set; } = true;
            public bool BonusEntries { get; set; } = true;
        }

        public PoolSettings Pools { get; set; } = new();

        public class CostSettings
        {
            public CostRandomizationType CostRandomizationType { get; set; } = CostRandomizationType.RandomPerEntry;

            [MenuLabel("Minimum Cost Weight")]
            [MenuRange(0f, 1f)]
            public float MinimumCostWeight { get; set; } = 0.25f;

            [MenuLabel("Maximum Cost Weight")]
            [MenuRange(0f, 1f)]
            public float MaximumCostWeight { get; set; } = 0.75f;
        }

        public CostSettings Costs { get; set; } = new();

        public class LongLocationSettings
        {
            public bool RandomizeMenderbug { get; set; } = false;

            [MenuLabel("Randomize Hunter's Mark")]
            public bool RandomizeHuntersMark { get; set; } = false;

            public bool RandomizePantheonBosses { get; set; } = false;

            public bool RandomizeWeatheredMask { get; set; } = false;

            public VoidIdol RandomizeVoidIdol { get; set; } = 0;
        }

        public LongLocationSettings LongLocations { get; set; } = new();
    }
}
