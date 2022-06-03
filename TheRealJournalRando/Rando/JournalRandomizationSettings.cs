using MenuChanger.Attributes;

namespace TheRealJournalRando.Rando
{
    public enum JournalRandomizationType
    {
        EntriesOnly,
        NotesOnly,
        [MenuLabel("Full Journal Rando")]
        All
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

    public class JournalRandomizationSettings
    {
        [MenuLabel("Enable Connection")]
        public bool Enabled { get; set; } = false;

        [MenuLabel("Randomization Type")]
        public JournalRandomizationType JournalRandomizationType { get; set; } = JournalRandomizationType.All;

        public CostItemPreview JournalPreviews { get; set; } = CostItemPreview.CostAndName;

        public class PoolSettings
        {
            public bool RegularEntries { get; set; } = true;
            public bool BossEntries { get; set; } = true;
            public bool BonusEntries { get; set; } = true;
        }

        public PoolSettings Pools { get; set; } = new();

        public class CostSettings
        {
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

            public bool RandomizePantheonBosses { get; set; } = false;

            public bool RandomizeWeatheredMask { get; set; } = false;

            [MenuRange(0, 3)]
            public int RandomizeVoidIdol { get; set; } = 0;
        }

        public LongLocationSettings LongLocations { get; set; } = new();
    }
}
