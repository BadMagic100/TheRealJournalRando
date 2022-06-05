using ItemChanger;
using ItemChanger.Placements;
using RandomizerCore.Logic;
using RandomizerMod.RC;
using System;
using System.Collections.Generic;
using System.Linq;
using TheRealJournalRando.Data;
using TheRealJournalRando.IC;

namespace TheRealJournalRando.Rando
{
    internal static class RequestModifier
    {
        private const string POOL_JOURNAL_ENTRIES = "JournalEntries"; //from rando's pools.json
        private static readonly HashSet<string> handledCostTerms = new()
        {
            "HORNETS",
            "CRYSTALGUARDIANS",
            "GRIMMKINNOVICES",
            "GRIMMKINMASTERS",
            "GRIMMKINNIGHTMARES",
            "KINGSMOULDS"
        };
        private static readonly Dictionary<string, string> icNameByTermName = new()
        {
            ["HORNETS"] = "Hornet",
            ["CRYSTALGUARDIANS"] = "Crystal_Guardian",
            ["GRIMMKINNOVICES"] = "Grimmkin_Novice",
            ["GRIMMKINMASTERS"] = "Grimmkin_Master",
            ["GRIMMKINNIGHTMARES"] = "Grimmkin_Nightmare",
            ["KINGSMOULDS"] = "Kingsmould",
        };

        public static void Hook()
        {
            RequestBuilder.OnUpdate.Subscribe(-500f, SetupRefs);
            RequestBuilder.OnUpdate.Subscribe(-490f, ApplyNotesCostRandomization);
            RequestBuilder.OnUpdate.Subscribe(0f, ApplyPoolSettings);
            RequestBuilder.OnUpdate.Subscribe(0f, AddVanillaFiniteEnemies);
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
                        foreach (LogicCost lc in rl.costs)
                        {
                            Cost? newCost = null;
                            if (lc is LogicEnemyKillCost kc)
                            {
                                newCost = EnemyKillCost.ConstructCustomCost(kc.EnemyIcName, kc.Amount);
                            }
                            else if (lc is SimpleCost sc && handledCostTerms.Contains(sc.term.Name))
                            {
                                newCost = EnemyKillCost.ConstructCustomCost(icNameByTermName[sc.term.Name], sc.threshold);
                            }

                            if (newCost != null)
                            {
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
                    }
                };
                if (!enemy.unkillable)
                {
                    info.onRandoLocationCreation += (factory, rl) =>
                    {
                        int cost = isNotes ? enemy.notesCost : 1;
                        if (enemy.icName == "Hornet")
                        {
                            rl.AddCost(new SimpleCost(factory.lm.GetTerm("HORNETS"), cost));
                        }
                        else if (enemy.icName == "Crystal_Guardian")
                        {
                            rl.AddCost(new SimpleCost(factory.lm.GetTerm("CRYSTALGUARDIANS"), cost));
                        }
                        else if (enemy.icName == "Grimmkin_Novice")
                        {
                            rl.AddCost(new SimpleCost(factory.lm.GetTerm("GRIMMKINNOVICES"), cost));
                        }
                        else if (enemy.icName == "Grimmkin_Master")
                        {
                            rl.AddCost(new SimpleCost(factory.lm.GetTerm("GRIMMKINMASTERS"), cost));
                        }
                        else if (enemy.icName == "Grimmkin_Nightmare")
                        {
                            rl.AddCost(new SimpleCost(factory.lm.GetTerm("GRIMMKINNIGHTMARES"), cost));
                        }
                        else if (enemy.icName == "Kingsmould")
                        {
                            rl.AddCost(new SimpleCost(factory.lm.GetTerm("KINGSMOULDS"), cost));
                        }
                        else
                        {
                            rl.AddCost(new LogicEnemyKillCost(factory.lm, enemy.icName, enemy.respawns, cost));
                        }
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
                        double weight = RandoInterop.Settings.Costs.CostRandomizationType switch
                        {
                            CostRandomizationType.RandomFixedWeight => fixedWeight,
                            CostRandomizationType.RandomPerEntry => ComputeWeight(factory.rng),
                            _ => throw new NotImplementedException("Invalid cost randomization type!")
                        };
                        foreach (LogicCost c in rl.costs)
                        {
                            if (c is SimpleCost sc && handledCostTerms.Contains(sc.term.Name))
                            {
                                sc.threshold = (int)Math.Max(1, Math.Round(sc.threshold * weight));
                            }
                            else if (c is LogicEnemyKillCost kc)
                            {
                                kc.Amount = (int)Math.Max(1, Math.Round(kc.Amount * weight));
                            }
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

        private static void AddVanillaFiniteEnemies(RequestBuilder rb)
        {
            rb.AddToVanilla(LogicItems.Hornet, LocationNames.Mothwing_Cloak);
            rb.AddToVanilla(LogicItems.Hornet, LogicLocations.Hornet2);

            rb.AddToVanilla(LogicItems.CrystalGuardian, LocationNames.Boss_Geo_Crystal_Guardian);
            rb.AddToVanilla(LogicItems.CrystalGuardian, LocationNames.Boss_Geo_Enraged_Guardian);

            rb.AddToVanilla(LogicItems.GrimmkinNovice, LocationNames.Grimmkin_Flame_City_Storerooms);
            rb.AddToVanilla(LogicItems.GrimmkinNovice, LocationNames.Grimmkin_Flame_Crystal_Peak);
            rb.AddToVanilla(LogicItems.GrimmkinNovice, LocationNames.Grimmkin_Flame_Greenpath);

            rb.AddToVanilla(LogicItems.GrimmkinMaster, LocationNames.Grimmkin_Flame_Kings_Pass);
            rb.AddToVanilla(LogicItems.GrimmkinMaster, LocationNames.Grimmkin_Flame_Resting_Grounds);
            rb.AddToVanilla(LogicItems.GrimmkinMaster, LocationNames.Grimmkin_Flame_Kingdoms_Edge);

            rb.AddToVanilla(LogicItems.GrimmkinNightmare, LocationNames.Grimmkin_Flame_Fungal_Core);
            rb.AddToVanilla(LogicItems.GrimmkinNightmare, LocationNames.Grimmkin_Flame_Hive);
            rb.AddToVanilla(LogicItems.GrimmkinNightmare, LocationNames.Grimmkin_Flame_Ancient_Basin);

            rb.AddToVanilla(LogicItems.Kingsmould, LogicLocations.KingsmouldPalaceEntry);
            rb.AddToVanilla(LogicItems.Kingsmould, LogicLocations.KingsmouldPalaceArena1);
            rb.AddToVanilla(LogicItems.RespawningKingsmould, LocationNames.Journal_Entry_Seal_of_Binding);
        }
    }
}
