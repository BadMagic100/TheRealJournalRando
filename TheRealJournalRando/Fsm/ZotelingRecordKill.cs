using HutongGames.PlayMaker;
using TheRealJournalRando.IC;

namespace TheRealJournalRando.Fsm
{
    internal class ZotelingRecordKill : FsmStateAction
    {
        private readonly JournalKillCounterModule module;
        private readonly FsmString pdVar;
        private readonly FsmBool livingVar;

        public ZotelingRecordKill(JournalKillCounterModule module, FsmString pdVar, FsmBool livingVar)
        {
            this.module = module;
            this.pdVar = pdVar;
            this.livingVar = livingVar;
        }

        public override void OnEnter()
        {
            if (livingVar.Value)
            {
                module.Record(pdVar.Value);
            }
            livingVar.Value = false;

            Finish();
        }
    }
}
