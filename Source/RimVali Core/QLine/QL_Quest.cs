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
            Log.Message($"Loading quest: {this.defName}");
            if (questAction == null)
            {
                Log.Error("questAction cannot be null!");
            }
        }

        public QL_Quest positiveQuestResult;
        public QL_Quest negativeQuestResult;
    }

    public abstract class QuestWorker
    {

        public virtual void NegativeEndAction()
        {

        }

        public virtual void PositiveEndAction()
        {

        }

        public virtual void Action()
        {

        }

        public abstract bool IsAvalible();
        public abstract int QuestWeight();
    }



}
