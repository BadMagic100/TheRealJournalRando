using ItemChanger;
using ItemChanger.Placements;
using System.Collections.Generic;
using System.Linq;

namespace TheRealJournalRando.IC
{
    public interface ICostMatcher
    {
        bool Match(Cost c);
    }

    /// <summary>
    /// Suppresses and removes costs matching a given pattern. Usually used on a DualLocation. Matched costs are
    /// removed when the tag loads, and added back when the tag unloads.
    /// </summary>
    public class SuppressSingleCostTag : Tag
    {
        public ICostMatcher? costMatcher;

        private readonly List<Cost> suppressedCosts = new();
        private ISingleCostPlacement? placement;

        public override void Load(object parent)
        {
            base.Load(parent);

            if (parent is AbstractLocation loc)
            {
                placement = loc.Placement as ISingleCostPlacement;
            }
            else if (parent is ISingleCostPlacement plt)
            {
                placement = plt;
            }

            if (placement != null)
            {
                TheRealJournalRando.Instance.LogDebug($"Modifying cost on tag load for {(placement as AbstractPlacement)?.Name}");
                placement.Cost = ModifyCost(placement.Cost);
                TheRealJournalRando.Instance.LogDebug($"Final modified cost is {placement.Cost}");
            }
        }

        public override void Unload(object parent)
        {
            base.Unload(parent);

            if (placement != null)
            {
                TheRealJournalRando.Instance.LogDebug($"Restoring cost on tag unload for {(placement as AbstractPlacement)?.Name}");
                placement.Cost = RestoreCost(placement.Cost);
                TheRealJournalRando.Instance.LogDebug($"Final restored cost is {placement.Cost}");
                suppressedCosts.Clear();
            }
        }

        private Cost? ModifyCost(Cost? c)
        {
            if (c == null)
            {
                return null;
            }

            if (c is MultiCost mc)
            {
                TheRealJournalRando.Instance.LogDebug("Suppressing costs nested within MultiCost");
                var partitionedCosts = mc.GroupBy(x => costMatcher?.Match(c) == true)
                    .ToDictionary(g => g.Key);
                suppressedCosts.AddRange(partitionedCosts[true]);
                foreach(Cost cc in suppressedCosts)
                {
                    TheRealJournalRando.Instance.LogDebug($"Matched cost {cc}, suppressing");
                    c.Unload();
                }
                return new MultiCost(partitionedCosts[false]);
            }
            else if (costMatcher?.Match(c) == true)
            {
                TheRealJournalRando.Instance.LogDebug($"Matched cost {c}, suppressing");
                c.Unload();
                suppressedCosts.Add(c);
                return null;
            }
            else
            {
                TheRealJournalRando.Instance.LogDebug("Ignoring unmatched cost");
                return c;
            }
        }

        private Cost? RestoreCost(Cost? c)
        {
            TheRealJournalRando.Instance.LogDebug($"Attempting to restore cost");
            if (c is MultiCost mc && suppressedCosts.Any())
            {
                TheRealJournalRando.Instance.LogDebug("Restoring suppressed costs into MultiCost");
                foreach (Cost cc in suppressedCosts)
                {
                    TheRealJournalRando.Instance.LogDebug($"Restoring suppressed cost {cc}");
                    cc.Load();
                }
                return mc + new MultiCost(suppressedCosts);

            }
            // a cost that was removed
            else if (c == null)
            {
                if (suppressedCosts.Any())
                {
                    Cost cc = suppressedCosts[0];
                    TheRealJournalRando.Instance.LogDebug($"Restoring suppressed cost {cc}");
                    return cc;
                }
                else
                {
                    TheRealJournalRando.Instance.LogDebug("No cost to restore");
                    return null;
                }
            }
            // some other cost that did not match the pattern
            else
            {
                return c;
            }
        }
    }
}
