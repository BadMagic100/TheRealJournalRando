using HutongGames.PlayMaker;
using TheRealJournalRando.IC;

namespace TheRealJournalRando.Fsm
{
    internal class KillCounterDisplayProxy : FsmStateAction
    {
        private readonly JournalControlModule module;

        public KillCounterDisplayProxy(JournalControlModule module)
        {
            this.module = module;
        }

        public override void OnEnter()
        {
            string pdKillsName = Fsm.GetFsmString("pdKillsName").Value;
            int pdKills = Fsm.GetFsmInt("PD Kills").Value;
            FsmString result = Fsm.GetFsmString("Item notes String");

            string enemyName = pdKillsName.Substring(5);

            result.Value = $"{Fsm.GetFsmString("Kill Msg 1")} {pdKills} {Fsm.GetFsmString("Kill Msg 2")}";

            if (module.EnemyNotesIsRegistered(enemyName))
            {
                result.Value = module.GetNotesPreview(enemyName) ?? result.Value;
            }

            Finish();
        }
    }
}
