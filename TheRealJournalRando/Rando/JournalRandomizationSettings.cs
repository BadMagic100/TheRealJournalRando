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

    public enum CostItemPreview
    {
        CostAndName,
        CostOnly,
        NameOnly,
        None
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
            public float MinimumCostWeight { get; set; } = 0.5f;

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
