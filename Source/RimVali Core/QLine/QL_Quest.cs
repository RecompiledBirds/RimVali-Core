using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimValiCore.QLine
{
    public class QL_Quest : Def
    {
        public Type questAction;
        private QuestWorker qWorker;
        public bool repeatable;
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

    }

    public abstract class QuestWorker : IExposable
    {

        private int curStage;
        public int CurrentStage
        {
            get
            {
                return curStage;
            }
        }

        /// <summary>
        /// This is overriden to define the stages in a quest.
        /// </summary>
        public abstract List<QuestStage> Stages();

        public void ChangeStage(int amount)
        {
            int value = amount+curStage;
            Mathf.Clamp(value, 0, this.Stages().Count - 1);
            curStage = value;

        }

        public void IncrementStage() => ChangeStage(1);

        public void ExposeData()
        {
            Scribe_Values.Look(ref curStage, nameof(curStage));
        }
    }



}
