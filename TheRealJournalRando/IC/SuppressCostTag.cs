using ItemChanger;
using ItemChanger.Placements;
using System.Collections.Generic;

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

        private readonly List<(string, Cost)> suppressedCosts = new();
        private ISingleCostPlacement? placement;

        public override void Load(object parent)
        {
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
            if (placement != null)
            {
                TheRealJournalRando.Instance.LogDebug($"Restoring cost on tag unload for {(placement as AbstractPlacement)?.Name}");
                foreach ((string path, Cost cost) in suppressedCosts)
                {
                    placement.Cost = RestoreCostPatch(placement.Cost, path, cost);
                }
                TheRealJournalRando.Instance.LogDebug($"Final restored cost is {placement.Cost}");
                suppressedCosts.Clear();
            }
        }

        private Cost? ModifyCost(Cost? c, string path = "/")
        {
            TheRealJournalRando.Instance.LogDebug($"Inspecting cost `{c}` at path `{path}`");
            if (c == null)
            {
                return null;
            }

            if (c is MultiCost mc)
            {
                int idx = 0;
                for (int i = 0; i < mc.Costs.Count; i++, idx++)
                {
                    TheRealJournalRando.Instance.LogDebug($"Processing cost {i} (actual {idx}) of {mc.Costs.Count}");
                    Cost? updated = ModifyCost(mc.Costs[i], path + $"{idx}/");
                    if (updated == null)
                    {
                        mc.Costs.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        mc.Costs[i] = updated;
                    }
                }
                return mc;
            }
            else if (costMatcher?.Match(c) == true)
            {
                TheRealJournalRando.Instance.LogDebug("Matched cost, suppressing");
                c.Unload();
                suppressedCosts.Add((path, c));
                return null;
            }
            else
            {
                TheRealJournalRando.Instance.LogDebug("Unmatched cost, leaving it be");
                return c;
            }
        }

        private Cost? RestoreCostPatch(Cost? c, string path, Cost patchedCost)
        {
            TheRealJournalRando.Instance.LogDebug($"Attempting to apply cost `{patchedCost}` to `{c}` at `{path}`");
            // a multicost that may or may not have been modified
            if (c is MultiCost mc)
            {
                int splitter = path.IndexOf('/', 1);
                int patchedIndex = int.Parse(path.Substring(1, splitter - 1));
                string nestedPath = path.Substring(splitter);
                if (nestedPath.IndexOf('/', 1) != -1)
                {
                    // this is a deeply nested path pointing to another multicost
                    mc.Costs[patchedIndex] = RestoreCostPatch(mc.Costs[patchedIndex], nestedPath, patchedCost);
                }
                else
                {
                    // this is trying to insert a removed cost back into this multicost
                    mc.Costs.Insert(patchedIndex, RestoreCostPatch(null, nestedPath, patchedCost));
                }
                return mc;
            }
            // a cost that was removed
            else if (c == null)
            {
                if (path == "/")
                {
                    TheRealJournalRando.Instance.LogDebug("Patch applied");
                    patchedCost.Load();
                    return patchedCost;
                }
                else
                {
                    // invalid patch path; we can only restore a root path to a removed cost
                    TheRealJournalRando.Instance.LogError("Failed to restore cost: invalid patch");
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
