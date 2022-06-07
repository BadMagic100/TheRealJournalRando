using ItemChanger;
using ItemChanger.Placements;
using ItemChanger.Tags;
using RandomizerCore.Exceptions;
using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;
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
        private static readonly Dictionary<string, string> icNameByTermName = new()
        {
            ["HORNETS"] = "Hornet",
            ["CRYSTALGUARDIANS"] = "Crystal_Guardian",
            ["GRIMMKINNOVICES"] = "Grimmkin_Novice",
            ["GRIMMKINMASTERS"] = "Grimmkin_Master",
            ["GRIMMKINNIGHTMARES"] = "Grimmkin_Nightmare",
            ["KINGSMOULDS"] = "Kingsmould",
            ["ELDERBALDURS"] = "Elder_Baldur",
            ["GRUZMOTHERS"] = "Gruz_Mother",
            ["VENGEFLYKINGS"] = "Vengefly_King",
        };
        private static readonly Dictionary<string, string> termNameByIcName = icNameByTermName.ToDictionary(i => i.Value, i => i.Key);

        public static void Hook()
        {
            RequestBuilder.OnUpdate.Subscribe(-1000, rb =>
            {
                rb.rm.OnError += e =>
                {
                    if (e is UnreachableLocationException u)
                    {
                        TheRealJournalRando.Instance.LogError(u.GetVerboseMessage());
                    }
                };
            });
            RequestBuilder.OnUpdate.Subscribe(-500f, SetupRefs);
            RequestBuilder.OnUpdate.Subscribe(-490f, ApplyNotesCostRandomization);
            RequestBuilder.OnUpdate.Subscribe(0f, ApplyPoolSettings);
            RequestBuilder.OnUpdate.Subscribe(0f, AddVanillaFiniteEnemies);
            RequestBuilder.OnUpdate.Subscribe(10f, RestoreSkippedGrimmkinFlames); // must be done after 0 to overwrite rando's request
            RequestBuilder.OnUpdate.Subscribe(20f, DupeJournal);
            RequestBuilder.OnUpdate.Subscribe(30f, ApplyLongLocationSettings);
            RequestBuilder.OnUpdate.Subscribe(30f, ApplyNotesPreviewSettings);
        }

        private static void SetupRefs(RequestBuilder rb)
        {
            if (!RandoInterop.Settings.Enabled)
            {
                return;
            }

            foreach (EnemyDef enemy in EnemyData.NormalData.Values)
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

                if (EnemyData.AllDefs.SelectMany(x => new[] {x.icName.AsEntryName(), x.icName.AsNotesName()}).Contains(item))
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
                            else if (lc is SimpleCost sc && icNameByTermName.ContainsKey(sc.term.Name))
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
                    info.onRandoLocationCreation += ApplyDefaultCost(enemy, isNotes);
                }
            });
        }

        private static Action<RandoFactory, RandoModLocation> ApplyDefaultCost(EnemyDef enemy, bool isNotes)
        {
            void ApplyCost(RandoFactory factory, RandoModLocation rl)
            {
                int cost = isNotes ? enemy.notesCost : 1;
                if (termNameByIcName.ContainsKey(enemy.icName))
                {
                    Term t = factory.lm.GetTerm(termNameByIcName[enemy.icName]);
                    rl.AddCost(new SimpleCost(t, cost));
                }
                else
                {
                    rl.AddCost(new LogicEnemyKillCost(factory.lm, enemy.icName, enemy.respawns, cost));
                }
            }
            return ApplyCost;
        }

        private static void ApplyPoolSettings(RequestBuilder rb)
        {
            if (!RandoInterop.Settings.Enabled)
            {
                return;
            }

            foreach (EnemyDef enemy in EnemyData.NormalData.Values)
            {
                if (enemy.unkillable)
                {
                    continue;
                }

                string entryName = enemy.icName.AsEntryName();
                string notesName = enemy.icName.AsNotesName();
                bool isNormalEntry = (!enemy.isBoss && !enemy.ignoredForHunterMark);
                // doing some implication here - if the enemy is a boss, BossEntries must be true to randomize,
                // otherwise we don't care about the setting. (and similar for others)
                bool normalRandomization = !isNormalEntry || RandoInterop.Settings.Pools.RegularEntries;
                bool bossRandomization = !enemy.isBoss || RandoInterop.Settings.Pools.BossEntries;
                bool bonusRandomization = !enemy.ignoredForHunterMark || RandoInterop.Settings.Pools.BonusEntries;
                if (normalRandomization && bossRandomization && bonusRandomization)
                {
                    if (RandoInterop.Settings.JournalRandomizationType.HasFlag(JournalRandomizationType.EntriesOnly))
                    {
                        rb.AddItemByName(entryName);
                        rb.AddLocationByName(entryName);
                    }
                    else
                    {
                        rb.AddToVanilla(entryName, entryName);
                    }

                    if (RandoInterop.Settings.JournalRandomizationType.HasFlag(JournalRandomizationType.NotesOnly))
                    {
                        rb.AddItemByName(notesName);
                        rb.AddLocationByName(notesName);
                    }
                    else
                    {
                        rb.AddToVanilla(notesName, notesName);
                    }
                }
                else
                {
                    rb.AddToVanilla(entryName, entryName);
                    rb.AddToVanilla(notesName, notesName);
                }
            }
        }

        private static void RestoreSkippedGrimmkinFlames(RequestBuilder rb)
        {
            // if we can need to kill grimmkin, we need to undo everything rando has done to not skip the first 2 tiers of the questline
            // see: https://github.com/homothetyhk/RandomizerMod/blob/934895871d9f28f0f6f5c1da00c331a6205d3ff3/RandomizerMod/RC/Requests/BuiltinRequests.cs#L855
            //      https://github.com/homothetyhk/RandomizerMod/blob/934895871d9f28f0f6f5c1da00c331a6205d3ff3/RandomizerMod/RC/Requests/BuiltinRequests.cs#L1254
            if (RandoInterop.Settings.Enabled && RandoInterop.Settings.Pools.BonusEntries)
            {
                if (rb.gs.PoolSettings.Charms && !rb.gs.PoolSettings.GrimmkinFlames)
                {
                    PoolDef flamePool = RandomizerMod.RandomizerData.Data.GetPoolDef(PoolNames.Flame);
                    foreach (VanillaDef def in flamePool.Vanilla.Take(6))
                    {
                        rb.AddToVanilla(def.Item, def.Location);
                    }
                    rb.ReplaceItem(ItemNames.Grimmchild2, ItemNames.Grimmchild1);
                }
            }
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

            foreach (EnemyDef enemy in EnemyData.NormalData.Values)
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
                            if (c is SimpleCost sc && icNameByTermName.ContainsKey(sc.term.Name))
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
            if (!RandoInterop.Settings.Enabled)
            {
                return;
            }

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

            rb.AddToVanilla(LogicItems.ElderBaldur, LogicLocations.BaldurShellLeftBaldur);
            rb.AddToVanilla(LogicItems.ElderBaldur, LogicLocations.BaldurShellRightBaldur);
            rb.AddToVanilla(LogicItems.ElderBaldur, LogicLocations.BaldurGreenpathEntrance);
            rb.AddToVanilla(LogicItems.ElderBaldur, LogicLocations.BaldurAncestralMound);

            rb.AddToVanilla(LogicItems.VengeflyKing, LocationNames.Boss_Geo_Vengefly_King);
            rb.AddToVanilla(LogicItems.RespawningVengeflyKing, LocationNames.Charm_Notch_Colosseum);

            rb.AddToVanilla(LogicItems.GruzMother, LocationNames.Boss_Geo_Gruz_Mother);
            rb.AddToVanilla(LogicItems.RespawningGruzMother, LocationNames.Charm_Notch_Colosseum);

            rb.AddToVanilla(LogicItems.Myla, LogicLocations.Myla);
        }

        private static void ApplyLongLocationSettings(RequestBuilder rb)
        {
            if (!RandoInterop.Settings.Enabled)
            {
                return;
            }

            // temporary, will need extra handling; for now just yeet it and this is a good place for it
            rb.RemoveItemByName("Mossy_Vagabond".AsNotesName());
            rb.RemoveLocationByName("Mossy_Vagabond".AsEntryName());

            if (!RandoInterop.Settings.LongLocations.RandomizeMenderbug)
            {
                rb.RemoveItemByName("Menderbug".AsEntryName());
                rb.RemoveLocationByName("Menderbug".AsEntryName());

                rb.RemoveItemByName("Menderbug".AsNotesName());
                rb.RemoveLocationByName("Menderbug".AsNotesName());
            }

            // todo - this still respects boss + bonus settings, meaning they must both be on for these locations to be placed.
            // not sure if that's desirable
            if (!RandoInterop.Settings.LongLocations.RandomizePantheonBosses)
            {
                foreach (string s in new[] {"Nailmasters_Oro_And_Mato", "Paintmaster_Sheo", "Great_Nailsage_Sly", "Pure_Vessel"})
                {
                    rb.RemoveItemByName(s.AsEntryName());
                    rb.RemoveLocationByName(s.AsEntryName());

                    rb.RemoveItemByName(s.AsNotesName());
                    rb.RemoveLocationByName(s.AsNotesName());
                }
            }

            if (RandoInterop.Settings.LongLocations.RandomizeWeatheredMask)
            {
                
            }

            if (RandoInterop.Settings.LongLocations.RandomizeVoidIdol > 0)
            {

            }

            if (RandoInterop.Settings.LongLocations.RandomizeHuntersMark)
            {

            }
        }

        private static void ApplyNotesPreviewSettings(RequestBuilder rb)
        {
            CostItemPreview preview = RandoInterop.Settings.JournalPreviews;
            if (!RandoInterop.Settings.Enabled || preview == CostItemPreview.CostAndName)
            {
                return;
            }

            foreach (EnemyDef enemy in EnemyData.NormalData.Values)
            {
                string notesName = enemy.icName.AsNotesName();
                rb.EditLocationRequest(notesName, info =>
                {
                    info.onPlacementFetch += (factory, rp, pmt) =>
                    {
                        if (preview.HasFlag(CostItemPreview.CostOnly))
                        {
                            pmt.AddTag<DisableItemPreviewTag>();
                        }
                        if (preview.HasFlag(CostItemPreview.NameOnly))
                        {
                            pmt.AddTag<DisableCostPreviewTag>();
                        }
                    };
                });
            }
        }
    }
}
