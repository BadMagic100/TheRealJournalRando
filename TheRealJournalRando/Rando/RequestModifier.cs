using ItemChanger;
using ItemChanger.Placements;
using RandomizerMod.RC;
using System;
using System.Linq;
using TheRealJournalRando.Data;
using TheRealJournalRando.IC;

namespace TheRealJournalRando.Rando
{
    internal static class RequestModifier
    {
        private const string POOL_JOURNAL_ENTRIES = "JournalEntries"; //from rando's pools.json

        public static void Hook()
        {
            RequestBuilder.OnUpdate.Subscribe(-500f, SetupRefs);
            RequestBuilder.OnUpdate.Subscribe(-490f, ApplyNotesCostRandomization);
            RequestBuilder.OnUpdate.Subscribe(0f, ApplyPoolSettings);
            RequestBuilder.OnUpdate.Subscribe(20f, DupeJournal);
        }

        /// <summary>
        /// Creates itemdefs/locationdefs, applies vanilla logic costs and split group
        /// </summary>
        private static void SetupRefs(RequestBuilder rb)
        {
            if (!RandoInterop.Settings.Enabled)
            {
                return;
            }

            foreach (EnemyDef enemy in EnemyData.NormalData.Values.Concat(EnemyData.SpecialData.Values))
            {
                EditJournalItemAndLocationRequest(enemy, false, rb);
                EditJournalItemAndLocationRequest(enemy, true, rb);
            }
            
            rb.OnGetGroupFor.Subscribe(0f, MatchJournalGroup);

            static bool MatchJournalGroup(RequestBuilder rb, string item, RequestBuilder.ElementType type, out GroupBuilder? gb)
            {
                if (type == RequestBuilder.ElementType.Transition)
                {
                    gb = default;
                    return false;
                }

                if (EnemyData.NormalData.Values.Concat(EnemyData.SpecialData.Values)
                    .SelectMany(x => new[] {x.icName.AsEntryName(), x.icName.AsNotesName()}).Contains(item))
                {
                    gb = rb.GetGroupFor(ItemNames.Hunters_Journal);
                    return true;
                }

                gb = default;
                return false;
            }
        }

        private static void EditJournalItemAndLocationRequest(EnemyDef enemy, bool isNotes, RequestBuilder rb)
        {
            string itemLocationName = isNotes ? enemy.icName.AsNotesName() : enemy.icName.AsEntryName();
            rb.EditItemRequest(itemLocationName, info =>
            {
                info.getItemDef = () => new()
                {
                    Name = itemLocationName,
                    Pool = POOL_JOURNAL_ENTRIES,
                    MajorItem = false,
                    PriceCap = 1,
                };
            });
            rb.EditLocationRequest(itemLocationName, info =>
            {
                info.getLocationDef = () => new()
                {
                    Name = itemLocationName,
                    SceneName = Finder.GetLocation(itemLocationName).sceneName,
                    FlexibleCount = false,
                    AdditionalProgressionPenalty = false,
                };
                info.customAddToPlacement += (factory, rp, pmt, item) =>
                {
                    pmt.Add(item);
                    if (rp.Location is not RandoModLocation rl || rl.costs == null)
                    {
                        return;
                    }
                    if (pmt is ISingleCostPlacement iscp)
                    {
                        foreach (LogicEnemyKillCost cost in rl.costs.OfType<LogicEnemyKillCost>())
                        {
                            EnemyKillCost newCost = EnemyKillCost.ConstructCustomCost(cost.EnemyIcName, cost.Amount);
                            if (iscp.Cost == null)
                            {
                                iscp.Cost = newCost;
                            }
                            else
                            {
                                iscp.Cost += newCost;
                            }
                        }
                    }
                };
                if (!enemy.unkillable)
                {
                    info.onRandoLocationCreation += (factory, rl) =>
                    {
                        rl.AddCost(new LogicEnemyKillCost(factory.lm, enemy.icName, enemy.respawns, isNotes ? enemy.notesCost : 1));
                    };
                }
            });
        }

        private static void ApplyPoolSettings(RequestBuilder rb)
        {
            string entry = EnemyData.NormalData["Hornet"].icName.AsEntryName();
            string notes = EnemyData.NormalData["Hornet"].icName.AsNotesName();
            rb.AddItemByName(entry);
            rb.AddItemByName(notes);
            rb.AddLocationByName(entry);
            rb.AddLocationByName(notes);

            //todo - real impl!
        }

        private static void DupeJournal(RequestBuilder rb)
        {
            if (!RandoInterop.Settings.Enabled || !RandoInterop.Settings.DupeJournal)
            {
                return;
            }

            if (!rb.IsAtStart(ItemNames.Hunters_Journal))
            {
                rb.AddItemByName($"{PlaceholderItem.Prefix}{ItemNames.Hunters_Journal}");
            }
        }

        private static void ApplyNotesCostRandomization(RequestBuilder rb)
        {
            if (!RandoInterop.Settings.Enabled || RandoInterop.Settings.Costs.CostRandomizationType == CostRandomizationType.Unrandomized)
            {
                return;
            }

            double fixedWeight = ComputeWeight(rb.rng);

            foreach (EnemyDef enemy in EnemyData.NormalData.Values.Concat(EnemyData.SpecialData.Values))
            {
                string notesName = enemy.icName.AsNotesName();
                rb.EditLocationRequest(notesName, info =>
                {
                    info.onRandoLocationCreation += (factory, rl) =>
                    {
                        if (rl.costs == null)
                        {
                            TheRealJournalRando.Instance.Log("Rejected cost randomization for " + notesName);
                            return;
                        }

                        double weight = RandoInterop.Settings.Costs.CostRandomizationType switch
                        {
                            CostRandomizationType.RandomFixedWeight => fixedWeight,
                            CostRandomizationType.RandomPerEntry => ComputeWeight(factory.rng),
                            _ => throw new NotImplementedException("Invalid cost randomization type!")
                        };
                        foreach (LogicEnemyKillCost c in rl.costs.OfType<LogicEnemyKillCost>())
                        {
                            c.Amount = (int)Math.Max(1, Math.Round(c.Amount * weight));
                        }
                    };
                });
            }
        }

        private static float ComputeWeight(Random rng)
        {
            double result = Math.Min(RandoInterop.Settings.Costs.MinimumCostWeight, RandoInterop.Settings.Costs.MaximumCostWeight)
                + rng.NextDouble() * Math.Abs(RandoInterop.Settings.Costs.MaximumCostWeight - RandoInterop.Settings.Costs.MinimumCostWeight);
            return (float)result;
        }
    }
}
