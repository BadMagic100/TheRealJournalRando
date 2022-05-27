using HutongGames.PlayMaker;
using TheRealJournalRando.IC;

namespace TheRealJournalRando.JournalManip
{
    internal class NotesInterceptProxyCompare : FsmStateAction
    {
        private readonly JournalControlModule module;

        public NotesInterceptProxyCompare(JournalControlModule module)
        {
            this.module = module;
        }

        public override void OnEnter()
        {
            string pdKillsName = Fsm.GetFsmString("pdKillsName").Value;
            int pdKills = Fsm.GetFsmInt("PD Kills").Value;

            string enemyName = pdKillsName.Substring(5);
            if (module.EnemyNotesIsRegistered(enemyName))
            {
                if (PlayerData.instance.GetBool(nameof(JournalControlModule.hasNotes) + enemyName))
                {
                    Fsm.Event("NOTES");
                }
                else
                {
                    Fsm.Event("NO NOTES");
                }
            }
            else
            {
                if (pdKills <= 0)
                {
                    Fsm.Event("NOTES");
                }
                else
                {
                    Fsm.Event("NO NOTES");
                }
            }

            Finish();
        }
    }
}
