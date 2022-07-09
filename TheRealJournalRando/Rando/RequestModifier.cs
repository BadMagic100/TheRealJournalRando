using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.Tags;
using RandomizerCore.Exceptions;
using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using System;
using System.Collections.Generic;
using System.Linq;
using TheRealJournalRando.Data;
using TheRealJournalRando.Data.Generated;
using TheRealJournalRando.IC;
using TheRealJournalRando.Rando.Generated;

namespace TheRealJournalRando.Rando
{
    internal static class RequestModifier
    {
        private const string POOL_JOURNAL_ENTRIES = "JournalEntries"; //from rando's pools.json
        private static readonly Dictionary<string, string> icNameByTermName = new()
        {
            [Terms.HORNETS] = EnemyNames.Hornet,
            [Terms.CRYSTALGUARDIANS] = EnemyNames.Crystal_Guardian,
            [Terms.GRIMMKINNOVICES] = EnemyNames.Grimmkin_Novice,
            [Terms.GRIMMKINMASTERS] = EnemyNames.Grimmkin_Master,
            [Terms.GRIMMKINNIGHTMARES] = EnemyNames.Grimmkin_Nightmare,
            [Terms.KINGSMOULDS] = EnemyNames.Kingsmould,
            [Terms.ELDERBALDURS] = EnemyNames.Elder_Baldur,
            [Terms.GRUZMOTHERS] = EnemyNames.Gruz_Mother,
            [Terms.VENGEFLYKINGS] = EnemyNames.Vengefly_King,
            [Terms.MIMICS] = EnemyNames.Grub_Mimic,
            [Terms.BLUGGSACS] = EnemyNames.Bluggsac,
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
            RequestBuilder.OnUpdate.Subscribe(5f, AutoGiveRandomizedLifeseeds);
            RequestBuilder.OnUpdate.Subscribe(10f, RestoreSkippedGrimmkinFlames); // must be done after 0 to overwrite rando's request
            RequestBuilder.OnUpdate.Subscribe(10f, GrantStartingItems);
            RequestBuilder.OnUpdate.Subscribe(20f, DupeJournal);
            RequestBuilder.OnUpdate.Subscribe(30f, ApplyLongLocationSettings);
            RequestBuilder.OnUpdate.Subscribe(30f, ApplyNotesPreviewSettings);
            RequestBuilder.OnUpdate.Subscribe(50f, ForceBluggsacLocations);
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

            EditJournalItemAndLocationRequest(EnemyData.SpecialData.Mossy_Vagabond, false, rb);
            EditJournalItemAndLocationRequest(EnemyData.SpecialData.Mossy_Vagabond, true, rb);

            EditJournalItemAndLocationRequest(EnemyData.SpecialData.Hunters_Mark, false, rb, noPrefix: true);
            EditJournalItemAndLocationRequest(EnemyData.SpecialData.Weathered_Mask, false, rb);
            EditJournalItemAndLocationRequest(EnemyData.SpecialData.Void_Idol_1, false, rb);
            EditJournalItemAndLocationRequest(EnemyData.SpecialData.Void_Idol_2, false, rb);
            EditJournalItemAndLocationRequest(EnemyData.SpecialData.Void_Idol_3, false, rb);

#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            rb.OnGetGroupFor.Subscribe(0f, MatchJournalGroup);
            #pragma warning restore CS8622

            static bool MatchJournalGroup(RequestBuilder rb, string item, RequestBuilder.ElementType type, out GroupBuilder? gb)
            {
                if (type == RequestBuilder.ElementType.Transition)
                {
                    gb = default;
                    return false;
                }

                if (EnemyData.NormalData.Values.Concat(EnemyData.SpecialData.Values.Except(EnemyData.SpecialData.Hunters_Mark.Yield()))
                    .SelectMany(x => new[] {x.icName.AsEntryName(), x.icName.AsNotesName()})
                    .Append(EnemyNames.Hunters_Mark)
                    .Contains(item))
                {
                    gb = rb.GetGroupFor(ItemNames.Hunters_Journal);
                    return true;
                }

                gb = default;
                return false;
            }
        }

