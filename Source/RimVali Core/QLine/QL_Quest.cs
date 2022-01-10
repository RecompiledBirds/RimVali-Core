using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiCore.QLine
{
    public class QL_Quest : Def
    {
        public Type questAction;
        private QuestWorker qWorker;
        public QuestWorker QuestWorker
        {
            get
            {
                if (qWorker == null && questAction!=null)
                {
                    qWorker = (QuestWorker)Activator.CreateInstance(questAction);
                }
                return qWorker;
            }
        }
        public override void PostLoad()
        {
            if (questAction!=null && !questAction.IsAssignableFrom(typeof(QuestWorker)))
            {
                Log.Error($"{questAction.FullName} is not a QuestWorker, and cannot be assigned to {defName}!");
                questAction = null;
            }
        }

        public QL_Quest positiveQuestResult;
        public QL_Quest negativeQuestResult;
    }

    public class QuestWorker
    {
        public virtual bool CanBeGiven => true;

        public virtual void NegativeEndAction()
        {

        }

        public virtual void PositiveEndAction()
        {

        }

        public virtual void Action()
        {

        }
    }



}
