using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheRealJournalRando.IC
{
    public class VoidIdolAggregatorModule : Module
    {
        private Dictionary<int, List<VoidIdolLocation>> statuePlacements = new();
        private ParametricFsmEditBuilder<int>? statueEditBuilder;

        public override void Initialize()
        {
            statueEditBuilder = new(GenerateStatueEdit);
            for (int i = 0; i < 3; i++)
            {
                string goFullPath = $"/GG_Statue_Knight/Base/Statue/Knight_v0{i + 1}/Interact";
                Events.AddFsmEdit(SceneNames.GG_Workshop, new(goFullPath, "Conversation Control"), statueEditBuilder.GetOrAddEdit(i));
            }
        }

        public override void Unload()
        {
            for (int i = 0; i < 3; i++)
            {
                string goFullPath = $"/GG_Statue_Knight/Base/Statue/Knight_v0{i + 1}/Interact";
                Events.RemoveFsmEdit(SceneNames.GG_Workshop, new(goFullPath, "Conversation Control"), statueEditBuilder?[i]);
            }
        }

        private bool AreAllStatuePlacementsCleared(int statueTier)
        {
            if (statuePlacements.TryGetValue(statueTier, out List<VoidIdolLocation> locs))
            {
                return locs.All(x => x.Placement.AllObtained());
            }
            // if there's no placement... well of course the entire statue is cleared, there's no items here.
            return true;
        }

        private void ChainGiveAllPlacementsAsync(int statueTier, Transform t, Action callback)
        {
            if (!statuePlacements.TryGetValue(statueTier, out List<VoidIdolLocation> locs))
            {
                callback?.Invoke();
            }

            Action aggregated = () => callback?.Invoke();
            foreach (VoidIdolLocation loc in locs)
            {
                aggregated = ConcatGiveAll(aggregated, t, loc);
            }
            aggregated?.Invoke();

            static Action ConcatGiveAll(Action aggregated, Transform t, VoidIdolLocation loc)
            {
                return () => loc.GiveAllAsync(t)(aggregated);
            }
        }

        private Action<PlayMakerFSM> GenerateStatueEdit(int statueTier)
        {
            return (self) =>
            {
                FsmState journal = self.GetState("Journal");
                journal.Actions = new FsmStateAction[]
                {
                    new DelegateBoolTest(() => AreAllStatuePlacementsCleared(statueTier), "FINISHED", null),
                    new AsyncLambda((callback) => ChainGiveAllPlacementsAsync(statueTier, self.gameObject.transform, callback))
                };
            };
        }

        public void PlaceItemsAtStatue(int statueTier, VoidIdolLocation loc)
        {
            if (!statuePlacements.ContainsKey(statueTier))
            {
                statuePlacements[statueTier] = new List<VoidIdolLocation>();
            }
            if (!statuePlacements[statueTier].Contains(loc))
            {
                statuePlacements[statueTier].Add(loc);
            }
        }

        public void RemoveItemsFromStatue(int statueTier, VoidIdolLocation loc)
        {
            if (statuePlacements.TryGetValue(statueTier, out List<VoidIdolLocation> locs))
            {
                locs.Remove(loc);
            }
        }
    }

    public class VoidIdolLocation : AutoLocation
    {
        // 0 = attuned
        // 1 = ascended
        // 2 = radiant
        public int tier;

        protected override void OnLoad()
        {
            VoidIdolAggregatorModule module = ItemChangerMod.Modules.GetOrAdd<VoidIdolAggregatorModule>();
            for (int i = tier; i < 3; i++)
            {
                module.PlaceItemsAtStatue(i, this);
            }
        }

        protected override void OnUnload()
        {
            VoidIdolAggregatorModule module = ItemChangerMod.Modules.Get<VoidIdolAggregatorModule>();
            for (int i = tier; i < 3; i++)
            {
                module.RemoveItemsFromStatue(i, this);
            }
        }
    }
}
