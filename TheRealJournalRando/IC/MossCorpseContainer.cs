using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Util;
using UnityEngine;

namespace TheRealJournalRando.IC
{
    public class MossCorpseContainer : Container
    {
        public const string MossCorpse = "MossCorpse";
        public override string Name => MossCorpse;

        public override void AddGiveEffectToFsm(PlayMakerFSM fsm, ContainerGiveInfo info)
        {
            TabletUtility.AddItemParticles(fsm.gameObject, info.placement, info.items);

            FsmState journal = fsm.GetState("Journal");
            journal.Actions = new FsmStateAction[]
            {
                new DelegateBoolTest(() => info.placement.AllObtained(), "FINISHED", null),
                new AsyncLambda(callback => ItemUtility.GiveSequentially(info.items, info.placement, new GiveInfo
                {
                    FlingType = info.flingType,
                    Container = MossCorpse,
                    MessageType = MessageType.Any,
                    Transform = fsm.transform,
                }, callback))
            };
        }

        public override void ApplyTargetContext(GameObject obj, float x, float y, float elevation)
        {
            obj.transform.position = new Vector3(x, y - elevation, 2.5f);
            obj.SetActive(true);
        }
    }
}
