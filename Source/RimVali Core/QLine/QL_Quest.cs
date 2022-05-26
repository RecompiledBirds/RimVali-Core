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

    public abstract class QuestWorker
    {
        private int curStage;
        public int CurrentStage
        {
            get
            {
                return curStage;
            }
        }
        public virtual List<QuestStage> stages
        {
            get
            {
                return new List<QuestStage>();
            }
        }
        
        public void ChangeStage(int amount)
        {
            int value = amount+curStage;
            Mathf.Clamp(value, 0, this.stages.Count - 1);
            curStage = value;

        }

        public void IncrementStage() => ChangeStage(1);

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref curStage, nameof(curStage));
        }
    }



}
