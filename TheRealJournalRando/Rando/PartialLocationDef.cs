using RandomizerMod.RandomizerData;
using System.Collections.Generic;
using System.Linq;

namespace TheRealJournalRando.Rando
{
    public record PartialLocationDef : LocationDef
    {
        private static Dictionary<string, string> titledToMapAreaLookup = RandomizerMod.RandomizerData.Data.Rooms.Values
            .GroupBy(r => r.TitledArea)
            .Select(g => g.First())
            .ToDictionary(r => r.TitledArea, r => r.MapArea);

        public string? ExplicitTitledArea { get; init; }
        public string? ExplicitMapArea { get; init; }

        public override string MapArea
        {
            get
            {
                if (ExplicitMapArea != null)
                {
                    return ExplicitMapArea;
                }
                else if (ExplicitTitledArea != null && titledToMapAreaLookup.TryGetValue(ExplicitTitledArea, out string mapArea))
                {
                    return mapArea;
                }
                else
                {
                    return base.MapArea;
                }
            }
        }

        public override string TitledArea
        {
            get
            {
                if (ExplicitTitledArea != null)
                {
                    return ExplicitTitledArea;
                }
                else
                {
                    return base.TitledArea;
                }
            }
        }
    }
}