        private static void EditJournalItemAndLocationRequest(EnemyDef enemy, bool isNotes, RequestBuilder rb, bool noPrefix = false)
        {
            string itemLocationName = isNotes ? enemy.icName.AsNotesName() : enemy.icName.AsEntryName();
            if (noPrefix)
            {
                itemLocationName = enemy.icName;
            }

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

            // mossy vagabond
            if (RandoInterop.Settings.Pools.RegularEntries)
            {
                EnemyDef enemy = EnemyData.SpecialData.Mossy_Vagabond;
                string entryName = enemy.icName.AsEntryName();
                string notesName = enemy.icName.AsNotesName();
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

        private static void AutoGiveRandomizedLifeseeds(RequestBuilder rb)
        {
            // when lifeseeds are randomized, it is impossible to kill lifeseeds, regardless of journal randomization settings.
            // to cope with this, auto-grant the full lifeseed entry and remove the locations from randomization
            if (!RandoInterop.Settings.Enabled || !rb.gs.PoolSettings.LifebloodCocoons)
            {
                return;
            }

            EnemyDef lifeseed = EnemyData.NormalData[EnemyNames.Lifeseed];
            string entryName = lifeseed.icName.AsEntryName();
            string notesName = lifeseed.icName.AsNotesName();

            rb.AddToStart(entryName);
            rb.AddToStart(notesName);

            if (RandoInterop.Settings.Pools.RegularEntries)
            {
                rb.RemoveItemByName(entryName);
                rb.RemoveLocationByName(entryName);
                rb.RemoveItemByName(notesName);
                rb.RemoveLocationByName(notesName);
            }
            else
            {
                rb.RemoveFromVanilla(entryName);
                rb.RemoveFromVanilla(notesName);
            }
        }

        private static void GrantStartingItems(RequestBuilder rb)
        {
            if (!RandoInterop.Settings.Enabled || RandoInterop.Settings.StartingItems == StartingItems.None)
            {
                return;
            }

            if (RandoInterop.Settings.StartingItems.HasFlag(StartingItems.Journal))
            {
                rb.AddToStart(ItemNames.Hunters_Journal);
                rb.GetItemGroupFor(ItemNames.Hunters_Journal).Items.Remove(ItemNames.Hunters_Journal, 1);
            }
            // the menu handles this too, but you shouldn't get starting entries if entries are rando'd (to avoid a stupid amount of empty locations)
            // in theory though you could manually edit GS to get to this point so redundant checks
            if (RandoInterop.Settings.StartingItems.HasFlag(StartingItems.Entries) && 
                !RandoInterop.Settings.JournalRandomizationType.HasFlag(JournalRandomizationType.EntriesOnly))
            {
                foreach (EnemyDef enemy in EnemyData.NormalData.Values)
                {
                    string name = enemy.icName.AsEntryName();
                    if (!rb.IsAtStart(name))
                    {
                        rb.AddToStart(name);
                    }
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

            foreach (EnemyDef enemy in EnemyData.NormalData.Values.Append(EnemyData.SpecialData.Mossy_Vagabond))
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

                        if (enemy.icName == EnemyNames.Mossy_Vagabond)
                        {
                            for(int i = 0; i < rl.costs.Count; i++)
                            {
                                rl.costs[i] = new ForkedLogicCost(rl.costs[i], new SimpleCost(rb.lm.GetTerm("DREAMER"), 1));
                            }
                        }
                    };
                });
            }

            #pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            rb.CostConverters.Subscribe(0f, ConvertKillCosts);
            #pragma warning restore CS8622

            static bool ConvertKillCosts(LogicCost lc, out Cost? cost)
            {
                if (lc is LogicEnemyKillCost kc)
                {
                    cost = EnemyKillCost.ConstructCost(kc.EnemyIcName, kc.Amount);
                    return true;
                }
                else if (lc is SimpleCost sc && icNameByTermName.ContainsKey(sc.term.Name))
                {
                    cost = EnemyKillCost.ConstructCost(icNameByTermName[sc.term.Name], sc.threshold);
                    return true;
                }
                else if (lc is ForkedLogicCost fc)
                {
                    if (fc.Cost1 is LogicEnemyKillCost kcc1)
                    {
                        cost = EnemyKillCost.ConstructCost(kcc1.EnemyIcName, kcc1.Amount);
                        return true;
                    }
                    else if (fc.Cost2 is LogicEnemyKillCost kcc2)
                    {
                        cost = EnemyKillCost.ConstructCost(kcc2.EnemyIcName, kcc2.Amount);
                        return true;
                    }
                }
                cost = default;
                return false;
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
            rb.AddToVanilla(LogicItems.Hornet, Locations.Hornet_2);

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

            rb.AddToVanilla(LogicItems.Kingsmould, Locations.Kingsmould_Palace_Entry);
            rb.AddToVanilla(LogicItems.Kingsmould, Locations.Kingsmould_Palace_Arena_1);
            rb.AddToVanilla(LogicItems.RespawningKingsmould, LocationNames.Journal_Entry_Seal_of_Binding);

            rb.AddToVanilla(LogicItems.ElderBaldur, Locations.Baldur_Shell_Left_Baldur);
            rb.AddToVanilla(LogicItems.ElderBaldur, Locations.Baldur_Shell_Right_Baldur);
            rb.AddToVanilla(LogicItems.ElderBaldur, Locations.Baldur_Greenpath_Entrance);
            rb.AddToVanilla(LogicItems.ElderBaldur, Locations.Baldur_Ancestral_Mound);

            rb.AddToVanilla(LogicItems.VengeflyKing, LocationNames.Boss_Geo_Vengefly_King);
            rb.AddToVanilla(LogicItems.RespawningVengeflyKing, LocationNames.Charm_Notch_Colosseum);

            rb.AddToVanilla(LogicItems.GruzMother, LocationNames.Boss_Geo_Gruz_Mother);
            rb.AddToVanilla(LogicItems.RespawningGruzMother, LocationNames.Charm_Notch_Colosseum);

            // non-respawning grubs are already placed by rando, and handled correctly if randomized as well.
            rb.AddToVanilla(ItemNames.Mimic_Grub, LocationNames.Pale_Ore_Colosseum);

            foreach (string bluggsac in EnemyData.BluggsacLocations)
            {
                rb.AddToVanilla(LogicItems.Bluggsac, bluggsac);
            }
        }

        private static void ApplyLongLocationSettings(RequestBuilder rb)
        {
            if (!RandoInterop.Settings.Enabled)
            {
                return;
            }

            if (!RandoInterop.Settings.LongLocations.RandomizeMenderbug)
            {
                rb.RemoveItemByName(EnemyNames.Menderbug.AsEntryName());
                rb.RemoveLocationByName(EnemyNames.Menderbug.AsEntryName());

                rb.RemoveItemByName(EnemyNames.Menderbug.AsNotesName());
                rb.RemoveLocationByName(EnemyNames.Menderbug.AsNotesName());
            }

            // todo - this still respects boss + bonus settings, meaning they must both be on for these locations to be placed.
            // not sure if that's desirable
            if (!RandoInterop.Settings.LongLocations.RandomizePantheonBosses)
            {
                foreach (string s in new[] {EnemyNames.Nailmasters_Oro_And_Mato, 
                    EnemyNames.Paintmaster_Sheo, EnemyNames.Great_Nailsage_Sly, EnemyNames.Pure_Vessel})
                {
                    rb.RemoveItemByName(s.AsEntryName());
                    rb.RemoveLocationByName(s.AsEntryName());

                    rb.RemoveItemByName(s.AsNotesName());
                    rb.RemoveLocationByName(s.AsNotesName());
                }
            }

            if (RandoInterop.Settings.LongLocations.RandomizeWeatheredMask)
            {
                string maskName = EnemyData.SpecialData.Weathered_Mask.icName.AsEntryName();
                rb.AddItemByName(maskName);
                rb.AddLocationByName(maskName);
            }

            for (int i = 0; i < (int)RandoInterop.Settings.LongLocations.RandomizeVoidIdol; i++)
            {
                string idolName = (SpecialEnemies.Void_Idol_Prefix + (i + 1)).AsEntryName();
                rb.AddItemByName(idolName);
                rb.AddLocationByName(idolName);
            }
            for (int i = (int)RandoInterop.Settings.LongLocations.RandomizeVoidIdol; i < 3; i++)
            {
                string idolName = (SpecialEnemies.Void_Idol_Prefix + (i + 1)).AsEntryName();
                rb.AddToVanilla(idolName, idolName);
            }

            if (RandoInterop.Settings.LongLocations.RandomizeHuntersMark)
            {
                rb.AddItemByName(EnemyNames.Hunters_Mark);
                rb.AddLocationByName(EnemyNames.Hunters_Mark);
            }
            else
            {
                rb.AddToVanilla(EnemyNames.Hunters_Mark, EnemyNames.Hunters_Mark);
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

        private static void ForceBluggsacLocations(RequestBuilder rb)
        {
            if (!RandoInterop.Settings.Enabled || !RandoInterop.Settings.Pools.RegularEntries)
            {
                return;
            }

            foreach (string loc in EnemyData.BluggsacLocations)
            {
                rb.EditLocationRequest(loc, info =>
                {
                    info.customPlacementFetch = (factory, _) =>
                    {
                        if (factory.TryFetchPlacement(loc, out AbstractPlacement pmt))
                        {
                            return pmt;
                        }
                        pmt = Finder.GetLocationFromSheet(loc, 0).Wrap();
                        factory.AddPlacement(pmt);
                        return pmt;
                    };
                });
            }
        }
    }
}
